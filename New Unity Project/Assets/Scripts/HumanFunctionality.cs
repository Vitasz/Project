﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COLORS_CONST;
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
        transform.gameObject.SetActive(true);
        if (EndHouse.GetTypeCell() == ThingsInCell.HousePeople) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHousePeople;
        if (EndHouse.GetTypeCell() == ThingsInCell.HouseCom) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHouseCom;
        if (EndHouse.GetTypeCell() == ThingsInCell.HouseFact) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHouseFact;
        //transform.localPosition = grid.tilemap.CellToWorld(new Vector3Int(waytogo[0].x, waytogo[0].y, 1));
    }
    public void DeleteHuman()
    {
        houseControlles.AddHumanToHouse(this, end);
        transform.gameObject.SetActive(false);
        //(grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        //Destroy(transform.gameObject);
    }
    public bool MoveToNext(out Vector3 from, out Vector3 to)
    {
        if (grid.GetCell(way[nowposition]) is CellWithRoad) (grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis(this, (wayInCell[positionInCell].x, wayInCell[positionInCell].y));
        if (positionInCell + 1 < wayInCell.Count)
        {
            from = wayInCell[positionInCell];
            positionInCell++;
            to = wayInCell[positionInCell];
            (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this, (wayInCell[positionInCell].x, wayInCell[positionInCell].y));
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
                (grid.GetCell(way[nowposition]) as CellWithRoad).MoveToThis(this, (wayInCell[positionInCell].x, wayInCell[positionInCell].y));
            }
            else
            {
                wayInCell = new List<Vector3>();
                if (grid.GetCell(way[nowposition]) != null)
                {
                    (int, int) housePosition = grid.GetCell(way[nowposition]).GetCellPosition();

                    to = new Vector3(housePosition.Item1 + 0.5f, housePosition.Item2 + 0.5f, 0); ;
                }
                else
                {
                    from = new Vector3();
                    to = new Vector3();
                    return true;
                }
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
            if (nowCell.CanMove((wayInCell[positionInCell + 1].x, wayInCell[positionInCell + 1].y)) != null&&wayInCell.Count-positionInCell>2)
            {
                List<Vector3> maybeway = nowCell.GetWayFromPositionInTheCell((wayInCell[positionInCell].x, wayInCell[positionInCell].y), way[nowposition + 1]);
                if (maybeway != null)
                {
                    positionInCell = 0;
                    wayInCell = maybeway;
                    return null;
                }
            }
            
            return nowCell.CanMove((wayInCell[positionInCell + 1].x ,wayInCell[positionInCell+1].y));
        }
        else if (nowposition + 1 < way.Count)
        {
            CellWithRoad nowCell = grid.GetCell(way[nowposition + 1]) as CellWithRoad;

            if (nowCell != null)
            {
                transform.localScale = new Vector3(0.25f / nowCell.throughput, 0.25f / nowCell.throughput, 0);
                List<Vector3> nextway = nowCell.GetWayInTheCell(way[nowposition], way[nowposition + 2]);
                return nowCell.CanMove((nextway[0].x ,nextway[0].y));
            }
            else return null;
        }
        else return null;
    }
}
