﻿using GameX.Cyanide.Formats;
using GameX.Cyanide.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cyanide;

#region CyanidePakFile

/// <summary>
/// CyanidePakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class CyanidePakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CyanidePakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CyanidePakFile(PakState state) : base(state, Binary_Cpk.Current)
    {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch
        {
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
