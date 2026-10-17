// PORT-SOURCE: Families/GameX.Cig.Explorer/Apps/Subsumption/Views/Missions/Navigator.xaml.cs
// PORT-SHA: 1e372be78e64fd67
// PORT-STATUS: done
//
// `NavigatorItem` is the tree row. `SelectedItem`'s body is commented out.
// A row with children selects its first child, as `OnNodeSelected` does.

use egui::Ui;

/// NavigatorItem
#[derive(Clone, Default)]
pub struct NavigatorItem {
    pub name: String,
    pub items: Vec<NavigatorItem>,
}

/// Navigator
#[derive(Default)]
pub struct Navigator {
    pub nodes: Vec<NavigatorItem>,
    selected: Option<String>,
}

impl Navigator {
    pub fn ui(&mut self, ui: &mut Ui) {
        let nodes = self.nodes.clone();
        egui::ScrollArea::vertical().auto_shrink(false).show(ui, |ui| {
            self.showNodes(ui, &nodes);
        });
    }

    fn showNodes(&mut self, ui: &mut Ui, nodes: &[NavigatorItem]) {
        for node in nodes {
            if node.items.is_empty() {
                if ui.selectable_label(self.selected.as_deref() == Some(node.name.as_str()), &node.name).clicked() {
                    self.selected = Some(node.name.clone());
                }
            } else {
                let open = egui::CollapsingHeader::new(&node.name).id_salt(&node.name).default_open(false).show(ui, |ui| {
                    self.showNodes(ui, &node.items);
                });
                if open.header_response.clicked() { self.selected = node.items.first().map(|s| s.name.clone()); }
            }
        }
    }
}
