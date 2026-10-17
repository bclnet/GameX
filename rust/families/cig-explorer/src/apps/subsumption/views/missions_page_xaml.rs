// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/Subsumption/Views/MissionsPage.xaml.cs
// PORT-SHA: 2e6db2df368b4ce7
// PORT-STATUS: done
//
// Left tabs are Missions (the navigator) and Current Mission ("TBD").
// The other two columns are the labels BODY and BAR.

use egui::Ui;
use super::missions::navigator_xaml::Navigator;

/// MissionsPage
#[derive(Default)]
pub struct MissionsPage {
    selected: usize,
    navigator: Navigator,
}

impl MissionsPage {
    pub fn ui(&mut self, ui: &mut Ui, n: u64) {
        egui::Panel::left(egui::Id::new(("sub-missions", n))).exact_size(250.).min_size(250.).resizable(true).show(ui, |ui| {
            ui.horizontal(|ui| {
                if ui.selectable_label(self.selected == 0, "Missions").clicked() { self.selected = 0; }
                if ui.selectable_label(self.selected == 1, "Current Mission").clicked() { self.selected = 1; }
            });
            if self.selected == 0 { self.navigator.ui(ui); } else { ui.label("TBD"); }
        });
        egui::Panel::right(egui::Id::new(("sub-missions-bar", n))).exact_size(250.).min_size(250.).resizable(true).show(ui, |ui| {
            ui.label("BAR");
        });
        egui::CentralPanel::default().show(ui, |ui| { ui.label("BODY"); });
    }
}
