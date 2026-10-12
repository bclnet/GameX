// PORT-SOURCE: Apps/GameX.App.Explorer/Views/MainPage.xaml.cs
// PORT-SHA: dbe4c7767627b95b
// PORT-STATUS: done
//
// MainPage.xaml's Grid becomes egui panels: the menu on top, the log and
// engine ComboBox at the bottom, MainTabControl in a resizable left panel (the
// GridSplitter), FileContent in the centre. The tab ContentTemplate triggers
// are `MainPageTab::ui`. The C#'s Current statics are gone: MainPage owns its
// FileContent and passes it down. Errors the C# would throw from opening an
// archive or app are written to the log.

use std::sync::Arc;
use eframe::egui;
use gamex::binary::{Archive, ArchiveExt};
use gamex::config::option;
use gamex::family::{Family, FamilyApp};
use indexmap::IndexMap;
use openx::platform::{activate, Platform};
use crate::app_xaml::App;
use crate::resources::resource_manager::ResourceManager;
use super::about_page_xaml::AboutPage;
use super::file_content_xaml::FileContent;
use super::file_explorer_xaml::FileExplorer;
use super::open_page_xaml::OpenPage;
use super::options_page_xaml::OptionsPage;
use super::view_extensions::ViewExtensions;

/// MainPageTab
#[derive(Default)]
pub struct MainPageTab {
    pub name: String,
    pub archive: Option<Arc<dyn Archive>>,
    pub appList: Option<Vec<Arc<FamilyApp>>>,
    pub text: Option<String>,
    fileExplorer: Option<FileExplorer>,
}

impl MainPageTab {
    fn ui(&mut self, ui: &mut egui::Ui, fileContent: &mut FileContent) -> Option<usize> {
        if self.archive.is_some() {
            match &self.appList {
                // TFileExplorer
                None => { if let Some(z) = &mut self.fileExplorer { z.ui(ui, fileContent); } }
                // TAppList
                Some(appList) => {
                    ui.add(egui::Label::new(self.text.as_deref().unwrap_or_default()).wrap());
                    for (i, s) in appList.iter().enumerate() { if ui.button(s.name.as_deref().unwrap_or_default()).clicked() { return Some(i); } }
                }
            }
            return None;
        }
        // TTextBlock
        egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| { ui.add(egui::Label::new(self.text.as_deref().unwrap_or_default()).wrap()); });
        None
    }
}

/// MainPage
pub struct MainPage {
    pub engines: Vec<Box<dyn Platform>>,
    engine: Option<usize>,
    mainTabs: Vec<MainPageTab>,
    mainTab: usize,
    log: String,
    fileContent: FileContent,
    pub archives: Vec<Arc<dyn Archive>>,
    pub familyApps: Option<IndexMap<String, Arc<FamilyApp>>>,
    openPage: Option<OpenPage>,
    optionsPage: Option<OptionsPage>,
    aboutPage: Option<AboutPage>,
}

impl MainPage {
    pub fn new() -> Self {
        let engines: Vec<_> = App::engines().into_iter().filter(|s| s.enabled()).collect();
        let engine = engines.iter().position(|s| Some(s.id()) == option().engine);
        let mut s = Self {
            engines, engine: None, mainTabs: vec![], mainTab: 0, log: String::new(), fileContent: FileContent::default(),
            archives: vec![], familyApps: None, openPage: None, optionsPage: None, aboutPage: None,
        };
        s.engine_SelectionChanged(engine);
        s
    }

    /// C# Window.Show(): runs the window until it closes.
    pub fn show(self) -> i32 {
        let options = eframe::NativeOptions { viewport: egui::ViewportBuilder::default().with_title("GameX Explorer").with_inner_size([1342., 798.]), ..Default::default() };
        match eframe::run_native("GameX Explorer", options, Box::new(|cc| {
            egui_extras::install_image_loaders(&cc.egui_ctx);
            ResourceManager::current().includeIcons(&cc.egui_ctx);
            Ok(Box::new(self))
        })) {
            Ok(()) => 0,
            Err(e) => { eprintln!("{e}"); 1 }
        }
    }

    pub fn open(&mut self, family: Option<Arc<Family>>, uris: Vec<String>, path: Option<String>) -> &mut Self {
        if let Some(selected) = self.engine { activate(App::engines().swap_remove(selected)); }
        for archive in &self.archives { let _ = archive.close(); }
        self.archives.clear();
        let Some(family) = family else { return self };
        self.familyApps = Some(family.apps.clone());
        for s in uris {
            self.log.writeLine(&format!("Opening {s}"));
            match family.getArchive(&s, true) {
                Ok(arc) => if let Some(arc) = arc { self.archives.push(arc); },
                Err(e) => self.log.writeLine(&e.to_string()),
            }
        }
        self.log.writeLine("Done");
        self.onOpenedAsync(&family, path);
        self
    }

    pub fn setEngine(&mut self, platform: Option<usize>) {
        let engine = platform.map(|i| App::engines().swap_remove(i));
        self.fileContent.setEngine(engine.as_deref());
        if let Some(engine) = engine { activate(engine); }
    }

    pub fn onOpenedAsync(&mut self, family: &Family, path: Option<String>) {
        let mut tabs = vec![];
        for archive in &self.archives {
            let fileExplorer = match FileExplorer::new(archive.clone(), &mut self.fileContent) {
                Ok(z) => Some(z),
                Err(e) => { self.log.writeLine(&e.to_string()); None }
            };
            tabs.push(MainPageTab { name: archive.base().name.clone(), archive: Some(archive.clone()), fileExplorer, ..Default::default() });
        }
        let firstArchive = tabs.first().and_then(|s| s.archive.clone()).unwrap_or_else(<dyn Archive>::empty);
        if let Some(familyApps) = self.familyApps.as_ref().filter(|s| !s.is_empty()) {
            tabs.push(MainPageTab {
                name: "Apps".into(),
                archive: Some(firstArchive),
                appList: Some(familyApps.values().cloned().collect()),
                text: Some("Choose an application.".into()),
                ..Default::default()
            });
        }
        if let Some(description) = family.description.as_ref().filter(|s| !s.is_empty()) {
            tabs.push(MainPageTab {
                name: "Information".into(),
                text: Some(description.clone()),
                ..Default::default()
            });
        }
        self.mainTabs = tabs;

        // default main tab to first / second
        self.mainTab = 0; // family.Apps != null ? 1 : 0;
    }

    fn engine_SelectionChanged(&mut self, selected: Option<usize>) {
        self.engine = selected;
        self.setEngine(selected);
    }
}

// Menu
impl MainPage {
    pub fn App_Click(&mut self, app: &FamilyApp) {
        if let Err(e) = app.openAsync(app.explorerType.as_deref().unwrap_or_default(), Some(ResourceManager::current())) { self.log.writeLine(&e.to_string()); }
    }

    pub fn openPage_Click(&mut self) { self.openPage = Some(OpenPage::new()); }

    fn optionsPage_Click(&mut self) { self.optionsPage = Some(OptionsPage); }

    fn worldMap_Click(&mut self) {
        //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
        //EngineView.ViewMode = ViewMode.Map;
    }

    fn aboutPage_Click(&mut self) { self.aboutPage = Some(AboutPage); }

    fn guide_Click(&mut self) {
        //Process.Start(@"docs\index.html");
    }
}

impl eframe::App for MainPage {
    fn ui(&mut self, ui: &mut egui::Ui, frame: &mut eframe::Frame) {
        egui::Panel::top("MainMenu").show(ui, |ui| {
            egui::MenuBar::new().ui(ui, |ui| {
                ui.menu_button("File", |ui| { if ui.button("Open").clicked() { self.openPage_Click(); } });
                ui.menu_button("Edit", |ui| { if ui.button("Options").clicked() { self.optionsPage_Click(); } });
                ui.menu_button("View", |ui| { if ui.button("World Map").clicked() { self.worldMap_Click(); } });
                ui.menu_button("Help", |ui| {
                    if ui.button("View Help").clicked() { self.guide_Click(); }
                    if ui.button("About").clicked() { self.aboutPage_Click(); }
                });
            });
        });

        egui::Panel::bottom("Log").exact_size(63.).show(ui, |ui| {
            egui::Panel::right("LogPane").exact_size(200.).show_separator_line(false).show(ui, |ui| {
                let mut engine = self.engine;
                egui::ComboBox::from_id_salt("Engine").width(ui.available_width())
                    .selected_text(engine.map_or("", |i| self.engines[i].name()))
                    .show_ui(ui, |ui| { for (i, s) in self.engines.iter().enumerate() { ui.selectable_value(&mut engine, Some(i), s.name()); } });
                if engine != self.engine { self.engine_SelectionChanged(engine); }
            });
            egui::ScrollArea::vertical().stick_to_bottom(true).auto_shrink(false).show(ui, |ui| {
                ui.add(egui::Label::new(self.log.as_str()).wrap());
            });
        });

        egui::Panel::left("MainTabControl").min_size(250.).default_size(250.).resizable(true).show(ui, |ui| {
            ui.horizontal_wrapped(|ui| {
                for (i, s) in self.mainTabs.iter().enumerate() { if ui.selectable_label(self.mainTab == i, &s.name).clicked() { self.mainTab = i; } }
            });
            ui.separator();
            let clicked = match self.mainTabs.get_mut(self.mainTab) { Some(tab) => tab.ui(ui, &mut self.fileContent), None => None };
            if let Some(app) = clicked.and_then(|i| self.mainTabs[self.mainTab].appList.as_ref().map(|s| s[i].clone())) { self.App_Click(&app); }
        });

        egui::CentralPanel::default().show(ui, |ui| self.fileContent.ui(ui));

        if let Some(openPage) = &mut self.openPage {
            match openPage.ui(ui.ctx()) {
                Some(true) => { let (family, uris) = (openPage.familySelected(), openPage.arcUris()); self.openPage = None; self.open(family, uris, None); }
                Some(false) => self.openPage = None,
                None => {}
            }
        }
        if let Some(z) = &mut self.optionsPage { if !z.ui(ui.ctx()) { self.optionsPage = None; } }
        if let Some(z) = &mut self.aboutPage { if !z.ui(ui.ctx()) { self.aboutPage = None; } }
    }
}
