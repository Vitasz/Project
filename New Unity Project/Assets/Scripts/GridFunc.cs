using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public class GridFunc : MonoBehaviour
{
    public EventSystem EventSystemManager;
    //public int SizeX, SizeY, SizeCell;
    public HouseControlles houseControlles;
    public GameControlls GameController;
    public Material MaterialForLines;
    public Camera nowCamera;
    public Tilemap tilemap;
    public Clock clock;
    public HumanController HumanControlles;
    //public readonly float _linesWidth = 0.3f;
    public Dictionary<Vector3Int, Cell> Map = new Dictionary<Vector3Int, Cell>();
    public List<CellWithRoad> Roads = new List<CellWithRoad>();
    public List<GameObject> Humans = new List<GameObject>();
    public OptimizationAlgorithm a;
    public Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> NowSystem = new Dictionary<Vector3Int, List<(int, List<Vector3Int>)>>();
    private bool isRedactorActive = false;
    private int ModeRedactor = 0;
    Vector3Int prevpositionClick = new Vector3Int(), nowpositionClick = new Vector3Int();
    bool hasfirstclick = false;
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            if (isRedactorActive && (tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition)) != prevpositionClick || !hasfirstclick))
            {
                prevpositionClick = nowpositionClick;
                nowpositionClick = tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition));
                if (!Map.ContainsKey(nowpositionClick)) CreateNewTile(nowpositionClick, (ThingsInCell)ModeRedactor, new List<Vector3Int>());
                if (hasfirstclick && Mathf.Abs(prevpositionClick.x - nowpositionClick.x) + Mathf.Abs(prevpositionClick.y - nowpositionClick.y) <= 1)
                {
                    UniteTiles(prevpositionClick, nowpositionClick, (ThingsInCell)ModeRedactor);
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
        Dictionary<Vector3Int, (float, Vector3Int)> timetoRoads = new Dictionary<Vector3Int, (float, Vector3Int)>();
        foreach (Vector3Int a in from)
        {
            now.Add(a);
            timetoRoads.Add(a, (0, a));
        }
        Vector3Int end = new Vector3Int();
        float MinWay = 100000000;
        while (now.Count != 0)
        {
            foreach (Vector3Int a in now)
            {
                if (Map.ContainsKey(a) && Map[a] is CellWithRoad)
                    foreach (Vector3Int b in (Map[a] as CellWithRoad).GetNearRoadsWays())
                    {
                        if (Map.ContainsKey(b))
                        {
                            if (timetoRoads.ContainsKey(b))
                            {
                                if (timetoRoads[b].Item1 > timetoRoads[a].Item1 + (Map[a] as CellWithRoad).WaitTime)
                                {
                                    timetoRoads[b] = (timetoRoads[a].Item1 + (Map[b] as CellWithRoad).WaitTime, a);
                                    nownew.Add(b);
                                }
                            }
                            else
                            {
                                timetoRoads.Add(b, (timetoRoads[a].Item1 + (Map[b] as CellWithRoad).WaitTime, a));
                                nownew.Add(b);
                            }
                        }
                    }
            }
            now.Clear();
            foreach (Vector3Int a in nownew) now.Add(a);
            nownew.Clear();
        }
        foreach (Vector3Int a in to)
        {
            if (timetoRoads.ContainsKey(a) && Map.ContainsKey(a) && (Map[a] is CellWithRoad))
            {
                if (MinWay > timetoRoads[a].Item1)
                {
                    MinWay = timetoRoads[a].Item1;
                    end = a;
                }
            }
        }
        if (MinWay == 100000000) return null;
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
    public Cell GetCell(Vector3Int Position)
    {
        if (Map.ContainsKey(Position))
            return Map[Position];
        else return null;
    }

    public void CreateNewTile(Vector3Int Position, ThingsInCell type, List<Vector3Int> roads)
    {
        if (type == ThingsInCell.HousePeople || type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact)
            Map.Add(Position, new CellWithHouse(this, houseControlles, Position, type));
        else if (type == ThingsInCell.RoadForCars)
        {
            Map.Add(Position, new CellWithRoad(this, houseControlles, Position, type, roads));
            Roads.Add(Map[Position] as CellWithRoad);
        }
        //UpdateSystem();
    }
    public void UniteTiles(Vector3Int PositionFrom, Vector3Int PositionTo, ThingsInCell Mode)
    {
        if (Map[PositionFrom] is CellWithRoad && Map[PositionTo] is CellWithRoad)
        {
            (Map[PositionFrom] as CellWithRoad).AddRoad(PositionFrom, PositionTo, true);
            (Map[PositionTo] as CellWithRoad).AddRoad(PositionTo, PositionFrom, false);
            //UpdateSystem();
        }
        /*else if (Map[PositionFrom] is CellWithHouse && Map[PositionTo] is CellWithHouse)
        {
            (Map[PositionFrom] as CellWithHouse).UniteHouse(PositionFrom, PositionTo, true, ForOA);
            (Map[PositionTo] as CellWithHouse).UniteHouse(PositionTo, PositionFrom, false, ForOA);
        }*/
    }

    private void UpdateSystem()
    {
        List<Vector3Int> GetNearTiles(Vector3Int a)
        {
            List<Vector3Int> ans2 = new List<Vector3Int>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector3Int now = new Vector3Int(a.x + i, a.y + j, a.z);
                    if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1) ans2.Add(now);
                }
            }
            return ans2;
        }
        List<Vector3Int> HousesInList = new List<Vector3Int>();
        Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> roadsandWaysFromThem = new Dictionary<Vector3Int, List<(int, List<Vector3Int>)>>();
        Dictionary<Vector3Int, List<int>> roadstohouses = new Dictionary<Vector3Int, List<int>>();
        Dictionary<Vector3Int, List<int>> RoadsWithHouses = new Dictionary<Vector3Int, List<int>>();
        foreach (Vector3Int a in Map.Keys) if (Map[a] is CellWithHouse) HousesInList.Add(a);
        for (int i = 0; i < HousesInList.Count; i++)
        {
            foreach (Vector3Int a in GetNearTiles(HousesInList[i]))
            {
                if (Map.ContainsKey(a) && (Map[a] is CellWithRoad))
                {
                    if (!roadstohouses.ContainsKey(a))
                    {
                        roadstohouses.Add(a, new List<int>());
                        roadsandWaysFromThem.Add(a, new List<(int, List<Vector3Int>)>());
                        RoadsWithHouses.Add(a, new List<int>());
                    }
                    if (!roadstohouses[a].Contains(i))
                    {
                        roadstohouses[a].Add(i);
                        RoadsWithHouses[a].Add(i);
                        roadsandWaysFromThem[a].Add((i, new List<Vector3Int>() { a }));
                    }
                }
            }
        }
        Dictionary<Vector3Int, List<Vector3Int>> roads = new Dictionary<Vector3Int, List<Vector3Int>>();
        foreach (Vector3Int a in Map.Keys) if (Map[a] is CellWithRoad) roads.Add(a, (Map[a] as CellWithRoad).GetNearRoadsWays());
        foreach (Vector3Int a in roads.Keys)
        {
            List<(Vector3Int, List<Vector3Int>)> nowpos = new List<(Vector3Int, List<Vector3Int>)>() { (a, new List<Vector3Int>()) }, newpos = new List<(Vector3Int, List<Vector3Int>)>();
            List<Vector3Int> Used = new List<Vector3Int>(); ;
            while (nowpos.Count != 0)
            {
                foreach ((Vector3Int, List<Vector3Int>) b in nowpos)
                {
                    Used.Add(b.Item1);
                    b.Item2.Add(b.Item1);
                    if (RoadsWithHouses.ContainsKey(b.Item1))
                    {
                        List<Vector3Int> copy = new List<Vector3Int>();
                        foreach (Vector3Int c in b.Item2) copy.Add(c);
                        if (!roadstohouses.ContainsKey(a))
                        {
                            roadsandWaysFromThem.Add(a, new List<(int, List<Vector3Int>)>());
                            roadstohouses.Add(a, new List<int>());
                        }
                        foreach (int c in RoadsWithHouses[b.Item1])
                        {
                            if (roadstohouses[a].Contains(c)) continue;
                            roadsandWaysFromThem[a].Add((c, copy));
                            roadstohouses[a].Add(c);
                        }
                    }
                    foreach (Vector3Int f in roads[b.Item1])
                    {
                        if (!Used.Contains(f))
                        {
                            bool ok = true;
                            foreach ((Vector3Int, List<Vector3Int>) k in newpos)
                            {
                                if (k.Item1 == f) ok = false;
                            }

                            if (ok)
                            {
                                List<Vector3Int> Copy = new List<Vector3Int>();
                                foreach (Vector3Int l in b.Item2) Copy.Add(l);
                                newpos.Add((f, Copy));
                            }
                        }
                    }
                }
                nowpos.Clear();
                foreach ((Vector3Int, List<Vector3Int>) b in newpos) nowpos.Add(b);
                newpos.Clear();
            }
        }
        NowSystem = roadsandWaysFromThem;
    }
    public void Optimize()
    {
        a.Optimization(this);
    }
    public void RemoveTileAt(Vector3Int position)
    {
        if (Map[position] is CellWithHouse)
        {
            tilemap.SetTile(position, null);
            houseControlles.RemoveHouse(Map[position] as CellWithHouse);
            Map.Remove(position);
        }
        else if (Map[position] is CellWithRoad)
        {
            (Map[position] as CellWithRoad).Remove();
            //tilemap.SetTile(position, null);
            //tilemap.SetTile(new Vector3Int(position.x, position.y, -1), null);
            Map.Remove(position);
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) <= 1 && Map.ContainsKey(new Vector3Int(position.x + i, position.y + j, 0)))
                    {
                        if (Map[new Vector3Int(position.x + i, position.y + j, 0)] as CellWithRoad != null)
                        {
                            (Map[new Vector3Int(position.x + i, position.y + j, 0)] as CellWithRoad).RemoveRoad(new Vector3Int(position.x + i, position.y + j, 0), position);
                        }
                    }
                }
            }
        }
    }
    public List<Vector3Int> PositionsRoadAround(Vector3Int position)
    {
        List<Vector3Int> ans = new List<Vector3Int>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (Math.Abs(i) + Math.Abs(j) <= 1 && Map.ContainsKey(new Vector3Int(position.x + i, position.y + j, 0)))
                {
                    if (Map[new Vector3Int(position.x + i, position.y + j, 0)] as CellWithRoad != null)
                    {
                        ans.Add(new Vector3Int(position.x + i, position.y + j, 0));
                    }
                }
            }
        }
        return ans;
    }
}
