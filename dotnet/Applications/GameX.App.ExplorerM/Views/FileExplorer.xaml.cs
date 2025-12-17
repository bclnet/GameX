using static GameX.FamilyManager;

namespace GameX.App.Explorer.Views;

public partial class FileExplorer : ContentView {
    public readonly static MetaManager Resource = ResourceManager.Current;
    public static FileExplorer Current;

    public FileExplorer() {
        InitializeComponent();
        Current = this;
        BindingContext = this;
    }

    public static readonly BindableProperty ArchiveProperty = BindableProperty.Create(nameof(Archive), typeof(Archive), typeof(FileExplorer),
        propertyChanged: (d, e, n) => {
            if (d is not FileExplorer fileExplorer || n is not Archive archive) return;
            fileExplorer.Filters = archive.GetMetaFilters(Resource);
            fileExplorer.Nodes = fileExplorer.ArcNodes = archive.GetMetaItems(Resource);
            fileExplorer.Ready(archive);
        });
    //public Archive Archive
    //{
    //    get => (Archive)GetValue(ArchiveProperty);
    //    set => SetValue(ArchiveProperty, value);
    //}

    Archive _archive;
    public Archive Archive {
        get => _archive;
        set {
            _archive = value; OnPropertyChanged();
            Filters = _archive.GetMetaFilters(Resource);
            Nodes = ArcNodes = _archive.GetMetaItems(Resource);
            Ready(_archive);
        }
    }

    List<MetaItem.Filter> _filters;
    public List<MetaItem.Filter> Filters {
        get => _filters;
        set { _filters = value; OnPropertyChanged(); }
    }

    //void OnFilterKeyUp(object sender, EventArgs e)
    //{
    //    if (string.IsNullOrEmpty(Filter.SelectedItem as string)) Nodes = ArcNodes;
    //    else Nodes = ArcNodes.Select(x => x.Search(y => y.Name.Contains(Filter.SelectedItem as string))).Where(x => x != null).ToList();
    //    //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
    //    //view.Filter = o =>
    //    //{
    //    //    if (string.IsNullOrEmpty(Filter.Text)) return true;
    //    //    else return (o as MetaItem).Name.Contains(Filter.Text);
    //    //};
    //    //view.Refresh();
    //}

    void OnFilterSelected(object s, EventArgs e) {
        var filter = (MetaItem.Filter)Filter.SelectedItem;
        if (filter == null) Nodes = ArcNodes;
        else Nodes = ArcNodes.Select(x => x.Search(y => y.Name.Contains(filter.Description))).Where(x => x != null).ToList();
    }

    List<MetaItem> ArcNodes;

    List<MetaItem> _nodes;
    public List<MetaItem> Nodes {
        get => _nodes;
        set { _nodes = value; OnPropertyChanged(); }
    }

    List<MetaInfo> _infos;
    public List<MetaInfo> Infos {
        get => _infos;
        set { _infos = value; OnPropertyChanged(); }
    }

    MetaItem _selectedItem;
    public MetaItem SelectedItem {
        get => _selectedItem;
        set {
            if (_selectedItem == value) return;
            _selectedItem = value;
            if (value == null) { OnInfo(); return; }
            var src = (value.Source as FileSource)?.Fix();
            var arc = src?.Arc;
            try {
                if (arc != null) {
                    if (arc.Status == Archive.BlobStatus.Opened) return;
                    arc.Open(value.Items, Resource);
                    //OnFilterKeyUp(null, null); //value.Items.AddRange(arc.GetMetaItemsAsync(Resource).Result);
                }
                OnInfo(value.Archive?.GetMetaInfos(Resource, value).Result);
            }
            catch (Exception ex) {
                OnInfo([
                    new MetaInfo($"EXCEPTION: {ex.Message}"),
                        new MetaInfo(ex.StackTrace),
                    ]);
            }
        }
    }

    public void OnInfoUpdated() {
    }

    public void OnInfo(IEnumerable<MetaInfo> infos = null) {
        FileContent.Current.OnInfo(Archive, infos?.Where(x => x.Name == null).ToList());
        Infos = infos?.Where(x => x.Name != null).ToList();
    }

    void OnNodeSelected(object s, EventArgs args) {
        var parameter = ((TappedEventArgs)args).Parameter;
        if (parameter is MetaItem item && item.Archive != null) SelectedItem = item;
        //e.Handled = true;
    }

    void Ready(Archive archive) {
        if (string.IsNullOrEmpty(Option.ForcePath) || Option.ForcePath.StartsWith("app:")) return;
        var sample = Option.ForcePath.StartsWith("sample:") ? archive.Game.GetSample(Option.ForcePath[7..]) : null;
        var paths = sample != null ? sample.Paths : [Option.ForcePath];
        if (paths == null) return;
        foreach (var path in paths) SelectedItem = MetaItem.FindByPathForNodes(ArcNodes, path, Resource);
    }
}
