﻿using GameX.Crytek.Formats.Models;
using System.Collections.Generic;

namespace GameX.Crytek.Formats.Core;

public class SkinningInfo {
    /// <summary>
    /// If there is skinning info in the model, set to true.
    /// </summary>
    public bool HasSkinningInfo { get; set; }
    /// <summary>
    /// If there is a BoneMap datastream, set to true
    /// </summary>
    public bool HasBoneMapDatastream { get; internal set; }
    /// <summary>
    /// If there is an internal vertex to external vertex mapping, set to true.
    /// </summary>
    public bool HasIntToExtMapping { get; internal set; }
    /// <summary>
    /// BoneEntities are the list of the bones in the object. Contains the info to find each of the necessary skinning components.
    /// </summary>
    public List<BoneEntity> BoneEntities { get; set; }
    public List<CompiledBone> CompiledBones { get; set; }
    public List<DirectionalBlends> LookDirectionBlends { get; set; }
    public List<PhysicalProxy> PhysicalBoneMeshes { get; set; } // Collision proxies
    public List<MorphTargets> MorphTargets { get; set; }
    public List<TFace> IntFaces { get; set; }
    public List<IntSkinVertex> IntVertices { get; set; }
    public List<ushort> Ext2IntMap { get; set; }
    public List<MeshCollisionInfo> Collisions { get; set; }
    public List<MeshBoneMapping> BoneMapping { get; set; } // Bone Mappings are read from a Datastream chunk

    /// <summary>
    /// Given a bone name, get the Controller ID.
    /// </summary>
    /// <param name="jointName">Bone name</param>
    /// <returns>Controller ID of the bone.</returns>
    public int GetJointIDByName(string jointName) {
        for (var i = 0; i < CompiledBones.Count; i++)
            if (string.Equals(CompiledBones[i].boneName, jointName)) return i;
        return -1;
    }

    /// <summary>
    /// Given a joint ID, get the name of the bone.
    /// </summary>
    /// <param name="jointID">ID of the bone.</param>
    /// <returns>Name of the bone.</returns>
    public string GetJointNameByID(int jointID)
        => jointID >= 0 && jointID < CompiledBones.Count ? CompiledBones[jointID].boneName : string.Empty; // Invalid bone Id
}
