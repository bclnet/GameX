// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/StarWords/Views/Navigator.xaml.cs
// PORT-SHA: 0b2b5c1bf4ca6f85
// PORT-STATUS: done
//
// `Nodes` binds to `StarWords.Node`. That type is unported, so the tree starts
// empty. A double-click calls `MainPage.AddContentTab`.

use egui::Ui;

/// The tree row the XAML binds: a name and its children.
#[derive(Clone, Default)]
pub struct Node {
    pub name: String,
    pub items: Vec<Node>,
}

/// Navigator
#[derive(Default)]
pub struct Navigator {
    pub nodes: Vec<Node>,
    selected: Option<String>,
}

impl Navigator {
    /// The double-clicked node's name, when there is one.
    pub fn ui(&mut self, ui: &mut Ui) -> Option<String> {
        let nodes = self.nodes.clone();
        let mut opened = None;
        egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| {
            opened = self.showNodes("starwords", ui, &nodes);
        });
        opened
    }

    fn showNodes(&mut self, salt: &str, ui: &mut Ui, nodes: &[Node]) -> Option<String> {
        let mut opened = None;
        for node in nodes {
            let id = egui::Id::new((salt, &node.name));
            if node.items.is_empty() {
                let response = ui.selectable_label(self.selected.as_deref() == Some(node.name.as_str()), &node.name);
                if response.clicked() { self.selected = Some(node.name.clone()); }
                if response.double_clicked() { opened = Some(node.name.clone()); }
            } else {
                let open = egui::CollapsingHeader::new(&node.name).id_salt(id).default_open(false).show(ui, |ui| {
                    if opened.is_none() { opened = self.showNodes(salt, ui, &node.items); }
                });
                if open.header_response.clicked() { self.selected = node.items.first().map(|s| s.name.clone()); }
                if open.header_response.double_clicked() { opened = Some(node.name.clone()); }
            }
        }
        opened
    }
}
