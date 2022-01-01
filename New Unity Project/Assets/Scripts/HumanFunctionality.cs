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
    /*public bool MoveToNext(out Vector3 from, out Vector3 to)
    {
        if (grid.Roads.ContainsKey(way[nowposition])) grid.Roads[way[nowposition]].MoveOutThis(this, wayInCell[positionInCell]);
        if (positionInCell + 1 < wayInCell.Count)
        {
            CellWithRoad nowRoad = grid.Roads[way[nowposition]];
            from = new Vector3(wayInCell[positionInCell].Item1+nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2,0);
            positionInCell++;
            to = new Vector3(wayInCell[positionInCell].Item1 + nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2, 0);
            nowRoad.MoveToThis(this, wayInCell[positionInCell]);
            return false;
        }
        else if (nowposition + 2 < way.Count)
        {
            from = transform.localPosition;
            nowposition++;
            CellWithRoad nowRoad = grid.Roads[way[nowposition]];
            positionInCell = 0;
            wayInCell = nowRoad.GetWayInTheCell(way[nowposition - 1], way[nowposition + 1]);
            to = new Vector3(wayInCell[positionInCell].Item1 + nowRoad.GetCellPosition().Item1, wayInCell[positionInCell].Item2 + nowRoad.GetCellPosition().Item2, 0);
            grid.Roads[way[nowposition]].MoveToThis(this, wayInCell[positionInCell]);
            return false;
        }
        else if (nowposition+1<way.Count)
        {
            from = transform.localPosition;
            nowposition++;
            wayInCell = new List<(float,float)>();
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
    }*/
    
}
