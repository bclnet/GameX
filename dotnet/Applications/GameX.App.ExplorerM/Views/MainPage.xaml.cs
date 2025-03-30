using OpenStack;
using static GameX.FamilyManager;
using Platform = OpenStack.Platform;

namespace GameX.App.Explorer.Views
{
    /// <summary>
    /// MainPageTab
    /// </summary>
    public class MainPageTab
    {
        public string Name { get; set; }
        public PakFile PakFile { get; set; }
        public IList<FamilyApp> AppList { get; set; }
        public string Text { get; set; }
        public override string ToString() => Name;
    }

    public partial class MainPage : ContentPage
    {
        public readonly static MetaManager Manager = ResourceManager.Current;
        public static MainPage Current;

        public MainPage()
        {
            InitializeComponent();
            Current = this;
            BindingContext = this;
            Platforms = [.. PlatformX.Platforms.Where(x => x != null && x.Enabled)];
            Platform.SelectedIndex = ((List<Platform>)Platforms)?.FindIndex(x => x.Id == Option.Platform) ?? -1;
        }

        // https://dev.to/davidortinau/making-a-tabbar-or-segmentedcontrol-in-net-maui-54ha
        void MainTab_Changed(object sender, CheckedChangedEventArgs e) => MainTabContent.BindingContext = ((RadioButton)sender).BindingContext;

        public MainPage Open(Family family, IEnumerable<Uri> pakUris, string path = null)
        {
            foreach (var pakFile in PakFiles) pakFile?.Dispose();
            PakFiles.Clear();
            if (family == null) return this;
            FamilyApps = family.Apps;
            foreach (var pakUri in pakUris)
            {
                Log.WriteLine($"Opening {pakUri}");
                var pak = family.OpenPakFile(pakUri);
                if (pak != null) PakFiles.Add(pak);
            }
            Log.WriteLine("Done");
            OnOpenedAsync(family, path).Wait();
            return this;
        }

        public void SetPlatform(Platform platform)
        {
            PlatformX.Activate(platform);
            foreach (var s in PakFiles) s.SetPlatform(platform);
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
        public IList<Platform> Platforms
        {
            get => _platforms;
            set { _platforms = value; OnPropertyChanged(); }
        }

        public static readonly BindableProperty MainTabsProperty = BindableProperty.Create(nameof(MainTabs), typeof(IList<MainPageTab>), typeof(MainPage),
            propertyChanged: (d, e, n) =>
            {
                var mainTab = ((MainPage)d).MainTab;
                if (mainTab.Children.FirstOrDefault() is RadioButton firstTab) firstTab.IsChecked = true;
            });
        public IList<MainPageTab> MainTabs
        {
            get => (IList<MainPageTab>)GetValue(MainTabsProperty);
            set => SetValue(MainTabsProperty, value);
        }

        public readonly IList<PakFile> PakFiles = [];
        public Dictionary<string, FamilyApp> FamilyApps;

        public Task OnOpenedAsync(Family family, string path = null)
        {
            var tabs = PakFiles.Select(pakFile => new MainPageTab
            {
                Name = pakFile.Name,
                PakFile = pakFile,
            }).ToList();
            var firstPakFile = tabs.FirstOrDefault()?.PakFile ?? PakFile.Empty;
            if (FamilyApps.Count > 0)
                tabs.Add(new MainPageTab
                {
                    Name = "Apps",
                    PakFile = firstPakFile,
                    AppList = [.. FamilyApps.Values],
                    Text = "Choose an application.",
                });
            if (!string.IsNullOrEmpty(family.Description))
                tabs.Add(new MainPageTab
                {
                    Name = "Information",
                    Text = family.Description,
                });
            MainTabs = tabs;

            // default main tab to first / second
            //MainTabContent.BindingContext = Tabs ...Children.FirstOrDefault(). s.LastOrDefault(); //.SelectedIndex = 0; // family.Apps != null ? 1 : 0
            return Task.CompletedTask;
        }

        void Platform_SelectionChanged(object sender, EventArgs e)
        {
            var selected = (Platform)Platform.SelectedItem;
            SetPlatform(selected);
        }

        internal void App_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var app = (FamilyApp)button.BindingContext;
            app.OpenAsync(app.ExplorerType, Manager).Wait();
        }
    }
}