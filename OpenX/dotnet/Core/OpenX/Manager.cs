using OpenX.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static OpenX.CellManager;

namespace OpenX;

#region Database

public interface IDatabase {
    object Convert(object src);
    object Query(object src);
}

public interface ICellDatabase {
    ISource Archive { get; }
    IQuery Query { get; }
    Int3 CellId { get; }
    bool CellInterior { get; }
    Vector3 PlayerPosition { get; }
    Quaternion PlayerRotation { get; }
}

#endregion

#region CellManager

public class CellManager(IQuery query, AsyncCoroutineQueue queue, CellBuilder builder) {
    public class Cell(object cellObj, object objectsObj, ICell record, IAsyncEnumerator<object> task) {
        public object Obj = cellObj;
        public object ObjectsObj = objectsObj;
        public ICell Record = record;
        public IAsyncEnumerator<object> Task = task;
    }

    public class CellRef {
        public ICellXref Obj;
        public object Record;
        public string ModelPath;
    }

    public interface ICellXref {
        string Name { get; }
        float? Scale { get; }
        Vector3 Position { get; }
        Vector3 EulerAngles { get; }
    }

    public interface ICellXrefModel {
        string ModelPath { get; }
    }

    public interface ICell {
        uint Id { get; }
        bool IsInterior { get; }
        Int3 GridId { get; }
        string Name { get; }
        Color? AmbientLight { get; }
        List<ICellXref> Xrefs { get; }
    }

    public interface ILand {
        Int3 GridId { get; }
        uint[] Vtex { get; }
        float HeightOffset { get; }
        sbyte[] Heights { get; }
    }

    public interface ILtex {
        long Intv { get; }
        string Path { get; }
    }

    public interface ILigh {
        float Radius { get; }
        Color LightColor { get; }
    }

    public interface IQueryFunc {
        IQuery GetQuery();
    }

    public interface IQuery {
        float MeterInUnits { get; }
        float CellLengthInMeters { get; }
        int[] Radius { get; }
        int World { get; }
        void SetWorld(int world);
        Int3 GetCellId(Vector3 point);
        object FindAnyByName(string name);
        ICell FindCell(Int3 cell);
        ICell FindCellByName(string name);
        ILand FindLand(Int3 cell);
        ILtex FindLtex(int index);
        CellBuilder CreateCellBuilder<O, M, T, S>(ISource source, IOpenGfx[] gfx) => new CellBuilder<O, M, T, S>(source, this, gfx);
    }

    public readonly IQuery Query = query;
    protected readonly AsyncCoroutineQueue Queue = queue;
    protected readonly CellBuilder Builder = builder;
    readonly Dictionary<Int3, Cell> Cells = [];
    readonly int Radius = query.Radius[0];
    readonly int Radius2 = query.Radius[1];

    public Cell BeginCell(Int3 point) {
        var record = Query.FindCell(point);
        if (record == null) return null;
        var cell = BuildCell(record); Cells[point] = cell;
        return cell;
    }

    public Cell BeginCellByName(string name) {
        var record = Query.FindCellByName(name);
        if (record == null) return null;
        var cell = BuildCell(record); Cells[Int3.Zero] = cell;
        return cell;
    }

    public async Task UpdateCells(Vector3 position, bool immediate = false, int radius = -1) {
        if (radius < 0) radius = Radius;
        var point = Query.GetCellId(position);
        int minX = point.X - radius, maxX = point.X + radius, minY = point.Y - radius, maxY = point.Y + radius;

        // destroy out of range cells
        var outOfRange = new List<Int3>();
        foreach (var s in Cells.Keys)
            if (s.X < minX || s.X > maxX || s.Y < minY || s.Y > maxY) outOfRange.Add(s);
        foreach (var s in outOfRange) DestroyCell(s);

        // create new cells
        var world = Query.World;
        for (var r = 0; r <= radius; r++)
            for (var x = minX; x <= maxX; x++)
                for (var y = minY; y <= maxY; y++) {
                    var p = new Int3(x, y, world); var d = Math.Max(Math.Abs(point.X - p.X), Math.Abs(point.Y - p.Y));
                    if (d == r && !Cells.ContainsKey(p)) {
                        var cell = BeginCell(p);
                        if (cell != null && immediate) await Queue.WaitFor(cell.Task);
                    }
                }

        // update LODs
        foreach (var (p, cell) in Cells) { var d = Math.Max(Math.Abs(point.X - p.X), Math.Abs(point.Y - p.Y)); Builder.SetVisible(cell.ObjectsObj, d <= Radius2); }
    }

    Cell BuildCell(ICell cell) {
        Debug.Assert(cell != null);
        string cellName;
        ILand land;
        if (!cell.IsInterior) { cellName = $"cell {cell.GridId}"; land = Query.FindLand(cell.GridId); }
        else { cellName = $"cell {cell.Name}"; land = null; }
        var (obj, objectsObj) = Builder.CreateContainers(cellName);
        var task = Builder.Coroutine(cell, land, obj, objectsObj); Queue.Add(task);
        return new Cell(obj, objectsObj, cell, task);
    }

    void DestroyCell(Int3 point) {
        if (Cells.TryGetValue(point, out var s)) { Queue.Cancel(s.Task); Builder.Destroy(s.Obj); Cells.Remove(point); }
        else Log.Error("Tried to destroy a cell that is not created.");
    }

    public void DestroyAllCells() {
        foreach (var s in Cells.Values) { Queue.Cancel(s.Task); Builder.Destroy(s.Obj); }
        Cells.Clear();
    }
}

public abstract class CellBuilder {
    public abstract (object, object) CreateContainers(string name);
    public abstract void SetVisible(object src, bool visible);
    public abstract IAsyncEnumerator<object> Coroutine(ICell cell, ILand land, object obj, object objectsObj);
    public abstract void Destroy(object src);
}

public class CellBuilder<Object, Material, Texture, Shader>(ISource source, IQuery query, IOpenGfx[] gfx) : CellBuilder {
    protected ISource Source = source;
    protected IQuery Query = query;
    protected IOpenGfxApi<Object, Material> GfxApi = (IOpenGfxApi<Object, Material>)gfx[GfX.XApi];
    protected IOpenGfxModel<Object, Material, Texture, Shader> GfxModel = (IOpenGfxModel<Object, Material, Texture, Shader>)gfx[GfX.XModel];
    protected IOpenGfxLight<Object> GfxLight = (IOpenGfxLight<Object>)gfx[GfX.XLight];
    protected IOpenGfxTerrain<Object, Material, Texture> GfxTerrain = (IOpenGfxTerrain<Object, Material, Texture>)gfx[GfX.XTerrain];
    protected float MeterInUnits = query.MeterInUnits;
    protected float CellLengthInMeters = query.CellLengthInMeters;

    public override (object, object) CreateContainers(string name) {
        var obj = GfxApi.CreateObject(name, "Cell");
        var objectsObj = GfxApi.CreateObject("objects", parent: obj);
        return (obj, objectsObj);
    }

    public override void SetVisible(object src, bool visible) => GfxApi.SetVisible((Object)src, visible);
    public override void Destroy(object src) => GfxApi.Destroy((Object)src);

    /// <summary>
    /// A coroutine that instantiates the terrain for, and all objects in, a cell.
    /// </summary>
    public async override IAsyncEnumerator<object> Coroutine(ICell cell, ILand land, object obj, object objectsObj) {
        if (cell == null && land == null) yield break;
        Object obj2 = (Object)obj, objectsObj2 = (Object)objectsObj;
        var cellRefs = GetCellRefs(cell);
        if (land != null && GfxTerrain != null) { yield return null; await CreateLand(land, obj2); yield return null; }
        foreach (var s in cellRefs) await CreateCell(cell, objectsObj2, s);
        if (GfxLight != null) await CreateReflectionProbe(cell, obj2);
    }

    CellRef[] GetCellRefs(ICell cell) => [.. cell.Xrefs.Select(s => {
        var record = Query.FindAnyByName(s.Name);
        var modelPath = record != null && record is ICellXrefModel modl ? modl.ModelPath : null;
        return new CellRef { Obj = s, Record = record, ModelPath = modelPath };
    })];

    /// <summary>
    /// Instantiates an object in a cell. Called by InstantiateCellObjectsCoroutine after the object's assets have been pre-loaded.
    /// </summary>
    async Task CreateCell(ICell cell, Object parent, CellRef r) {
        if (r.Record == null) return; //{ Log.Info($"Unknown Object: {r.Obj.Name}"); return; }
        Object modelObj = default; var obj = r.Obj;
        if (r.ModelPath != null) { (modelObj, _) = await GfxModel.Create(Source, r.ModelPath, true); GfxModel.Post(modelObj, obj.Position, obj.EulerAngles, obj.Scale, parent); }
        if (r.Record is ILigh ligh && GfxLight != null) {
            var s = GfxLight.Create("Light", null, ligh.Radius, ligh.LightColor, cell.IsInterior);
            if (modelObj != null) GfxApi.Attach(GfxAttach.Find, s, modelObj, "AttachLight");
            else GfxModel.Post(s, obj.Position, obj.EulerAngles, obj.Scale, parent);
        }
    }

    /// <summary>
    /// Family builders override this. The base draws no terrain.
    /// </summary>
    protected virtual Task CreateLand(ILand land, Object parent) => Task.CompletedTask;

    async Task CreateReflectionProbe(ICell cell, Object parent) {
        if (cell.IsInterior) return;
        var gridId = cell.GridId;
        var position = new Vector3(gridId.X * CellLengthInMeters, 0f, gridId.Y * CellLengthInMeters);
        GfxLight.CreateProbe("probe", position, parent);
    }
}

#endregion

//List<string> GetLandTextures(ILand land) {
//    var vtex = land.Vtex;
//    if (land.Heights == null || vtex == null) return null;
//    var paths = new List<string>();
//    var indexs = vtex.Distinct().ToArray();
//    for (var i = 0; i < indexs.Length; i++) {
//        var index = (short)indexs[i] - 1;
//        if (index < 0) { paths.Add(DefaultLandTexturePath); continue; }
//        var ltex = Query.FindLtex(index);
//        paths.Add(ltex.Path);
//    }
//    return paths;
//}

//public override IEnumerator CellCoroutine(ICell cell, ILand land, object obj, object objectsObj) {
//    // Start pre-loading all required textures for the terrain.
//    //if (land != null) {
//    //    var landTextures = GetLandTextures(land);
//    //    //if (landTextures != null)
//    //    //    foreach (var landTexture in landTextures) GfxModel.PreloadTexture(landTexture);
//    //    yield return null;
//    //}

//    // Start pre-loading all required files for referenced objects. The NIF manager will load the textures as well.
//    //foreach (var s in cellRefs)
//    //    if (s.ModelPath != null) GfxModel.PreloadObject(s.ModelPath);
//    //yield return null;

//    // Extract information about referenced objects.
//    var cellRefs = GetCellRefs(cell);

//    // Instantiate terrain.
//    if (land != null) {
//        var task = LandCoroutine(land, (Object)obj);
//        while (task.MoveNext()) yield return null;
//        yield return null;
//    }

//    // Instantiate objects.
//    foreach (var s in cellRefs) CellObject(cell, (Object)objectsObj, s);
//}