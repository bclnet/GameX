namespace GameX.Bethesda;

public class Database(Archive source) {
    public readonly BinaryArchive Source = source as BinaryArchive;

    public override string ToString() => Source.Name;

    //public ConcurrentDictionary<uint, FileType> FileCache { get; } = new ConcurrentDictionary<uint, FileType>();
}
