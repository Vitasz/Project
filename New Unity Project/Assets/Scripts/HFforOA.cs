using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HFforOA
{
    public List<Vector3Int> way;
    CellWithHouse end;
    GridFunc grid;
    public int nowposition = 0;
    public void StartGo(List<Vector3Int> waytogo, CellWithHouse EndHouse, GridFunc Grid, CellWithHouse FromHouse)
    {
        grid = Grid;
        way = waytogo;
        end = EndHouse;
    }
    public void DeleteHuman()
    {
        end.AddHuman();
        (grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
    }
    public bool MoveToNext()
    {
        (grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        if (nowposition + 1 != way.Count)
        {
            nowposition++;
            (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this);
            return false;
        }
        else
        {
            return true;
        }
    }
    public HFforOA CanMove()
    {
        if (nowposition + 1 != way.Count)
        {
            Cell nowCell = grid.GetCell(way[nowposition + 1]);
            return (nowCell as CellWithRoad).CanMoveForOA();
        }
        else return null;
    }
}
