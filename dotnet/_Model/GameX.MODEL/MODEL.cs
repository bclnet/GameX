﻿using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.MODEL.Formats;
using GameX.MODEL.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;
using GameX.Unknown;

namespace GameX.MODEL
{
    #region MODELPakFile

    /// <summary>
    /// MODELPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class MODELPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MODELPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public MODELPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFunc = ObjectFactory;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => extension switch
            {
                ".xxx" => PakBinary_XXX.Current,
                _ => throw new ArgumentOutOfRangeException(nameof(extension)),
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
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
}