// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/Subsumption/Views/MainPage.xaml.cs
// PORT-SHA: 16261570dc46379e
// PORT-STATUS: done
//
// C# `MainPage` is a `Window` whose tab control starts on Missions
// (`SelectedIndex` 9). Here it is an egui window.

use std::sync::{Mutex, atomic::{AtomicU64, Ordering}};
use egui::Ui;
use super::activites_page_xaml::ActivitesPage;
use super::commercial_products_page_xaml::CommercialProductsPage;
use super::events_page_xaml::EventsPage;
use super::global_variables_page_xaml::GlobalVariablesPage;
use super::jurisdictions_page_xaml::JurisdictionsPage;
use super::missions_page_xaml::MissionsPage;
use super::persistant_entites_page_xaml::PersistantEntitesPage;
use super::platforms_page_xaml::PlatformsPage;
use super::setup_page_xaml::SetupPage;
use super::star_systems_page_xaml::StarSystemsPage;
use super::task_archetypes_page_xaml::TaskArchetypesPage;
use super::variable_types_page_xaml::VariableTypesPage;

static NEXT: AtomicU64 = AtomicU64::new(1);
static PAGES: Mutex<Vec<MainPage>> = Mutex::new(Vec::new());

const TABS: &[&str] = &[
    "Setup", "Variable Types", "Global Variables", "Persistant Entites",
    "Commercial Products", "Events", "Task Archetypes", "Engines",
    "Activites", "Missions", "Jurisdictions", "Star Systems",
];

/// MainPage
pub struct MainPage {
    n: u64,
    /// C# `MainTabs.SelectedIndex`, which the XAML sets to 9.
    selected: usize,
    missions: MissionsPage,
}

impl MainPage {
    fn new() -> Self { Self { n: NEXT.fetch_add(1, Ordering::Relaxed), selected: 9, missions: MissionsPage::default() } }

    fn ui(&mut self, ui: &mut Ui) {
        let n = self.n;
        ui.horizontal_wrapped(|ui| {
            for (i, name) in TABS.iter().enumerate() {
                if ui.selectable_label(self.selected == i, *name).clicked() { self.selected = i; }
            }
        });
        ui.separator();
        match self.selected {
            0 => SetupPage::ui(ui),
            1 => VariableTypesPage::ui(ui),
            2 => GlobalVariablesPage::ui(ui),
            3 => PersistantEntitesPage::ui(ui),
            4 => CommercialProductsPage::ui(ui),
            5 => EventsPage::ui(ui),
            6 => TaskArchetypesPage::ui(ui),
            7 => PlatformsPage::ui(ui),
            8 => ActivitesPage::ui(ui),
            9 => self.missions.ui(ui, n),
            10 => JurisdictionsPage::ui(ui),
            _ => StarSystemsPage::ui(ui),
        }
    }
}

pub(crate) fn showNew() { PAGES.lock().unwrap().push(MainPage::new()); }

pub fn show(ctx: &egui::Context) {
    let mut pages = std::mem::take(&mut *PAGES.lock().unwrap());
    let mut keep = Vec::new();
    for mut page in pages.drain(..) {
        let n = page.n;
        if crate::showWindow(ctx, "Subsumption", n, "Subsumption", |ui| page.ui(ui)) { keep.push(page); }
    }
    let added = std::mem::take(&mut *PAGES.lock().unwrap());
    keep.extend(added);
    *PAGES.lock().unwrap() = keep;
}

#[cfg(test)]
pub(crate) fn openCount() -> usize { PAGES.lock().unwrap().len() }
#[cfg(test)]
pub(crate) fn clear() { PAGES.lock().unwrap().clear(); }
