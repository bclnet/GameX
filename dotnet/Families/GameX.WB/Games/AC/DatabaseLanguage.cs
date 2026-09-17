using GameX.WB.Formats.AC.FileTypes;

namespace GameX.WB;

public class DatabaseLanguage : Database {
    public DatabaseLanguage(Archive archive) : base(archive)
        => CharacterTitles = GetFile<StringTable>(StringTable.CharacterTitle_FileID);

    public StringTable CharacterTitles { get; }
}
