// PORT-SOURCE: Apps/GameX.App.Explorer/Resources/ResourceManager.cs
// PORT-SHA: ba917bf90dd15e91
// PORT-STATUS: done
//
// C# embeds python/gamex/resources/Icons/*.png in GameX.Resource; this reads
// the same folder at startup, as Python's resourcemgr.py does. An icon is the
// egui image uri (`bytes://icons/{name}.png`) whose bytes `includeIcons`
// hands to egui. GetImage loads WPF pack resources, which the Rust Explorer
// doesn't have.

use std::collections::HashMap;
use std::sync::OnceLock;
use eframe::egui;
use gamex::meta::{MetaManager, Tag};

pub struct ResourceManager {
    icons: HashMap<String, (String, Vec<u8>)>,
    _defaultIcon: Tag,
    _folderIcon: Tag,
    _packageIcon: Tag,
}

impl ResourceManager {
    pub fn current() -> &'static ResourceManager {
        static CURRENT: OnceLock<ResourceManager> = OnceLock::new();
        CURRENT.get_or_init(ResourceManager::new)
    }

    fn new() -> Self {
        let icons = Self::loadIcons();
        let icon = |name: &str| icons.get(name).map_or(Tag::None, |(uri, _)| Tag::Str(uri.clone()));
        let (_defaultIcon, _folderIcon, _packageIcon) = (icon("_default"), icon("_folder"), icon("_package"));
        Self { icons, _defaultIcon, _folderIcon, _packageIcon }
    }

    fn loadIcons() -> HashMap<String, (String, Vec<u8>)> {
        let dir = concat!(env!("CARGO_MANIFEST_DIR"), "/../../../python/gamex/resources/Icons");
        std::fs::read_dir(dir).into_iter().flatten().flatten().filter_map(|s| {
            let path = s.path();
            let name = path.file_stem()?.to_str()?.to_string();
            Some((name.clone(), (format!("bytes://icons/{name}.png"), std::fs::read(&path).ok()?)))
        }).collect()
    }

    /// Hands the icons' bytes to egui so their uris resolve.
    pub fn includeIcons(&self, ctx: &egui::Context) {
        for (uri, bytes) in self.icons.values() { ctx.include_bytes(uri.clone(), bytes.clone()); }
    }
}

impl MetaManager for ResourceManager {
    fn folderIcon(&self) -> Tag { self._folderIcon.clone() }

    fn packageIcon(&self) -> Tag { self._packageIcon.clone() }

    fn getIcon(&self, name: &str) -> Tag { self.icons.get(&name.to_lowercase()).map_or_else(|| self._defaultIcon.clone(), |(uri, _)| Tag::Str(uri.clone())) }

    fn getImage(&self, name: &str) -> Tag { Tag::None }
}
