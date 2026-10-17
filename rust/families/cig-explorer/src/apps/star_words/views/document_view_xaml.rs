// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/StarWords/Views/DocumentView.xaml.cs
// PORT-SHA: a074b5abdd7c06b9
// PORT-STATUS: done
//
// The grid columns are the XAML. The `Document` setter's body is commented out,
// so the rows stay empty.

use egui::Ui;

/// DocumentView
#[derive(Default)]
pub struct DocumentView {
    /// C# `object`.
    pub document: Option<Vec<String>>,
}

impl DocumentView {
    pub fn ui(&mut self, ui: &mut Ui) {
        let _ = self.document;
        egui::Grid::new("starwords-doc").show(ui, |ui| {
            for header in ["LocId", "English Text", "Status", "Label", "Use", ""] {
                ui.set_min_width(50.);
                ui.label(header);
            }
            ui.end_row();
        });
    }
}
