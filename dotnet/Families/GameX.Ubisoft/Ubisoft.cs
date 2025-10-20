﻿using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Ubisoft.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Ubisoft;

#region UbisoftPakFile

/// <summary>
/// UbisoftPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class UbisoftPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="UbisoftPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public UbisoftPakFile(PakState state) : base(state, GetPakBinary(state.Game, state.Path)) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string filePath)
        => filePath == null || Path.GetExtension(filePath).ToLowerInvariant() != ".zip"
            ? Binary_Ubi.Current
            : Binary_Zip.GetPakBinary(game);

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
       => Path.GetExtension(source.Path).ToLowerInvariant() switch {
           ".dds" => (0, Binary_Dds.Factory),
           _ => (0, null),
       };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
