namespace GameX.Bethesda;

public class Database(Archive source) {
    public readonly BinaryAsset Source = source as BinaryAsset;

    public override string ToString() => Source.Name;

    //public ConcurrentDictionary<uint, FileType> FileCache { get; } = new ConcurrentDictionary<uint, FileType>();
}
