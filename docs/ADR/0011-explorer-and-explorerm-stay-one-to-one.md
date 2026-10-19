ADR 0011: Explorer and ExplorerM stay one-to-one
===

* **Status:** Accepted
* **Date:** 2026-10-19

## Context

`GameX.App.Explorer` is the WPF explorer. `GameX.App.ExplorerM` is the MAUI explorer. They are the same app: open a family, pick an archive tab, filter the file tree, and open an entry. [ADR 0001](0001-ports-stay-one-to-one.md) keeps C#, Python, and Rust one-to-one. ExplorerM has no Python or Rust twin. Its twin is Explorer.

The views are file for file. Explorer also has `Custom.xaml`, which ExplorerM does not.

A change that lands in only one of them is the same drift ADR 0001 forbids between languages. The MAUI tree reconnect on 2026-10-19 is the case that prompted this record. It was compared with Explorer and left in place. The differences are below.

## Decision

Explorer is the reference. A behaviour change in one explorer is made in the other in the same change.

1. **Same views, same behaviour.** Tabs, the three content templates, archive load, filter, file tree, info tree, and app launch follow Explorer.
2. **Nearest toolkit equivalent, named at the site.** Where the toolkit cannot say it the WPF way, use the closest MAUI equivalent and say so. Do not add a second mechanism beside that equivalent.
3. **Known stand-ins.** `Window` is `ContentPage`. `TabControl` is the radio group. `DependencyProperty` is `BindableProperty`. `DataContext` is `BindingContext`. `{Binding Content.Archive, RelativeSource={RelativeSource TemplatedParent}}` is `{TemplateBinding BindingContext.Archive}`. `SelectedIndex = 0` is checking the first radio and setting the content `BindingContext`. `HierarchicalDataTemplate` on `MetaItem` / `MetaInfo` is the existing `TreeView.Maui` converters. `ComboBox` `KeyUp` has no `Picker` equivalent.
4. **Say what was updated.** Each change says Explorer, ExplorerM, or not applicable.

## Check, 2026-10-19

The MAUI tree reconnect was not edited again after this comparison.

| Explorer | ExplorerM after the reconnect |
| -- | -- |
| `Archive` is a dependency property. The property-changed callback loads filters, nodes, and `Ready`. | Same shape on a bindable property. This replaced a separate field whose setter loaded the tree and whose binding did not. |
| Content style default is `TAppList`. `AppList` null selects the file explorer. `Archive` null selects the text block. The app button calls `App_Click`. | The style and `App_Click` now match. Before this edit the default was the file explorer and the app button had no click. |
| `OnOpenedAsync` sets `MainTabControl.SelectedIndex = 0`. | `MainTabs` property-changed calls `ShowTab` on the first tab, then checks the first radio on the dispatcher. `MainTab_Changed` ignores the uncheck. |
| The archive binding is the only assignment. | `ShowTab` also assigns the control template. `ApplyArchive` walks visual descendants and sets `Archive`. `PullArchive` walks parents on `Loaded` and sets `Archive`. |
| Four rows: label 25, editable combo 22.5, file tree `.7*`, info tree `.3*`. No text colours. The trees scroll themselves. Info rows start expanded. The file row shows the icon. | Three rows: `Auto` label and picker, file tree `.7*`, info tree `.3*`. Was `25`, `.8*`, `.2*`, and the info scroller had no row. File names are black on light gray. Info names are white on dark grey. Each tree is a `VerticalStackLayout` with `VerticalOptions` `Start`. Info rows are not forced open. The icon stays commented out. |
| `HierarchicalDataTemplate` binds `MetaItem` / `MetaInfo` `Name`. | Converters wrap those as `TreeViewNode` and now return a list. The info label binds `TreeViewNode.Name`. It had been typed as `MetaInfo`, which is the WPF type, on a row whose context is `TreeViewNode`. |
| `Open` activates the selected engine, then opens. The method is `SetEngine`. | `Open` does not activate the engine. The method is `SetPlatform`. This was already true before the reconnect. |

## Consequences

* A fix in one explorer is checked against the other before it is called done.
* Toolkit limits stay as the one stand-in in the table above. A visual-tree walk on top of the template binding is a second path, and this record says not to add another.
* The 2026-10-19 reconnect stays as it is until a later edit aligns it.
