// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/Subsumption/App.xaml.cs
// PORT-SHA: 743da870e9a9f03d
// PORT-STATUS: done
//
// C# casts `sender` to `SubsumptionApp` and shows `MainPage`. `SubsumptionApp`
// only calls the base `OpenAsync`. C# passes null for `StartupEventArgs`.

use std::io;
use gamex::family::{registerType, AppStartup, FamilyApp};
use gamex::meta::MetaManager;

pub fn register() {
    registerType::<AppStartup>("GameX.Cig.Apps.Subsumption.App, GameX.Cig.Explorer", application_Startup);
}

fn application_Startup(_app: &FamilyApp, _manager: Option<&dyn MetaManager>) -> io::Result<()> {
    super::views::main_page_xaml::showNew();
    Ok(())
}
