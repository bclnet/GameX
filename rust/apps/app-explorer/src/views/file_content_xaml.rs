// PORT-SOURCE: Apps/GameX.App.Explorer/Views/FileContent.xaml.cs
// PORT-SHA: b80edf914dcdc04e
// PORT-STATUS: done
//
// FileContent.xaml's DataTemplates and triggers become `FileContent::key` and
// the `Template` each content tab builds once (Python's `updateTabs`).
// TViewGfx and TAudioPlayer are placeholders: no OpenX engine control or audio
// player is hosted in egui yet. TDataGrid shows as text, as in Python.
// C#'s Dispose of the old tabs is Drop; images are also dropped from egui's
// cache, which keys them by uri.

use std::sync::atomic::{AtomicUsize, Ordering};
use std::sync::Arc;
use eframe::egui;
use gamex::meta::{MetaContent, MetaInfo, MetaItem, Tag};
use openx::platform::Platform;
use openx_poly::core::SourceRef;
use super::app_hex_widget::AppHexWidget;
use super::custom_xaml::Custom;

enum Template {
    TNull,
    TText(String),
    TImage(String, egui::load::Bytes, f32, f32),
    THex(AppHexWidget),
    TAudioPlayer,
    TViewGfx(String),
    TCustom(Custom),
}

/// FileContent
#[derive(Default)]
pub struct FileContent {
    viewGfx: String,
    pub source: Option<SourceRef>,
    pub path: Option<MetaItem>,
    contentTabs: Option<Vec<(Arc<MetaContent>, Template)>>,
    selectedIndex: i32,
    ctx: Option<egui::Context>,
}

impl FileContent {
    pub fn setEngine(&mut self, engine: Option<&dyn Platform>) {
        let plat = engine.map_or("UK", |s| s.id());
        self.viewGfx = match plat {
            "GD" | "O3" | "OG" | "GL" | "SD" | "ST" | "UN" | "UR" => format!("TViewGfx:{plat}"),
            _ => "TText".into(),
        };
    }

    pub fn contentTabs(&self) -> Option<Vec<&Arc<MetaContent>>> { self.contentTabs.as_ref().map(|s| s.iter().map(|(c, _)| c).collect()) }

    pub fn setContentTabs(&mut self, value: Option<Vec<Arc<MetaContent>>>) {
        if let (Some(ctx), Some(tabs)) = (&self.ctx, &self.contentTabs) {
            for (_, t) in tabs { if let Template::TImage(uri, ..) = t { ctx.forget_image(uri); } }
        }
        self.contentTabs = value.map(|s| s.into_iter().map(|c| { let t = self.template(&c); (c, t) }).collect());
        self.selectedIndex = if self.contentTabs.is_some() { 0 } else { -1 };
    }

    fn key(&self, s: &MetaContent) -> &str {
        if s.value.isNone() { return "TNull"; }
        if s.engineType.as_deref() == Some("ICustomFormatter") { return "TCustom"; }
        match s.type_.as_str() {
            "Hex" => "THex",
            "DataGrid" => "TDataGrid",
            "AudioPlayer" => "TAudioPlayer",
            "Image" => "TImage",
            "Material" | "Particle" | "TestTri" | "Texture" | "VideoTexture" | "Object" | "World" | "Engine" => &self.viewGfx,
            _ => "TText",
        }
    }

    fn template(&self, s: &MetaContent) -> Template {
        static IMAGES: AtomicUsize = AtomicUsize::new(0);
        let text = || match &s.value { Tag::Str(z) => z.clone(), z => format!("{z:?}") };
        match self.key(s) {
            "TNull" => Template::TNull,
            "THex" => Template::THex(AppHexWidget::new(&s.name, &s.value)),
            "TImage" => match &s.value {
                Tag::Bytes(z) => Template::TImage(format!("bytes://content/{}", IMAGES.fetch_add(1, Ordering::Relaxed)), z.clone().into(), s.maxWidth as f32, s.maxHeight as f32),
                _ => Template::TText(text()),
            },
            "TAudioPlayer" => Template::TAudioPlayer,
            "TCustom" => Template::TCustom(Custom::default()),
            z if z.starts_with("TViewGfx:") => Template::TViewGfx(z.into()),
            _ => Template::TText(text()),
        }
    }

    pub fn onInfo(&mut self, item: Option<MetaItem>, source: Option<SourceRef>, infos: Option<Vec<MetaInfo>>) {
        self.source = source;
        self.path = item;
        self.setContentTabs(infos.map(|s| s.iter().filter_map(|s| s.tag.asMetaContent().cloned()).collect()));
    }

    pub fn ui(&mut self, ui: &mut egui::Ui) {
        self.ctx.get_or_insert_with(|| ui.ctx().clone());
        let Some(tabs) = &mut self.contentTabs else { return };
        ui.horizontal_wrapped(|ui| {
            for (i, (s, _)) in tabs.iter().enumerate() {
                if ui.selectable_label(self.selectedIndex == i as i32, &s.name).clicked() { self.selectedIndex = i as i32; }
            }
        });
        ui.separator();
        let Some((s, t)) = tabs.get_mut(self.selectedIndex as usize) else { return };
        match t {
            Template::TNull => { ui.label(egui::RichText::new("Null").monospace()); }
            Template::TText(z) => {
                egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| { ui.add(egui::Label::new(egui::RichText::new(z.as_str()).monospace()).wrap()); });
            }
            Template::TImage(uri, z, maxWidth, maxHeight) => {
                let mut image = egui::Image::from_bytes(uri.clone(), z.clone()).shrink_to_fit();
                if *maxWidth > 0. && *maxHeight > 0. { image = image.max_size(egui::vec2(*maxWidth, *maxHeight)); }
                ui.add(image);
            }
            Template::THex(z) => z.ui(ui),
            Template::TAudioPlayer => { ui.label("Audio player not ported to Rust yet."); }
            Template::TViewGfx(z) => { ui.label(format!("{z}: engine view not ported to Rust yet.")); }
            Template::TCustom(z) => z.ui(ui),
        }
    }
}
