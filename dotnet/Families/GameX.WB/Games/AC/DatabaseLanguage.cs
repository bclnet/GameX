using GameX.WB.Formats.AC.FileTypes;

namespace GameX.WB;

public class DatabaseLanguage : Database {
    public DatabaseLanguage(Archive pakFile) : base(pakFile)
        => CharacterTitles = GetFile<StringTable>(StringTable.CharacterTitle_FileID);

    public StringTable CharacterTitles { get; }
}
