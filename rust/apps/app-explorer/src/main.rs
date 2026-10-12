//! `gamex-app-explorer` binary: C#'s WPF entry point, which runs
//! `App.Application_Startup` with the command-line arguments.

fn main() {
    std::process::exit(gamex_app_explorer::app_xaml::App::application_Startup(std::env::args().skip(1).collect()));
}
