// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/DataForge/App.xaml.cs
// PORT-SHA: 03b022a3a424a19c
// PORT-STATUS: done
//
// C# casts `sender` to `DataForgeApp` and shows `MainPage`. `appType` still
// builds the base `FamilyApp`, and `Database` is unported, so the navigator
// has no nodes. C# passes null for `StartupEventArgs`.

use std::io;
use gamex::family::{registerType, AppStartup, FamilyApp};
use gamex::meta::MetaManager;

pub fn register() {
    registerType::<AppStartup>("GameX.Cig.Apps.DataForge.App, GameX.Cig.Explorer", application_Startup);
}

fn application_Startup(_app: &FamilyApp, _manager: Option<&dyn MetaManager>) -> io::Result<()> {
    super::views::main_page_xaml::showNew();
    Ok(())
}
