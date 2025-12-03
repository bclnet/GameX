using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Base;

internal class SomePlatform {
    public static bool Startup() => true;
}

internal static class Some {
    public const string FamilyJson =
@"{
    'id': 'Some',
    'name': 'Some Family',
    'games': {
        '*': {
            'pakFileType': 'GameX.Some+SomePakFile, GameX.BaseTests'
        },
        'Found': {
            'name': 'Found',
            'pak': 'game:/path#Found'
        },
        'Missing': {
            'name': 'Missing',
            'pak': 'game:/path#Missing'
        }
    },
    'fileManager': {
    }
}";
    public static readonly Family Family = FamilyManager.CreateFamily(FamilyJson.Replace("'", "\""));

    public class SomePakFile : Archive {
        public SomePakFile(ArchiveState state) : base(state) { Name = "Some Name"; }
        public override int Count => 0;
        public override void Closing() { }
        public override void Opening() { }
        public override bool Contains(object path) => false;
        public override (Archive, FileSource) GetSource(object path, bool throwOnError = true) => throw new NotImplementedException();
        public override Task<Stream> GetData(object path, object option = default, bool throwOnError = true) => throw new NotImplementedException();
        public override Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true) => throw new NotImplementedException();
    }

    public const string FileManagerJson =
@"{
}";
}