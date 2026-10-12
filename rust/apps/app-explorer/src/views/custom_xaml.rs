// PORT-SOURCE: Apps/GameX.App.Explorer/Views/Custom.xaml.cs
// PORT-SHA: 387688aeac877e55
// PORT-STATUS: done
//
// Custom.xaml is an empty Grid and Load() does nothing, so the C#'s Source and
// Value bindings have nothing to draw yet.

use eframe::egui;

/// Custom
#[derive(Default)]
pub struct Custom;

impl Custom {
    fn load(&mut self) {}

    pub fn ui(&mut self, ui: &mut egui::Ui) {}
}
