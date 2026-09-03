using OpenStack;
using Platform = OpenStack.Platform;

namespace GameX.App.Explorer.Views;

public partial class FileContent : ContentView {
    public static FileContent Current;

    public FileContent() {
        InitializeComponent();
        Current = this;
        BindingContext = this;
    }

    public void SetPlatform(Platform platform) {
        var res = ((Grid)Content).Resources;
        var plat = platform?.Id ?? "UK";
        res["TViewGfx"] = res[$"TViewGfx:{plat}"] ?? res["THex"];
    }

    void ContentTab_Changed(object sender, CheckedChangedEventArgs e) => ContentTabContent.BindingContext = ((RadioButton)sender).BindingContext;

    ISource _source;
    public ISource Source {
        get => _source;
        set { _source = value; OnPropertyChanged(); }
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

    public void OnInfo(MetaItem item, ISource source, List<MetaInfo> infos) {
        if (ContentTabs != null) foreach (var dispose in ContentTabs.Where(s => s.Dispose != null).Select(s => s.Dispose)) dispose.Dispose();
        Source = source;
        Path = item.Path;
        ContentTabs = infos?.Select(s => s.Tag as MetaContent).Where(s => s != null).ToList();
        //ContentTab.CurrentItem = ContentTabs != null ? ContentTabs.FirstOrDefault() : null;
    }
}
