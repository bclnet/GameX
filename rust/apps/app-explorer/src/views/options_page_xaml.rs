// PORT-SOURCE: Apps/GameX.App.Explorer/Views/OptionsPage.xaml.cs
// PORT-SHA: d2e2168c5ee5658b
// PORT-STATUS: done

use eframe::egui;

/// OptionsPage
#[derive(Default)]
pub struct OptionsPage;

impl OptionsPage {
    /// Returns false once the window is closed.
    pub fn ui(&mut self, ctx: &egui::Context) -> bool {
        let mut open = true;
        egui::Window::new("Options").open(&mut open).default_size([380., 280.]).show(ctx, |ui| {});
        open
    }
}
