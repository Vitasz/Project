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
public class Cell
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
    public int GetIndexNearCell(Vector3Int from, Vector3Int to)
    {
        if (from.x - 1 == to.x && from.y + 1 == to.y) return 0;
        if (from.x == to.x && from.y + 1 == to.y) return 1;
        if (from.x + 1 == to.x && from.y + 1 == to.y) return 2;
        if (from.x + 1 == to.x && from.y == to.y) return 3;
        if (from.x + 1 == to.x && from.y - 1 == to.y) return 4;
        if (from.x == to.x && from.y - 1 == to.y) return 5;
        if (from.x - 1 == to.x && from.y - 1 == to.y) return 6;
        if (from.x-1==to.x && from.y==to.y) return 7;
        return -1;
    }
    public Vector3Int GetPositionNearCell(Vector3Int from, int index)
    {
        if (index == 0) return new Vector3Int(from.x - 1, from.y + 1,0);
        if (index == 1) return new Vector3Int(from.x, from.y + 1, 0);
        if (index == 2) return new Vector3Int(from.x + 1, from.y + 1, 0);
        if (index == 3) return new Vector3Int(from.x + 1, from.y, 0);
        if (index == 4) return new Vector3Int(from.x + 1, from.y - 1, 0);
        if (index == 5) return new Vector3Int(from.x, from.y -1, 0);
        if (index == 6) return new Vector3Int(from.x - 1, from.y - 1, 0);
        if (index == 7) return new Vector3Int(from.x - 1, from.y, 0);
        return new Vector3Int();
    }
    protected virtual void UpdateTile()
    {
        /*foreach(KeyValuePair<int, int[]> a in whatNearCells)
        {
            string temp = "";
            foreach (int b in a.Value) temp += Convert.ToString(b);
            grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, a.Key), Resources.Load<Tile>(ResourcesLoadedPaths[a.Key] + temp));
        }*/
    }
    public List<Vector3Int> GetNearTiles()
    {
        List<Vector3Int> ans = new List<Vector3Int>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if ((i != 0 || j != 0)&&Math.Abs(i)+Math.Abs(j)<=1) ans.Add(new Vector3Int(positioninTileMap.x + i, positioninTileMap.y + j, positioninTileMap.z));
            }
        }
        return ans;
    }
    public Vector3Int GetCellPosition() => positioninTileMap;
}
