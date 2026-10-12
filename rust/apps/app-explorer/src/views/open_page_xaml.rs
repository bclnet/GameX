// PORT-SOURCE: Apps/GameX.App.Explorer/Views/OpenPage.xaml.cs
// PORT-SHA: 72620c5e72bab169
// PORT-STATUS: done
//
// The dialog is an egui Window over the main page; `ui` returns
// `Some(DialogResult)` once Cancel or Open is clicked. The file dialogs are rfd.

use std::sync::Arc;
use eframe::egui;
use gamex::config::option;
use gamex::family::{families, Family, FamilyGame, FamilyGameEdition};

/// OpenPage
pub struct OpenPage {
    pub families: Vec<Arc<Family>>,
    pub family: Option<usize>,
    games: Option<Vec<Arc<FamilyGame>>>,
    game: Option<usize>,
    editions: Option<Vec<Arc<FamilyGameEdition>>>,
    edition: Option<usize>,
    pak1Uri: String,
    pak2Uri: String,
    pak3Uri: String,
    forceOpen: bool,
}

impl OpenPage {
    pub fn new() -> Self {
        let mut s = Self { families: families().values().cloned().collect(), family: None, games: None, game: None, editions: None, edition: None, pak1Uri: String::new(), pak2Uri: String::new(), pak3Uri: String::new(), forceOpen: false };
        s.loadedx();
        s
    }

    pub fn familySelected(&self) -> Option<Arc<Family>> { self.family.map(|i| self.families[i].clone()) }

    fn gameSelected(&self) -> Option<&Arc<FamilyGame>> { self.games.as_ref().zip(self.game).map(|(s, i)| &s[i]) }

    fn editionSelected(&self) -> Option<&Arc<FamilyGameEdition>> { self.editions.as_ref().zip(self.edition).map(|(s, i)| &s[i]) }

    pub fn arcUris(&self) -> Vec<String> { [&self.pak1Uri, &self.pak2Uri, &self.pak3Uri].into_iter().filter(|x| !x.is_empty()).cloned().collect() }

    fn setArcUris(&mut self, value: Option<Vec<String>>) {
        let mut v = value.unwrap_or_default().into_iter().filter(|s| !s.is_empty());
        self.pak1Uri = v.next().unwrap_or_default();
        self.pak2Uri = v.next().unwrap_or_default();
        self.pak3Uri = v.next().unwrap_or_default();
    }

    fn family_SelectionChanged(&mut self) {
        self.games = self.familySelected().map(|s| s.games.values().filter(|x| x.files.is_some()).cloned().collect());
        self.game = None;
        self.game_SelectionChanged();
    }

    fn game_SelectionChanged(&mut self) {
        self.editions = self.gameSelected().map(|s| s.editions.values().cloned().collect());
        self.edition = self.editions.as_ref().and_then(|s| s.iter().position(|x| x.id.is_empty()));
        self.edition_SelectionChanged();
    }

    fn edition_SelectionChanged(&mut self) {
        let uris = self.gameSelected().map(|s| s.toUris(self.editionSelected().map(|x| x.id.as_str())));
        self.setArcUris(uris);
    }

    fn pakUri_Click(&self) -> Option<String> {
        let file = rfd::FileDialog::new().add_filter("Arc files", &["*"]).pick_file()?;
        Some(format!("file://{}#{}", file.display(), self.gameSelected().map_or("Unknown", |s| s.id.as_str())))
    }

    fn loadedx(&mut self) {
        let opt = option();
        let Some(family) = opt.family else { return };
        self.family = families().keys().position(|s| s == family);
        self.family_SelectionChanged();
        let Some(game) = opt.game else { return };
        self.game = self.games.as_ref().and_then(|s| s.iter().position(|x| x.id == game));
        self.game_SelectionChanged();
        self.edition = self.editions.as_ref().and_then(|s| s.iter().position(|x| x.id == opt.edition.unwrap_or("")));
        self.edition_SelectionChanged();
        self.forceOpen = opt.forceOpen;
    }

    fn combo(ui: &mut egui::Ui, id: &str, selected: &mut Option<usize>, names: Vec<String>) -> bool {
        let before = *selected;
        egui::ComboBox::from_id_salt(id).width(260.).selected_text(selected.and_then(|i| names.get(i)).cloned().unwrap_or_default()).show_ui(ui, |ui| {
            for (i, s) in names.iter().enumerate() { ui.selectable_value(selected, Some(i), s); }
        });
        *selected != before
    }

    pub fn ui(&mut self, ctx: &egui::Context) -> Option<bool> {
        let mut result = None;
        egui::Window::new("File Open").collapsible(false).resizable(false).default_size([380., 240.]).show(ctx, |ui| {
            egui::Grid::new("OpenPage").num_columns(3).show(ui, |ui| {
                ui.label("Family:");
                if Self::combo(ui, "Family", &mut self.family, self.families.iter().map(|s| s.name.clone().unwrap_or_default()).collect()) { self.family_SelectionChanged(); }
                ui.end_row();
                ui.label("Game:");
                if Self::combo(ui, "Game", &mut self.game, self.games.iter().flatten().map(|s| s.displayedName()).collect()) { self.game_SelectionChanged(); }
                ui.end_row();
                ui.label("Game Edition:");
                if Self::combo(ui, "Edition", &mut self.edition, self.editions.iter().flatten().map(|s| s.name.clone().unwrap_or_default()).collect()) { self.edition_SelectionChanged(); }
                ui.end_row();
                for i in 0..3 {
                    ui.label("Resource Uri:");
                    ui.text_edit_singleline([&mut self.pak1Uri, &mut self.pak2Uri, &mut self.pak3Uri][i]);
                    if ui.button("*").clicked() { if let Some(uri) = self.pakUri_Click() { *[&mut self.pak1Uri, &mut self.pak2Uri, &mut self.pak3Uri][i] = uri; } }
                    ui.end_row();
                }
            });
            ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                if ui.button(" Open ").clicked() { result = Some(true); }
                if ui.button(" Cancel ").clicked() { result = Some(false); }
            });
        });
        if std::mem::take(&mut self.forceOpen) { result = Some(true); }
        result
    }
}
