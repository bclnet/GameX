﻿using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Valve.Formats;
using GameX.Valve.Formats.Vpk;
using GameX.Valve.Transforms;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Valve
{
    #region ValvePakFile

    /// <summary>
    /// ValvePakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class ValvePakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValvePakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public ValvePakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFunc = ObjectFactoryFactory;
            PathFinders.Add(typeof(object), FindBinary);
        }

        #region Factories

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new();

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => PakBinarys.GetOrAdd(game.Id, _ => game.Engine switch
            {
                "Unity" => Unity.Formats.PakBinary_Unity.Current,
                "Source" => PakBinary_Vpk.Current,
                "HL" => PakBinary_Wad.Current,
                _ => throw new ArgumentOutOfRangeException(nameof(game.Engine), game.Engine),
                //_ => extension switch
                //{
                //    ".wad" => PakBinaryWad.Instance,
                //    _ => null,
                //}
            });

        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
        {
            Task<object> BinaryPakFactory(BinaryReader r, FileSource f, PakFile s)
            {
                if (r.BaseStream.Length < 6) return null;
                var input = r.Peek(z => z.ReadBytes(6));
                var magic = BitConverter.ToUInt32(input, 0);
                var magicResourceVersion = BitConverter.ToUInt16(input, 4);
                if (magic == PakBinary_Vpk.MAGIC) throw new InvalidOperationException("Pak File");
                else if (magic == CompiledShader.MAGIC) return Task.FromResult((object)new CompiledShader(r, f.Path));
                else if (magic == ClosedCaptions.MAGIC) return Task.FromResult((object)new ClosedCaptions(r));
                else if (magic == ToolsAssetInfo.MAGIC) return Task.FromResult((object)new ToolsAssetInfo(r));
                else if (magic == XKV3.MAGIC || magic == XKV3.MAGIC2) { var kv3 = new XKV3 { Size = (uint)r.BaseStream.Length }; kv3.Read(null, r); return Task.FromResult((object)kv3); }
                else if (magicResourceVersion == Binary_Pak.KnownHeaderVersion) return Task.FromResult((object)new Binary_Pak(r));
                //else if (magicResourceVersion == BinaryPak.KnownHeaderVersion)
                //{
                //    var pak = new BinaryPak(r);
                //    switch (pak.DataType)
                //    {
                //        //case DATA.DataType.Mesh: return Task.FromResult((object)new DATAMesh(pak));
                //        default: return Task.FromResult((object)pak);
                //    }
                //}
                else return null;
            }
            return game.Engine switch
            {
                "HL" => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    var x when x == ".txt" || x == ".ini" || x == ".asl" => (0, Binary_Txt.Factory),
                    ".wav" => (0, Binary_Snd.Factory),
                    var x when x == ".bmp" || x == ".jpg" => (0, Binary_Img.Factory),
                    var x when x == ".pic" || x == ".tex" || x == ".tex2" || x == ".fnt" => (0, Binary_Wad3.Factory),
                    ".bsp" => (0, Binary_Bsp.Factory),
                    ".spr" => (0, Binary_Spr.Factory),
                    _ => default,
                },
                _ => (0, BinaryPakFactory)
            };
        }

        #endregion

        #region PathFinders

        /// <summary>
        /// Finds the actual path of a texture.
        /// </summary>
        public string FindBinary(string path)
        {
            if (Contains(path)) return path;
            if (!path.EndsWith("_c", StringComparison.Ordinal)) path = $"{path}_c";
            if (Contains(path)) return path;
            Log($"Could not find file '{path}' in a PAK file.");
            return null;
        }

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }

    #endregion
}