// PORT-SOURCE: Apps/GameX.App.Explorer/Views/ViewExtensions.cs
// PORT-SHA: d9099898c58f17fd
// PORT-STATUS: done
//
// The C# extends WPF's TextBox; egui has no retained text box, so this extends
// the String the log draws from. ScrollToEnd is the log's stick-to-bottom
// scroll area in main_page_xaml.rs.

pub trait ViewExtensions {
    fn writeLine(&mut self, line: &str);
}

impl ViewExtensions for String {
    fn writeLine(&mut self, line: &str) {
        if !self.is_empty() { self.push('\n'); }
        self.push_str(line);
    }
}
