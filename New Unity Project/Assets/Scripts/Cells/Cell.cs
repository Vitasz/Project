using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using System.IO;
using System;
public enum ThingsInCell : int
{
    Nothing,
    HousePeople,
    HouseCom,
    HouseFact,
    RoadForCars,
}
public class Cell : MonoBehaviour
{
    protected GridFunc grid;
    protected HouseControlles houseControlles;
    protected Vector3Int positioninTileMap;
    public Cell(GridFunc Grid, HouseControlles HouseControlles, Vector3Int PositionCell)
    {
        grid = Grid;
        houseControlles = HouseControlles;
        positioninTileMap = PositionCell;
    }
    protected virtual void UpdateTileWithNewNeighboors() { }
    protected int GetIndexNearCell(Vector3Int from, Vector3Int to)
    {
        if (from.x - 1 == to.x && from.y + 1 == to.y) return 0;
        if (from.x == to.x && from.y + 1 == to.y) return 1;
        if (from.x + 1 == to.x && from.y + 1 == to.y) return 2;
        if (from.x + 1 == to.x && from.y == to.y) return 3;
        if (from.x + 1 == to.x && from.y - 1 == to.y) return 4;
        if (from.x == to.x && from.y - 1 == to.y) return 5;
        if (from.x - 1 == to.x && from.y - 1 == to.y) return 6;
        else  return 7;
    }
   public void UpdateTile()
   {
        /*foreach(KeyValuePair<int, int[]> a in whatNearCells)
        {
            string temp = "";
            foreach (int b in a.Value) temp += Convert.ToString(b);
            grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, a.Key), Resources.Load<Tile>(ResourcesLoadedPaths[a.Key] + temp));
        }*/
   }
    public Vector3Int GetCellPosition() => positioninTileMap;
}
