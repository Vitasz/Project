using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems ;
using UnityEngine.Tilemaps;
public class GridFunc : MonoBehaviour
{
    public EventSystem EventSystemManager;
    public int SizeX, SizeY, SizeCell;
    public HouseControlles houseControlles;
    public GameControlls GameController;
    public Material MaterialForLines;
    public Camera nowCamera;
    public Tilemap tilemap;
    public readonly float _linesWidth = 0.3f;
    private Dictionary<Vector3Int, Cell> Map = new Dictionary<Vector3Int, Cell>();
    private bool isRedactorActive = false;
    private int ModeRedactor = 0;
    Vector3Int prevpositionClick = new Vector3Int(), nowpositionClick = new Vector3Int();
    bool hasfirstclick = false;
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            if (isRedactorActive && (tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition)) != prevpositionClick||!hasfirstclick))
            {
                prevpositionClick = nowpositionClick;
                nowpositionClick = tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition));
                
                if (!Map.ContainsKey(nowpositionClick)) CreateNewTile(nowpositionClick, (ThingsInCell) ModeRedactor);
                if (hasfirstclick && Mathf.Abs(prevpositionClick.x - nowpositionClick.x) + Mathf.Abs(prevpositionClick.y - nowpositionClick.y)<=1)
                {
                    UniteTiles(prevpositionClick, nowpositionClick, (ThingsInCell) ModeRedactor);
                }
                hasfirstclick = true;
            }
        }
    }
    public void OpenRedactorCell() => isRedactorActive = true;
    public void CloseRedactorCell()
    {
        isRedactorActive = false;
        hasfirstclick = false;
    }
    public void SetMode(ThingsInCell mode)
    {
        ModeRedactor = (int)mode;
        hasfirstclick = false;
    }
    public List<Vector3Int> FindWay(List<Vector3Int> from, List<Vector3Int> to)
    {
        List<Vector3Int> now = new List<Vector3Int>(), nownew = new List<Vector3Int>();
        Dictionary<Vector3Int, (int, Vector3Int)> timetoRoads = new Dictionary<Vector3Int, (int, Vector3Int)>();
        foreach (Vector3Int a in from)
        {
            now.Add(a);
            timetoRoads.Add(a, (0, a));
        }
        int timer = 0;
        bool ok = false;
        Vector3Int end = new Vector3Int();
        while (now.Count != 0)
        {
            timer++;
            foreach (Vector3Int a in now)
            {
                if (to.Contains(a)&&!from.Contains(a))
                {
                    end = a;
                    ok = true;
                    break;
                }
                if (Map.ContainsKey(a)&&Map[a]is CellWithRoad)
                    foreach (Vector3Int b in (Map[a] as CellWithRoad).GetNearRoadsWays())
                    {
                        if (!nownew.Contains(b) && !timetoRoads.ContainsKey(b))
                        {
                            nownew.Add(b); 
                            timetoRoads[b] = (timer, a);
                        }
                    }
            }
            now.Clear();
            foreach (Vector3Int a in nownew) now.Add(a);
            nownew.Clear();
        }
        if (!ok) {
            return null;
        }
        List<Vector3Int> ans = new List<Vector3Int>();
        ans.Add(end);
        Vector3Int nowPos = end;
        while (!from.Contains(nowPos))
        {
            nowPos = timetoRoads[nowPos].Item2;
            ans.Add(nowPos);
        }
        ans.Reverse();
        return ans;
    }
    public Cell GetCell(Vector3Int Position) => Map[Position];
    
    private void CreateNewTile(Vector3Int Position, ThingsInCell type)
    {
        if (type == ThingsInCell.HousePeople || type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact)
            Map.Add(Position, new CellWithHouse(this, houseControlles, Position, type));
        else if (type == ThingsInCell.RoadForCars) Map.Add(Position, new CellWithRoad(this, houseControlles, Position));
    }
    private void UniteTiles(Vector3Int PositionFrom, Vector3Int PositionTo, ThingsInCell Mode)
    {
        if (Map[PositionFrom] is CellWithRoad && Map[PositionTo] is CellWithRoad)
        {
            (Map[PositionFrom] as CellWithRoad).AddRoad(PositionFrom, PositionTo, true);
            (Map[PositionTo] as CellWithRoad).AddRoad(PositionTo, PositionFrom, false);
        }
        else if (Map[PositionFrom] is CellWithHouse && Map[PositionTo] is CellWithHouse)
        {
            (Map[PositionFrom] as CellWithHouse).UniteHouse(PositionFrom, PositionTo, true);
            (Map[PositionTo] as CellWithHouse).UniteHouse(PositionTo, PositionFrom, false);
        }
    }
    /*public Vector3 PositionCell((int,int) cell)
    {
        Vector3 toret = new Vector3(0, 0,-100);
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + cell.Item1 * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + cell.Item2 * SizeCell;
        return toret;
    }*/
}
