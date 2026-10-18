// PORT-SOURCE: Families/GameX.Origin.UO/Client.cs
// PORT-SHA: pending
// PORT-STATUS: partial
// WORKSPACE: not added yet
//
// Not ported: typed Assets / ClientVersion fields, and the call
// `GameX.Origin.Clients.UO.Assets.Load`. This crate is not a workspace member
// and does not depend on gamex-origin. The playable client is C# only:
// `UOGameClient.LoadContent` loads the install from `UOGame.Found.Root` and
// calls `MgClientHost.SetScene` with `LoginScene`. This shell is not that
// bootstrap. Hue sampler 512x1024, lights sampler 32x63, and the renderer
// objects are still not here (GameX.Engine.Mg).

/// ClassicUO `UltimaOnline` asset slot. Does not load files or build Texture2D objects.
/// Not the process entry. Renderer home later is GameX.Engine.Mg, not FNA.
#[derive(Clone, Default)]
pub struct UltimaOnline {
    /// Install directory. C# sets this from `FamilyGame.Found.Root`.
    pub clientPath: Option<String>,
    // assets: GameX.Origin.Clients.UO.Assets. ClassicUO's FileManager slot. Caller sets it in C#.
    // version: ClientVersion. UOGame.Version is internal. Not stored here.
    // Animations, Arts, Gumps, Texmaps, Lights, MultiMaps, Sounds:
    //   renderer objects. Next slice, GameX.Engine.Mg. Not FNA Texture2D.
    // World: C# login scene holds the entity lists only. GameCursor is not in yet.
    // Protocol: ClientFlags is internal.
}

impl UltimaOnline {
    /// ClassicUO folders and the GameX home for each. Not a port of those trees.
    pub const Map: [&'static str; 4] = [
        "ClassicUO.Assets and ClassicUO.IO -> stay in GameX.Origin formats (already the resource parsers). Do not re-port them here.",
        "ClassicUO.Client Game/ scenes and UI -> GameX.Origin.UO. LoginScene is the first scene. The gumps are not in yet.",
        "ClassicUO.Renderer -> GameX.Engine.Mg, later.",
        "ClassicUO.Utility -> only if a later slice needs a specific helper; not copied now.",
    ];

    /// C# signature is `Load(Archive archive, UOGameClient ctx)`.
    /// This does not call `Assets.Load`. See the header.
    pub fn load(&mut self) {}
}
