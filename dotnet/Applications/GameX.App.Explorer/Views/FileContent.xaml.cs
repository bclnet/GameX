using OpenStack;
using OpenStack.Gfx;
using OpenStack.Sfx;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

// https://stackoverflow.com/questions/2783378/wpf-byte-array-to-hex-view-similar-to-notepad-hex-editor-plugin
namespace GameX.App.Explorer.Views {
    /// <summary>
    /// Interaction logic for FileContent.xaml
    /// </summary>
    public partial class FileContent : UserControl, INotifyPropertyChanged {
        public static FileContent Current;

        public FileContent() {
            InitializeComponent();
            Current = this;
            DataContext = this;
        }

        public void SetPlatform(Platform platform) {
            var res = ((Grid)Content).Resources;
            var plat = platform?.Id ?? "UK";
            res["TViewGfx"] = res[$"TViewGfx:{plat}"] ?? res[true ? "TText" : "THex"];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        IList<IOpenGfx> _gfx;
        public IList<IOpenGfx> Gfx {
            get => _gfx;
            set { _gfx = value; OnPropertyChanged(); }
        }

        IList<IOpenSfx> _sfx;
        public IList<IOpenSfx> Sfx {
            get => _sfx;
            set { _sfx = value; OnPropertyChanged(); }
        }

        object _path;
        public object Path {
            get => _path;
            set { _path = value; OnPropertyChanged(); }
        }

        IList<MetaContent> _contentTabs;
        public IList<MetaContent> ContentTabs {
            get => _contentTabs;
            set { _contentTabs = value; OnPropertyChanged(); }
        }

        public void OnInfo(MetaItem item, PakFile pakFile, List<MetaInfo> infos) {
            if (ContentTabs != null) foreach (var dispose in ContentTabs.Where(x => x.Dispose != null).Select(x => x.Dispose)) dispose.Dispose();
            Gfx = pakFile.Gfx;
            Sfx = pakFile.Sfx;
            Path = "PATH";
            ContentTabs = infos?.Select(x => x.Tag as MetaContent).Where(x => x != null).ToList();
            ContentTab.SelectedIndex = ContentTabs != null ? 0 : -1;
        }
    }
}
