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
    protected (int, int) positioninTileMap;
    ThingsInCell typeCell;
    public Cell(GridFunc Grid, (int, int) PositionCell, ThingsInCell type)
    {
        grid = Grid;
        positioninTileMap = PositionCell;
        typeCell = type;
    }
    public int GetIndexNearCell((int, int) to)
    {
        if (positioninTileMap.Item1 == to.Item1 && positioninTileMap.Item2 + 1 == to.Item2) return 0;
        if (positioninTileMap.Item1 + 1 == to.Item1 && positioninTileMap.Item2 == to.Item2) return 1;
        if (positioninTileMap.Item1 == to.Item1 && positioninTileMap.Item2 - 1 == to.Item2) return 2;
        if (positioninTileMap.Item1 - 1==to.Item1 && positioninTileMap.Item2 == to.Item2) return 3;
        return -1;
    }
    public (int, int) GetPositionNearCell(int index)
    {
        if (index == 0) return (positioninTileMap.Item1, positioninTileMap.Item2 + 1);
        if (index == 1) return (positioninTileMap.Item1 + 1, positioninTileMap.Item2);
        if (index == 2) return (positioninTileMap.Item1, positioninTileMap.Item2 - 1);
        if (index == 3) return (positioninTileMap.Item1 - 1, positioninTileMap.Item2);
        return (0,0);
    }
    protected virtual void UpdateTile() { }
    public ThingsInCell GetTypeCell() => typeCell;
    public List<(int,int)> GetNearTiles()
    {
        List<(int, int)> ans = new List<(int, int)>();
        for (int i = -1; i < 2; i+=2)
            ans.Add((positioninTileMap.Item1 + i, positioninTileMap.Item2));
        for (int i = -1; i < 2; i += 2)
            ans.Add((positioninTileMap.Item1, positioninTileMap.Item2 + i));
        return ans;
    }
    public (int,int) GetCellPosition() => positioninTileMap;
}
