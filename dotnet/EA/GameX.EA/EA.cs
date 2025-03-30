﻿using GameX.EA.Formats;
using GameX.EA.Transforms;
using GameX.Formats.Unknown;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.EA;

#region EAPakFile

/// <summary>
/// EAPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class EAPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EAPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public EAPakFile(PakState state) : base(state, Binary_Hpl.Current)
    {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch
        {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
