// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/DataForge/Views/Navigator.xaml.cs
// PORT-SHA: 46d80511278f9616
// PORT-STATUS: done
//
// `Nodes` binds to `Database.Node`. That type is unported, so the tree starts
// empty. `OnNodeDoubleClick` is commented out in C#.

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
    pub fn ui(&mut self, ui: &mut Ui) {
        let nodes = self.nodes.clone();
        egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| {
            self.showNodes("dataforge", ui, &nodes);
        });
    }

    fn showNodes(&mut self, salt: &str, ui: &mut Ui, nodes: &[Node]) {
        for node in nodes {
            let id = egui::Id::new((salt, &node.name));
            if node.items.is_empty() {
                if ui.selectable_label(self.selected.as_deref() == Some(node.name.as_str()), &node.name).clicked() {
                    self.selected = Some(node.name.clone());
                }
            } else {
                let open = egui::CollapsingHeader::new(&node.name).id_salt(id).default_open(false).show(ui, |ui| {
                    self.showNodes(salt, ui, &node.items);
                });
                // C# selects the first child when the clicked row has children.
                if open.header_response.clicked() { self.selected = node.items.first().map(|s| s.name.clone()); }
            }
        }
    }
}
