using System.Collections;
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
    List<(float, float)> wayInCell = new List<(float, float)>();
    int positionInCell = 0;
    public bool destroyed = false;
    public void StartGo(List<(int, int)> waytogo, CellWithHouse EndHouse)
    {
        nowposition = 0;
        positionInCell = -1;
        way = waytogo;
        end = EndHouse;
        if (!destroyed)transform.gameObject.SetActive(true);

        if (!destroyed) if (EndHouse.GetTypeCell() == ThingsInCell.HousePeople) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHousePeople;
        if (!destroyed) if (EndHouse.GetTypeCell() == ThingsInCell.HouseCom) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHouseCom;
        if (!destroyed) if (EndHouse.GetTypeCell() == ThingsInCell.HouseFact) transform.GetComponent<SpriteRenderer>().color = COLORS.ColorHouseFact;
        //transform.localPosition = grid.tilemap.CellToWorld(new Vector3Int(waytogo[0].x, waytogo[0].y, 1));
    }
    public void DeleteHuman()
    {
        houseControlles.AddHumanToHouse(this, end);
        if (!destroyed) transform.gameObject.SetActive(false);
        //(grid.GetCell(way[nowposition]) as CellWithRoad).MoveOutThis();
        //Destroy(transform.gameObject);
    }
    public void OnDestroy()
    {
        destroyed = true;

    }
    public bool MoveToNext(out Vector3 from, out Vector3 to)
    {
        if (grid.Roads.ContainsKey(way[nowposition]))
            grid.Roads[way[nowposition]].MoveOutThis(this, wayInCell[positionInCell]);
        if (positionInCell + 1 < wayInCell.Count)
        {
            CellWithRoad nowRoad = grid.Roads[way[nowposition]];
            from = new Vector3(wayInCell[positionInCell].Item1 + nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2, 0);
            positionInCell++;
            to = new Vector3(wayInCell[positionInCell].Item1 + nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2, 0);
            nowRoad.MoveToThis(this, wayInCell[positionInCell]);
            return false;
        }
        else if (nowposition + 2 < way.Count)
        {
            if (!destroyed) from = transform.localPosition;
            else from = new Vector3();
            nowposition++;
            CellWithRoad nowRoad = grid.Roads[way[nowposition]];
            positionInCell = 0;
            wayInCell = nowRoad.GetWayInTheCell(way[nowposition - 1], way[nowposition + 1]);
            to = new Vector3(wayInCell[positionInCell].Item1 + nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2, 0);
            grid.Roads[way[nowposition]].MoveToThis(this, wayInCell[positionInCell]);
            return false;
        }
        else if (nowposition + 1 < way.Count)
        {
            if (!destroyed) from = transform.localPosition;
            else from = new Vector3();
            nowposition++;
            wayInCell = new List<(float, float)>();
            if (grid.GetCell(way[nowposition]) != null)
            {
                (int, int) housePosition = grid.GetCell(way[nowposition]).GetCellPosition();
                to = new Vector3(housePosition.Item1 + 0.5f, housePosition.Item2 + 0.5f, 0);
                return false;
            }
            else
            {
                from = new Vector3();
                to = new Vector3();
                return true;
            }
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
            CellWithRoad nowCell = grid.Roads[way[nowposition]];
            /*if (nowCell.CanMove(wayInCell[positionInCell + 1]) != null)
            {
                List<(float, float)> maybeway = nowCell.GetWayFromPositionInTheCell(wayInCell[positionInCell], way[nowposition + 1]);
                if (maybeway != null)
                {
                    positionInCell = 0;
                    wayInCell = maybeway;
                    return null;
                }
            }
            */
            return nowCell.CanMove(wayInCell[positionInCell + 1], way[nowposition-1], this, positionInCell+1);
        }
        else if (nowposition + 2 < way.Count)
        {
            CellWithRoad nowCell = grid.Roads[way[nowposition + 1]];

            if (nowCell != null)
            {
                if (!destroyed)transform.localScale = new Vector3(0.25f / nowCell.throughput, 0.25f / nowCell.throughput, 0);
                List<(float,float)> nextway = nowCell.GetWayInTheCell(way[nowposition], way[nowposition + 2]);
                return nowCell.CanMove(nextway[0], way[nowposition], this, 0);
            }
            else return null;
        }
        else return null;
    }
}
