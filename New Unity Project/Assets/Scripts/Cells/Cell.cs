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
    protected (int, int) positioninTileMap;
    ThingsInCell typeCell;
    public Cell(GridFunc Grid, HouseControlles HouseControlles, (int, int) PositionCell, ThingsInCell type)
    {
        grid = Grid;
        houseControlles = HouseControlles;
        positioninTileMap = PositionCell;
        typeCell = type;
    }
    protected virtual void UpdateTileWithNewNeighboors() { }
    public int GetIndexNearCell((int, int) from, (int, int) to)
    {
        if (from.Item1 - 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 0;
        if (from.Item1 == to.Item1 && from.Item2 + 1 == to.Item2) return 1;
        if (from.Item1 + 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 2;
        if (from.Item1 + 1 == to.Item1 && from.Item2 == to.Item2) return 3;
        if (from.Item1 + 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 4;
        if (from.Item1 == to.Item1 && from.Item2 - 1 == to.Item2) return 5;
        if (from.Item1 - 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 6;
        if (from.Item1 - 1==to.Item1 && from.Item2 == to.Item2) return 7;
        return -1;
    }
    public (int, int) GetPositionNearCell((int, int) from, int index)
    {
        if (index == 0) return (from.Item1 - 1, from.Item2 + 1);
        if (index == 1) return (from.Item1, from.Item2 + 1);
        if (index == 2) return (from.Item1 + 1, from.Item2 + 1);
        if (index == 3) return (from.Item1 + 1, from.Item2);
        if (index == 4) return (from.Item1 + 1, from.Item2 - 1);
        if (index == 5) return (from.Item1, from.Item2 - 1);
        if (index == 6) return (from.Item1 - 1, from.Item2 - 1);
        if (index == 7) return (from.Item1 - 1, from.Item2);
        return (0,0);
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
    public ThingsInCell GetTypeCell()
    {
        return typeCell;
    }
    public List<(int,int)> GetNearTiles()
    {
        List<(int, int)> ans = new List<(int, int)>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if ((i != 0 || j != 0)&&Math.Abs(i)+Math.Abs(j)<=1) ans.Add((positioninTileMap.Item1 + i, positioninTileMap.Item2 + j));
            }
        }
        return ans;
    }
    public (int,int) GetCellPosition() => positioninTileMap;
}
