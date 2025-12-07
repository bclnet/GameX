using GameX.Formats;
using GameX.Unknown;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Sfx;
using OpenStack.Vfx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GameX.MetaItem;

namespace GameX;

#region FileOption

[Flags]
public enum FileOption {
    Default = 0x0,
    Raw = 0x1,
    Marker = 0x2,
    Object = 0x4,
    BinaryObject = Object | 0x8,
    StreamObject = Object | 0x10,
    UnknownFileModel = 0x100,
    Hosting = Raw | Marker,
}

#endregion

#region ITransformAsset

/// <summary>
/// ITransformAsset
/// </summary>
public interface ITransformAsset<T> {
    /// <summary>
    /// Determines whether this instance [can transform file object] the specified transform to.
    /// </summary>
    /// <param name="transformTo">The transform to.</param>
    /// <param name="source">The source.</param>
    /// <returns>
    ///   <c>true</c> if this instance [can transform file object] the specified transform to; otherwise, <c>false</c>.
    /// </returns>
    bool CanTransformAsset(Archive transformTo, object source);
    /// <summary>
    /// Transforms the file object asynchronous.
    /// </summary>
    /// <param name="transformTo">The transform to.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    Task<T> TransformAsset(Archive transformTo, object source);
}

#endregion

#region ArchiveState

/// <summary>
/// ArchiveState
/// </summary>
/// <param name="vfx">The file system.</param>
/// <param name="game">The game.</param>
/// <param name="edition">The edition.</param>
/// <param name="path">The path.</param>
/// <param name="tag">The tag.</param>
public class ArchiveState(FileSystem vfx, FamilyGame game, FamilyGame.Edition edition = null, string path = null, object tag = null) {
    /// <summary>
    /// Gets the filesystem.
    /// </summary>
    public readonly FileSystem Vfx = vfx;

    /// <summary>
    /// Gets the arc family game.
    /// </summary>
    public readonly FamilyGame Game = game;

    /// <summary>
    /// Gets the filesystem.
    /// </summary>
    public readonly FamilyGame.Edition Edition = edition;

    /// <summary>
    /// Gets the path.
    /// </summary>
    public readonly string Path = path ?? string.Empty;

    /// <summary>
    /// Gets the tag.
    /// </summary>
    public object Tag = tag;
}

#endregion

#region Archive

/// <summary>
/// Asset
/// </summary>
/// <seealso cref="System.IDisposable" />
public abstract class Archive : ISource, IDisposable {
    public delegate (object option, Func<BinaryReader, FileSource, Archive, Task<object>> factory) FuncObjectFactory(FileSource source, FamilyGame game);

    /// <summary>
    /// An empty family.
    /// </summary>
    public static Archive Empty = new UnknownArchive(new ArchiveState(new DirectoryFileSystem("", null), FamilyGame.Empty)) { Name = "Empty" };

    public enum ArcStatus { Opening, Opened, Closing, Closed }

    /// <summary>
    /// The status
    /// </summary>
    public volatile ArcStatus Status;

    /// <summary>
    /// The filesystem.
    /// </summary>
    public readonly FileSystem Vfx;

    /// <summary>
    /// The arc family.
    /// </summary>
    public readonly Family Family;

    /// <summary>
    /// The arc family game.
    /// </summary>
    public readonly FamilyGame Game;

    /// <summary>
    /// The filesystem.
    /// </summary>
    public readonly FamilyGame.Edition Edition;

    /// <summary>
    /// The arc path.
    /// </summary>
    public string ArcPath;

    /// <summary>
    /// The arc name.
    /// </summary>
    public string Name;

    /// <summary>
    /// The tag.
    /// </summary>
    public object Tag;

    /// <summary>
    /// The arc path finders.
    /// </summary>
    public readonly Dictionary<Type, Func<object, object>> PathFinders = [];

    /// <summary>
    /// The arc path finders.
    /// </summary>
    public FuncObjectFactory AssetFactoryFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="Archive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="name">The name.</param>
    public Archive(ArchiveState state) {
        string z;
        Status = ArcStatus.Closed;
        Vfx = state.Vfx ?? throw new ArgumentNullException(nameof(state.Vfx));
        Family = state.Game.Family ?? throw new ArgumentNullException(nameof(state.Game.Family));
        Game = state.Game ?? throw new ArgumentNullException(nameof(state.Game));
        Edition = state.Edition;
        ArcPath = state.Path;
        Name = string.IsNullOrEmpty(state.Path) ? ""
            : !string.IsNullOrEmpty(z = Path.GetFileName(state.Path)) ? z : Path.GetFileName(Path.GetDirectoryName(state.Path));
        Tag = state.Tag;
        AssetFactoryFunc = null;
        Gfx = null;
        Sfx = null;
    }

    /// <summary>
    /// Determines whether this instance is valid.
    /// </summary>
    public virtual bool Valid => true;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
        Close();
        GC.SuppressFinalize(this);
    }
    ~Archive() => Close();

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public Archive Close() {
        Status = ArcStatus.Closing;
        Closing();
        if (Tag is IDisposable s) s.Dispose();
        Status = ArcStatus.Closed;
        return this;
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public abstract void Closing();

    /// <summary>
    /// Opens this instance.
    /// </summary>
    public virtual Archive Open(List<MetaItem> items = null, MetaManager manager = null) {
        if (Status != ArcStatus.Closed) return this;
        Status = ArcStatus.Opening;
        var watch = new Stopwatch();
        watch.Start();
        Opening();
        watch.Stop();
        Status = ArcStatus.Opened;
        items?.AddRange(GetMetaItems(manager));
        Log.Info($"Opened[{Game.Id}]: {Name} @ {watch.ElapsedMilliseconds}ms");
        return this;
    }

    /// <summary>
    /// Opens this instance.
    /// </summary>
    public abstract void Opening();

    /// <summary>
    /// Determines whether this instance contains the item.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>
    ///   <c>true</c> if [contains] [the specified file path]; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool Contains(object path);

    /// <summary>
    /// Gets the arc item count.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    public abstract int Count { get; }

    /// <summary>
    /// Finds the path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public object FindPath<T>(object path) {
        if (PathFinders.Count != 1) return PathFinders.TryGetValue(typeof(T), out var z) ? z(path) : path;
        var first = PathFinders.First();
        return first.Key == typeof(T) || first.Key == typeof(object) ? first.Value(path) : path;
    }

    /// <summary>
    /// Sets the platform.
    /// </summary>
    /// <param name="archive">The arc file.</param>
    /// <returns></returns>
    public Archive SetPlatform(Platform platform) {
        Gfx = platform?.GfxFactory?.Invoke(this);
        Sfx = platform?.SfxFactory?.Invoke(this);
        return this;
    }

    /// <summary>
    /// Gets the gfx.
    /// </summary>
    /// <value>
    /// The gfx.
    /// </value>
    public IOpenGfx[] Gfx { get; internal set; }

    /// <summary>
    /// Gets the gfx.
    /// </summary>
    /// <value>
    /// The sfx.
    /// </value>
    public IOpenSfx[] Sfx { get; internal set; }

    /// <summary>
    /// Gets the file source.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public abstract (Archive, FileSource) GetSource(object path, bool throwOnError = true);

    /// <summary>
    /// Loads the file data asynchronous.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public abstract Task<Stream> GetData(object path, object option = default, bool throwOnError = true);

    /// <summary>
    /// Loads the object asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The file path.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public abstract Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true);

    /// Opens the family arc file.
    /// </summary>
    /// <param name="res">The res.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public Archive GetArchive(object path, bool throwOnError = true)
        => path switch {
            string s => Game.GetArchive(Vfx, Edition, s, throwOnError)?.Open(),
            _ => throw new ArgumentOutOfRangeException(nameof(path)),
        };

    #region Transform

    /// <summary>
    /// Loads the object transformed asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The file path.</param>
    /// <param name="transformTo">The transformTo.</param>
    /// <returns></returns>
    public async Task<T> GetAsset<T>(object path, Archive transformTo) => await TransformAsset<T>(transformTo, await GetAsset<object>(path));

    /// <summary>
    /// Transforms the file object asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transformTo">The transformTo.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    Task<T> TransformAsset<T>(Archive transformTo, object source) {
        if (this is ITransformAsset<T> left && left.CanTransformAsset(transformTo, source)) return left.TransformAsset(transformTo, source);
        else if (transformTo is ITransformAsset<T> right && right.CanTransformAsset(transformTo, source)) return right.TransformAsset(transformTo, source);
        else throw new ArgumentOutOfRangeException(nameof(transformTo));
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Gets the metadata item filters.
    /// </summary>
    /// <param name="manager">The resource.</param>
    /// <returns></returns>
    public virtual List<Filter> GetMetaFilters(MetaManager manager) => Game.Filters?.Select(x => new Filter(x.Key, x.Value)).ToList();

    /// <summary>
    /// Gets the metadata infos.
    /// </summary>
    /// <param name="manager">The resource.</param>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    public virtual Task<List<MetaInfo>> GetMetaInfos(MetaManager manager, MetaItem item) => throw new NotImplementedException();

    /// <summary>
    /// Gets the metadata items.
    /// </summary>
    /// <param name="manager">The resource.</param>
    /// <returns></returns>
    public virtual List<MetaItem> GetMetaItems(MetaManager manager) => throw new NotImplementedException();

    #endregion
}

#endregion

#region BinaryAsset

/// <summary>
/// Initializes a new instance of the <see cref="BinaryAsset" /> class.
/// </summary>
/// <param name="state">The state.</param>
/// <param name="name">The name.</param>
/// <param name="arcBinary">The arc binary.</param>
[DebuggerDisplay("{Name}")]
public abstract class BinaryAsset(ArchiveState state, ArcBinary arcBinary) : Archive(state) {
    public readonly ArcBinary ArcBinary = arcBinary;
    // options
    public int RetainInPool = 10;
    public bool UseReader = true;
    public bool UseWriter = true;
    public bool UseFileId = false;
    // state
    public Func<string, string> FileMask;
    public readonly Dictionary<string, string> Params = [];
    public uint Magic;
    public uint Version;
    // metadata/factory
    protected Dictionary<string, Func<MetaManager, BinaryAsset, FileSource, Task<List<MetaInfo>>>> MetaInfos = [];

    // binary
    public IList<FileSource> Files;
    public HashSet<string> FilesRawSet;
    public ILookup<int, FileSource> FilesById { get; private set; }
    public ILookup<string, FileSource> FilesByPath { get; private set; }
    public int PathSkip;
    public bool AtEnd;

    /// <summary>
    /// Valid
    /// </summary>
    public override bool Valid => Files != null;

    #region Pool

    readonly ConcurrentDictionary<string, GenericPoolX<BinaryReader>> Readers = new();
    readonly ConcurrentDictionary<string, GenericPoolX<BinaryWriter>> Writers = new();

    /// <summary>
    /// Gets the binary reader.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public virtual IGenericPool<BinaryReader> GetReader(string path = default, bool pooled = true) => pooled
        ? Readers.GetOrAdd(path ?? ArcPath, path => Vfx.FileExists(path) ? new GenericPoolX<BinaryReader>(() => new(Vfx.Open(path, null)), r => r.Seek(0), RetainInPool) : null)
        : new SinglePool<BinaryReader>(Vfx.FileExists(path ??= ArcPath) ? new(Vfx.Open(path, null)) : null);

    /// <summary>
    /// Reader
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <param name="pooled">The pooled.</param>
    /// <returns></returns>
    public void Reader(Action<BinaryReader> func, string path = default, bool pooled = true) => GetReader(path, pooled).Action(func);

    /// <summary>
    /// Reader
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <param name="pooled">The pooled.</param>
    /// <returns></returns>
    public TResult ReaderT<TResult>(Func<BinaryReader, TResult> func, string path = default, bool pooled = true) => GetReader(path, pooled).Func(func);

    /// <summary>
    /// Reader
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <param name="pooled">The ppooledath.</param>
    /// <returns></returns>
    public Task ReaderAsync(Func<BinaryReader, Task> func, string path = default, bool pooled = true) => GetReader(path, pooled).ActionAsync(func);

    /// <summary>
    /// Reader
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <param name="pooled">The pooled.</param>
    /// <returns></returns>
    public Task<TResult> ReaderTAsync<TResult>(Func<BinaryReader, Task<TResult>> func, string path = default, bool pooled = true) => GetReader(path, pooled).FuncAsync(func);

    /// <summary>
    /// Gets the binary reader.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public GenericPoolX<BinaryWriter> GetWriter(string path = default)
        => Writers.GetOrAdd(path ?? ArcPath, path => Vfx.FileExists(path) ? new GenericPoolX<BinaryWriter>(() => new(Vfx.Open(path, "w")), r => r.Seek(0), RetainInPool) : null);

    /// <summary>
    /// Writer
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Task WriterActionAsync(Func<BinaryWriter, Task> func, string path = default) => GetWriter(path).ActionAsync(w => func(w));

    /// <summary>
    /// Writer
    /// </summary>
    /// <param name="func">The func.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Task<TResult> WriterFuncAsync<TResult>(Func<BinaryWriter, Task<TResult>> func, string path = default) => GetWriter(path).FuncAsync(w => func(w));

    #endregion

    /// <summary>
    /// Opens this instance.
    /// </summary>
    public async override void Opening() {
        await Read(Tag);
        await Process();
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public override void Closing() {
        Files = null;
        FilesRawSet = null;
        FilesById = null;
        FilesByPath = null;
        foreach (var r in Readers.Values) r.Dispose();
        Readers.Clear();
    }

    /// <summary>
    /// Determines whether the arc contains the specified file path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>
    ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
    /// </returns>
    public override bool Contains(object path) {
        switch (path) {
            case null: throw new ArgumentNullException(nameof(path));
            case string s: {
                    var (arc, s2) = FindPath(s);
                    return arc != null
                    ? arc.Contains(s2)
                    : FilesByPath != null && FilesByPath.Contains(s.Replace('\\', '/'));
                }
            case int i: return FilesById != null && FilesById.Contains(i);
            default: throw new ArgumentOutOfRangeException(nameof(path));
        }
    }

    /// <summary>Gets the count.</summary>
    /// <value>The count.</value>
    public override int Count => FilesByPath.Count;

    /// <summary>
    /// Finds the texture.
    /// </summary>
    /// <param name="path">The texture path.</param>
    /// <returns></returns>
    //public override string FindTexture(string path) => Contains(path) ? path : null;

    /// <summary>
    /// Gets the file source.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public override (Archive, FileSource) GetSource(object path, bool throwOnError = true) {
        switch (path) {
            case null: throw new ArgumentNullException(nameof(path));
            case FileSource f: return (this, f);
            case string s: {
                    var (arc, next) = FindPath(s);
                    if (arc != null) return next != null ? arc.GetSource(next) : (arc, null);
                    var files = FilesByPath[s.Replace('\\', '/')].ToArray();
                    if (files.Length == 1) return (this, files[0]);
                    Log.Info($"ERROR.GetData: {s} @ {files.Length}");
                    if (throwOnError) throw new FileNotFoundException(files.Length == 0 ? $"File not found: {s}" : $"More then one file found: {s}");
                    return (null, null);
                }
            case int i: {
                    var files = FilesById[i].ToArray();
                    if (files.Length == 1) return (this, files[0]);
                    Log.Info($"ERROR.GetData: {i} @ {files.Length}");
                    if (throwOnError) throw new FileNotFoundException(files.Length == 0 ? $"File not found: {i}" : $"More then one file found: {i}");
                    return (null, null);
                }
            default: throw new ArgumentOutOfRangeException(nameof(path));
        }
    }

    /// <summary>
    /// Loads the file data asynchronous.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public override Task<Stream> GetData(object path, object option = default, bool throwOnError = true) {
        if (path == null) return default;
        else if (path is not FileSource) {
            var (arc, next) = GetSource(path, throwOnError);
            return arc?.GetData(next, option, throwOnError);
        }
        var f = (FileSource)path;
        return ReadData(f.Fix(), option);
    }

    /// <summary>
    /// Loads the file object asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The file.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    public override async Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true) {
        if (path == null) return default;
        else if (path is not FileSource) {
            var (arc, next) = GetSource(path, throwOnError);
            return await arc.GetAsset<T>(next, option, throwOnError);
        }
        var f = (FileSource)path;
        if (Game.IsArcPath(f.Path)) return default;
        var type = typeof(T);
        var data = await GetData(f, option, throwOnError);
        if (data == null) return default;
        var objectFactory = EnsureCachedObjectFactory(f);
        if (objectFactory != FileSource.EmptyObjectFactory) {
            var r = new BinaryReader(data);
            object value = null;
            Task<object> task = null;
            try {
                task = objectFactory(r, f, this);
                if (task != null) return (value = await task) is T z ? z : value is Indirect<T> y ? y.Value : throw new InvalidCastException();
            }
            catch (Exception e) { Log.Error(e.Message); throw e; }
            finally {
                AtEnd = r.AtEnd();
                if (task != null && !(value != null && value is IDisposable)) r.Dispose();
            }
        }
        return type == typeof(Stream) || type == typeof(object)
            ? (T)(object)data
            : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {f.Path} with {type.Name}");
    }

    /// <summary>
    /// Ensures the file object factory.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns></returns>
    public Func<BinaryReader, FileSource, Archive, Task<object>> EnsureCachedObjectFactory(FileSource file) {
        if (AssetFactoryFunc == null) return FileSource.EmptyObjectFactory;
        if (file.CachedObjectFactory != null) return file.CachedObjectFactory;
        var factory = AssetFactoryFunc(file, Game);
        file.CachedObjectOption = factory.option;
        file.CachedObjectFactory = factory.factory ?? FileSource.EmptyObjectFactory;
        return file.CachedObjectFactory;
    }

    /// <summary>
    /// Processes this instance.
    /// </summary>
    public async virtual Task Process() {
        if (UseFileId) FilesById = Files.Where(x => x != null).ToLookup(x => x.Id);
        FilesByPath = Files.Where(x => x != null).ToLookup(x => x.Path, StringComparer.OrdinalIgnoreCase);
        if (ArcBinary != null) await ArcBinary.Process(this);
    }

    /// <summary>
    /// FindPath.
    /// </summary>
    /// <param name="path">The path.</param>
    (Archive arc, string next) FindPath(string path) {
        var paths = path.Split([':'], 2);
        var p = paths[0].Replace('\\', '/');
        var arc = FilesByPath[p]?.FirstOrDefault()?.Arc?.Open();
        return (arc, arc != null && paths.Length > 1 ? paths[1] : null);
    }

    #region ArcBinary

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="r">The r.</param>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    public virtual Task Read(object tag = default) => UseReader
        ? ReaderT(r => ArcBinary.Read(this, r, tag))
        : ArcBinary.Read(this, null, tag);

    /// <summary>
    /// Reads the file data asynchronous.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="option">The option.</param>
    /// <returns></returns>
    public virtual Task<Stream> ReadData(FileSource file, object option = default) => UseReader
        ? ReaderT(r => ArcBinary.ReadData(this, r, file, option))
        : ArcBinary.ReadData(this, null, file, option);

    /// <summary>
    /// Writes the asynchronous.
    /// </summary>
    /// <param name="w">The w.</param>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    public virtual Task Write(object tag = default) => UseWriter
        ? WriterActionAsync(w => ArcBinary.Write(this, w, tag))
        : ArcBinary.Write(this, null, tag);

    /// <summary>
    /// Writes the file data asynchronous.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="data">The data.</param>
    /// <param name="option">The option.</param>
    /// <returns></returns>
    public virtual Task WriteData(FileSource file, Stream data, object option = default) => UseWriter
        ? WriterActionAsync(w => ArcBinary.WriteData(this, w, file, data, option))
        : ArcBinary.WriteData(this, null, file, data, option);

    #endregion

    #region Metadata

    /// <summary>
    /// Gets the explorer information nodes.
    /// </summary>
    /// <param name="manager">The manager.</param>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override Task<List<MetaInfo>> GetMetaInfos(MetaManager manager, MetaItem item)
        => Valid ? MetaManager.GetMetaInfos(manager, this, item.Source as FileSource) : Task.FromResult(new List<MetaInfo>());

    /// <summary>
    /// Gets the explorer item nodes.
    /// </summary>
    /// <param name="manager">The resource.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override List<MetaItem> GetMetaItems(MetaManager manager)
        => Valid ? MetaManager.GetMetaItems(manager, this) : new List<MetaItem>();

    #endregion
}

#endregion

#region ManyArchive

public class ManyArchive : BinaryAsset {
    /// <summary>
    /// The paths
    /// </summary>
    public readonly string[] Paths;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiArchive" /> class.
    /// </summary>
    /// <param name="basis">The basis.</param>
    /// <param name="state">The state.</param>
    /// <param name="name">The name.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="pathSkip">The pathSkip.</param>
    public ManyArchive(Archive basis, ArchiveState state, string name, string[] paths, int pathSkip = 0) : base(state, null) {
        AssetFactoryFunc = basis.AssetFactoryFunc;
        Name = name;
        Paths = paths;
        PathSkip = pathSkip;
        UseReader = false;
    }

    #region ArcBinary

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    public override Task Read(object tag = default) {
        Files = [.. Paths.Select(s => new FileSource {
            Path = s.Replace('\\', '/'),
            Arc = Game.IsArcPath(s) ? (BinaryAsset)Game.CreateArchive(new ArchiveState(Vfx, Game, Edition, s)) : default,
            Lazy = x => { x.FileSize = Vfx.FileInfo(s).length; x.Lazy = null; }
        })];
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(FileSource file, object option = default)
        => file.Arc != null
            ? file.Arc.ReadData(file, option)
            : Task.FromResult<Stream>(new MemoryStream(new BinaryReader(Vfx.Open(file.Path, null)).ReadBytes((int)file.FileSize)));

    #endregion
}

#endregion

#region MultiArchive

[DebuggerDisplay("Arcs: {Arcs.Count}")]
public class MultiArchive : Archive {
    /// <summary>
    /// The paks
    /// </summary>
    public readonly IList<Archive> Archives;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="name">The name.</param>
    /// <param name="archives">The archives.</param>
    /// <param name="tag">The tag.</param>
    public MultiArchive(ArchiveState state, string name, IList<Archive> archives) : base(state) {
        Name = name;
        Archives = archives ?? throw new ArgumentNullException(nameof(archives));
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public override void Closing() {
        foreach (var s in Archives) s.Close();
    }

    /// <summary>
    /// Opens this instance.
    /// </summary>
    public override void Opening() {
        foreach (var s in Archives) s.Open();
    }

    /// <summary>
    /// Determines whether the specified file path contains file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
    /// </returns>
    public override bool Contains(object path)
        => path switch {
            null => throw new ArgumentNullException(nameof(path)),
            string s => FindArchives(s, out var next).Any(s => s.Valid && s.Contains(next)),
            int i => Archives.Any(s => s.Valid && s.Contains(i)),
            _ => throw new ArgumentOutOfRangeException(nameof(path)),
        };

    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    public override int Count {
        get { var count = 0; foreach (var s in Archives) count += s.Count; return count; }
    }

    IList<Archive> FindArchives(string path, out string next) {
        var paths = path.Split(['\\', '/', ':'], 2);
        if (paths.Length == 1) { next = path; return Archives; }
        path = paths[0]; next = paths[1];
        var archives = Archives.Where(s => s.Name.StartsWith(path)).ToList();
        foreach (var s in archives) s.Open();
        return archives;
    }

    /// <summary>
    /// Gets the file source.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
    public override (Archive, FileSource) GetSource(object path, bool throwOnError = true)
        => path switch {
            null => throw new ArgumentNullException(nameof(path)),
            string s => (FindArchives(s, out var s2).FirstOrDefault(s => s.Valid && s.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                .GetSource(s2, throwOnError),
            int i => (Archives.FirstOrDefault(s => s.Valid && s.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                .GetSource(i, throwOnError),
            _ => throw new ArgumentOutOfRangeException(nameof(path)),
        };

    /// <summary>
    /// Loads the file data asynchronous.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
    public override Task<Stream> GetData(object path, object option = default, bool throwOnError = true)
        => path switch {
            null => throw new ArgumentNullException(nameof(path)),
            string s => (FindArchives(s, out var s2).FirstOrDefault(s => s.Valid && s.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                .GetData(s2, option, throwOnError),
            int i => (Archives.FirstOrDefault(s => s.Valid && s.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                .GetData(i, option, throwOnError),
            _ => throw new ArgumentOutOfRangeException(nameof(path)),
        };

    /// <summary>
    /// Loads the object asynchronous.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="option">The option.</param>
    /// <param name="throwOnError">Throws on error.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
    public override Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true)
        => path switch {
            null => throw new ArgumentNullException(nameof(path)),
            string s => (FindArchives(s, out var s2).FirstOrDefault(s => s.Valid && s.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                .GetAsset<T>(s2, option, throwOnError),
            int i => (Archives.FirstOrDefault(s => s.Valid && s.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                .GetAsset<T>(i, option, throwOnError),
            _ => throw new ArgumentOutOfRangeException(nameof(path)),
        };

    #region Metadata

    /// <summary>
    /// Gets the metadata items.
    /// </summary>
    /// <param name="manager">The resource.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override List<MetaItem> GetMetaItems(MetaManager manager) {
        var r = new List<MetaItem>();
        foreach (var s in Archives.Where(t => t.Valid)) r.Add(new MetaItem(s, s.Name, manager.PackageIcon, archive: s, items: s.GetMetaItems(manager)));
        return r;
    }

    #endregion
}

#endregion

#region ArcBinary

public class ArcBinary {
    /// <summary>
    /// The file
    /// </summary>
    public readonly static ArcBinary Stream = new PakBinaryCanStream();

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="r">The r.</param>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Task Read(BinaryAsset source, BinaryReader r, object tag = default) => throw new NotSupportedException();

    /// <summary>
    /// Reads the asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="r">The r.</param>
    /// <param name="file">The file.</param>
    /// <param name="option">The option.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Task<Stream> ReadData(BinaryAsset source, BinaryReader r, FileSource file, object option = default) => throw new NotSupportedException();

    /// <summary>
    /// Writes the asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="w">The w.</param>
    /// <param name="tag">The tag.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Task Write(BinaryAsset source, BinaryWriter w, object tag = default) => throw new NotSupportedException();

    /// <summary>
    /// Writes the asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="w">The w.</param>
    /// <param name="file">The file.</param>
    /// <param name="option">The option.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Task WriteData(BinaryAsset source, BinaryWriter w, FileSource file, Stream data, object option = default) => throw new NotSupportedException();

    /// <summary>
    /// Processes this instance.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Task Process(BinaryAsset source) => Task.CompletedTask;

    /// <summary>
    /// handles an exception.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="option">The option.</param>
    /// <param name="message">The message.</param>
    /// <exception cref="NotSupportedException"></exception>
    public static void HandleException(object source, object option, string message) {
        Log.Info(message);
        // if ((option & FileOption.Supress) != 0) throw new Exception(message);
    }
}

#endregion

#region ArcBinary<T>

public class ArcBinary<Self> : ArcBinary where Self : ArcBinary, new() {
    public static readonly ArcBinary Current = new Self();

    protected class SubArchive : BinaryAsset {
        readonly FileSource File;
        readonly BinaryAsset Source;
        StaticPool<BinaryReader> Pool;
        BinaryReader R;

        public SubArchive(BinaryAsset source, FileSource file, string path, object tag = null, ArcBinary instance = null) : base(new ArchiveState(source.Vfx, source.Game, source.Edition, path, tag), instance ?? Current) {
            File = file;
            Source = source;
            AssetFactoryFunc = source.AssetFactoryFunc;
            Open();
        }

        public async override void Opening() { R = new BinaryReader(await Source.ReadData(File)); Pool = new StaticPool<BinaryReader>(R); base.Opening(); }
        public override void Closing() { R?.Dispose(); base.Closing(); }
        public override IGenericPool<BinaryReader> GetReader(string path = null, bool pooled = true) => Pool;
    }
}

#endregion
