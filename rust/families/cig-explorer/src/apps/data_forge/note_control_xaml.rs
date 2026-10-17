// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/DataForge/NoteControl.xaml.cs
// PORT-SHA: de9abc21a268745b
// PORT-STATUS: done
//
// The XAML text. Nothing in MainPage hosts this control.

use egui::Ui;

/// NoteControl
pub struct NoteControl;

impl NoteControl {
    pub fn ui(ui: &mut Ui) {
        ui.label("Use the DataForge App");
    }
}
