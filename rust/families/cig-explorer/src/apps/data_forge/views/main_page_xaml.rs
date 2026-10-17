// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/DataForge/Views/MainPage.xaml.cs
// PORT-SHA: 1ed4cb292fc9f2c5
// PORT-STATUS: done
//
// C# `MainPage` is a `Window`. Here it is an egui window. `App.Db.Nodes` is
// not available, so `Navigator.Nodes` stays empty.

use std::sync::{Mutex, atomic::{AtomicU64, Ordering}};
use egui::Ui;
use super::navigator_xaml::Navigator;

static NEXT: AtomicU64 = AtomicU64::new(1);
static PAGES: Mutex<Vec<MainPage>> = Mutex::new(Vec::new());

/// MainPage
pub struct MainPage {
    n: u64,
    navigator: Navigator,
}

impl MainPage {
    fn new() -> Self { Self { n: NEXT.fetch_add(1, Ordering::Relaxed), navigator: Navigator::default() } }

    fn ui(&mut self, ui: &mut Ui) {
        let n = self.n;
        egui::Panel::top(egui::Id::new(("dataforge-menu", n))).exact_size(20.).resizable(false).show(ui, |ui| {
            egui::MenuBar::new().ui(ui, |ui| {
                for item in ["File", "Edit", "View", "Tools", "Network", "Settings"] { ui.menu_button(item, |_| {}); }
            });
        });
        egui::Panel::left(egui::Id::new(("dataforge-nav", n))).exact_size(250.).min_size(250.).resizable(true).show(ui, |ui| {
            self.navigator.ui(ui);
        });
        egui::CentralPanel::default().show(ui, |ui| { ui.label("BAR"); });
    }
}

pub(crate) fn showNew() { PAGES.lock().unwrap().push(MainPage::new()); }

pub fn show(ctx: &egui::Context) {
    let mut pages = std::mem::take(&mut *PAGES.lock().unwrap());
    let mut keep = Vec::new();
    for mut page in pages.drain(..) {
        let n = page.n;
        if crate::showWindow(ctx, "DataForge", n, "DataForge", |ui| page.ui(ui)) { keep.push(page); }
    }
    let added = std::mem::take(&mut *PAGES.lock().unwrap());
    keep.extend(added);
    *PAGES.lock().unwrap() = keep;
}

#[cfg(test)]
pub(crate) fn openCount() -> usize { PAGES.lock().unwrap().len() }
#[cfg(test)]
pub(crate) fn clear() { PAGES.lock().unwrap().clear(); }
