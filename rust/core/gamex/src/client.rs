// PORT-SOURCE: Core/GameX/Client.cs
// PORT-SHA: 01fcca58fa8ea167
// PORT-STATUS: done
//
// PARTIAL PORT — eight live lines, two of which need types that are not ported.
//
//     public class GameClient(ClientState state) : ClientBase() {
//         public Archive Archive = state.Archive;
//         public object Tag = state.Tag;
//     }
//
// `ClientBase` and `ClientState` come from `OpenStack.Client`, whose Rust
// counterpart is `openstack::client` — ported, and its `ClientHost`/`Scene`
// traits are the shape this plugs into. `Archive` is in `GameX.FileSystems`,
// still outstanding, so the field is left out rather than stubbed.
//
// Note the primary constructor takes `state` and copies two fields out of it,
// discarding the rest — so `GameClient` is a projection of `ClientState`, not a
// holder of it. Worth preserving deliberately when `Archive` lands, because
// keeping the whole `state` instead would change lifetime behaviour.

/// C# `GameClient`'s untyped `Tag`, as the rest of the tree models it.
pub use crate::meta::Tag;

/// C# `GameClient`, minus its `Archive` field.
///
/// Completed once `GameX.FileSystems` is ported; `Archive` is the only missing
/// piece.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct GameClient {
    pub tag: Tag,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn a_default_client_has_no_tag() {
        assert_eq!(GameClient::default().tag, Tag::None);
    }
}
