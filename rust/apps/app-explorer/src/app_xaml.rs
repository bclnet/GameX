// PORT-SOURCE: Apps/GameX.App.Explorer/App.xaml.cs
// PORT-SHA: b38c85989607c81b
// PORT-STATUS: done
//
// CommandLineParser's verbs are parsed by hand: `default`, `test` and
// `open -f <family> -u <uri> [-p <path>]`.
//
// LoadFamilies and GetFamily errors, which the C# throws, go to RunError.

use openx::platform::{Platform, UnknownPlatform};
use openx_engine_opengl::engine_open_gl::OpenGLEngine;
use gamex::{config::option, family::{getFamily, loadFamilies}};
use crate::app_shell_xaml::AppShell;

/// App
pub struct App;

impl App {
    /// C# `EngineX.Engines.UnionWith([GodotShellEngine.This, ...])`, with EngineX's own
    /// UnknownEngine. Of those engines only OpenGL has a Rust port.
    pub fn engines() -> Vec<Box<dyn Platform>> { vec![Box::new(UnknownPlatform), Box::new(OpenGLEngine::This)] }

    const args: Option<&'static [&'static str]> = Some(&[]);
    //const args: Option<&'static [&'static str]> = Some(&["open", "-f", "Arkane", "-u", "game:/readme.txt#AF"]);
    //const args: Option<&'static [&'static str]> = Some(&["open", "-f", "WB", "-u", "game:/client_portal.dat#AC", "-p", "01000001.obj"]);
    //const args: Option<&'static [&'static str]> = Some(&["open", "-f", "Red", "-u", "game:/basegame_2_mainmenu.archive#CP77"]);
    //const args: Option<&'static [&'static str]> = Some(&["open", "-f", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "materials/models/npc_minions/siege1_color_psd_12a9c12b.vtex_c"]);

    pub fn application_Startup(e: Vec<String>) -> i32 {
        // C# finds archive types by reflection; Rust's family crates register theirs.
        gamex_uncore::uncore::register();
        gamex_arkane::arkane::register();
        if let Err(e) = loadFamilies(option().family.map(|s| vec![s.to_string()]), true) { return Self::runError(e.to_string()); }
        let shell = AppShell::new();
        let args = Self::args.map(|s| s.iter().map(|s| s.to_string()).collect()).unwrap_or(e);
        match args.first().map(String::as_str) {
            None | Some("default") => Self::runDefault(&shell),
            Some("test") => Self::runTest(&shell),
            Some("open") => match OpenOptions::parse(&args[1..]) {
                Ok(opts) => Self::runOpen(&shell, opts),
                Err(e) => Self::runError(e),
            },
            Some(verb) => Self::runError(format!("Verb '{verb}' is not recognized.")),
        }
    }

    fn runDefault(shell: &AppShell) -> i32 { shell.startup() }

    fn runTest(shell: &AppShell) -> i32 { shell.startup() }

    fn runOpen(shell: &AppShell, opts: OpenOptions) -> i32 {
        match getFamily(&opts.family, true) {
            Ok(family) => shell.startupOpen(family, vec![opts.uri], opts.path),
            Err(e) => Self::runError(e.to_string()),
        }
    }

    fn runError(errs: String) -> i32 {
        rfd::MessageDialog::new().set_description(format!("Errors: \n\n{errs}")).show();
        1
    }
}

// Options

/// Extract files contents to folder.
struct OpenOptions {
    /// Family
    family: String,
    /// Arc file to be opened
    uri: String,
    /// optional file to be opened
    path: Option<String>,
}

impl OpenOptions {
    fn parse(args: &[String]) -> Result<Self, String> {
        let (mut family, mut uri, mut path) = (None, None, None);
        let mut it = args.iter();
        while let Some(s) = it.next() {
            let value = it.next().cloned();
            match s.as_str() {
                "-f" | "--family" => family = value,
                "-u" | "--uri" => uri = value,
                "-p" | "--path" => path = value,
                _ => return Err(format!("Option '{s}' is unknown.")),
            }
        }
        Ok(Self { family: family.ok_or("Required option 'f, family' is missing.")?, uri: uri.ok_or("Required option 'u, uri' is missing.")?, path })
    }
}
