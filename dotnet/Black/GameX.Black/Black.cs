﻿using GameX.Black.Formats;
using GameX.Black.Transforms;
using GameX.Formats.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Black;

#region BlackPakFile

/// <summary>
/// BlackPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BlackPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlackPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BlackPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => Binary_Dat.Current;

    //string.IsNullOrEmpty(extension)
    //? PakBinary_Dat.Instance
    //: extension switch
    //{
    //    ".dat" => PakBinary_Dat.Instance,
    //    _ => throw new ArgumentOutOfRangeException(nameof(extension)),
    //};

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            var x when x.StartsWith(".fr") => (0, Binary_Frm.Factory),
            ".pal" => (0, Binary_Pal2.Factory),
            ".rix" => (0, Binary_Rix.Factory),
            _ => (0, null),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
