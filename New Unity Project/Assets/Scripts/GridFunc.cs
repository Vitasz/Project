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
    public Clock clock;
    public HumanController HumanControlles;
    public readonly float _linesWidth = 0.3f;
    public Dictionary<Vector3Int, Cell> Map = new Dictionary<Vector3Int, Cell>();
    public List<CellWithRoad> Roads = new List<CellWithRoad>();
    public List<GameObject> Humans = new List<GameObject>();
    public OptimizationAlgorithm a;
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
                if (!Map.ContainsKey(nowpositionClick)) CreateNewTile(nowpositionClick, (ThingsInCell) ModeRedactor, false);
                if (hasfirstclick && Mathf.Abs(prevpositionClick.x - nowpositionClick.x) + Mathf.Abs(prevpositionClick.y - nowpositionClick.y)<=1)
                {
                    UniteTiles(prevpositionClick, nowpositionClick, (ThingsInCell) ModeRedactor, false);
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
                if (Map.ContainsKey(a)&&Map[a]is CellWithRoad)
                    foreach (Vector3Int b in (Map[a] as CellWithRoad).GetNearRoadsWays())
                    {
                        if (timetoRoads.ContainsKey(b))
                        {
                            if (timetoRoads[b].Item1>timetoRoads[a].Item1+(Map[a] as CellWithRoad).WaitTime)
                            {
                                timetoRoads[b] = (timetoRoads[a].Item1 + (Map[b] as CellWithRoad).WaitTime, a);
                                nownew.Add(b);
                            }
                        }
                        else
                        {
                            timetoRoads.Add(b,  (timetoRoads[a].Item1 + (Map[b] as CellWithRoad).WaitTime, a));
                            nownew.Add(b);
                        }
                    }
            }
            now.Clear();
            foreach (Vector3Int a in nownew) now.Add(a);
            nownew.Clear();
        }
        foreach(Vector3Int a in to)
        {
            if (timetoRoads.ContainsKey(a)&&Map.ContainsKey(a)&&(Map[a] is CellWithRoad))
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
    
    public void CreateNewTile(Vector3Int Position, ThingsInCell type,bool ForOA)
    {
        if (type == ThingsInCell.HousePeople || type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact)
            Map.Add(Position, new CellWithHouse(this, houseControlles, Position, type, ForOA));
        else if (type == ThingsInCell.RoadForCars)
        {
            Map.Add(Position, new CellWithRoad(this, houseControlles, Position, ForOA));
            Roads.Add(Map[Position] as CellWithRoad);
        }
    }
    public void UniteTiles(Vector3Int PositionFrom, Vector3Int PositionTo, ThingsInCell Mode, bool ForOA)
    {
        if (Map[PositionFrom] is CellWithRoad && Map[PositionTo] is CellWithRoad)
        {
            (Map[PositionFrom] as CellWithRoad).AddRoad(PositionFrom, PositionTo, true, ForOA);
            (Map[PositionTo] as CellWithRoad).AddRoad(PositionTo, PositionFrom, false, ForOA);
        }
        /*else if (Map[PositionFrom] is CellWithHouse && Map[PositionTo] is CellWithHouse)
        {
            (Map[PositionFrom] as CellWithHouse).UniteHouse(PositionFrom, PositionTo, true, ForOA);
            (Map[PositionTo] as CellWithHouse).UniteHouse(PositionTo, PositionFrom, false, ForOA);
        }*/
    }
    /*public Vector3 PositionCell((int,int) cell)
    {
        Vector3 toret = new Vector3(0, 0,-100);
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + cell.Item1 * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + cell.Item2 * SizeCell;
        return toret;
    }*/
    public void Optimize()
    {
        //OptimizationAlgorithm a = new OptimizationAlgorithm();
        a.Optimization(this);
    }
    public double StartSimmulation(OptimizationAlgorithm OA)
    {
        houseControlles.CanSpawn = true;
        houseControlles.CoroutineWork = true;
        clock.MaxEfficiency = -1;
        houseControlles.SpawnHumanNotInf();
        return clock.GetEfficiencyForOA();
        
    }
    public void StopSimmulation()
    {
        houseControlles.CanSpawn = true;
        foreach (GameObject a in Humans) Destroy(a.gameObject);
        Humans.Clear();
    }
    public void RemoveTileAt(Vector3Int position)
    {
        if (Map[position] is CellWithHouse)
        {
            tilemap.SetTile(position, null);
            Map.Remove(position);
            houseControlles.RemoveHouse(position);
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
                    if (Math.Abs(i)+Math.Abs(j)<=1 && Map.ContainsKey(new Vector3Int(position.x+i, position.y + j,0))){
                        if (Map[new Vector3Int(position.x + i, position.y + j, 0)] as CellWithRoad!=null)
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
                if (Math.Abs(i)+Math.Abs(j)<=1 && Map.ContainsKey(new Vector3Int(position.x + i, position.y + j, 0)))
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
