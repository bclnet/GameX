// using OpenStack;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Numerics;

// namespace GameX;

// public interface IRecord { }

// public class CellManager(Archive archive, IDatabase dbase, CoroutineQueue queue) {
//     public int DefaultRadius = 4;
//     public int DetailRadius = 3;
//     public string DefaultLandTexturePath = "textures/_land_default.dds";

//     public class Cell(object obj, object container, IRecord record, IEnumerator action) {
//         public object Obj = obj;
//         public object Container = container;
//         public IRecord Record = record;
//         public IEnumerator Action = action;
//         public void SetVisible(bool visible) {
//             //if (visible) { if (!Container.activeSelf) Container.SetActive(true); }
//             //else { if (Container.activeSelf) Container.SetActive(false); }
//         }
//     }

//     public class Reference {
//         public object Obj;
//         public IRecord Record;
//         public string Path;
//     }

//     const float PointFactor = 0.5f;
//     public Archive Archive = archive;
//     public Dbase Dbase = dbase;
//     public CoroutineQueue Queue = queue;
//     public Dictionary<Point3D, Cell> Cells = [];

//     public Point3D GetPoint(Vector3 point, int world) => new((int)Math.Floor(point.X / PointFactor), (int)Math.Floor(point.Z / PointFactor), world);

//     public Cell BeginCell(Point3D point) {
//         var record = Data.FindCellRecord(point);
//         if (record == null) return null;
//         var cell = BuildCell(record);
//         Cells[point.Z != -1 ? point : Point3D.Zero] = cell;
//         return cell;
//     }

//     public Cell BeginCellByName(string name, int id, int world = -1) {
//         var record = Data.FindCellRecordByName(name, id, world);
//         if (record == null) return null;
//         var cell = BuildCell(record);
//         Cells[Point3D.Zero] = cell;
//         return cell;
//     }

//     public void UpdateCells(Vector3 position, int world = -1, bool immediate = false, int radius = -1) {
//         var point = GetPoint(position, world);
//         if (radius < 0) radius = DefaultRadius;
//         int minX = point.X - radius, maxX = point.X + radius, minY = point.Y - radius, maxY = point.Y + radius;

//         // destroy out of range cells
//         var outOfRange = new List<Point3D>();
//         foreach (var s in Cells.Keys)
//             if (s.X < minX || s.X > maxX || s.Y < minY || s.Y > maxY) outOfRange.Add(s);
//         foreach (var s in outOfRange) DestroyCell(s);

//         // create new cells
//         for (var r = 0; r <= radius; r++)
//             for (var x = minX; x <= maxX; x++)
//                 for (var y = minY; y <= maxY; y++) {
//                     var p = new Point3D(x, y, world);
//                     var d = Math.Max(Math.Abs(point.X - p.X), Math.Abs(point.Y - p.Y));
//                     if (d == r && !Cells.ContainsKey(p)) {
//                         var cell = BeginCell(p);
//                         if (cell != null && immediate) Queue.WaitFor(cell.Action);
//                     }
//                 }

//         // update LODs
//         foreach (var (p, cell) in Cells) {
//             var d = Math.Max(Math.Abs(point.X - p.X), Math.Abs(point.Y - p.Y));
//             cell.SetVisible(d <= DetailRadius);
//         }
//     }

//     Cell BuildCell(CELLRecord cell) {
//         Debug.Assert(cell != null);
//         string cellObjName = null;
//         LANDRecord land = null;
//         if (!cell.IsInterior) {
//             cellObjName = "cell " + cell.GridId.ToString();
//             land = _data.FindLANDRecord(cell.GridId);
//         }
//         else cellObjName = cell.EDID.Value;
//         var cellObj = new GameObject(cellObjName) { tag = "Cell" };
//         var cellObjectsContainer = new GameObject("objects");
//         cellObjectsContainer.transform.parent = cellObj.transform;
//         var cellObjectsCreationCoroutine = InstantiateCellObjectsCoroutine(cell, land, cellObj, cellObjectsContainer);
//         _loadBalancer.AddTask(cellObjectsCreationCoroutine);
//         return new InRangeCellInfo(cellObj, cellObjectsContainer, cell, cellObjectsCreationCoroutine);
//     }

//     void DestroyCell(Point3D point) {
//         if (Cells.TryGetValue(point, out var s)) {
//             Queue.Cancel(s.Action);
//             // Object.Destroy(s.Obj);
//             Cells.Remove(point);
//         }
//         else Log.Error("Tried to destroy a cell that isn't created.");
//     }

//     public void DestroyAllCells() {
//         foreach (var s in Cells.Values) {
//             Queue.Cancel(s.Action);
//             // Object.Destroy(s.Obj);
//         }
//         Cells.Clear();
//     }
// }

// public class PlayerManager() {

// }