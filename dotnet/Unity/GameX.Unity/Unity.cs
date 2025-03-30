﻿using GameX.Formats.Unknown;
using GameX.Unity.Formats;
using GameX.Unity.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Unity;

#region UnityPakFile

/// <summary>
/// UnityPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class UnityPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnityPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public UnityPakFile(PakState state) : base(state, Binary_Unity.Current)
    {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    public static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
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
