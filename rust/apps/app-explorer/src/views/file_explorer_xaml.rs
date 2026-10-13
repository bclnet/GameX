// PORT-SOURCE: Apps/GameX.App.Explorer/Views/FileExplorer.xaml.cs
// PORT-SHA: 4b16efb90ff876b1
// PORT-STATUS: done
//
// The TreeViews are egui collapsing headers: folders collapsed (IsExpanded
// False), infos expanded (IsExpanded True). The editable Filter ComboBox is a
// text box (KeyUp) beside a ComboBox of the archive's filters. The C#'s
// Current statics are gone: MainPage passes its FileContent in.
//
// C# MetaItems are shared references, so `Nodes = [.. ArcNodes]` sees nested
// archives opened into ArcNodes. Rust clones them, so an item opened through a
// copy is found in `arcNodes` by value, and `nodes` is rebuilt from it.

use std::io;
use std::sync::Arc;
use eframe::egui;
use gamex::binary::{getResult, Archive, ArchiveExt, BinaryStat};
use gamex::config::option;
use gamex::meta::{MetaInfo, MetaItem, MetaItemFilter, Tag};
use openx_poly::core::{ISource, SourceRef};
use crate::resources::resource_manager::ResourceManager;
use super::file_content_xaml::FileContent;

/// FileExplorer
pub struct FileExplorer {
    pub archive: Arc<dyn Archive>,
    filters: Option<Vec<MetaItemFilter>>,
    filter: String,
    arcNodes: Vec<MetaItem>,
    nodes: Vec<MetaItem>,
    infos: Option<Vec<MetaInfo>>,
    selectedItem: Option<MetaItem>,
}

impl FileExplorer {
    pub fn new(archive: Arc<dyn Archive>, fileContent: &mut FileContent) -> io::Result<Self> {
        let resource = ResourceManager::current();
        let arcNodes = archive.getMetaItems(resource)?;
        let mut s = Self { filters: archive.getMetaFilters(resource), filter: String::new(), nodes: arcNodes.clone(), arcNodes, archive, infos: None, selectedItem: None };
        s.ready(fileContent)?;
        Ok(s)
    }

    fn onFilterKeyUp(&mut self) {
        if self.filter.is_empty() { self.nodes = self.arcNodes.clone(); }
        else { self.nodes = self.arcNodes.iter().filter_map(|x| x.search(&|y: &MetaItem| y.name.contains(&self.filter))).collect(); }
    }

    fn onFilterSelected(&mut self, filter: &MetaItemFilter) {
        if self.filter.is_empty() { self.nodes = self.arcNodes.clone(); }
        else { self.nodes = self.arcNodes.iter().filter_map(|s| s.search(&|y: &MetaItem| y.name.contains(&filter.description))).collect(); }
    }

    fn findNode<'a>(nodes: &'a mut [MetaItem], item: &MetaItem) -> Option<&'a mut MetaItem> {
        for s in nodes {
            if s == item { return Some(s); }
            if let Some(z) = Self::findNode(&mut s.items, item) { return Some(z); }
        }
        None
    }

    fn setSelectedItem(&mut self, value: Option<MetaItem>, fileContent: &mut FileContent) {
        if self.selectedItem == value { return; }
        self.selectedItem = value.clone();
        let Some(value) = value else { self.onInfo(None, None, fileContent); return };
        let resource = ResourceManager::current();
        let arc = value.source.asFileSource().and_then(|s| s.fix().arc.clone());
        if arc.as_ref().is_some_and(|s| s.base.status() == BinaryStat::Opened) { return; }
        let result = (|| -> io::Result<Option<Vec<MetaInfo>>> {
            if let Some(arc) = arc {
                arc.open(Self::findNode(&mut self.arcNodes, &value).map(|s| &mut s.items), Some(resource))?;
                self.onFilterKeyUp();
            }
            match &value.archive { Some(z) => getResult(z.getMetaInfos(resource, &value)), None => Ok(None) }
        })();
        match result {
            Ok(infos) => self.onInfo(Some(value), infos, fileContent),
            Err(e) => self.onInfo(Some(value), Some(vec![MetaInfo::new(Some(format!("EXCEPTION: {e}")), Tag::None, vec![], false)]), fileContent),
        }
    }

    pub fn onInfoUpdated(&mut self) {}

    pub fn onInfo(&mut self, item: Option<MetaItem>, infos: Option<Vec<MetaInfo>>, fileContent: &mut FileContent) {
        let (content, named) = match infos {
            Some(s) => { let (a, b): (Vec<_>, Vec<_>) = s.into_iter().partition(|s| s.name.is_none()); (Some(a), Some(b)) }
            None => (None, None),
        };
        fileContent.onInfo(item, Some(SourceRef(self.archive.clone() as Arc<dyn ISource>)), content);
        self.infos = named;
    }

    fn onNodeSelected(&mut self, itemNode: MetaItem, fileContent: &mut FileContent) {
        if itemNode.archive.is_some() && self.selectedItem.as_ref() != Some(&itemNode) { self.setSelectedItem(Some(itemNode), fileContent); }
    }

    fn ready(&mut self, fileContent: &mut FileContent) -> io::Result<()> {
        let Some(forcePath) = option().forcePath.filter(|s| !s.starts_with("app:")) else { return Ok(()) };
        let sample = match forcePath.strip_prefix("sample:") { Some(s) => self.archive.base().game.getSample(s)?, None => None };
        let paths = match sample { Some(s) => s.paths.clone(), None => Some(vec![forcePath.to_string()]) };
        let Some(paths) = paths else { return Ok(()) };
        for path in paths {
            let item = MetaItem::findByPathForNodes(&mut self.arcNodes, &path, ResourceManager::current()).cloned();
            self.onFilterKeyUp();
            self.setSelectedItem(item, fileContent);
        }
        Ok(())
    }

    fn nodeUi(ui: &mut egui::Ui, id: egui::Id, s: &MetaItem, selected: &Option<MetaItem>, clicked: &mut Option<MetaItem>) {
        let header = |ui: &mut egui::Ui, clicked: &mut Option<MetaItem>| {
            if let Some(icon) = s.icon.asStr() { ui.add(egui::Image::new(icon).fit_to_exact_size(egui::vec2(16., 16.))); }
            if ui.selectable_label(selected.as_ref() == Some(s), &s.name).clicked() { *clicked = Some(s.clone()); }
        };
        if s.items.is_empty() { ui.horizontal(|ui| header(ui, clicked)); return; }
        egui::collapsing_header::CollapsingState::load_with_default_open(ui.ctx(), id, false)
            .show_header(ui, |ui| header(ui, clicked))
            .body(|ui| for x in &s.items { Self::nodeUi(ui, id.with(&x.name), x, selected, clicked); });
    }

    fn infoUi(ui: &mut egui::Ui, id: egui::Id, s: &MetaInfo) {
        let name = s.name.as_deref().unwrap_or_default();
        if s.items.is_empty() { ui.label(name); return; }
        egui::CollapsingHeader::new(name).id_salt(id).default_open(true).show(ui, |ui| {
            for (i, x) in s.items.iter().enumerate() { Self::infoUi(ui, id.with(i), x); }
        });
    }

    pub fn ui(&mut self, ui: &mut egui::Ui, fileContent: &mut FileContent) {
        ui.label("File Filter");
        let (mut keyUp, mut selected) = (false, None);
        // right to left, so the text box takes exactly what the drop-down leaves: a row wider than
        // the panel makes the resizable panel grow a little every frame
        ui.allocate_ui_with_layout(egui::vec2(ui.available_width(), ui.spacing().interact_size.y), egui::Layout::right_to_left(egui::Align::Center), |ui| {
            egui::ComboBox::from_id_salt("Filter").width(20.).selected_text("").show_ui(ui, |ui| {
                for s in self.filters.iter().flatten() { if ui.selectable_label(false, &s.name).clicked() { selected = Some(s.clone()); } }
            });
            keyUp = ui.add(egui::TextEdit::singleline(&mut self.filter).desired_width(ui.available_width())).changed();
        });
        if keyUp { self.onFilterKeyUp(); }
        if let Some(filter) = selected { self.filter = filter.name.clone(); self.onFilterSelected(&filter); }

        let height = ui.available_height();
        let mut clicked = None;
        egui::ScrollArea::vertical().id_salt("Node").max_height(height * 0.7).auto_shrink(false).show(ui, |ui| {
            let id = ui.id().with("Node");
            for s in &self.nodes { Self::nodeUi(ui, id.with(&s.name), s, &self.selectedItem, &mut clicked); }
        });
        if let Some(item) = clicked { self.onNodeSelected(item, fileContent); }
        ui.separator();
        egui::ScrollArea::vertical().id_salt("Info").auto_shrink(false).show(ui, |ui| {
            let id = ui.id().with("Info");
            for (i, s) in self.infos.iter().flatten().enumerate() { Self::infoUi(ui, id.with(i), s); }
        });
    }
}
