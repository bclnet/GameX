use clap::Parser;

/// Simple program to greet a person or run a task
#[derive(Parser, Debug)]
#[command(author, version, about, long_about = None)]
struct Args {
    /// Name of the person to greet (Positional argument)
    name: String,

    /// Number of times to greet (Optional named argument)
    #[arg(short, long, default_value_t = 1)]
    count: u8,

    /// Activates verbose mode (Boolean flag)
    #[arg(short, long)]
    verbose: bool,
}

fn main() {
    // This line automatically parses std::env::args_os()
    let args = Args::parse();

    if args.verbose {
        println!("Verbose mode is ON!");
    }

    for _ in 0..args.count {
        println!("Hello, {}!", args.name);
    }
}


// cargo run -- Alice
// cargo run -- Bob --count 3
// cargo run -- Charlie -c 2 -v
// cargo run -- --help