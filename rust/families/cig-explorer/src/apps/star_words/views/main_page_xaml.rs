// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/StarWords/Views/MainPage.xaml.cs
// PORT-SHA: 6d6b1723f07f186d
// PORT-STATUS: done
//
// C# `MainPage` is a `Window`. Here it is an egui window. The first tab is the
// Dashboard. A null `Document` shows `DashboardView`; anything else shows
// `DocumentView`. The close control is a button: the XAML image is an embedded
// icon from `ResourceManagerProvider`, which is still unported.

use std::sync::{Mutex, atomic::{AtomicU64, Ordering}};
use egui::Ui;
use super::dashboard_view_xaml::DashboardView;
use super::document_view_xaml::DocumentView;
use super::navigator_xaml::Navigator;

static NEXT: AtomicU64 = AtomicU64::new(1);
static PAGES: Mutex<Vec<MainPage>> = Mutex::new(Vec::new());

/// ContentTab
pub struct ContentTab {
    pub fixed: bool,
    pub name: String,
    /// C# `object`. `None` is null.
    pub document: Option<Vec<String>>,
}

/// MainPage
pub struct MainPage {
    n: u64,
    navigator: Navigator,
    contentTabs: Vec<ContentTab>,
    selected: usize,
    document: DocumentView,
}

impl MainPage {
    fn new() -> Self {
        Self {
            n: NEXT.fetch_add(1, Ordering::Relaxed),
            navigator: Navigator::default(),
            contentTabs: vec![ContentTab { fixed: false, name: "Dashboard".into(), document: None }],
            selected: 0,
            document: DocumentView::default(),
        }
    }

    pub fn addContentTab(&mut self, contentTab: ContentTab) {
        self.contentTabs.push(contentTab);
        self.selected = self.contentTabs.len() - 1;
    }

    fn onContentTabClose(&mut self, index: usize) {
        if index < self.contentTabs.len() { self.contentTabs.remove(index); }
        if self.selected >= self.contentTabs.len() && self.selected > 0 { self.selected -= 1; }
    }

    fn ui(&mut self, ui: &mut Ui) {
        let n = self.n;
        egui::Panel::top(egui::Id::new(("starwords-menu", n))).exact_size(20.).resizable(false).show(ui, |ui| {
            egui::MenuBar::new().ui(ui, |ui| {
                for item in ["File", "Search", "Perforce", "Options", "Help"] { ui.menu_button(item, |_| {}); }
            });
        });
        let mut opened = None;
        egui::Panel::left(egui::Id::new(("starwords-nav", n))).exact_size(250.).min_size(250.).resizable(true).show(ui, |ui| {
            opened = self.navigator.ui(ui);
        });
        if let Some(name) = opened { self.addContentTab(ContentTab { fixed: false, name, document: Some(vec![]) }); }
        egui::Panel::right(egui::Id::new(("starwords-bar", n))).exact_size(250.).min_size(250.).resizable(true).show(ui, |_| {});
        egui::CentralPanel::default().show(ui, |ui| {
            let mut close = None;
            ui.horizontal(|ui| {
                for (i, tab) in self.contentTabs.iter().enumerate() {
                    if ui.selectable_label(self.selected == i, &tab.name).clicked() { self.selected = i; }
                    if ui.small_button("x").clicked() { close = Some(i); }
                }
            });
            if let Some(i) = close { self.onContentTabClose(i); }
            let dashboard = self.contentTabs.get(self.selected).map(|s| s.document.is_none()).unwrap_or(true);
            if dashboard { DashboardView::ui(ui); } else { self.document.ui(ui); }
        });
    }
}

pub(crate) fn showNew() { PAGES.lock().unwrap().push(MainPage::new()); }

pub fn show(ctx: &egui::Context) {
    let mut pages = std::mem::take(&mut *PAGES.lock().unwrap());
    let mut keep = Vec::new();
    for mut page in pages.drain(..) {
        let n = page.n;
        if crate::showWindow(ctx, "StarWords", n, "StarWords", |ui| page.ui(ui)) { keep.push(page); }
    }
    let added = std::mem::take(&mut *PAGES.lock().unwrap());
    keep.extend(added);
    *PAGES.lock().unwrap() = keep;
}

#[cfg(test)]
pub(crate) fn openCount() -> usize { PAGES.lock().unwrap().len() }
#[cfg(test)]
pub(crate) fn clear() { PAGES.lock().unwrap().clear(); }
