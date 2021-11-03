using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
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
        transform.localPosition = grid.tilemap.CellToWorld(new Vector3Int(waytogo[0].x, waytogo[0].y, 1));
    }
    public void DeleteHuman()
    {
        end.AddHuman();
        //(grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        Destroy(transform.gameObject);
    }
    public bool MoveToNext(out Vector3 from, out Vector3 to)
    {
        if (grid.GetCell(way[nowposition]) is CellWithRoad) (grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        if (nowposition + 1 != way.Count)
        {
            from = grid.tilemap.CellToWorld(new Vector3Int(way[nowposition].x, way[nowposition].y, 1));
            nowposition++;
            to = grid.tilemap.CellToWorld(new Vector3Int(way[nowposition].x, way[nowposition].y, 1));
            //transform.localPosition = grid.tilemap.CellToWorld(new Vector3Int(way[nowposition].x, way[nowposition].y, 1));
            if (grid.GetCell(way[nowposition]) is CellWithRoad) (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this);
            return false;
        }
        else
        {
            from = new Vector3();
            to = new Vector3();
            return true;
        }
    }
    public HumanFunctionality CanMove()
    {
        if (nowposition + 1 != way.Count)
        {
            Cell nowCell = grid.GetCell(way[nowposition + 1]);
            if (nowCell is CellWithRoad) return (nowCell as CellWithRoad).CanMove();
            else return null;
        }
        else return null;
    }
}
