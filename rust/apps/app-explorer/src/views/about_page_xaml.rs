// PORT-SOURCE: Apps/GameX.App.Explorer/Views/AboutPage.xaml.cs
// PORT-SHA: e17aa594515fc581
// PORT-STATUS: done

use eframe::egui;

/// AboutPage
#[derive(Default)]
pub struct AboutPage;

impl AboutPage {
    /// Returns false once OK closes the dialog.
    pub fn ui(&mut self, ctx: &egui::Context) -> bool {
        let mut open = true;
        egui::Window::new("About").collapsible(false).resizable(false).fixed_size([280., 170.]).show(ctx, |ui| {
            ui.label(egui::RichText::new("GameX Explorer").strong());
            ui.add_space(8.);
            ui.label("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
            ui.vertical_centered(|ui| if ui.add_sized([75., 20.], egui::Button::new("OK")).clicked() { open = false; });
        });
        open
    }
}
