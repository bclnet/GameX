// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Gfx/OpenGLOpenEngine.cs
// PORT-SHA: 09f8f2907f4bee3a
// PORT-STATUS: partial
//
// `CellManager` is the one in `openx::manager`. It does not take an
// `AsyncCoroutineQueue`: `updateCells` is synchronous, `stepCells` is that
// queue's `Run` for this workload, and `finishCell` is `Queue.WaitFor`.
// `DesiredWorkTimePerFrame` has nothing to bound, because a step is one call
// per cell rather than a timed slice. The generic cell coroutine is not
// ported (`OpenGLCellBuilder::step`), so a begun cell is empty containers.
// `CellChanged` has no subscribers. `CreatePlayer` is empty in the C#.

use std::sync::Arc;
use openx::manager::{CellDatabase, CellManager, Query, SharedQuery};
use openx_gfx_egin::egin_render::Camera;
use crate::engine_open_gl::OpenGLBackend;
use crate::gfx::open_gl::OpenGLCellBuilder;

/// Owns the shared query `CellDatabase::query` returns and implements `Query`
/// by locking it. `CellManager` keeps this, not the `Arc` itself.
pub struct QueryHandle(pub SharedQuery);

impl Query for QueryHandle {
    fn meterInUnits(&self) -> f32 { self.0.lock().unwrap().meterInUnits() }
    fn cellLengthInMeters(&self) -> f32 { self.0.lock().unwrap().cellLengthInMeters() }
    fn loadRadius(&self) -> i32 { self.0.lock().unwrap().loadRadius() }
    fn visibleRadius(&self) -> i32 { self.0.lock().unwrap().visibleRadius() }
    fn world(&self) -> i32 { self.0.lock().unwrap().world() }
    fn setWorld(&mut self, world: i32) { self.0.lock().unwrap().setWorld(world); }
    fn getCellId(&self, point: glam::Vec3) -> openx_poly::poly::Int3 { self.0.lock().unwrap().getCellId(point) }
    fn findCell(&self, cell: openx_poly::poly::Int3) -> Option<Box<dyn openx::manager::Cell>> { self.0.lock().unwrap().findCell(cell) }
    fn findCellByName(&self, name: &str) -> Option<Box<dyn openx::manager::Cell>> { self.0.lock().unwrap().findCellByName(name) }
    fn findLand(&self, cell: openx_poly::poly::Int3) -> Option<Box<dyn openx::manager::Land>> { self.0.lock().unwrap().findLand(cell) }
    fn findLtex(&self, index: i32) -> Option<Box<dyn openx::manager::Ltex>> { self.0.lock().unwrap().findLtex(index) }
}

/// OpenGLOpenEngine
pub struct OpenGLOpenEngine {
    cells: CellManager<OpenGLBackend, QueryHandle, OpenGLCellBuilder>,
    pub camera: Option<Camera>,
    /// `None` is C# `_cell == null`. `Some` is `ICell.IsInterior`.
    cellInterior: Option<bool>,
}

impl OpenGLOpenEngine {
    /// C# `OpenGLOpenEngine(Func<AsyncCoroutineQueue, CellManager>, bool)`.
    ///
    /// The function existed to hand the queue to `CellManager`. There is no
    /// queue, so the manager is built here. `sunCycle` is unused in the C#.
    pub fn new(query: SharedQuery, builder: OpenGLCellBuilder, _sunCycle: bool) -> Self {
        Self { cells: CellManager::new(QueryHandle(query), builder), camera: None, cellInterior: None }
    }

    pub fn update(&mut self) {
        // The current cell can be null if the player is outside of the defined game world.
        let outside = self.cellInterior != Some(true);
        if let Some(camera) = &self.camera {
            if outside {
                let position = camera.location;
                self.cells.updateCells(position, false, None);
            }
        }
        self.cells.stepCells();
    }

    /// C# `SpawnPlayer`. `Debug.Assert(Cell != null)` is `debug_assert`.
    pub fn spawnPlayer(&mut self, db: &dyn CellDatabase) {
        let found = self.cells.query().findCell(db.cellId());
        debug_assert!(found.is_some());
        self.cellInterior = found.as_ref().map(|c| c.isInterior());
        // CreatePlayer is empty.
        let id = db.cellId();
        self.cells.beginCell(id);
        self.cells.finishCell(id);
    }
}

#[cfg(test)]
mod tests {
    use std::io::{Error, ErrorKind};
    use std::sync::{Arc, Mutex};
    use glam::Vec3;
    use openx::manager::{Cell, CellDatabase, CellXref, Land, Ltex, Query};
    use openx_gfx_egin::egin_render::Camera;
    use openx_poly::core::{BoxFuture, ISource, SourcePath, SourceRef};
    use openx_poly::poly::Int3;
    use super::*;

    struct NoSource;
    impl ISource for NoSource {
        fn getAssetAny<'a>(&'a self, _path: &'a SourcePath, _option: Option<&'a (dyn std::any::Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn std::any::Any + Send + Sync>, Error>> {
            Box::pin(async { Err(Error::new(ErrorKind::Other, "no asset")) })
        }
    }

    struct TestCell;
    impl Cell for TestCell {
        fn id(&self) -> u32 { 1 }
        fn isInterior(&self) -> bool { false }
        fn gridId(&self) -> Int3 { Int3::ZERO }
        fn name(&self) -> &str { "test" }
        fn ambientLight(&self) -> Option<u32> { None }
        fn xrefs(&self) -> &[Box<dyn CellXref>] { &[] }
    }

    struct TestQuery;
    impl Query for TestQuery {
        fn meterInUnits(&self) -> f32 { 1. }
        fn cellLengthInMeters(&self) -> f32 { 1. }
        fn loadRadius(&self) -> i32 { 0 }
        fn visibleRadius(&self) -> i32 { 0 }
        fn world(&self) -> i32 { 0 }
        fn setWorld(&mut self, _world: i32) {}
        fn getCellId(&self, _point: Vec3) -> Int3 { Int3::ZERO }
        fn findCell(&self, _cell: Int3) -> Option<Box<dyn Cell>> { Some(Box::new(TestCell)) }
        fn findCellByName(&self, _name: &str) -> Option<Box<dyn Cell>> { None }
        fn findLand(&self, _cell: Int3) -> Option<Box<dyn Land>> { None }
        fn findLtex(&self, _index: i32) -> Option<Box<dyn Ltex>> { None }
    }

    struct TestDb { query: SharedQuery }
    impl CellDatabase for TestDb {
        fn archive(&self) -> SourceRef { SourceRef::new(NoSource) }
        fn query(&self) -> SharedQuery { self.query.clone() }
        fn cellId(&self) -> Int3 { Int3::ZERO }
        fn cellInterior(&self) -> bool { false }
        fn playerPosition(&self) -> Vec3 { Vec3::ZERO }
        fn playerRotation(&self) -> glam::Quat { glam::Quat::IDENTITY }
    }

    #[test]
    fn spawn_player_loads_one_cell_and_update_keeps_it() {
        let query: SharedQuery = Arc::new(Mutex::new(Box::new(TestQuery)));
        let db = TestDb { query: query.clone() };
        let api = Arc::new(Mutex::new(crate::engine_open_gl::OpenGLGfxApi));
        let mut engine = OpenGLOpenEngine::new(query, OpenGLCellBuilder::new(db.archive(), api), false);
        engine.spawnPlayer(&db);
        assert_eq!(engine.cells.loadedLen(), 1);
        engine.camera = Some(Camera::default());
        engine.update();
        assert_eq!(engine.cells.loadedLen(), 1);
        assert!(engine.cellInterior == Some(false));
    }
}
