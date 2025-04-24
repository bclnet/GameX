using GameX.Crytek.Formats.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks;

#region Chunk

public abstract class Chunk : IBinaryChunk {
    protected static readonly Random _rnd = new Random();
    protected static readonly HashSet<int> _alreadyPickedRandoms = new HashSet<int>();

    static readonly Dictionary<Type, Dictionary<uint, Func<dynamic>>> _chunkFactoryCache = new Dictionary<Type, Dictionary<uint, Func<dynamic>>> { };

    internal ChunkHeader _header;
    internal Model _model;

    /// <summary>
    /// Position of the start of the chunk
    /// </summary>
    public uint Offset { get; internal set; }
    /// <summary>
    /// The Type of the Chunk
    /// </summary>
    public ChunkType ChunkType { get; internal set; }
    /// <summary>
    /// The Version of this Chunk
    /// </summary>
    internal uint Version;
    /// <summary>
    /// The ID of this Chunk
    /// </summary>
    internal int ID;
    /// <summary>
    /// The Size of this Chunk (in Bytes)
    /// </summary>
    internal uint Size;
    /// <summary>
    /// Size of the data in the chunk. This is the chunk size, minus the header (if there is one)
    /// </summary>
    public uint DataSize { get; set; }

    internal Dictionary<long, byte> SkippedBytes = new Dictionary<long, byte> { };

    public static Chunk New(ChunkType chunkType, uint version)
        => chunkType switch {
            ChunkType.SourceInfo => New<ChunkSourceInfo>(version),
            ChunkType.Timing => New<ChunkTimingFormat>(version),
            ChunkType.ExportFlags => New<ChunkExportFlags>(version),
            ChunkType.MtlName => New<ChunkMtlName>(version),
            ChunkType.DataStream => New<ChunkDataStream>(version),
            ChunkType.Mesh => New<ChunkMesh>(version),
            ChunkType.MeshSubsets => New<ChunkMeshSubsets>(version),
            ChunkType.Node => New<ChunkNode>(version),
            ChunkType.Helper => New<ChunkHelper>(version),
            ChunkType.Controller => New<ChunkController>(version),
            ChunkType.SceneProps => New<ChunkSceneProp>(version),
            ChunkType.MeshPhysicsData => New<ChunkMeshPhysicsData>(version),
            ChunkType.BoneAnim => New<ChunkBoneAnim>(version),
            // Compiled chunks
            ChunkType.CompiledBones => New<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalProxies => New<ChunkCompiledPhysicalProxies>(version),
            ChunkType.CompiledPhysicalBones => New<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledIntSkinVertices => New<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargets => New<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledExt2IntMap => New<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFaces => New<ChunkCompiledIntFaces>(version),
            // Star Citizen equivalents
            ChunkType.CompiledBonesSC => New<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalBonesSC => New<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledExt2IntMapSC => New<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFacesSC => New<ChunkCompiledIntFaces>(version),
            ChunkType.CompiledIntSkinVerticesSC => New<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargetsSC => New<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledPhysicalProxiesSC => New<ChunkCompiledPhysicalProxies>(version),
            // Star Citizen IVO chunks
            ChunkType.MtlNameIvo => New<ChunkMtlName>(version),
            ChunkType.CompiledBonesIvo => New<ChunkCompiledBones>(version),
            ChunkType.MeshIvo => New<ChunkMesh>(version),
            ChunkType.IvoSkin => New<ChunkIvoSkin>(version),
            // Star Citizen
            ChunkType.BinaryXmlDataSC => New<ChunkBinaryXmlData>(version),
            // Old chunks
            ChunkType.BoneNameList => New<ChunkBoneNameList>(version),
            ChunkType.MeshMorphTarget => New<ChunkMeshMorphTargets>(version),
            ChunkType.Mtl => new ChunkUnknown(),// Obsolete. Not used
            _ => new ChunkUnknown(),
        };

    public static T New<T>(uint version) where T : Chunk {
        if (!_chunkFactoryCache.TryGetValue(typeof(T), out var versionMap)) _chunkFactoryCache[typeof(T)] = versionMap = new Dictionary<uint, Func<dynamic>> { };
        if (!versionMap.TryGetValue(version, out var factory)) {
            var targetType = typeof(T).Assembly.GetTypes()
                .FirstOrDefault(type => !type.IsAbstract && type.IsClass && !type.IsGenericType && typeof(T).IsAssignableFrom(type) && type.Name == $"{typeof(T).Name}_{version:X}");
            if (targetType != null) factory = () => Activator.CreateInstance(targetType) as T;
            _chunkFactoryCache[typeof(T)][version] = factory;
        }
        return (factory?.Invoke() as T) ?? throw new NotSupportedException($"Version {version:X} of {typeof(T).Name} is not supported");
    }

    public void Load(Model model, ChunkHeader header) {
        _model = model;
        _header = header;
    }

    public void SkipBytes(BinaryReader r, long bytesToSkip) {
        if (bytesToSkip == 0) return;
        if (r.BaseStream.Position > Offset + Size && Size > 0) Log($"Buffer Overflow in {GetType().Name} 0x{ID:X} ({r.BaseStream.Position - Offset - Size} bytes)");
        if (r.BaseStream.Length < Offset + Size) Log($"Corrupt Headers in {GetType().Name} 0x{ID:X}");
        //if (!bytesToSkip.HasValue) bytesToSkip = Size - Math.Max(r.BaseStream.Position - Offset, 0);
        for (var i = 0L; i < bytesToSkip; i++) SkippedBytes[r.BaseStream.Position - Offset] = r.ReadByte();
    }
    public void SkipBytesRemaining(BinaryReader r) => SkipBytes(r, Size - Math.Max(r.BaseStream.Position - Offset, 0));

    public virtual void Read(BinaryReader r) {
        if (r == null) throw new ArgumentNullException(nameof(r));
        ChunkType = _header.ChunkType;
        Version = _header.Version;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;
        DataSize = Size; // For SC files, there is no header in chunks.  But need Datasize to calculate things.

        r.BaseStream.Seek(_header.Offset, SeekOrigin.Begin);

        // Star Citizen files don't have the type, version, offset and ID at the start of a chunk, so don't read them.
        if (_model.FileVersion == FileVersion.CryTek_3_4 || _model.FileVersion == FileVersion.CryTek_3_5) {
            ChunkType = (ChunkType)r.ReadUInt32();
            Version = r.ReadUInt32();
            Offset = r.ReadUInt32();
            ID = r.ReadInt32();
            DataSize = Size - 16;
        }
        if (Offset != _header.Offset || Size != _header.Size) {
            Log($"Conflict in chunk definition");
            Log($"{_header.Offset:X}+{_header.Size:X}");
            Log($"{Offset:X}+{Size:X}");
        }
    }

    /// <summary>
    /// Gets a link to the SkinningInfo model.
    /// </summary>
    /// <returns>Link to the SkinningInfo model.</returns>
    public SkinningInfo GetSkinningInfo() {
        if (_model.SkinningInfo == null) _model.SkinningInfo = new SkinningInfo();
        return _model.SkinningInfo;
    }

    public virtual void Write(BinaryWriter w) => throw new NotImplementedException();

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, Ver: {Version:X}, Offset: {Offset:X}, ID: {ID:X}, Size: {Size}";

    protected static int GetNextRandom() {
        var available = false;
        var rand = 0;
        while (!available) {
            rand = _rnd.Next(100000);
            if (!_alreadyPickedRandoms.Contains(rand)) { _alreadyPickedRandoms.Add(rand); available = true; }
        }
        return rand;
    }

    #region Log
#if LOG
    public virtual void LogChunk() {
        Log($"*** CHUNK ***");
        Log($"    ChunkType: {ChunkType}");
        Log($"    ChunkVersion: {Version:X}");
        Log($"    Offset: {Offset:X}");
        Log($"    ID: {ID:X}");
        Log($"    Size: {Size:X}");
        Log($"*** END CHUNK ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkBinaryXmlData

public abstract class ChunkBinaryXmlData : Chunk { } // 0xCCCBF004:  Binary XML Data

#endregion

#region ChunkBinaryXmlData_3

public class ChunkBinaryXmlData_3 : ChunkBinaryXmlData {
    public XmlDocument Data;

    public override void Read(BinaryReader r) {
        base.Read(r);

        //var bytesToRead = (int)(Size - Math.Max(r.BaseStream.Position - Offset, 0));
        //var buffer = r.ReadBytes(bytesToRead);
        //using var memoryStream = new MemoryStream(buffer);
        //Data = new CryXmlFile(memoryStream);
        Data = new CryXmlFile(r, true);
    }
}

#endregion

#region ChunkBoneAnim

public abstract class ChunkBoneAnim : Chunk {
    public int NumBones;

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Number of Targets:   {NumBones:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkBoneAnim_290

public class ChunkBoneAnim_290 : ChunkBoneAnim {
    public override void Read(BinaryReader r) {
        base.Read(r);

        //TODO: Implement this.
    }
}

#endregion

#region ChunkBoneNameList

/// <summary>
/// Legacy class. Not used
/// </summary>
public abstract class ChunkBoneNameList : Chunk {
    public int NumEntities;
    public List<string> BoneNames;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Number of Targets: {NumEntities}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Number of Targets:   {NumEntities:X}");
        foreach (var name in BoneNames) Log($"    Bone Name:       {name}");
    }
#endif
    #endregion
}

#endregion

#region ChunkBoneNameList_745

public class ChunkBoneNameList_745 : ChunkBoneNameList {
    public override void Read(BinaryReader r) {
        base.Read(r);

        BoneNames = r.ReadVUString().Split(' ').ToList();
    }
}

#endregion

#region ChunkCompiledBones

public abstract class ChunkCompiledBones : Chunk //  0xACDC0000:  Bones info
{
    public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
    public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
    public int NumBones;                // Number of bones in the chunk

    // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
    public List<CompiledBone> BoneList = new List<CompiledBone>();

    public List<CompiledBone> GetAllChildBones(CompiledBone bone) => bone.numChildren > 0 ? BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList() : null;

    public List<string> GetBoneNames() => BoneList.Select(a => a.boneName).ToList();

    protected void AddChildIDToParent(CompiledBone bone) {
        if (bone.parentID != 0) BoneList.FirstOrDefault(a => a.ControllerID == bone.parentID)?.childIDs.Add(bone.ControllerID); // Should only be one parent.
    }

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START CompiledBone Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
    }

    /// <summary>
    /// Writes the results of common matrix math.  For testing purposes.
    /// </summary>
    /// <param name="localRotation">The matrix that the math functions will be applied to.</param>
    void LogMatrices(Matrix3x3 localRotation) {
        localRotation.LogMatrix3x3("Regular");
        localRotation.Inverse().LogMatrix3x3("Inverse");
        localRotation.Conjugate().LogMatrix3x3("Conjugate");
        localRotation.ConjugateTranspose().LogMatrix3x3("Conjugate Transpose");
    }
#endif
    #endregion
}
//public Dictionary<int, CompiledBone> BoneDictionary = new Dictionary<int, CompiledBone>(); // Dictionary of all the CompiledBone objects based on parent offset(?).
//public CompiledBone GetParentBone(CompiledBone bone, int boneIndex) => bone.offsetParent != 0 ? BoneDictionary[boneIndex + bone.offsetParent] : null; // Should only be one parent.

////bone.ParentBone = BoneMap[i + bone.offsetParent];
//bone.ParentBone = GetParentBone(bone, i);
//bone.parentID = bone.ParentBone != null ? bone.ParentBone.ControllerID : 0;
//if (bone.parentID != 0)
//{
//    localRotation = GetParentBone(bone, i).boneToWorld.GetBoneToWorldRotationMatrix().ConjugateTransposeThisAndMultiply(bone.boneToWorld.GetBoneToWorldRotationMatrix());
//    localTranslation = GetParentBone(bone, i).LocalRotation * (bone.LocalTranslation - GetParentBone(bone, i).boneToWorld.GetBoneToWorldTranslationVector());
//}
//else
//{
//    localTranslation = bone.boneToWorld.GetBoneToWorldTranslationVector();
//    localRotation = bone.boneToWorld.GetBoneToWorldRotationMatrix();
//}
//bone.LocalTransform = GetTransformFromParts(localTranslation, localRotation);

#endregion

#region ChunkCompiledBones_800

public class ChunkCompiledBones_800 : ChunkCompiledBones {
    public override void Read(BinaryReader r) {
        base.Read(r);
        SkipBytes(r, 32); // Padding between the chunk header and the first bone.

        // Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
        // Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.
        NumBones = (int)((Size - 32) / 584);
        for (var i = 0; i < NumBones; i++) {
            var bone = new CompiledBone();
            bone.ReadCompiledBone_800(r);
            // First bone read is root bone
            if (RootBone == null) RootBone = bone;
            if (bone.offsetParent != 0) bone.ParentBone = BoneList[i + bone.offsetParent];
            bone.parentID = bone.ParentBone != null ? bone.ParentBone.ControllerID : 0;
            BoneList.Add(bone);
        }
        // Add the ChildID to the parent bone. This will help with navigation. Also set up the TransformSoFar
        foreach (var bone in BoneList) AddChildIDToParent(bone);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.HasSkinningInfo = true;
        skin.CompiledBones = BoneList;
    }
}

#endregion

#region ChunkCompiledBones_801

public class ChunkCompiledBones_801 : ChunkCompiledBones {
    public override void Read(BinaryReader r) {
        base.Read(r);
        SkipBytes(r, 32); // Padding between the chunk header and the first bone.

        // Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
        // Each bone structure is 324 bytes, so will need to seek childOffset * 324 each time, and go back.
        NumBones = (int)((Size - 48) / 324);
        for (var i = 0; i < NumBones; i++) {
            var bone = new CompiledBone();
            bone.ReadCompiledBone_801(r);
            // First bone read is root bone
            if (RootBone == null) RootBone = bone;
            if (bone.offsetParent != 0) bone.ParentBone = BoneList[i + bone.offsetParent];
            bone.parentID = bone.ParentBone != null ? bone.ParentBone.ControllerID : 0;
            BoneList.Add(bone);
        }
        // Add the ChildID to the parent bone. This will help with navigation. Also set up the TransformSoFar
        foreach (var bone in BoneList) AddChildIDToParent(bone);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.HasSkinningInfo = true;
        skin.CompiledBones = BoneList;
    }
}

#endregion

#region ChunkCompiledBones_900

public class ChunkCompiledBones_900 : ChunkCompiledBones {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumBones = r.ReadInt32();
        for (var i = 0; i < NumBones; i++) {
            var bone = new CompiledBone();
            bone.ReadCompiledBone_900(r);
            // First bone read is root bone
            if (RootBone == null) RootBone = bone;
            BoneList.Add(bone);
        }

        // Post bone read setup. Parents, children, etc.
        // Add the ChildID to the parent bone. This will help with navigation.
        var boneNames = r.ReadCStringArray(NumBones);
        for (var i = 0; i < NumBones; i++) {
            BoneList[i].boneName = boneNames[i];
            SetParentBone(BoneList[i]);
            AddChildIDToParent(BoneList[i]);
        }

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.CompiledBones = new List<CompiledBone>();
        skin.HasSkinningInfo = true;
        skin.CompiledBones = BoneList;
    }

    void SetParentBone(CompiledBone bone) {
        // offsetParent is really parent index.
        if (bone.offsetParent != -1) {
            bone.parentID = BoneList[bone.offsetParent].ControllerID;
            bone.ParentBone = BoneList[bone.offsetParent];
        }
    }
}

#endregion

#region ChunkCompiledExtToIntMap

public abstract class ChunkCompiledExtToIntMap : Chunk {
    public int Reserved;
    public int NumExtVertices;
    public ushort[] Source;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledExtToIntMap_800

public class ChunkCompiledExtToIntMap_800 : ChunkCompiledExtToIntMap {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumExtVertices = (int)(DataSize / sizeof(ushort));
        Source = r.ReadPArray<ushort>("H", NumExtVertices);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.Ext2IntMap = Source.ToList();
        skin.HasIntToExtMapping = true;
    }
}

#endregion

#region ChunkCompiledIntFaces

public abstract class ChunkCompiledIntFaces : Chunk {
    public int Reserved;
    public int NumIntFaces;
    public TFace[] Faces;

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledIntFaces_800

public class ChunkCompiledIntFaces_800 : ChunkCompiledIntFaces {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumIntFaces = (int)(DataSize / 6); // This is an array of TFaces, which are 3 uint16.
        Faces = r.ReadSArray<TFace>(NumIntFaces);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.IntFaces = Faces.ToList();
    }
}

#endregion

#region ChunkCompiledIntSkinVertices

public abstract class ChunkCompiledIntSkinVertices : Chunk {
    public int Reserved;
    public int NumIntVertices; // Calculate by size of data div by size of IntSkinVertex structure.
    public IntSkinVertex[] IntSkinVertices;

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledIntSkinVertices_800

public unsafe class ChunkCompiledIntSkinVertices_800 : ChunkCompiledIntSkinVertices {
    public override void Read(BinaryReader r) {
        base.Read(r);
        SkipBytes(r, 32); // Padding between the chunk header and the first IntVertex.

        // Size of the IntSkinVertex is 64 bytes
        NumIntVertices = (int)((Size - 32) / 64);
        IntSkinVertices = new IntSkinVertex[NumIntVertices];
        for (var i = 0; i < NumIntVertices; i++) {
            IntSkinVertices[i].Obsolete0 = r.ReadVector3();
            IntSkinVertices[i].Position = r.ReadVector3();
            IntSkinVertices[i].Obsolete2 = r.ReadVector3();
            IntSkinVertices[i].BoneIDs = r.ReadPArray<ushort>("H", 4); // Read 4 bone IDs
            IntSkinVertices[i].Weights = r.ReadPArray<float>("f", 4); // Read the weights for those bone IDs
            IntSkinVertices[i].Color.value = r.ReadInt32(); // Read the color
        }

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.IntVertices = IntSkinVertices.ToList();
    }
}

#endregion

#region ChunkCompiledIntSkinVertices_801

public unsafe class ChunkCompiledIntSkinVertices_801 : ChunkCompiledIntSkinVertices {
    public override void Read(BinaryReader r) {
        base.Read(r);
        SkipBytes(r, 32); // Padding between the chunk header and the first IntVertex.

        // Size of the IntSkinVertex is 40 bytes
        NumIntVertices = (int)((Size - 32) / 40);
        IntSkinVertices = new IntSkinVertex[NumIntVertices];
        for (var i = 0; i < NumIntVertices; i++) {
            IntSkinVertices[i].Position = r.ReadVector3();
            IntSkinVertices[i].BoneIDs = r.ReadPArray<ushort>("H", 4); // Read 4 bone IDs
            IntSkinVertices[i].Weights = r.ReadPArray<float>("f", 4); // Read the weights for those bone IDs
            IntSkinVertices[i].Color.value = r.ReadInt32(); // Read the color
        }

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.IntVertices = IntSkinVertices.ToList();
    }
}

#endregion

#region ChunkCompiledMorphTargets

public abstract class ChunkCompiledMorphTargets : Chunk {
    public int NumberOfMorphTargets;
    public MeshMorphTargetVertex[] MorphTargetVertices;

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Number of Targets:   {NumberOfMorphTargets:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledMorphTargets_800

public class ChunkCompiledMorphTargets_800 : ChunkCompiledMorphTargets {
    // TODO: Implement this.
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumberOfMorphTargets = (int)r.ReadUInt32();
        MorphTargetVertices = r.ReadSArray<MeshMorphTargetVertex>(NumberOfMorphTargets);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        //skin.MorphTargets = MorphTargetVertices.ToList();
    }
}

#endregion

#region ChunkCompiledMorphTargets_801

public class ChunkCompiledMorphTargets_801 : ChunkCompiledMorphTargets {
    // TODO: Implement this.
    public override void Read(BinaryReader r) {
        base.Read(r);

        //NumberOfMorphTargets = (int)r.ReadUInt32();
        //MorphTargetVertices = r.ReadSArray<MeshMorphTargetVertex>(NumberOfMorphTargets);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        //skin.MorphTargets = MorphTargetVertices.ToList();
    }
}

#endregion

#region ChunkCompiledPhysicalBones

public abstract class ChunkCompiledPhysicalBones : Chunk     //  0xACDC0000:  Bones info
{
    public char[] Reserved;             // 32 byte array
    public CompiledPhysicalBone RootPhysicalBone;  // First bone in the data structure.  Usually Bip01
    public int NumBones;                // Number of bones in the chunk

    public Dictionary<uint, CompiledPhysicalBone> PhysicalBoneDictionary = new Dictionary<uint, CompiledPhysicalBone>(); // Dictionary of all the CompiledBone objects based on bone name.
    public List<CompiledPhysicalBone> PhysicalBoneList = new List<CompiledPhysicalBone>();

    protected void AddChildIDToParent(CompiledPhysicalBone bone) {
        // Root bone parent ID will be zero.
        if (bone.parentID != 0) PhysicalBoneList.Where(a => a.ControllerID == bone.parentID).FirstOrDefault()?.childIDs.Add(bone.ControllerID); // Should only be one parent.
    }

    public List<CompiledPhysicalBone> GetAllChildBones(CompiledPhysicalBone bone)
        => bone.NumChildren > 0 ? PhysicalBoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList() : null;

    protected Matrix4x4 GetTransformFromParts(Vector3 localTranslation, Matrix3x3 localRotation)
        => new Matrix4x4 {
            // Translation part
            M14 = localTranslation.X,
            M24 = localTranslation.Y,
            M34 = localTranslation.Z,
            // Rotation part
            M11 = localRotation.M11,
            M12 = localRotation.M12,
            M13 = localRotation.M13,
            M21 = localRotation.M21,
            M22 = localRotation.M22,
            M23 = localRotation.M23,
            M31 = localRotation.M31,
            M32 = localRotation.M32,
            M33 = localRotation.M33,
            // Set final row
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START CompiledBone Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledPhysicalBones_800

public class ChunkCompiledPhysicalBones_800 : ChunkCompiledPhysicalBones     //  0xACDC0000:  Bones info
{
    public override void Read(BinaryReader r) {
        base.Read(r);
        SkipBytes(r, 32); // Padding between the chunk header and the first bone.

        NumBones = (int)((Size - 32) / 152);
        for (var i = 0U; i < NumBones; i++) {
            // Start reading at the root bone.  First bone found is root, then read until no more bones.
            var bone = new CompiledPhysicalBone();
            bone.ReadCompiledPhysicalBone(r);
            // Set root bone if not already set
            if (RootPhysicalBone != null) RootPhysicalBone = bone;
            PhysicalBoneList.Add(bone);
            PhysicalBoneDictionary[i] = bone;
        }
        // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
        foreach (var bone in PhysicalBoneList) AddChildIDToParent(bone);

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        //skin.PhysicalBoneMeshes
    }
}

#endregion

#region ChunkCompiledPhysicalProxies

public abstract partial class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
{
    // Properties. VERY similar to datastream, since it's essential vertex info.
    public uint Flags2;
    public int NumPhysicalProxies;     // Number of data entries
    public int BytesPerElement;        // Bytes per data entry
    //public uint Reserved1;
    //public uint Reserved2;
    public PhysicalProxy[] PhysicalProxies;

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START CompiledPhysicalProxies Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Number of Targets:   {NumPhysicalProxies:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkCompiledPhysicalProxies_800

public class ChunkCompiledPhysicalProxies_800 : ChunkCompiledPhysicalProxies {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumPhysicalProxies = (int)r.ReadUInt32(); // number of Bones in this chunk.
        PhysicalProxies = new PhysicalProxy[NumPhysicalProxies]; // now have an array of physical proxies
        for (var i = 0; i < NumPhysicalProxies; i++) {
            ref PhysicalProxy proxy = ref PhysicalProxies[i];
            // Start populating the physical stream array.  This is the Header.
            proxy.ID = r.ReadUInt32();
            proxy.NumVertices = (int)r.ReadUInt32();
            proxy.NumIndices = (int)r.ReadUInt32();
            proxy.Material = r.ReadUInt32(); // Probably a fill of some sort?
            proxy.Vertices = r.ReadPArray<Vector3>("3f", proxy.NumVertices);
            proxy.Indices = r.ReadPArray<ushort>("H", proxy.NumIndices);
            // read the crap at the end so we can move on.
            SkipBytes(r, proxy.Material);
        }

        // Add to SkinningInfo
        var skin = GetSkinningInfo();
        skin.PhysicalBoneMeshes = PhysicalProxies.ToList();
    }
}

#endregion

#region ChunkController

public abstract class ChunkController : Chunk    // cccc000d:  Controller chunk
{
    public CtrlType ControllerType;
    public int NumKeys;
    public uint ControllerFlags;    // technically a bitstruct to identify a cycle or a loop.
    public uint ControllerID;       // Unique id based on CRC32 of bone name.  Ver 827 only?
    public Key[] Keys;              // array length NumKeys.  Ver 827?

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Number of Keys: {NumKeys}, Controller ID: {ControllerID:X}, Controller Type: {ControllerType}, Controller Flags: {ControllerFlags}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** Controller Chunk ***");
        Log($"Version:                 {Version:X}");
        Log($"ID:                      {ID:X}");
        Log($"Number of Keys:          {NumKeys}");
        Log($"Controller Type:         {ControllerType}");
        Log($"Conttroller Flags:       {ControllerFlags}");
        Log($"Controller ID:           {ControllerID}");
        for (var i = 0; i < NumKeys; i++) {
            Log($"        Key {i}:       Time: {Keys[i].Time}");
            Log($"        AbsPos {i}:    {Keys[i].AbsPos.X:F7}, {Keys[i].AbsPos.Y:F7}, {Keys[i].AbsPos.Z:F7}");
            Log($"        RelPos {i}:    {Keys[i].RelPos.X:F7}, {Keys[i].RelPos.Y:F7}, {Keys[i].RelPos.Z:F7}");
        }
    }
#endif
    #endregion
}

#endregion

#region ChunkController_826

public class ChunkController_826 : ChunkController {
    public override void Read(BinaryReader r) {
        base.Read(r);

        //Log($"ID is: {id}");
        ControllerType = (CtrlType)r.ReadUInt32();
        NumKeys = (int)r.ReadUInt32();
        ControllerFlags = r.ReadUInt32();
        ControllerID = r.ReadUInt32();
        Keys = new Key[NumKeys];
        for (var i = 0; i < NumKeys; i++) {
            ref Key key = ref Keys[i];
            // Will implement fully later. Not sure I understand the structure, or if it's necessary.
            key.Time = r.ReadInt32(); //Log($"Time {Keys[i].Time}");
            key.AbsPos = r.ReadVector3(); //Log($"Abs Pos: {Keys[i].AbsPos.X:F7}  {Keys[i].AbsPos.Y:F7}  {Keys[i].AbsPos.Z:F7}");
            key.RelPos = r.ReadVector3(); //Log($"Rel Pos: {Keys[i].RelPos.X:F7}  {Keys[i].RelPos.Y:F7}  {Keys[i].RelPos.Z:F7}");
        }
    }
}

#endregion

#region ChunkDataStream

public abstract class ChunkDataStream : Chunk // cccc0016:  Contains data such as vertices, normals, etc.
{
    public uint Flags;                  // not used, but looks like the start of the Data Stream chunk
    public uint Flags1;                 // not used. UInt32 after Flags that looks like offsets
    public uint Flags2;                 // not used, looks almost like a filler.
    public DataStreamType DataStreamType { get; set; } // type of data (vertices, normals, uv, etc)
    public int NumElements;             // Number of data entries
    public int BytesPerElement;         // Bytes per data entry
    public uint Reserved1;
    public uint Reserved2;

    // Need to be careful with using float for Vertices and normals. technically it's a floating point of length BytesPerElement. May need to fix this.
    public Vector3[] Vertices;          // For dataStreamType of 0, length is NumElements. 
    public Vector3[] Normals;           // For dataStreamType of 1, length is NumElements.
    public Vector2[] UVs;               // for datastreamType of 2, length is NumElements.
    public uint[] Indices;              // for dataStreamType of 5, length is NumElements.
    public IRGBA[] Colors;              // for dataStreamType of 4, length is NumElements. Bytes per element of 4

    // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
    public Tangent[,] Tangents;         // for dataStreamType of 6, length is NumElements, 2.  
    public byte[,] ShCoeffs;            // for dataStreamType of 7, length is NumElement,BytesPerElements.
    public byte[,] ShapeDeformation;    // for dataStreamType of 8, length is NumElements,BytesPerElement.
    public byte[,] BoneMap;             // for dataStreamType of 9, length is NumElements,BytesPerElement, 2.
    //public MeshBoneMapping[] BoneMap; // for dataStreamType of 9, length is NumElements,BytesPerElement.
    public byte[,] FaceMap;             // for dataStreamType of 10, length is NumElements,BytesPerElement.
    public byte[,] VertMats;            // for dataStreamType of 11, length is NumElements,BytesPerElement.

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START DATASTREAM ***");
        Log($"    ChunkType:                       {ChunkType}");
        Log($"    Version:                         {Version:X}");
        Log($"    DataStream chunk starting point: {Flags:X}");
        Log($"    Chunk ID:                        {ID:X}");
        Log($"    DataStreamType:                  {DataStreamType}");
        Log($"    Number of Elements:              {NumElements}");
        Log($"    Bytes per Element:               {BytesPerElement}");
        Log($"*** END DATASTREAM ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkDataStream_800

public class ChunkDataStream_800 : ChunkDataStream {
    // This includes changes for 2. (byte4/1/2hex, and 20 byte per element vertices).
    short starCitizenFlag = 0;

    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags2 = r.ReadUInt32(); // another filler
        DataStreamType = (DataStreamType)r.ReadUInt32();
        NumElements = (int)r.ReadUInt32(); // number of elements in this chunk
        if (_model.FileVersion == FileVersion.CryTek_3_5 || _model.FileVersion == FileVersion.CryTek_3_4) BytesPerElement = (int)r.ReadUInt32(); // bytes per element
        else if (_model.FileVersion == FileVersion.CryTek_3_6) { BytesPerElement = r.ReadInt16(); r.ReadInt16(); } // Star Citizen 2.0 is using an int16 here now. Second value is unknown. Doesn't look like padding though.
        SkipBytes(r, 8);

        // Now do loops to read for each of the different Data Stream Types. If vertices, need to populate Vector3s for example.
        switch (DataStreamType) {
            case DataStreamType.VERTICES: // Ref is 0x00000000
                switch (BytesPerElement) {
                    case 12: Vertices = r.ReadPArray<Vector3>("3f", NumElements); break;
                    // Prey files, and old Star Citizen files. 2 byte floats.
                    case 8: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadHalfVector3(); r.ReadUInt16(); } break;
                    case 16: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadVector3(); SkipBytes(r, 4); } break;
                }
                break;
            case DataStreamType.INDICES:  // Ref is
                if (BytesPerElement == 2) { Indices = new uint[NumElements]; for (var i = 0; i < NumElements; i++) Indices[i] = r.ReadUInt16(); }
                else if (BytesPerElement == 4) Indices = r.ReadPArray<uint>("I", NumElements);
                break;
            case DataStreamType.NORMALS: Normals = r.ReadPArray<Vector3>("3f", NumElements); break;
            case DataStreamType.UVS: UVs = r.ReadPArray<Vector2>("2f", NumElements); break;
            case DataStreamType.TANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++)
                    switch (BytesPerElement) {
                        // These have to be divided by 32767 to be used properly (value between 0 and 1)
                        case 0x10:
                            // Tangent
                            Tangents[i, 0].X = r.ReadInt16();
                            Tangents[i, 0].Y = r.ReadInt16();
                            Tangents[i, 0].Z = r.ReadInt16();
                            Tangents[i, 0].W = r.ReadInt16();

                            // Binormal
                            Tangents[i, 1].X = r.ReadInt16();
                            Tangents[i, 1].Y = r.ReadInt16();
                            Tangents[i, 1].Z = r.ReadInt16();
                            Tangents[i, 1].W = r.ReadInt16();
                            break;
                        // These have to be divided by 127 to be used properly (value between 0 and 1)
                        case 0x08:
                            // Tangent
                            Tangents[i, 0].W = r.ReadSByte() / 127f;
                            Tangents[i, 0].X = r.ReadSByte() / 127f;
                            Tangents[i, 0].Y = r.ReadSByte() / 127f;
                            Tangents[i, 0].Z = r.ReadSByte() / 127f;

                            // Binormal
                            Tangents[i, 1].W = r.ReadSByte() / 127f;
                            Tangents[i, 1].X = r.ReadSByte() / 127f;
                            Tangents[i, 1].Y = r.ReadSByte() / 127f;
                            Tangents[i, 1].Z = r.ReadSByte() / 127f;

                            // Calculate the normal based on the cross product of the tangents.
                            Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                            Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                            Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                            break;
                        default: throw new Exception("Need to add new Tangent Size");
                    }
                break;
            case DataStreamType.COLORS:
                switch (BytesPerElement) {
                    case 3:
                        Colors = new IRGBA[NumElements];
                        for (var i = 0; i < NumElements; i++)
                            Colors[i] = new IRGBA(
                                r: r.ReadByte(),
                                g: r.ReadByte(),
                                b: r.ReadByte(),
                                a: 255);
                        break;
                    case 4: Colors = r.ReadSArray<IRGBA>(NumElements); break;
                    default: Log("Unknown Color Depth"); SkipBytes(r, NumElements * BytesPerElement); break;
                }
                break;
            case DataStreamType.VERTSUVS:  // 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs
                Vertices = new Vector3[NumElements];
                Normals = new Vector3[NumElements];
                Colors = new IRGBA[NumElements];
                UVs = new Vector2[NumElements];
                switch (BytesPerElement) {
                    // Used in 2.6 skin files. 3 floats for vertex position, 4 bytes for normals, 2 halfs for UVs.  Normals are calculated from Tangents
                    case 20:
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                            // Normals are stored in a signed byte, prob div by 127.
                            Normals[i].X = r.ReadSByte() / 127f;
                            Normals[i].Y = r.ReadSByte() / 127f;
                            Normals[i].Z = r.ReadSByte() / 127f;
                            r.ReadSByte(); // Should be FF.
                            UVs[i].X = (float)r.ReadHalf();
                            UVs[i].Y = (float)r.ReadHalf();
                        }
                        break;
                    // 3 half floats for verts, 3 colors, 2 half floats for UVs
                    case 16 when starCitizenFlag == 257:
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadHalf16Vector3();
                            SkipBytes(r, 2);

                            Colors[i] = new IRGBA(
                                b: r.ReadByte(),
                                g: r.ReadByte(),
                                r: r.ReadByte(),
                                a: r.ReadByte());

                            // Inelegant hack for Blender, as it's Collada importer doesn't support Alpha channels, and some materials need the alpha channel more than the green channel.
                            // This is complicated, as some materials need the green channel more.
                            byte a = Colors[i].a, g = Colors[i].g; Colors[i].a = g; Colors[i].g = a;

                            // UVs ABSOLUTELY should use the Half structures.
                            UVs[i].X = (float)r.ReadHalf();
                            UVs[i].Y = (float)r.ReadHalf();
                        }
                        break;
                    case 16 when starCitizenFlag != 257:
                        Normals = new Vector3[NumElements];
                        // Legacy version using Halfs (Also Hunt models)
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadHalfVector3();
                            Normals[i] = r.ReadHalfVector3();
                            UVs[i] = r.ReadHalfVector2();
                        }
                        break;
                    default:
                        Log("Unknown VertUV structure");
                        SkipBytes(r, NumElements * BytesPerElement);
                        break;
                }
                break;
            case DataStreamType.BONEMAP:
                var skin = GetSkinningInfo();
                skin.HasBoneMapDatastream = true;
                skin.BoneMapping = new List<MeshBoneMapping>();
                // Bones should have 4 bone IDs (index) and 4 weights.
                for (var i = 0; i < NumElements; i++) {
                    var map = new MeshBoneMapping();
                    switch (BytesPerElement) {
                        case 8:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadByte();    // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                            skin.BoneMapping.Add(map);
                            break;
                        case 12:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadUInt16();  // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights.
                            skin.BoneMapping.Add(map);
                            break;
                        default: Log("Unknown BoneMapping structure"); break;
                    }
                }
                break;
            case DataStreamType.QTANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Tangents[i, 0].W = r.ReadSByte() / 127f;
                    Tangents[i, 0].X = r.ReadSByte() / 127f;
                    Tangents[i, 0].Y = r.ReadSByte() / 127f;
                    Tangents[i, 0].Z = r.ReadSByte() / 127f;
                    // Binormal
                    Tangents[i, 1].W = r.ReadSByte() / 127f;
                    Tangents[i, 1].X = r.ReadSByte() / 127f;
                    Tangents[i, 1].Y = r.ReadSByte() / 127f;
                    Tangents[i, 1].Z = r.ReadSByte() / 127f;
                    // Calculate the normal based on the cross product of the tangents.
                    Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                    Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                    Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                }
                break;
            default: Log("***** Unknown DataStream Type *****"); break;
        }
    }
}

#endregion

#region ChunkDataStream_80000800

// Reversed endian class of x0800 for console games
public class ChunkDataStream_80000800 : ChunkDataStream {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags2 = MathX.SwapEndian(r.ReadUInt32()); // another filler
        DataStreamType = (DataStreamType)MathX.SwapEndian(r.ReadUInt32());
        NumElements = (int)MathX.SwapEndian(r.ReadUInt32()); // number of elements in this chunk
        BytesPerElement = (int)MathX.SwapEndian(r.ReadUInt32()); // bytes per element
        SkipBytes(r, 8);

        // Now do loops to read for each of the different Data Stream Types. If vertices, need to populate Vector3s for example.
        switch (DataStreamType) {
            case DataStreamType.VERTICES: // Ref is 0x00000000
                Vertices = new Vector3[NumElements];
                switch (BytesPerElement) {
                    case 12:
                        for (int i = 0; i < NumElements; i++) {
                            Vertices[i].X = MathX.SwapEndian(r.ReadSingle());
                            Vertices[i].Y = MathX.SwapEndian(r.ReadSingle());
                            Vertices[i].Z = MathX.SwapEndian(r.ReadSingle());
                        }
                        break;
                }
                break;
            case DataStreamType.INDICES:  // Ref is
                Indices = new uint[NumElements];
                if (BytesPerElement == 2) for (var i = 0; i < NumElements; i++) Indices[i] = MathX.SwapEndian(r.ReadUInt16());
                else if (BytesPerElement == 4) for (var i = 0; i < NumElements; i++) Indices[i] = MathX.SwapEndian(r.ReadUInt32());
                break;
            case DataStreamType.NORMALS:
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Normals[i].X = MathX.SwapEndian(r.ReadSingle());
                    Normals[i].Y = MathX.SwapEndian(r.ReadSingle());
                    Normals[i].Z = MathX.SwapEndian(r.ReadSingle());
                }
                break;
            case DataStreamType.UVS:
                UVs = new Vector2[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Normals[i].X = MathX.SwapEndian(r.ReadSingle());
                    Normals[i].Y = MathX.SwapEndian(r.ReadSingle());
                }
                break;
            case DataStreamType.TANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++)
                    switch (BytesPerElement) {
                        // These have to be divided by 32767 to be used properly (value between 0 and 1)
                        case 0x10:
                            // Tangent
                            Tangents[i, 0].X = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 0].Y = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 0].Z = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 0].W = MathX.SwapEndian(r.ReadInt16());

                            // Binormal
                            Tangents[i, 1].X = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 1].Y = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 1].Z = MathX.SwapEndian(r.ReadInt16());
                            Tangents[i, 1].W = MathX.SwapEndian(r.ReadInt16());
                            break;
                        // These have to be divided by 127 to be used properly (value between 0 and 1)
                        case 0x08:
                            // Tangent
                            Tangents[i, 0].W = r.ReadSByte() / 127f;
                            Tangents[i, 0].X = r.ReadSByte() / 127f;
                            Tangents[i, 0].Y = r.ReadSByte() / 127f;
                            Tangents[i, 0].Z = r.ReadSByte() / 127f;

                            // Binormal
                            Tangents[i, 1].W = r.ReadSByte() / 127f;
                            Tangents[i, 1].X = r.ReadSByte() / 127f;
                            Tangents[i, 1].Y = r.ReadSByte() / 127f;
                            Tangents[i, 1].Z = r.ReadSByte() / 127f;
                            break;
                        default: throw new Exception("Need to add new Tangent Size");
                    }
                break;
            case DataStreamType.COLORS:
                switch (BytesPerElement) {
                    case 3:
                        Colors = new IRGBA[NumElements];
                        for (var i = 0; i < NumElements; i++)
                            Colors[i] = new IRGBA(
                                r: r.ReadByte(),
                                g: r.ReadByte(),
                                b: r.ReadByte(),
                                a: 255);
                        break;
                    case 4: Colors = r.ReadSArray<IRGBA>(IRGBA.SizeOf, NumElements); break;
                    default: Log("Unknown Color Depth"); SkipBytes(r, NumElements * BytesPerElement); break;
                }
                break;
            case DataStreamType.BONEMAP:
                var skin = GetSkinningInfo();
                skin.HasBoneMapDatastream = true;
                skin.BoneMapping = new List<MeshBoneMapping>();
                // Bones should have 4 bone IDs (index) and 4 weights.
                for (var i = 0; i < NumElements; i++) {
                    var map = new MeshBoneMapping();
                    switch (BytesPerElement) {
                        case 8:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadByte();    // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                            skin.BoneMapping.Add(map);
                            break;
                        case 12:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = MathX.SwapEndian(r.ReadUInt16());  // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights.
                            skin.BoneMapping.Add(map);
                            break;
                        default: Log("Unknown BoneMapping structure"); break;
                    }
                }
                break;
            case DataStreamType.QTANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Tangents[i, 0].W = r.ReadSByte() / 127f;
                    Tangents[i, 0].X = r.ReadSByte() / 127f;
                    Tangents[i, 0].Y = r.ReadSByte() / 127f;
                    Tangents[i, 0].Z = r.ReadSByte() / 127f;
                    // Binormal
                    Tangents[i, 1].W = r.ReadSByte() / 127f;
                    Tangents[i, 1].X = r.ReadSByte() / 127f;
                    Tangents[i, 1].Y = r.ReadSByte() / 127f;
                    Tangents[i, 1].Z = r.ReadSByte() / 127f;
                    // Calculate the normal based on the cross product of the tangents.
                    Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                    Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                    Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                }
                break;
            default: Log("***** Unknown DataStream Type *****"); break;
        }
    }
}

#endregion

#region ChunkDataStream_801

public class ChunkDataStream_801 : ChunkDataStream {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags2 = r.ReadUInt32(); // another filler
        DataStreamType = (DataStreamType)r.ReadUInt32();
        SkipBytes(r, 4);
        NumElements = (int)r.ReadUInt32(); // number of elements in this chunk
        BytesPerElement = (int)r.ReadUInt32();
        SkipBytes(r, 8);

        switch (DataStreamType) {
            case DataStreamType.VERTICES:
                switch (BytesPerElement) {
                    case 12: Vertices = r.ReadPArray<Vector3>("3f", NumElements); break;
                    // Prey files, and old Star Citizen files. 2 byte floats.
                    case 8: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadHalfVector3(); r.ReadUInt16(); } break;
                    case 16: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadVector3(); SkipBytes(r, 4); } break;
                }
                break;
            case DataStreamType.INDICES:
                if (BytesPerElement == 2) { Indices = new uint[NumElements]; for (var i = 0; i < NumElements; i++) Indices[i] = r.ReadUInt16(); }
                else if (BytesPerElement == 4) Indices = r.ReadPArray<uint>("I", NumElements);
                break;
            case DataStreamType.NORMALS: Normals = r.ReadPArray<Vector3>("3f", NumElements); break;
            case DataStreamType.UVS: UVs = r.ReadPArray<Vector2>("2f", NumElements); break;
            case DataStreamType.TANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++)
                    switch (BytesPerElement) {
                        // These have to be divided by 32767 to be used properly (value between 0 and 1)
                        case 0x10:
                            // Tangent
                            Tangents[i, 0].X = r.ReadInt16();
                            Tangents[i, 0].Y = r.ReadInt16();
                            Tangents[i, 0].Z = r.ReadInt16();
                            Tangents[i, 0].W = r.ReadInt16();

                            // Binormal
                            Tangents[i, 1].X = r.ReadInt16();
                            Tangents[i, 1].Y = r.ReadInt16();
                            Tangents[i, 1].Z = r.ReadInt16();
                            Tangents[i, 1].W = r.ReadInt16();
                            break;
                        // These have to be divided by 127 to be used properly (value between 0 and 1)
                        case 0x08:
                            // Tangent
                            Tangents[i, 0].W = r.ReadSByte() / 127f;
                            Tangents[i, 0].X = r.ReadSByte() / 127f;
                            Tangents[i, 0].Y = r.ReadSByte() / 127f;
                            Tangents[i, 0].Z = r.ReadSByte() / 127f;

                            // Binormal
                            Tangents[i, 1].W = r.ReadSByte() / 127f;
                            Tangents[i, 1].X = r.ReadSByte() / 127f;
                            Tangents[i, 1].Y = r.ReadSByte() / 127f;
                            Tangents[i, 1].Z = r.ReadSByte() / 127f;

                            // Calculate the normal based on the cross product of the tangents.
                            //Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                            //Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                            //Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                            break;
                        default: throw new Exception("Need to add new Tangent Size");
                    }
                break;
            case DataStreamType.COLORS:
                switch (BytesPerElement) {
                    case 3:
                        Colors = new IRGBA[NumElements];
                        for (var i = 0; i < NumElements; i++)
                            Colors[i] = new IRGBA(
                                r: r.ReadByte(),
                                g: r.ReadByte(),
                                b: r.ReadByte(),
                                a: 255);
                        break;
                    case 4: Colors = r.ReadSArray<IRGBA>(IRGBA.SizeOf, NumElements); break;
                    default: Log("Unknown Color Depth"); SkipBytes(r, NumElements * BytesPerElement); break;
                }
                break;
            case DataStreamType.VERTSUVS:  // 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs
                Vertices = new Vector3[NumElements];
                Normals = new Vector3[NumElements];
                Colors = new IRGBA[NumElements];
                UVs = new Vector2[NumElements];
                switch (BytesPerElement) {
                    // Used in 2.6 skin files. 3 floats for vertex position, 4 bytes for normals, 2 halfs for UVs.  Normals are calculated from Tangents
                    case 20:
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                            // Normals are stored in a signed byte, prob div by 127.
                            Normals[i].X = r.ReadSByte() / 127f;
                            Normals[i].Y = r.ReadSByte() / 127f;
                            Normals[i].Z = r.ReadSByte() / 127f;
                            r.ReadSByte();
                            UVs[i].X = (float)r.ReadHalf();
                            UVs[i].Y = (float)r.ReadHalf();
                        }
                        break;
                    // 3 half floats for verts, 3 colors, 2 half floats for UVs
                    case 16:
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadHalf16Vector3();
                            SkipBytes(r, 2);

                            // Read a Quat, convert it to vector3
                            var quat = new Vector4 {
                                X = (r.ReadByte() - 128.0f) / 127.5f,
                                Y = (r.ReadByte() - 128.0f) / 127.5f,
                                Z = (r.ReadByte() - 128.0f) / 127.5f,
                                W = (r.ReadByte() - 128.0f) / 127.5f
                            };
                            Normals[i].X = (2f * (quat.X * quat.Z + quat.Y * quat.W));
                            Normals[i].Y = (2f * (quat.Y * quat.Z - quat.X * quat.W));
                            Normals[i].Z = (2f * (quat.Z * quat.Z + quat.W * quat.W)) - 1f;

                            // UVs ABSOLUTELY should use the Half structures.
                            UVs[i].X = (float)r.ReadHalf();
                            UVs[i].Y = (float)r.ReadHalf();
                        }
                        break;
                    default:
                        Log("Unknown VertUV structure");
                        SkipBytes(r, NumElements * BytesPerElement);
                        break;
                }
                break;
            case DataStreamType.BONEMAP:
                var skin = GetSkinningInfo();
                skin.HasBoneMapDatastream = true;
                skin.BoneMapping = new List<MeshBoneMapping>();
                // Bones should have 4 bone IDs (index) and 4 weights.
                for (var i = 0; i < NumElements; i++) {
                    var map = new MeshBoneMapping();
                    switch (BytesPerElement) {
                        case 8:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadByte();    // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                            skin.BoneMapping.Add(map);
                            break;
                        case 12:
                            map.BoneIndex = new int[4];
                            map.Weight = new int[4];
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadUInt16();  // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights.
                            skin.BoneMapping.Add(map);
                            break;
                        default: Log("Unknown BoneMapping structure"); break;
                    }
                }
                break;
            case DataStreamType.QTANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Tangents[i, 0].W = r.ReadSByte() / 127f;
                    Tangents[i, 0].X = r.ReadSByte() / 127f;
                    Tangents[i, 0].Y = r.ReadSByte() / 127f;
                    Tangents[i, 0].Z = r.ReadSByte() / 127f;
                    // Binormal
                    Tangents[i, 1].W = r.ReadSByte() / 127f;
                    Tangents[i, 1].X = r.ReadSByte() / 127f;
                    Tangents[i, 1].Y = r.ReadSByte() / 127f;
                    Tangents[i, 1].Z = r.ReadSByte() / 127f;
                    // Calculate the normal based on the cross product of the tangents.
                    Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                    Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                    Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                }
                break;
            default: Log("***** Unknown DataStream Type *****"); break;
        }
    }
}

#endregion

#region ChunkDataStream_900

public class ChunkDataStream_900 : ChunkDataStream {
    public ChunkDataStream_900(int numElements)
        => NumElements = numElements;

    public override void Read(BinaryReader r) {
        base.Read(r);

        DataStreamType = (DataStreamType)r.ReadUInt32();
        SkipBytes(r, 4);
        BytesPerElement = (int)r.ReadUInt32();

        switch (DataStreamType) {
            case DataStreamType.IVOINDICES:
                if (BytesPerElement == 2) {
                    Indices = new uint[NumElements]; for (var i = 0; i < NumElements; i++) Indices[i] = r.ReadUInt16();
                    if (NumElements % 2 == 1) SkipBytes(r, 2);
                    else {
                        var peek = Convert.ToChar(r.ReadByte()); // Sometimes the next Ivo chunk has a 4 byte filler, sometimes it doesn't.
                        r.BaseStream.Position -= 1;
                        if (peek == 0) SkipBytes(r, 4);
                    }
                }
                else if (BytesPerElement == 4) Indices = r.ReadPArray<uint>("I", NumElements);
                break;
            case DataStreamType.IVOVERTSUVS:
                Vertices = new Vector3[NumElements];
                Normals = new Vector3[NumElements];
                Colors = new IRGBA[NumElements];
                UVs = new Vector2[NumElements];
                switch (BytesPerElement) {
                    case 20:
                        for (var i = 0; i < NumElements; i++) {
                            Vertices[i] = r.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                            Colors[i] = new IRGBA(
                                b: r.ReadByte(),
                                g: r.ReadByte(),
                                r: r.ReadByte(),
                                a: r.ReadByte());

                            // Inelegant hack for Blender, as it's Collada importer doesn't support Alpha channels, and some materials need the alpha channel more than the green channel.
                            // This is complicated, as some materials need the green channel more.
                            byte a = Colors[i].a, g = Colors[i].g; Colors[i].a = g; Colors[i].g = a;

                            UVs[i].X = (float)r.ReadHalf();
                            UVs[i].Y = (float)r.ReadHalf();
                        }
                        if (NumElements % 2 == 1) SkipBytes(r, 4);
                        break;
                }
                break;
            case DataStreamType.IVONORMALS:
            case DataStreamType.IVONORMALS2:
                switch (BytesPerElement) {
                    case 4:
                        Normals = new Vector3[NumElements];
                        for (var i = 0; i < NumElements; i++) {
                            var x = r.ReadSByte() / 128f;
                            var y = r.ReadSByte() / 128f;
                            var z = r.ReadSByte() / 128f;
                            var w = r.ReadSByte() / 128f;
                            Normals[i].X = 2.0f * (x * z + y * w);
                            Normals[i].Y = 2.0f * (y * z - x * w);
                            Normals[i].Z = (2.0f * (z * z + w * w)) - 1.0f;
                        }
                        if (NumElements % 2 == 1) SkipBytes(r, 4);
                        break;
                    default: Log("Unknown Normals Format"); SkipBytes(r, NumElements * BytesPerElement); break;
                }
                break;
            case DataStreamType.IVONORMALS3: break;
            case DataStreamType.IVOTANGENTS:
                Tangents = new Tangent[NumElements, 2];
                Normals = new Vector3[NumElements];
                for (var i = 0; i < NumElements; i++) {
                    Tangents[i, 0].W = r.ReadSByte() / 127f;
                    Tangents[i, 0].X = r.ReadSByte() / 127f;
                    Tangents[i, 0].Y = r.ReadSByte() / 127f;
                    Tangents[i, 0].Z = r.ReadSByte() / 127f;

                    // Binormal
                    Tangents[i, 1].W = r.ReadSByte() / 127f;
                    Tangents[i, 1].X = r.ReadSByte() / 127f;
                    Tangents[i, 1].Y = r.ReadSByte() / 127f;
                    Tangents[i, 1].Z = r.ReadSByte() / 127f;

                    // Calculate the normal based on the cross product of the tangents.
                    Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                    Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                    Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                }
                break;
            case DataStreamType.IVOBONEMAP:
                var skin = GetSkinningInfo();
                skin.HasBoneMapDatastream = true;
                skin.BoneMapping = new List<MeshBoneMapping>();
                switch (BytesPerElement) {
                    case 12:
                        for (var i = 0; i < NumElements; i++) {
                            var map = new MeshBoneMapping();
                            for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadUInt16();  // read the 4 bone indexes first
                            for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                            skin.BoneMapping.Add(map);
                        }
                        if (NumElements % 2 == 1) SkipBytes(r, 4);
                        break;
                    default: Log("Unknown BoneMapping structure"); break;
                }
                break;
            case DataStreamType.IVOUNKNOWN2: break;
                //default: Log("***** Unknown DataStream Type *****"); break;
        }
    }
}

#endregion

#region ChunkExportFlags

public abstract class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
{
    public uint ChunkOffset;                    // for some reason the offset of Export Flag chunk is stored here.
    public uint Flags;                          // ExportFlags type technically, but it's just 1 value
    public uint[] RCVersion;                    // 4 uints
    public string RCVersionString;              // Technically String16

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START EXPORT FLAGS ***");
        Log($"    Export Chunk ID: {ID:X}");
        Log($"    ChunkType: {ChunkType}");
        Log($"    Version: {Version}");
        Log($"    Flags: {Flags}");
        var b = new StringBuilder("    RC Version: ");
        for (var i = 0; i < 4; i++) b.Append(RCVersion[i]);
        Log(b.ToString());
        Log();
        Log("    RCVersion String: {RCVersionString}");
        Log("*** END EXPORT FLAGS ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkExportFlags_1

public class ChunkExportFlags_1 : ChunkExportFlags {
    public override void Read(BinaryReader r) {
        base.Read(r);

        ChunkType = (ChunkType)r.ReadUInt32();
        Version = r.ReadUInt32();
        ChunkOffset = r.ReadUInt32();
        ID = r.ReadInt32();
        SkipBytes(r, 4);
        RCVersion = r.ReadPArray<uint>("I", 4);
        RCVersionString = r.ReadFUString(16);
        SkipBytesRemaining(r);
    }
}

#endregion

#region ChunkHeader

public abstract class ChunkHeader : Chunk {
    public override string ToString() {
        var b = new StringBuilder();
        b.Append($"*** CHUNK HEADER ***");
        b.Append($"    ChunkType: {ChunkType}");
        b.Append($"    ChunkVersion: {Version:X}");
        b.Append($"    Offset: {Offset:X}");
        b.Append($"    ID: {ID:X}");
        b.Append($"    Size: {Size:X}");
        b.Append($"*** END CHUNK HEADER ***");
        return b.ToString();
    }
}

#endregion

#region ChunkHeader_744

public class ChunkHeader_744 : ChunkHeader {
    public override void Read(BinaryReader r) {
        ChunkType = (ChunkType)r.ReadUInt32();
        Version = r.ReadUInt32();
        Offset = r.ReadUInt32();
        ID = r.ReadInt32();
        Size = 0; // TODO: Figure out how to return a size - postprocess header table maybe?
    }
}

#endregion

#region ChunkHeader_745

public class ChunkHeader_745 : ChunkHeader {
    public override void Read(BinaryReader r) {
        ChunkType = (ChunkType)r.ReadUInt32();
        Version = r.ReadUInt32();
        Offset = r.ReadUInt32();
        ID = r.ReadInt32();
        Size = r.ReadUInt32();
    }
}

#endregion

#region ChunkHeader_746

public class ChunkHeader_746 : ChunkHeader {
    public override void Read(BinaryReader r) {
        ChunkType = (ChunkType)r.ReadUInt16() + 0xCCCBF000;
        Version = r.ReadUInt16();
        ID = r.ReadInt32();
        Size = r.ReadUInt32();
        Offset = r.ReadUInt32();
    }
}

#endregion

#region ChunkHeader_900

public class ChunkHeader_900 : ChunkHeader {
    public override void Read(BinaryReader r) {
        ChunkType = (ChunkType)r.ReadUInt32();
        Version = r.ReadUInt32();
        Offset = (uint)r.ReadUInt64(); // All other versions use uint. No idea why uint64 is needed.
        // 0x900 version chunks no longer have chunk IDs. Use a randon mumber for now.
        ID = GetNextRandom();
    }
}

#endregion

#region ChunkHelper

/// <summary>
/// Helper chunk. This is the top level, then nodes, then mesh, then mesh subsets. CCCC0001
/// </summary>
public abstract class ChunkHelper : Chunk // CCCC0001
{
    public string Name;
    public HelperType HelperType;
    public Vector3 Pos;
    public Matrix4x4 Transform;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START Helper Chunk ***");
        Log($"    ChunkType:   {ChunkType}");
        Log($"    Version:     {Version:X}");
        Log($"    ID:          {ID:X}");
        Log($"    HelperType:  {HelperType}");
        Log($"    Position:    {Pos.X}, {Pos.Y}, {Pos.Z}");
        Log($"*** END Helper Chunk ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkHelper_744

public class ChunkHelper_744 : ChunkHelper {
    public override void Read(BinaryReader r) {
        base.Read(r);
        HelperType = (HelperType)Enum.ToObject(typeof(HelperType), r.ReadUInt32());
        if (Version == 0x744) Pos = r.ReadVector3(); // only has the Position.
        else if (Version == 0x362)   // will probably never see these.
        {
            var name = r.ReadChars(64);
            var stringLength = 0;
            for (int i = 0, j = name.Length; i < j; i++) if (name[i] == 0) { stringLength = i; break; }
            Name = new string(name, 0, stringLength);
            HelperType = (HelperType)r.ReadUInt32();
            Pos = r.ReadVector3();
        }
    }
}

#endregion

#region ChunkIvoSkin

public class ChunkIvoSkin : Chunk {
    public GeometryInfo geometryInfo;
    public ChunkMesh meshChunk;
    public ChunkMeshSubsets meshSubsetsChunk;
    public ChunkDataStream indices;
    public ChunkDataStream vertsUvs;
    public ChunkDataStream colors;
    public ChunkDataStream tangents;
}

#endregion

#region ChunkIvoSkin_900

class ChunkIvoSkin_900 : ChunkIvoSkin {
    // Node IDs for Ivo models
    // 1: NodeChunk
    // 2: MeshChunk
    // 3: MeshSubsets
    // 4: Indices
    // 5: VertsUVs (contains vertices, UVs and colors)
    // 6: Normals
    // 7: Tangents
    // 8: Bonemap  (assume all #ivo files have armatures)
    // 9: Colors
    bool hasNormalsChunk = false; // If Flags2 of the meshchunk is 5, there is a separate normals chunk

    public override void Read(BinaryReader r) {
        var model = _model;
        base.Read(r);
        SkipBytes(r, 4);

        _header.Offset = (uint)r.BaseStream.Position;
        var meshChunk = new ChunkMesh_900 {
            _model = _model,
            _header = _header,
            ChunkType = ChunkType.Mesh,
            ID = 2,
            MeshSubsetsData = 3
        };
        meshChunk.Read(r);

        model.ChunkMap.Add(meshChunk.ID, meshChunk);
        if (meshChunk.Flags2 == 5) hasNormalsChunk = true;

        SkipBytes(r, 120);  // Unknown data.  All 0x00

        _header.Offset = (uint)r.BaseStream.Position;
        // Create dummy header info here (ChunkType, version, size, offset)
        var subsetsChunk = new ChunkMeshSubsets_900(meshChunk.NumVertSubsets) {
            _model = _model,
            _header = _header,
            ChunkType = ChunkType.MeshSubsets,
            ID = 3
        };
        subsetsChunk.Read(r);
        model.ChunkMap.Add(subsetsChunk.ID, subsetsChunk);

        while (r.BaseStream.Position != r.BaseStream.Length) {
            var chunkType = (DataStreamType)r.ReadUInt32();
            r.BaseStream.Position = r.BaseStream.Position - 4;
            switch (chunkType) {
                case DataStreamType.IVOINDICES:
                    // Indices datastream
                    _header.Offset = (uint)r.BaseStream.Position;
                    var indicesDatastreamChunk = new ChunkDataStream_900(meshChunk.NumIndices) {
                        _model = _model,
                        _header = _header,
                        DataStreamType = DataStreamType.INDICES,
                        ChunkType = ChunkType.DataStream,
                        ID = 4
                    };
                    indicesDatastreamChunk.Read(r);
                    model.ChunkMap.Add(indicesDatastreamChunk.ID, indicesDatastreamChunk);
                    break;
                case DataStreamType.IVOVERTSUVS:
                    _header.Offset = (uint)r.BaseStream.Position;
                    var vertsUvsDatastreamChunk = new ChunkDataStream_900(meshChunk.NumVertices) {
                        _model = _model,
                        _header = _header,
                        DataStreamType = DataStreamType.VERTSUVS,
                        ChunkType = ChunkType.DataStream,
                        ID = 5
                    };
                    vertsUvsDatastreamChunk.Read(r);
                    model.ChunkMap.Add(vertsUvsDatastreamChunk.ID, vertsUvsDatastreamChunk);

                    // Create colors chunk
                    var c = new ChunkDataStream_900(meshChunk.NumVertices) {
                        _model = _model,
                        _header = _header,
                        ChunkType = ChunkType.DataStream,
                        BytesPerElement = 4,
                        DataStreamType = DataStreamType.COLORS,
                        Colors = vertsUvsDatastreamChunk.Colors,
                        ID = 9
                    };
                    model.ChunkMap.Add(c.ID, c);
                    break;
                case DataStreamType.IVONORMALS:
                case DataStreamType.IVONORMALS2:
                case DataStreamType.IVONORMALS3:
                    _header.Offset = (uint)r.BaseStream.Position;
                    var normals = new ChunkDataStream_900(meshChunk.NumVertices) {
                        _model = _model,
                        _header = _header,
                        DataStreamType = DataStreamType.NORMALS,
                        ChunkType = ChunkType.DataStream,
                        ID = 6
                    };
                    normals.Read(r);
                    model.ChunkMap.Add(normals.ID, normals);
                    break;
                case DataStreamType.IVOTANGENTS:
                    _header.Offset = (uint)r.BaseStream.Position;
                    var tangents = new ChunkDataStream_900(meshChunk.NumVertices) {
                        _model = _model,
                        _header = _header,
                        DataStreamType = DataStreamType.TANGENTS,
                        ChunkType = ChunkType.DataStream,
                        ID = 7
                    };
                    tangents.Read(r);
                    model.ChunkMap.Add(tangents.ID, tangents);
                    if (!hasNormalsChunk) {
                        // Create a normals chunk from Tangents data
                        var norms = new ChunkDataStream_900(meshChunk.NumVertices) {
                            _model = _model,
                            _header = _header,
                            ChunkType = ChunkType.DataStream,
                            BytesPerElement = 4,
                            DataStreamType = DataStreamType.NORMALS,
                            Normals = tangents.Normals,
                            ID = 6
                        };
                        model.ChunkMap.Add(norms.ID, norms);
                    }
                    break;
                case DataStreamType.IVOBONEMAP:
                    _header.Offset = (uint)r.BaseStream.Position;
                    var bonemap = new ChunkDataStream_900(meshChunk.NumVertices) {
                        _model = _model,
                        _header = _header,
                        DataStreamType = DataStreamType.BONEMAP,
                        ChunkType = ChunkType.DataStream,
                        ID = 8
                    };
                    bonemap.Read(r);
                    model.ChunkMap.Add(bonemap.ID, bonemap);
                    break;
                default: r.BaseStream.Position = r.BaseStream.Position + 4; break;
            }
        }
    }
}

#endregion

#region ChunkMesh

public abstract partial class ChunkMesh : Chunk      //  cccc0000:  Object that points to the datastream chunk.
{
    // public uint Version;             // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
    //public bool HasVertexWeights;     // 744
    //public bool HasVertexColors;      // 744
    //public bool InWorldSpace;         // 623
    //public byte Reserved1;            // 744, padding byte, 
    //public byte Reserved2;            // 744, padding byte
    public int Flags1;                  // 800 Offset of this chunk. 
    public int Flags2;                  // 801 and 802
    // public uint ID;                  // 800 Chunk ID
    public int NumVertices;             // 
    public int NumIndices;              // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
    //public uint NumUVs;               // 744
    //public uint NumFaces;             // 744
    // Pointers to various Chunk types
    //public ChunkMtlName Material;     // 623, Material Chunk, never encountered?
    public int NumVertSubsets;          // 801, Number of vert subsets
    public int VertsAnimID;
    public int MeshSubsetsData;         // 800  Reference of the mesh subsets
    // public ChunkVertAnim VertAnims;  // 744 Not implemented
    //public Vertex[] Vertices;         // 744 Not implemented
    //public Face[,] Faces;             // 744 Not implemented
    //public UV[] UVs;                  // 744 Not implemented
    //public UVFace[] UVFaces;          // 744 Not implemented
    // public VertexWeight[] VertexWeights; // 744 not implemented
    //public IRGB[] VertexColors;       // 744 not implemented
    public int VerticesData;            // 800, 801.  Need an array because some 801 files have NumVertSubsets
    public int NumBuffs;
    public int NormalsData;             // 800
    public int UVsData;                 // 800
    public int ColorsData;              // 800
    public int Colors2Data;             // 800 
    public int IndicesData;             // 800
    public int TangentsData;            // 800
    public int ShCoeffsData;            // 800
    public int ShapeDeformationData;    // 800
    public int BoneMapData;             // 800
    public int FaceMapData;             // 800
    public int VertMatsData;            // 800
    public int MeshPhysicsData;         // 801
    public int VertsUVsData;            // 801
    public int[] PhysicsData = new int[4]; // 800
    public Vector3 MinBound;            // 800 minimum coordinate values
    public Vector3 MaxBound;            // 800 Max coord values

    /// <summary>
    /// The actual geometry info for this mesh.
    /// </summary>
    //public GeometryInfo GeometryInfo { get; set; }
    //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MESH CHUNK ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Chunk ID:            {ID:X}");
        Log($"    MeshSubSetID:        {MeshSubsetsData:X}");
        Log($"    Vertex Datastream:   {VerticesData:X}");
        Log($"    Normals Datastream:  {NormalsData:X}");
        Log($"    UVs Datastream:      {UVsData:X}");
        Log($"    Indices Datastream:  {IndicesData:X}");
        Log($"    Tangents Datastream: {TangentsData:X}");
        Log($"    Mesh Physics Data:   {MeshPhysicsData:X}");
        Log($"    VertUVs:             {VertsUVsData:X}");
        Log($"    MinBound:            {MinBound.X:F7}, {MinBound.Y:F7}, {MinBound.Z:F7}");
        Log($"    MaxBound:            {MaxBound.X:F7}, {MaxBound.Y:F7}, {MaxBound.Z:F7}");
        Log($"*** END MESH CHUNK ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkMesh_800

public class ChunkMesh_800 : ChunkMesh {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumVertSubsets = 1;
        SkipBytes(r, 8);
        NumVertices = r.ReadInt32();
        NumIndices = r.ReadInt32();             //  Number of indices
        SkipBytes(r, 4);
        MeshSubsetsData = r.ReadInt32();        // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
        SkipBytes(r, 4);
        VerticesData = r.ReadInt32();           // ID of the datastream for the vertices for this mesh
        NormalsData = r.ReadInt32();            // ID of the datastream for the normals for this mesh
        UVsData = r.ReadInt32();                // refers to the ID in the Normals datastream?
        ColorsData = r.ReadInt32();
        Colors2Data = r.ReadInt32();
        IndicesData = r.ReadInt32();
        TangentsData = r.ReadInt32();
        ShCoeffsData = r.ReadInt32();
        ShapeDeformationData = r.ReadInt32();
        BoneMapData = r.ReadInt32();
        FaceMapData = r.ReadInt32();
        VertMatsData = r.ReadInt32();
        SkipBytes(r, 16);
        for (var i = 0; i < 4; i++) {
            PhysicsData[i] = r.ReadInt32();
            if (PhysicsData[i] != 0) MeshPhysicsData = PhysicsData[i];
        }
        MinBound = r.ReadVector3();
        MaxBound = r.ReadVector3();
    }
}

#endregion

#region ChunkMesh_80000800

public class ChunkMesh_80000800 : ChunkMesh {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumVertSubsets = 1;
        SkipBytes(r, 8);
        NumVertices = MathX.SwapEndian(r.ReadInt32());
        NumIndices = MathX.SwapEndian(r.ReadInt32());           //  Number of indices
        SkipBytes(r, 4);
        MeshSubsetsData = MathX.SwapEndian(r.ReadInt32());      // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
        SkipBytes(r, 4);
        VerticesData = MathX.SwapEndian(r.ReadInt32());         // ID of the datastream for the vertices for this mesh
        NormalsData = MathX.SwapEndian(r.ReadInt32());          // ID of the datastream for the normals for this mesh
        UVsData = MathX.SwapEndian(r.ReadInt32());              // refers to the ID in the Normals datastream?
        ColorsData = MathX.SwapEndian(r.ReadInt32());
        Colors2Data = MathX.SwapEndian(r.ReadInt32());
        IndicesData = MathX.SwapEndian(r.ReadInt32());
        TangentsData = MathX.SwapEndian(r.ReadInt32());
        ShCoeffsData = MathX.SwapEndian(r.ReadInt32());
        ShapeDeformationData = MathX.SwapEndian(r.ReadInt32());
        BoneMapData = MathX.SwapEndian(r.ReadInt32());
        FaceMapData = MathX.SwapEndian(r.ReadInt32());
        VertMatsData = MathX.SwapEndian(r.ReadInt32());
        SkipBytes(r, 16);
        for (var i = 0; i < 4; i++) {
            PhysicsData[i] = MathX.SwapEndian(r.ReadInt32());
            if (PhysicsData[i] != 0) MeshPhysicsData = PhysicsData[i];
        }
        MinBound.X = MathX.SwapEndian(r.ReadSingle());
        MinBound.Y = MathX.SwapEndian(r.ReadSingle());
        MinBound.Z = MathX.SwapEndian(r.ReadSingle());
        MaxBound.X = MathX.SwapEndian(r.ReadSingle());
        MaxBound.Y = MathX.SwapEndian(r.ReadSingle());
        MaxBound.Z = MathX.SwapEndian(r.ReadSingle());
    }
}

#endregion

#region ChunkMesh_801

public class ChunkMesh_801 : ChunkMesh {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags1 = r.ReadInt32();
        Flags2 = r.ReadInt32();
        NumVertices = r.ReadInt32();
        NumIndices = r.ReadInt32();
        NumVertSubsets = r.ReadInt32();
        MeshSubsetsData = r.ReadInt32();        // Chunk ID of mesh subsets 
        VertsAnimID = r.ReadInt32();
        VerticesData = r.ReadInt32();
        NormalsData = r.ReadInt32();            // Chunk ID of the datastream for the normals for this mesh
        UVsData = r.ReadInt32();                // Chunk ID of the Normals datastream
        ColorsData = r.ReadInt32();
        Colors2Data = r.ReadInt32();
        IndicesData = r.ReadInt32();
        TangentsData = r.ReadInt32();
        SkipBytes(r, 16);
        for (var i = 0; i < 4; i++) PhysicsData[i] = r.ReadInt32();
        VertsUVsData = r.ReadInt32();           // This should be a vertsUV Chunk ID.
        ShCoeffsData = r.ReadInt32();
        ShapeDeformationData = r.ReadInt32();
        BoneMapData = r.ReadInt32();
        FaceMapData = r.ReadInt32();
        MinBound = r.ReadVector3();
        MaxBound = r.ReadVector3();
    }
}

#endregion

#region ChunkMesh_802

public class ChunkMesh_802 : ChunkMesh {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags1 = r.ReadInt32();
        Flags2 = r.ReadInt32();
        NumVertices = r.ReadInt32();
        NumIndices = r.ReadInt32();
        NumVertSubsets = r.ReadInt32();
        MeshSubsetsData = r.ReadInt32();        // Chunk ID of mesh subsets 
        SkipBytes(r, 4);
        VerticesData = r.ReadInt32();
        SkipBytes(r, 28);
        NormalsData = r.ReadInt32();            // Chunk ID of the datastream for the normals for this mesh
        SkipBytes(r, 28);
        UVsData = r.ReadInt32();               // Chunk ID of the Normals datastream
        SkipBytes(r, 28);
        ColorsData = r.ReadInt32();
        SkipBytes(r, 28);
        Colors2Data = r.ReadInt32();
        SkipBytes(r, 28);
        IndicesData = r.ReadInt32();
        SkipBytes(r, 28);
        TangentsData = r.ReadInt32();
        SkipBytes(r, 28);
        SkipBytes(r, 16);
        for (var i = 0; i < 4; i++) PhysicsData[i] = r.ReadInt32();
        VertsUVsData = r.ReadInt32();          // This should be a vertsUV Chunk ID.
        SkipBytes(r, 28);
        ShCoeffsData = r.ReadInt32();
        SkipBytes(r, 28);
        ShapeDeformationData = r.ReadInt32();
        SkipBytes(r, 28);
        BoneMapData = r.ReadInt32();
        SkipBytes(r, 28);
        FaceMapData = r.ReadInt32();
        SkipBytes(r, 28);
        SkipBytes(r, 16);
        SkipBytes(r, 96);                      // Lots of unknown data here.
        MinBound = r.ReadVector3();
        MaxBound = r.ReadVector3();
    }
}

#endregion

#region ChunkMesh_900

public class ChunkMesh_900 : ChunkMesh {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags1 = 0;
        Flags2 = r.ReadInt32();
        NumVertices = r.ReadInt32();
        NumIndices = r.ReadInt32();
        NumVertSubsets = (int)r.ReadUInt32();
        SkipBytes(r, 4);
        MinBound = r.ReadVector3();
        MaxBound = r.ReadVector3();
        ID = 2; // Node chunk ID = 1
        IndicesData = 4;
        VertsUVsData = 5;
        NormalsData = 6;
        TangentsData = 7;
        BoneMapData = 8;
        ColorsData = 9;
    }
}

#endregion

#region ChunkMeshMorphTargets

/// <summary>
/// Legacy class.  No longer used.
/// </summary>
public abstract class ChunkMeshMorphTargets : Chunk {
    public uint ChunkIDMesh;
    public int NumMorphVertices;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Chunk ID Mesh: {ChunkIDMesh}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START MorphTargets Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Chunk ID Mesh:       {ChunkIDMesh:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkMeshMorphTargets_800

public abstract class ChunkMeshMorphTargets_800 : ChunkMeshMorphTargets {
    public override void Read(BinaryReader r) {
        base.Read(r);

        // TODO: Implement ChunkMeshMorphTargets ver 0x801.
    }
}

#endregion

#region ChunkMeshPhysicsData

/// <summary>
/// Collision mesh or something like that. TODO
/// </summary>
/// <seealso cref="GameX.Crytek.Formats.Core.Chunks.Chunk" />
class ChunkMeshPhysicsData : Chunk {
    public int PhysicsDataSize;             // Size of the physical data at the end of the chunk.
    public int Flags;
    public int TetrahedraDataSize;          // Bytes per data entry
    public int TetrahedraID;                // Chunk ID of the data stream
    public ChunkDataStream Tetrahedra;
    public uint Reserved1;
    public uint Reserved2;

    public PhysicsData physicsData;  // if physicsdatasize != 0
    public byte[] TetrahedraData; // Array length TetrahedraDataSize.  

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START CompiledBone Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Node ID:             {PhysicsDataSize:X}");
        Log($"    Node ID:             {TetrahedraDataSize:X}");
        Log($"    Node ID:             {TetrahedraID:X}");
        Log($"    Node ID:             {ID:X}");
    }
#endif
    #endregion
}

#endregion

#region ChunkMeshPhysicsData_800

class ChunkMeshPhysicsData_800 : ChunkMeshPhysicsData {
    public override void Read(BinaryReader r) {
        base.Read(r);

        // TODO: Implement ChunkMeshPhysicsData ver 0x800.
    }
}

#endregion

#region ChunkMeshPhysicsData_80000800

class ChunkMeshPhysicsData_80000800 : ChunkMeshPhysicsData {
    public override void Read(BinaryReader r) {
        base.Read(r);

        // TODO: Implement ChunkMeshPhysicsData ver 0x800.
    }
}

#endregion

#region ChunkMeshSubsets

public abstract class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
{
    public uint Flags; // probably the offset
    public int NumMeshSubset; // number of mesh subsets
    public MeshSubset[] MeshSubsets;

    // For bone ID meshes? Not sure where this is used yet.
    public int NumberOfBoneIDs;
    public ushort[] BoneIDs;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Number of Mesh Subsets: {NumMeshSubset}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log("*** START MESH SUBSET CHUNK ***");
        Log("    ChunkType:       {ChunkType}");
        Log("    Mesh SubSet ID:  {ID:X}");
        Log("    Number of Mesh Subsets: {NumMeshSubset}");
        for (var i = 0; i < NumMeshSubset; i++) {
            Log($"        ** Mesh Subset:          {i}");
            Log($"           First Index:          {MeshSubsets[i].FirstIndex}");
            Log($"           Number of Indices:    {MeshSubsets[i].NumIndices}");
            Log($"           First Vertex:         {MeshSubsets[i].FirstVertex}");
            Log($"           Number of Vertices:   {MeshSubsets[i].NumVertices}  (next will be {MeshSubsets[i].NumVertices + MeshSubsets[i].FirstVertex})");
            Log($"           Material ID:          {MeshSubsets[i].MatID}");
            Log($"           Radius:               {MeshSubsets[i].Radius}");
            Log($"           Center:   {MeshSubsets[i].Center.X},{MeshSubsets[i].Center.Y},{MeshSubsets[i].Center.Z}");
            Log($"        ** Mesh Subset {i} End");
        }
        Log("*** END MESH SUBSET CHUNK ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkMeshSubsets_800

public class ChunkMeshSubsets_800 : ChunkMeshSubsets {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags = r.ReadUInt32();   // Might be a ref to this chunk
        NumMeshSubset = (int)r.ReadUInt32();  // number of mesh subsets
        SkipBytes(r, 8);
        MeshSubsets = new MeshSubset[NumMeshSubset];
        for (var i = 0; i < NumMeshSubset; i++) {
            MeshSubsets[i].FirstIndex = (int)r.ReadUInt32();
            MeshSubsets[i].NumIndices = (int)r.ReadUInt32();
            MeshSubsets[i].FirstVertex = (int)r.ReadUInt32();
            MeshSubsets[i].NumVertices = (int)r.ReadUInt32();
            MeshSubsets[i].MatID = r.ReadUInt32();
            MeshSubsets[i].Radius = r.ReadSingle();
            MeshSubsets[i].Center.X = r.ReadSingle();
            MeshSubsets[i].Center.Y = r.ReadSingle();
            MeshSubsets[i].Center.Z = r.ReadSingle();
        }
    }
}

#endregion

#region ChunkMeshSubsets_800000800

public class ChunkMeshSubsets_800000800 : ChunkMeshSubsets {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Flags = MathX.SwapEndian(r.ReadUInt32());   // Might be a ref to this chunk
        NumMeshSubset = (int)MathX.SwapEndian(r.ReadUInt32());  // number of mesh subsets
        SkipBytes(r, 8);
        MeshSubsets = new MeshSubset[NumMeshSubset];
        for (var i = 0; i < NumMeshSubset; i++) {
            MeshSubsets[i].FirstIndex = (int)MathX.SwapEndian(r.ReadUInt32());
            MeshSubsets[i].NumIndices = (int)MathX.SwapEndian(r.ReadUInt32());
            MeshSubsets[i].FirstVertex = (int)MathX.SwapEndian(r.ReadUInt32());
            MeshSubsets[i].NumVertices = (int)MathX.SwapEndian(r.ReadUInt32());
            MeshSubsets[i].MatID = MathX.SwapEndian(r.ReadUInt32());
            MeshSubsets[i].Radius = MathX.SwapEndian(r.ReadSingle());
            MeshSubsets[i].Center.X = MathX.SwapEndian(r.ReadSingle());
            MeshSubsets[i].Center.Y = MathX.SwapEndian(r.ReadSingle());
            MeshSubsets[i].Center.Z = MathX.SwapEndian(r.ReadSingle());
        }
    }
}

#endregion

#region ChunkMeshSubsets_900

public class ChunkMeshSubsets_900 : ChunkMeshSubsets {
    public ChunkMeshSubsets_900(int numMeshSubset) => NumMeshSubset = numMeshSubset;

    public override void Read(BinaryReader r) {
        base.Read(r);
        MeshSubsets = new MeshSubset[NumMeshSubset];
        for (var i = 0; i < NumMeshSubset; i++) {
            MeshSubsets[i].MatID = (uint)r.ReadInt32();
            MeshSubsets[i].FirstIndex = r.ReadInt32();
            MeshSubsets[i].NumIndices = r.ReadInt32();
            MeshSubsets[i].FirstVertex = r.ReadInt32();
            MeshSubsets[i].NumVertices = r.ReadInt32();
            MeshSubsets[i].Radius = r.ReadSingle();
            MeshSubsets[i].Center = r.ReadVector3();
            SkipBytes(r, 12); // 3 unknowns; possibly floats;
        }
    }
}

#endregion

#region ChunkMtlName

public abstract class ChunkMtlName : Chunk  // cccc0014:  provides material name as used in the .mtl file
{
    /// <summary>
    /// Type of Material associated with this name
    /// </summary>
    public MtlNameType MatType;
    /// <summary>
    /// Name of the Material
    /// </summary>
    public string Name;
    public MtlNamePhysicsType[] PhysicsType;
    /// <summary>
    /// Number of Materials in this name (Max: 66)
    /// </summary>
    public int NumChildren;
    public uint[] ChildIDs;
    public uint NFlags2;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Material Name: {Name}, Number of Children: {NumChildren}, Material Type: {MatType}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log("*** START MATERIAL NAMES ***");
        Log($"    ChunkType:           {ChunkType} ({ChunkType:X})");
        Log($"    Material Name:       {Name}");
        Log($"    Material ID:         {ID:X}");
        Log($"    Version:             {Version:X}");
        Log($"    Number of Children:  {NumChildren}");
        Log($"    Material Type:       {MatType} ({MatType:X})");
        foreach (var physicsType in PhysicsType) Log($"    Physics Type:        {physicsType} ({physicsType:X})");
        Log("*** END MATERIAL NAMES ***");
    }
#endif
    #endregion

}

#endregion

#region ChunkMtlName_744

// provides material name as used in the .mtl file. CCCC0014
public class ChunkMtlName_744 : ChunkMtlName {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(128);
        NumChildren = (int)r.ReadUInt32();
        MatType = NumChildren == 0 ? MtlNameType.Single : MtlNameType.Library;
        PhysicsType = new MtlNamePhysicsType[NumChildren];
        for (var i = 0; i < NumChildren; i++) PhysicsType[i] = (MtlNamePhysicsType)r.ReadUInt32();
    }
}

#endregion

#region ChunkMtlName_800

public class ChunkMtlName_800 : ChunkMtlName {
    public override void Read(BinaryReader r) {
        base.Read(r);

        MatType = (MtlNameType)r.ReadUInt32();
        // if 0x01, then material lib. If 0x12, mat name. This is actually a bitstruct.
        NFlags2 = r.ReadUInt32(); // NFlags2
        Name = r.ReadFUString(128);
        PhysicsType = [(MtlNamePhysicsType)r.ReadUInt32()];
        NumChildren = (int)r.ReadUInt32();
        // Now we need to read the Children references. 2 parts; the number of children, and then 66 - numchildren padding
        ChildIDs = r.ReadPArray<uint>("I", NumChildren);
        SkipBytes(r, 32);
    }
}

#endregion

#region ChunkMtlName_80000800

public class ChunkMtlName_80000800 : ChunkMtlName {
    public override void Read(BinaryReader r) {
        base.Read(r);

        MatType = (MtlNameType)MathX.SwapEndian(r.ReadUInt32());
        // if 0x01, then material lib. If 0x12, mat name. This is actually a bitstruct.
        NFlags2 = MathX.SwapEndian(r.ReadUInt32()); // NFlags2
        Name = r.ReadFUString(128);
        PhysicsType = new[] { (MtlNamePhysicsType)MathX.SwapEndian(r.ReadUInt32()) };
        NumChildren = (int)MathX.SwapEndian(r.ReadUInt32());
        // Now we need to read the Children references. 2 parts; the number of children, and then 66 - numchildren padding
        ChildIDs = new uint[NumChildren];
        for (var i = 0; i < NumChildren; i++) ChildIDs[i] = MathX.SwapEndian(r.ReadUInt32());
        SkipBytes(r, 32);
    }
}

#endregion

#region ChunkMtlName_802

public class ChunkMtlName_802 : ChunkMtlName {
    // Appears to have 4 more Bytes than ChunkMtlName_744
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(128);
        NumChildren = (int)r.ReadUInt32();
        MatType = NumChildren == 0 ? MtlNameType.Single : MtlNameType.Library;
        PhysicsType = new MtlNamePhysicsType[NumChildren];
        for (var i = 0; i < NumChildren; i++) PhysicsType[i] = (MtlNamePhysicsType)r.ReadUInt32();
    }
}

#endregion

#region ChunkMtlName_900

public class ChunkMtlName_900 : ChunkMtlName {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(128);
        NumChildren = 0;
    }
}

#endregion

#region ChunkNode

public abstract class ChunkNode : Chunk // cccc000b:   Node
{
    protected float VERTEX_SCALE = 1f / 100;

    /// <summary>Chunk Name (String[64])</summary>
    public string Name { get; internal set; }
    /// <summary>Mesh or Helper Object ID</summary>
    public int ObjectNodeID { get; internal set; }
    /// <summary>Node parent.  if 0xFFFFFFFF, it's the top node.  Maybe...</summary>
    public int ParentNodeID { get; internal set; }  // Parent nodeID
    public int __NumChildren;
    /// <summary>Material ID for this chunk</summary>
    public int MatID { get; internal set; }
    public bool IsGroupHead { get; internal set; }
    public bool IsGroupMember { get; internal set; }
    /// <summary>Transformation Matrix</summary>
    public Matrix4x4 Transform { get; internal set; }
    /// <summary>Position vector of Transform</summary>
    public Vector3 Pos { get; internal set; }
    /// <summary>Rotation component of Transform</summary>
    public Quaternion Rot { get; internal set; }
    /// <summary>Scalar component of Transform</summary>
    public Vector3 Scale { get; internal set; }
    /// <summary>Position Controller ID - Obsolete</summary>
    public int PosCtrlID { get; internal set; }
    /// <summary>Rotation Controller ID - Obsolete</summary>
    public int RotCtrlID { get; internal set; }
    /// <summary>Scalar Controller ID - Obsolete</summary>
    public int SclCtrlID { get; internal set; }
    /// <summary>Appears to be a Blob of properties, separated by new lines</summary>
    public string Properties { get; internal set; }

    // Calculated Properties
    public Matrix4x4 LocalTransform
        => Matrix4x4.Transpose(Transform);

    ChunkNode _parentNode;
    public ChunkNode ParentNode {
        get {
            if (ParentNodeID == ~0) return null; // aka 0xFFFFFFFF, or -1
            if (_parentNode == null) _parentNode = _model.ChunkMap.TryGetValue(ParentNodeID, out var node) ? node as ChunkNode : _model.RootNode;
            return _parentNode;
        }
        set {
            ParentNodeID = value == null ? ~0 : value.ID;
            _parentNode = value;
        }
    }

    public List<ChunkNode> ChildNodes { get; set; }

    Chunk _objectChunk;
    public Chunk ObjectChunk {
        get {
            if (_objectChunk == null) _model.ChunkMap.TryGetValue(ObjectNodeID, out _objectChunk);
            return _objectChunk;
        }
        set => _objectChunk = value;
    }

    public List<ChunkNode> AllChildNodes
        => __NumChildren == 0 ? null : _model.NodeMap.Values.Where(a => a.ParentNodeID == ID).ToList();

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START Node Chunk ***");
        Log($"    ChunkType:           {ChunkType}");
        Log($"    Node ID:             {ID:X}");
        Log($"    Node Name:           {Name}");
        Log($"    Object ID:           {ObjectNodeID:X}");
        Log($"    Parent ID:           {ParentNodeID:X}");
        Log($"    Number of Children:  {__NumChildren}");
        Log($"    Material ID:         {MatID:X}"); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
        Log($"    Position:            {Pos.X:F7}   {Pos.Y:F7}   {Pos.Z:F7}");
        Log($"    Scale:               {Scale.X:F7}   {Scale.Y:F7}   {Scale.Z:F7}");
        Log($"    Transformation:      {Transform.M11:F7}  {Transform.M12:F7}  {Transform.M13:F7}  {Transform.M14:F7}");
        Log($"                         {Transform.M21:F7}  {Transform.M22:F7}  {Transform.M23:F7}  {Transform.M24:F7}");
        Log($"                         {Transform.M31:F7}  {Transform.M32:F7}  {Transform.M33:F7}  {Transform.M34:F7}");
        Log($"                         {Transform.M41 / 100:F7}  {Transform.M42 / 100:F7}  {Transform.M43 / 100:F7}  {Transform.M44:F7}");
        //Log($"    Transform_sum:       {TransformSoFar.X:F7}  {TransformSoFar.Y:F7}  {TransformSoFar.Z:F7}");
        //Log($"    Rotation_sum:");
        //RotSoFar.LogMatrix3x3();
        Log($"*** END Node Chunk ***");
    }
#endif
    #endregion
}

#if false
    public Vector3 TransformSoFar => ParentNode != null
        ? ParentNode.TransformSoFar + Transform.GetTranslation()
        : Transform.GetTranslation();

    public Matrix3x3 RotSoFar => ParentNode != null
        ? Transform.GetRotation() * ParentNode.RotSoFar
        : _model.RootNode.Transform.GetRotation();

    /// <summary>
    /// Gets the transform of the vertex.  This will be both the rotation and translation of this vertex, plus all the parents.
    /// The transform matrix is a 4x4 matrix.  Vector3 is a 3x1.  We need to convert vector3 to vector4, multiply the matrix, then convert back to vector3.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Vector3 GetTransform(Vector3 transform)
    {
        var vec3 = transform;
        // Apply the local transforms (rotation and translation) to the vector
        // Do rotations.  Rotations must come first, then translate.
        vec3 = RotSoFar * vec3;
        // Do translations.  I think this is right.  Objects in right place, not rotated right.
        vec3 += TransformSoFar;
        return vec3;
    }

    
#endif

#endregion

#region ChunkNode_80000823

public class ChunkNode_80000823 : ChunkNode {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(64);
        if (string.IsNullOrEmpty(Name)) Name = "unknown";
        ObjectNodeID = MathX.SwapEndian(r.ReadInt32()); // Object reference ID
        ParentNodeID = MathX.SwapEndian(r.ReadInt32());
        __NumChildren = MathX.SwapEndian(r.ReadInt32());
        MatID = MathX.SwapEndian(r.ReadInt32());  // Material ID?
        SkipBytes(r, 4);

        // Read the 4x4 transform matrix.
        var transform = new Matrix4x4 {
            M11 = MathX.SwapEndian(r.ReadSingle()),
            M12 = MathX.SwapEndian(r.ReadSingle()),
            M13 = MathX.SwapEndian(r.ReadSingle()),
            M14 = MathX.SwapEndian(r.ReadSingle()),
            M21 = MathX.SwapEndian(r.ReadSingle()),
            M22 = MathX.SwapEndian(r.ReadSingle()),
            M23 = MathX.SwapEndian(r.ReadSingle()),
            M24 = MathX.SwapEndian(r.ReadSingle()),
            M31 = MathX.SwapEndian(r.ReadSingle()),
            M32 = MathX.SwapEndian(r.ReadSingle()),
            M33 = MathX.SwapEndian(r.ReadSingle()),
            M34 = MathX.SwapEndian(r.ReadSingle()),
            M41 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            M42 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            M43 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            M44 = MathX.SwapEndian(r.ReadSingle()),
        };
        // original transform matrix is 3x4 stored as 4x4
        transform.M14 = transform.M24 = transform.M34 = 0f;
        transform.M44 = 1f;
        Transform = transform;

        // Read the position Pos Vector3
        Pos = new Vector3 {
            X = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            Y = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            Z = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
        };

        // Read the rotation Rot Quad
        Rot = new Quaternion {
            X = MathX.SwapEndian(r.ReadSingle()),
            Y = MathX.SwapEndian(r.ReadSingle()),
            Z = MathX.SwapEndian(r.ReadSingle()),
            W = MathX.SwapEndian(r.ReadSingle()),
        };

        // Read the Scale Vector 3
        Scale = new Vector3 {
            X = MathX.SwapEndian(r.ReadSingle()),
            Y = MathX.SwapEndian(r.ReadSingle()),
            Z = MathX.SwapEndian(r.ReadSingle()),
        };

        // read the controller pos/rot/scale
        PosCtrlID = MathX.SwapEndian(r.ReadInt32());
        RotCtrlID = MathX.SwapEndian(r.ReadInt32());
        SclCtrlID = MathX.SwapEndian(r.ReadInt32());

        Properties = r.ReadL32UString();
    }
}

#endregion

#region ChunkNode_823

public class ChunkNode_823 : ChunkNode {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(64);
        if (string.IsNullOrEmpty(Name)) Name = "unknown";
        ObjectNodeID = r.ReadInt32(); // Object reference ID
        ParentNodeID = r.ReadInt32();
        __NumChildren = r.ReadInt32();
        MatID = r.ReadInt32(); // Material ID?
        SkipBytes(r, 4);

        // Read the 4x4 transform matrix.
        var transform = new Matrix4x4 {
            M11 = r.ReadSingle(),
            M12 = r.ReadSingle(),
            M13 = r.ReadSingle(),
            M14 = r.ReadSingle(),
            M21 = r.ReadSingle(),
            M22 = r.ReadSingle(),
            M23 = r.ReadSingle(),
            M24 = r.ReadSingle(),
            M31 = r.ReadSingle(),
            M32 = r.ReadSingle(),
            M33 = r.ReadSingle(),
            M34 = r.ReadSingle(),
            M41 = r.ReadSingle() * VERTEX_SCALE,
            M42 = r.ReadSingle() * VERTEX_SCALE,
            M43 = r.ReadSingle() * VERTEX_SCALE,
            M44 = r.ReadSingle(),
        };
        // original transform matrix is 3x4 stored as 4x4.
        transform.M14 = transform.M24 = transform.M34 = 0f;
        transform.M44 = 1f;
        Transform = transform;

        Pos = r.ReadVector3() * VERTEX_SCALE;
        Rot = r.ReadQuaternion();
        Scale = r.ReadVector3();

        // read the controller pos/rot/scale
        PosCtrlID = r.ReadInt32();
        RotCtrlID = r.ReadInt32();
        SclCtrlID = r.ReadInt32();

        Properties = r.ReadL32UString();
    }
}

#endregion

#region ChunkNode_824

public class ChunkNode_824 : ChunkNode {
    public override void Read(BinaryReader r) {
        base.Read(r);

        Name = r.ReadFUString(64);
        if (string.IsNullOrEmpty(Name)) Name = "unknown";
        ObjectNodeID = r.ReadInt32(); // Object reference ID
        ParentNodeID = r.ReadInt32();
        __NumChildren = r.ReadInt32();
        MatID = r.ReadInt32(); // Material ID?
        SkipBytes(r, 4);

        // Read the 4x4 transform matrix.
        var transform = new Matrix4x4 {
            M11 = r.ReadSingle(),
            M12 = r.ReadSingle(),
            M13 = r.ReadSingle(),
            M14 = r.ReadSingle(),
            M21 = r.ReadSingle(),
            M22 = r.ReadSingle(),
            M23 = r.ReadSingle(),
            M24 = r.ReadSingle(),
            M31 = r.ReadSingle(),
            M32 = r.ReadSingle(),
            M33 = r.ReadSingle(),
            M34 = r.ReadSingle(),
            M41 = r.ReadSingle() * VERTEX_SCALE,
            M42 = r.ReadSingle() * VERTEX_SCALE,
            M43 = r.ReadSingle() * VERTEX_SCALE,
            M44 = r.ReadSingle(),
        };
        // original transform matrix is 3x4 stored as 4x4.
        transform.M14 = transform.M24 = transform.M34 = 0f;
        transform.M44 = 1f;
        Transform = transform;

        Pos = r.ReadVector3() * VERTEX_SCALE;
        Rot = r.ReadQuaternion();
        Scale = r.ReadVector3();

        // read the controller pos/rot/scale
        PosCtrlID = r.ReadInt32();
        RotCtrlID = r.ReadInt32();
        SclCtrlID = r.ReadInt32();

        Properties = r.ReadL32UString();
    }
}

#endregion

#region ChunkSceneProp

public abstract class ChunkSceneProp : Chunk     // cccc0008 
{
    // This chunk isn't really used, but contains some data probably necessary for the game.
    // Size for 0x744 type is always 0xBB4 (test this)
    public int NumProps; // number of elements in the props array  (31 for type 0x744)
    public string[] PropKey;
    public string[] PropValue;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** START SceneProp Chunk ***");
        Log($"    ChunkType:   {ChunkType}");
        Log($"    Version:     {Version:X}");
        Log($"    ID:          {ID:X}");
        for (var i = 0; i < NumProps; i++) Log($"{PropKey[i],30}{PropValue[i],20}");
        Log("*** END SceneProp Chunk ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkSceneProp_744

public class ChunkSceneProp_744 : ChunkSceneProp {
    public override void Read(BinaryReader r) {
        base.Read(r);

        NumProps = (int)r.ReadUInt32(); // Should be 31 for 0x744
        PropKey = new string[NumProps];
        PropValue = new string[NumProps];
        // Read the array of scene props and their associated values
        for (var i = 0; i < NumProps; i++) { PropKey[i] = r.ReadFUString(32); PropValue[i] = r.ReadFUString(64); }
    }
}

#endregion

#region ChunkSourceInfo

public abstract class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
{
    public string SourceFile;
    public string Date;
    public string Author;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Sourcefile: {SourceFile}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** SOURCE INFO CHUNK ***");
        Log($"    ID: {ID:X}");
        Log($"    Sourcefile: {SourceFile}.");
        Log($"    Date:       {Date}.");
        Log($"    Author:     {Author}.");
        Log($"*** END SOURCE INFO CHUNK ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkSourceInfo_0

public class ChunkSourceInfo_0 : ChunkSourceInfo {
    public override void Read(BinaryReader r) {
        ChunkType = _header.ChunkType;
        Version = _header.Version;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;

        r.BaseStream.Seek(_header.Offset, 0);
        var peek = r.ReadUInt32();
        // Try and detect SourceInfo type - if it's there, we need to skip ahead a few bytes
        if ((peek == (uint)ChunkType.SourceInfo) || (peek + 0xCCCBF000 == (uint)ChunkType.SourceInfo)) SkipBytes(r, 12);
        else r.BaseStream.Seek(_header.Offset, 0);

        if (Offset != _header.Offset || Size != _header.Size) {
            Log($"Conflict in chunk definition:  SourceInfo chunk");
            Log($"{_header.Offset:X}+{_header.Size:X}");
            Log($"{Offset:X}+{Size:X}");
            LogChunk();
        }

        ChunkType = ChunkType.SourceInfo; // this chunk doesn't actually have the chunktype header.
        SourceFile = r.ReadVUString();
        Date = r.ReadVUString().TrimEnd(); // Strip off last 2 Characters, because it contains a return
        // It is possible that Date has a newline in it instead of a null.  If so, split it based on newline.  Otherwise read Author.
        if (Date.Contains('\n')) { Author = Date.Split('\n')[1]; Date = Date.Split('\n')[0]; }
        else Author = r.ReadVUString();
    }
}

#endregion

#region ChunkTimingFormat

public abstract class ChunkTimingFormat : Chunk  // cccc000e:  Timing format chunk
{
    // This chunk doesn't have an ID, although one may be assigned in the chunk table.
    public float SecsPerTick;
    public int TicksPerFrame;
    public RangeEntity GlobalRange;
    public int NumSubRanges;

    public override string ToString()
        => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Ticks per Frame: {TicksPerFrame}, Seconds per Tick: {SecsPerTick}";

    #region Log
#if LOG
    public override void LogChunk() {
        Log($"*** TIMING CHUNK ***");
        Log($"    ID: {ID:X}");
        Log($"    Version: {Version:X}");
        Log($"    Secs Per Tick: {SecsPerTick}");
        Log($"    Ticks Per Frame: {TicksPerFrame}");
        Log($"    Global Range:  Name: {GlobalRange.Name}");
        Log($"    Global Range:  Start: {GlobalRange.Start}");
        Log($"    Global Range:  End:  {GlobalRange.End}");
        Log($"*** END TIMING CHUNK ***");
    }
#endif
    #endregion
}

#endregion

#region ChunkTimingFormat_918

public class ChunkTimingFormat_918 : ChunkTimingFormat {
    public override void Read(BinaryReader r) {
        base.Read(r);

        SecsPerTick = r.ReadSingle();
        TicksPerFrame = r.ReadInt32();
        GlobalRange.Name = r.ReadFUString(32); // Name is technically a String32, but F those structs
        GlobalRange.Start = r.ReadInt32();
        GlobalRange.End = r.ReadInt32();
    }
}

#endregion

#region ChunkTimingFormat_919

public class ChunkTimingFormat_919 : ChunkTimingFormat {
    public override void Read(BinaryReader r) {
        base.Read(r);

        // TODO:  This is copied from 918 but may not be entirely accurate.  Not tested.
        SecsPerTick = r.ReadSingle();
        TicksPerFrame = r.ReadInt32();
        GlobalRange.Name = r.ReadFUString(32); // Name is technically a String32, but F those structs
        GlobalRange.Start = r.ReadInt32();
        GlobalRange.End = r.ReadInt32();
    }
}

#endregion

#region ChunkUnknown

public class ChunkUnknown : Chunk { }

#endregion

#region GeometryInfo

/// <summary>
/// Geometry info contains all the vertex, color, normal, UV, tangent, index, etc.  Basically if you have a Node chunk with a Mesh and Submesh, this will contain the summary of all
/// the datastream chunks that contain geometry info.
/// </summary>
public class GeometryInfo {
    /// <summary>
    /// The MeshSubset chunk that contains this geometry.
    /// </summary>
    public ChunkMeshSubsets GeometrySubset { get; set; }
    public Vector3[] Vertices { get; set; }     // For dataStreamType of 0, length is NumElements. 
    public Vector3[] Normals { get; set; }      // For dataStreamType of 1, length is NumElements.
    public Vector2[] UVs { get; set; }               // for datastreamType of 2, length is NumElements.
    public IRGBA[] Colors { get; set; }     // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
    public uint[] Indices { get; set; }         // for dataStreamType of 5, length is NumElements.
    // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
    public Tangent[,] Tangents { get; set; }    // for dataStreamType of 6, length is NumElements, 2.  
    public byte[,] ShCoeffs { get; set; }       // for dataStreamType of 7, length is NumElement,BytesPerElements.
    public byte[,] ShapeDeformation { get; set; } // for dataStreamType of 8, length is NumElements,BytesPerElement.
    public byte[,] BoneMap { get; set; }        // for dataStreamType of 9, length is NumElements,BytesPerElement, 2.
    public byte[,] FaceMap { get; set; }        // for dataStreamType of 10, length is NumElements,BytesPerElement.
    public byte[,] VertMats { get; set; }       // for dataStreamType of 11, length is NumElements,BytesPerElement.
}

#endregion