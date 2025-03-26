﻿using OpenStack.Gfx;
using Platform = GameX.Platforms.Platform;

namespace GameX.App.Explorer.Views
{
    public partial class FileContent : ContentView
    {
        public static FileContent Current;

        public FileContent()
        {
            InitializeComponent();
            Current = this;
            BindingContext = this;
        }

        public void SetPlatform(Platform platform)
        {
            //var res = ((Grid)Content).Resources;
            //var plat = platform?.Id ?? "UK";
            //res["TViewGfx"] = res[$"TViewGfx:{plat}"] ?? res["THex"];
        }

        void ContentTab_Changed(object sender, CheckedChangedEventArgs e) => ContentTabContent.BindingContext = ((RadioButton)sender).BindingContext;

        IOpenGfx _gfx;
        public IOpenGfx Gfx
        {
            get => _gfx;
            set { _gfx = value; OnPropertyChanged(); }
        }

        IList<MetaContent> _contentTabs;
        public IList<MetaContent> ContentTabs
        {
            get => _contentTabs;
            set { _contentTabs = value; OnPropertyChanged(); }
        }

        public void OnInfo(PakFile pakFile, List<MetaInfo> infos)
        {
            if (ContentTabs != null) foreach (var dispose in ContentTabs.Where(x => x.Dispose != null).Select(x => x.Dispose)) dispose.Dispose();
            Gfx = pakFile.Gfx;
            ContentTabs = infos?.Select(x => x.Tag as MetaContent).Where(x => x != null).ToList();
            //ContentTab.CurrentItem = ContentTabs != null ? ContentTabs.FirstOrDefault() : null;
        }
    }
}