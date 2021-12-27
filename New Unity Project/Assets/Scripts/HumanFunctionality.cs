using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    public List<(int, int)> way;
    CellWithHouse end;
    public GridFunc grid;
    public int nowposition = 0;
    public HouseControlles houseControlles;
    List<Vector3> wayInCell = new List<Vector3>();
    int positionInCell = 0;
    public void StartGo(List<(int, int)> waytogo, CellWithHouse EndHouse)
    {
        nowposition = 0;
        positionInCell = -1;
        way = waytogo;
        end = EndHouse;
        //transform.localPosition = grid.tilemap.CellToWorld(new Vector3Int(waytogo[0].x, waytogo[0].y, 1));
    }
    public void DeleteHuman()
    {
        houseControlles.AddHumanToHouse(this, end);
        //(grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        //Destroy(transform.gameObject);
    }
    public bool MoveToNext(out Vector3 from, out Vector3 to)
    {
        if (grid.GetCell(way[nowposition]) is CellWithRoad) (grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis(this, wayInCell[positionInCell]);
        if (positionInCell + 1 < wayInCell.Count)
        {
            from = wayInCell[positionInCell];
            positionInCell++;
            to = wayInCell[positionInCell];
            (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this, wayInCell[positionInCell]);
            return false;
        }
        else if (nowposition + 1 < way.Count)
        {
            from = transform.localPosition;
            nowposition++;
            CellWithRoad nowRoad= grid.GetCell(way[nowposition]) as CellWithRoad;
            positionInCell = 0;
            if (nowRoad != null)
            {
                wayInCell = nowRoad.GetWayInTheCell(way[nowposition - 1], way[nowposition + 1]);
                to = wayInCell[positionInCell];
                (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this, wayInCell[positionInCell]);
            }
            else
            {
                wayInCell = new List<Vector3>();
                (int, int) housePosition = grid.GetCell(way[nowposition]).GetCellPosition();

                to = new Vector3(housePosition.Item1 + 0.5f, housePosition.Item2 + 0.5f, 0); ;
            }
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
        if (positionInCell + 1 < wayInCell.Count)
        {
            CellWithRoad nowCell = grid.GetCell(way[nowposition]) as CellWithRoad;
            return nowCell.CanMove(wayInCell[positionInCell + 1]);
        }
        else if (nowposition + 1 < way.Count)
        {
            CellWithRoad nowCell = grid.GetCell(way[nowposition + 1]) as CellWithRoad;

            if (nowCell != null)
            {
                List<Vector3> nextway = nowCell.GetWayInTheCell(way[nowposition], way[nowposition + 1]);
                return nowCell.CanMove(nextway[0]);
            }
            else return null;
        }
        else return null;
    }
}
