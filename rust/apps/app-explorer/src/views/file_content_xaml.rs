// PORT-SOURCE: Apps/GameX.App.Explorer/Views/FileContent.xaml.cs
// PORT-SHA: b80edf914dcdc04e
// PORT-STATUS: done
//
// FileContent.xaml's DataTemplates and triggers become `FileContent::key` and
// the `Template` each content tab builds once (Python's `updateTabs`).
// TViewGfx:GL hosts OpenX's OpenGL control (`AppOpenGLControl`) in an egui
// paint callback; the other engines' TViewGfx are placeholders. TAudioPlayer
// is the Play / Pause button.
// TDataGrid is WPF's auto-column DataGrid. Rust has no property reflection,
// so the shape is fixed: a `Tag::Map` is Key/Value rows (a dictionary's
// pairs), and `Tag::Any` of `Vec<IndexMap<String, Tag>>` (indexmap 2, the
// same type as `Tag::Map`) is one row per map, columns in first-seen order.
// A missing key is an empty cell. A `Vec<Tag>` whose elements are not maps
// is one "Value" column, the stand-in for a list of primitives, which have
// no properties. A mixed list takes its columns from the maps and leaves
// the other rows blank. An empty map keeps the two headers; an empty list
// is a blank grid. Anything else falls back to the debug text. Cell text is
// C#'s `ToString` for bool, int and string (unquoted); other tags use the
// debug text. Python still shows this tab as text.
// C#'s Dispose of the old tabs is Drop; images are also dropped from egui's
// cache, which keys them by uri.

use std::any::Any;
use std::sync::atomic::{AtomicUsize, Ordering};
use std::sync::{Arc, Mutex};
use eframe::egui;
use egui_extras::{Column, TableBuilder};
use gamex::meta::{MetaContent, MetaInfo, MetaItem, Tag};
use openx::engine::Engine;
use openx_poly::core::SourceRef;
use openx_wpf_control::open_gl_control::OpenGLControl;
use crate::controls::app_gfx_control::AppOpenGLControl;
use super::app_hex_widget::AppHexWidget;
use super::audio_player_xaml::AudioPlayer;
use super::custom_xaml::Custom;

enum Template {
    TNull,
    TText(String),
    TImage(String, egui::load::Bytes, f32, f32),
    THex(AppHexWidget),
    TAudioPlayer(AudioPlayer),
    /// `<DataGrid ItemsSource="{Binding Value}" />`. Columns come from `dataGrid`.
    TDataGrid,
    TViewGfx(String),
    /// `TViewGfx:GL`: `<controls:AppOpenGLControl Source2="{Binding}" Value="{Binding Value}" Type="{Binding Type}" />`.
    TViewGfxGL(Arc<Mutex<OpenGLControl>>),
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
    pub fn setEngine(&mut self, engine: Option<&Engine>) {
        let plat = engine.map_or("UK", |s| s.id);
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
            "TAudioPlayer" => Template::TAudioPlayer(AudioPlayer::new(self.source.clone(), self.path.as_ref(), &s.value, &s.tag)),
            "TDataGrid" => Template::TDataGrid,
            "TCustom" => Template::TCustom(Custom::default()),
            "TViewGfx:GL" => match &self.source {
                Some(source) => Template::TViewGfxGL(AppOpenGLControl::new(source.clone(), s.value.clone(), s.type_.clone())),
                None => Template::TText(text()),
            },
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
            Template::TAudioPlayer(z) => z.ui(ui),
            Template::TDataGrid => dataGridUi(ui, &s.value),
            Template::TViewGfx(z) => { ui.label(format!("{z}: engine view not ported to Rust yet.")); }
            Template::TViewGfxGL(z) => OpenGLControl::ui(z, ui),
            Template::TCustom(z) => z.ui(ui),
        }
    }
}

/// C# `Boolean.ToString` / `Int64.ToString` / the string itself. Other tags use `Tag`'s debug text.
fn cellText(tag: &Tag) -> String {
    match tag {
        Tag::None => String::new(),
        Tag::Bool(true) => "True".into(),
        Tag::Bool(false) => "False".into(),
        Tag::Int(z) => z.to_string(),
        Tag::Str(z) => z.clone(),
        z => format!("{z:?}"),
    }
}

/// One row per map. Columns are the union of keys in first-seen order.
fn recordGrid(rows: &[indexmap::IndexMap<String, Tag>]) -> (Vec<String>, Vec<Vec<String>>) {
    let mut columns = Vec::new();
    for row in rows {
        for key in row.keys() {
            if !columns.iter().any(|seen| seen == key) { columns.push(key.clone()); }
        }
    }
    let body = rows.iter().map(|row| columns.iter().map(|c| row.get(c).map(cellText).unwrap_or_default()).collect()).collect();
    (columns, body)
}

/// A list of primitives is one Value column. Maps in a mixed list supply the columns.
fn tagListGrid(rows: &[Tag]) -> (Vec<String>, Vec<Vec<String>>) {
    if rows.iter().all(|z| !matches!(z, Tag::Map(_))) {
        let columns = if rows.is_empty() { Vec::new() } else { vec!["Value".into()] };
        return (columns, rows.iter().map(|z| vec![cellText(z)]).collect());
    }
    let mut columns = Vec::new();
    for row in rows {
        let Tag::Map(map) = row else { continue };
        for key in map.keys() {
            if !columns.iter().any(|seen| seen == key) { columns.push(key.clone()); }
        }
    }
    let body = rows.iter().map(|row| match row {
        Tag::Map(map) => columns.iter().map(|c| map.get(c).map(cellText).unwrap_or_default()).collect(),
        _ => vec![String::new(); columns.len()],
    }).collect();
    (columns, body)
}

/// `None` is not a table, so the tab shows the debug text. `Some` is a grid, even when empty.
fn dataGrid(value: &Tag) -> Option<(Vec<String>, Vec<Vec<String>>)> {
    match value {
        Tag::Map(z) => Some((vec!["Key".into(), "Value".into()], z.iter().map(|(k, v)| vec![k.clone(), cellText(v)]).collect())),
        Tag::Any(z) => {
            if let Some(rows) = z.downcast_ref::<Vec<indexmap::IndexMap<String, Tag>>>() { return Some(recordGrid(rows)); }
            z.downcast_ref::<Vec<Tag>>().map(|rows| tagListGrid(rows))
        }
        _ => None,
    }
}

fn dataGridUi(ui: &mut egui::Ui, value: &Tag) {
    let Some((columns, rows)) = dataGrid(value) else {
        let z = match value { Tag::Str(z) => z.clone(), z => format!("{z:?}") };
        egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| { ui.add(egui::Label::new(egui::RichText::new(z).monospace()).wrap()); });
        return;
    };
    // Zero columns: `TableBuilder` logs when a row adds a cell past the preallocated count.
    if columns.is_empty() { return; }
    TableBuilder::new(ui)
        .striped(false)
        .columns(Column::auto().resizable(true), columns.len())
        .header(20.0, |mut header| {
            for name in &columns { header.col(|ui| { ui.label(name); }); }
        })
        .body(|mut body| {
            body.rows(18.0, rows.len(), |mut row| {
                let line = &rows[row.index()];
                for cell in line { row.col(|ui| { ui.label(cell); }); }
            });
        });
}

#[cfg(test)]
mod tests {
    use super::*;
    use indexmap::IndexMap;

    fn row(pairs: &[(&str, Tag)]) -> IndexMap<String, Tag> {
        pairs.iter().map(|(k, v)| ((*k).to_string(), v.clone())).collect()
    }

    #[test]
    fn two_row_vec_of_maps_unions_keys_in_first_seen_order() {
        let rows = vec![
            row(&[("Name", Tag::from("a")), ("Id", Tag::from(1))]),
            row(&[("Name", Tag::from("b")), ("Extra", Tag::None)]),
        ];
        let (columns, body) = dataGrid(&Tag::Any(Arc::new(rows))).unwrap();
        assert_eq!(columns, vec!["Name", "Id", "Extra"]);
        assert_eq!(body, vec![
            vec!["a".to_string(), "1".into(), String::new()],
            vec!["b".into(), String::new(), String::new()],
        ]);
    }

    #[test]
    fn map_is_key_and_value() {
        let mut map = IndexMap::new();
        map.insert("on".into(), Tag::Bool(true));
        map.insert("off".into(), Tag::Bool(false));
        map.insert("name".into(), Tag::from("x"));
        map.insert("n".into(), Tag::from(3));
        map.insert("empty".into(), Tag::None);
        let (columns, body) = dataGrid(&Tag::Map(map)).unwrap();
        assert_eq!(columns, ["Key", "Value"]);
        assert_eq!(body, vec![
            vec!["on".to_string(), "True".into()],
            vec!["off".into(), "False".into()],
            vec!["name".into(), "x".into()],
            vec!["n".into(), "3".into()],
            vec!["empty".into(), String::new()],
        ]);
    }

    #[test]
    fn empty_map_keeps_the_headers() {
        let (columns, body) = dataGrid(&Tag::Map(IndexMap::new())).unwrap();
        assert_eq!(columns, ["Key", "Value"]);
        assert!(body.is_empty());
    }

    #[test]
    fn empty_vec_is_an_empty_grid() {
        let rows: Vec<IndexMap<String, Tag>> = Vec::new();
        assert_eq!(dataGrid(&Tag::Any(Arc::new(rows))), Some((Vec::new(), Vec::new())));
    }

    #[test]
    fn non_map_list_is_one_value_column() {
        let rows = vec![Tag::from(2), Tag::Bool(false), Tag::from("q")];
        let (columns, body) = dataGrid(&Tag::Any(Arc::new(rows))).unwrap();
        assert_eq!(columns, ["Value"]);
        assert_eq!(body, vec![vec!["2".to_string()], vec!["False".into()], vec!["q".into()]]);
    }

    #[test]
    fn mixed_list_takes_columns_from_maps_and_blanks_the_rest() {
        let rows = vec![Tag::Map(row(&[("Name", Tag::from("a"))])), Tag::from(7)];
        let (columns, body) = dataGrid(&Tag::Any(Arc::new(rows))).unwrap();
        assert_eq!(columns, ["Name"]);
        assert_eq!(body, vec![vec!["a".to_string()], vec![String::new()]]);
    }

    #[test]
    fn wrong_any_and_a_string_are_not_a_grid() {
        assert!(dataGrid(&Tag::Any(Arc::new(1i64))).is_none());
        assert!(dataGrid(&Tag::from("hi")).is_none());
    }
}
