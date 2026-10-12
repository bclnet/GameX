// PORT-SOURCE: Apps/GameX.App.Explorer/AppShell.xaml.cs
// PORT-SHA: c33c24d6f4c85012
// PORT-STATUS: done
//
// AppShell.xaml's menu is commented out, so its handlers are too. MainPage's
// show() runs the window until it closes, so it is called after the page is
// set up rather than before.

use std::sync::Arc;
use gamex::{config::option, family::Family};
use crate::views::main_page_xaml::MainPage;

/// AppShell
pub struct AppShell;

impl AppShell {
    pub fn new() -> Self { Self }

    pub(crate) fn startup(&self) -> i32 {
        let mut p = MainPage::new();
        if let Some(app) = option().forcePath.and_then(|s| s.strip_prefix("app:")).and_then(|s| p.familyApps.as_ref()?.get(s).cloned()) { p.App_Click(&app); }
        p.openPage_Click();
        p.show()
    }

    pub(crate) fn startupOpen(&self, family: Option<Arc<Family>>, uris: Vec<String>, path: Option<String>) -> i32 {
        let mut p = MainPage::new();
        p.open(family, uris, path);
        p.show()
    }

    fn openPage_Click(&self) {
        //var openPage = new OpenPage();
        //if (openPage.ShowDialog() == true) Instance.Open((Family)openPage.Family.SelectedItem, openPage.ArcUris);
    }

    fn optionsPage_Click(&self) {
        //var optionsPage = new OptionsPage();
        //optionsPage.ShowDialog();
    }

    fn worldMap_Click(&self) {
        //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
        //EngineView.ViewMode = ViewMode.Map;
    }

    fn aboutPage_Click(&self) {
        //var aboutPage = new AboutPage();
        //aboutPage.ShowDialog();
    }

    fn guide_Click(&self) {
        //Process.Start(@"docs\index.html");
    }
}
