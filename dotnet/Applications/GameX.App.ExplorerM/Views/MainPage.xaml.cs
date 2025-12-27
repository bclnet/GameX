using OpenStack;
using static GameX.FamilyManager;
using Platform = OpenStack.Platform;

namespace GameX.App.Explorer.Views;

/// <summary>
/// MainPageTab
/// </summary>
public class MainPageTab {
    public string Name { get; set; }
    public Archive Archive { get; set; }
    public IList<FamilyApp> AppList { get; set; }
    public string Text { get; set; }
    public override string ToString() => Name;
}

public partial class MainPage : ContentPage {
    public readonly static MetaManager Manager = ResourceManager.Current;
    public static MainPage Current;

    public MainPage() {
        InitializeComponent();
        Current = this;
        BindingContext = this;
        Platforms = [.. PlatformX.Platforms.Where(s => s != null && s.Enabled)];
        Platform.SelectedIndex = ((List<Platform>)Platforms)?.FindIndex(s => s.Id == Option.Platform) ?? -1;
    }

    // https://dev.to/davidortinau/making-a-tabbar-or-segmentedcontrol-in-net-maui-54ha
    void MainTab_Changed(object sender, CheckedChangedEventArgs e) => MainTabContent.BindingContext = ((RadioButton)sender).BindingContext;

    public MainPage Open(Family family, IEnumerable<Uri> uris, string path = null) {
        foreach (var archive in Archives) archive?.Dispose();
        Archives.Clear();
        if (family == null) return this;
        FamilyApps = family.Apps;
        foreach (var s in uris) {
            Log.WriteLine($"Opening {s}");
            var arc = family.GetArchive(s);
            if (arc != null) Archives.Add(arc);
        }
        Log.WriteLine("Done");
        OnOpenedAsync(family, path).Wait();
        return this;
    }

    public void SetPlatform(Platform platform) {
        PlatformX.Activate(platform);
        foreach (var s in Archives) s.SetPlatform(platform);
        FileContent.SetPlatform(platform);
    }

    //public static readonly BindableProperty PlatformProperty = BindableProperty.Create(nameof(MainTabs), typeof(IList<Platform>), typeof(MainPage),
    //    propertyChanged: (d, e, n) =>
    //    {
    //        //var selected = (Platform)Platform.SelectedItem;
    //        //SetPlatform(selected);
    //    });
    //public IList<Platform> Platforms
    //{
    //    get => (IList<Platform>)GetValue(PlatformProperty);
    //    set => SetValue(PlatformProperty, value);
    //}

    IList<Platform> _platforms;
    public IList<Platform> Platforms {
        get => _platforms;
        set { _platforms = value; OnPropertyChanged(); }
    }

    public static readonly BindableProperty MainTabsProperty = BindableProperty.Create(nameof(MainTabs), typeof(IList<MainPageTab>), typeof(MainPage),
        propertyChanged: (d, e, n) => {
            var mainTab = ((MainPage)d).MainTab;
            if (mainTab.Children.FirstOrDefault() is RadioButton firstTab) firstTab.IsChecked = true;
        });
    public IList<MainPageTab> MainTabs {
        get => (IList<MainPageTab>)GetValue(MainTabsProperty);
        set => SetValue(MainTabsProperty, value);
    }

    public readonly IList<Archive> Archives = [];
    public Dictionary<string, FamilyApp> FamilyApps;

    public Task OnOpenedAsync(Family family, string path = null) {
        var tabs = Archives.Select(archive => new MainPageTab {
            Name = archive.Name,
            Archive = archive,
        }).ToList();
        var firstArchive = tabs.FirstOrDefault()?.Archive ?? Archive.Empty;
        if (FamilyApps.Count > 0)
            tabs.Add(new MainPageTab {
                Name = "Apps",
                Archive = firstArchive,
                AppList = [.. FamilyApps.Values],
                Text = "Choose an application.",
            });
        if (!string.IsNullOrEmpty(family.Description))
            tabs.Add(new MainPageTab {
                Name = "Information",
                Text = family.Description,
            });
        MainTabs = tabs;

        // default main tab to first / second
        //MainTabContent.BindingContext = Tabs ...Children.FirstOrDefault(). s.LastOrDefault(); //.SelectedIndex = 0; // family.Apps != null ? 1 : 0
        return Task.CompletedTask;
    }

    void Platform_SelectionChanged(object sender, EventArgs e) {
        var selected = (Platform)Platform.SelectedItem;
        SetPlatform(selected);
    }

    internal void App_Click(object sender, EventArgs e) {
        var button = (Button)sender;
        var app = (FamilyApp)button.BindingContext;
        app.OpenAsync(app.ExplorerType, Manager).Wait();
    }
}