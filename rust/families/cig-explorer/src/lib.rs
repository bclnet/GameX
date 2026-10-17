//! `gamex-cig-explorer` — 1:1 port of .NET project `GameX.Cig.Explorer`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod apps;
pub mod resource_manager_provider;

/// Registers DataForge, StarWords and Subsumption `Application_Startup`.
pub fn register() {
    apps::data_forge::app_xaml::register();
    apps::star_words::app_xaml::register();
    apps::subsumption::app_xaml::register();
}

/// Draws every family-app window opened since the last frame.
pub fn show(ctx: &egui::Context) {
    apps::data_forge::views::main_page_xaml::show(ctx);
    apps::star_words::views::main_page_xaml::show(ctx);
    apps::subsumption::views::main_page_xaml::show(ctx);
}

/// C# `Window.Show`. An egui window in the explorer, the same host as About.
/// `false` means the user closed it.
pub(crate) fn showWindow(ctx: &egui::Context, salt: &str, n: u64, title: &str, add: impl FnOnce(&mut egui::Ui)) -> bool {
    let mut open = true;
    egui::Window::new(title).id(egui::Id::new((salt, n))).open(&mut open).collapsible(false).default_size([1342.0, 798.0]).show(ctx, |ui| add(ui));
    open
}

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::family::FamilyApp;

    #[test]
    fn openAsyncFindsTheThreeApps() {
        register();
        apps::data_forge::views::main_page_xaml::clear();
        apps::star_words::views::main_page_xaml::clear();
        apps::subsumption::views::main_page_xaml::clear();
        let open = |name, typeName| {
            let app = FamilyApp::new(name, &serde_json::json!({ "name": name, "explorerAppType": typeName })).unwrap();
            app.openAsync(app.explorerType.as_deref().unwrap(), None).unwrap();
        };
        open("DataForge", "GameX.Cig.Apps.DataForge.App, GameX.Cig.Explorer");
        open("StarWords", "GameX.Cig.Apps.StarWords.App, GameX.Cig.Explorer");
        open("Subsumption", "GameX.Cig.Apps.Subsumption.App, GameX.Cig.Explorer");
        assert_eq!(apps::data_forge::views::main_page_xaml::openCount(), 1);
        assert_eq!(apps::star_words::views::main_page_xaml::openCount(), 1);
        assert_eq!(apps::subsumption::views::main_page_xaml::openCount(), 1);
        let missing = FamilyApp::new("x", &serde_json::json!({ "explorerAppType": "Missing.App" })).unwrap();
        let err = missing.openAsync("Missing.App", None).unwrap_err();
        assert!(err.to_string().contains("No Application_Startup"), "{err}");
        apps::data_forge::views::main_page_xaml::clear();
        apps::star_words::views::main_page_xaml::clear();
        apps::subsumption::views::main_page_xaml::clear();
    }
}
