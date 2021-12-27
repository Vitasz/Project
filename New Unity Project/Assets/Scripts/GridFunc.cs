using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Diagnostics;
public class GridFunc : MonoBehaviour
{
    public EventSystem EventSystemManager;
    public HouseControlles houseControlles;
    public GameControlls GameController;
    public Material MaterialForLines;
    public Camera nowCamera;
    public Tilemap tilemap;
    public Clock clock;
    public HumanController HumanControlles;
    public Dictionary<(int,int), Cell> Map = new Dictionary<(int, int), Cell>();
    public Dictionary<(int, int), CellWithRoad> Roads = new Dictionary<(int, int), CellWithRoad>();
    public List<GameObject> Humans = new List<GameObject>();
    public OptimizationAlgorithm a;
    //public Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> NowSystem = new Dictionary<Vector3Int, List<(int, List<Vector3Int>)>>();
    private Dictionary<(int, int), List<(int, int)>> waysFromRoads = new Dictionary<(int, int), List<(int, int)>>();
    //FOR OPTIMIZATION
    public Dictionary<((int, int), (int, int)), List<(int, int)>> WaysFromTo = new Dictionary<((int, int), (int, int)), List<(int, int)>>();
    private List<(int, int)> WaitList = new List<(int, int)>();
    
    //
    private bool isRedactorActive = false;
    private int ModeRedactor = 0;
    Vector3Int prevpositionClick = new Vector3Int(), nowpositionClick = new Vector3Int();
    private bool hasfirstclick = false;
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            if (isRedactorActive && (tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition)) != prevpositionClick || !hasfirstclick))
            {
                prevpositionClick = nowpositionClick;
                nowpositionClick = tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition));
                if (!Map.ContainsKey((nowpositionClick.x, nowpositionClick.y))) CreateNewTile((nowpositionClick.x, nowpositionClick.y), (ThingsInCell)ModeRedactor);
                if (hasfirstclick && Mathf.Abs(prevpositionClick.x - nowpositionClick.x) + Mathf.Abs(prevpositionClick.y - nowpositionClick.y) <= 1)
                {
                    UniteTiles((prevpositionClick.x, prevpositionClick.y), new List<(int,int)>() { (nowpositionClick.x, nowpositionClick.y) });
                    UniteTiles((nowpositionClick.x, nowpositionClick.y), new List<(int, int)>() { (prevpositionClick.x, prevpositionClick.y) });
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
    public List<(int, int)> FindWay(List<(int, int)> from, List<(int, int)> to)
    {
        HashSet<(int,int)> now0 = new HashSet<(int,int)>(), now1 = new HashSet<(int, int)>();
        Dictionary<(int, int), float> waittimetoRoads = new Dictionary<(int, int), float>();
        Dictionary<(int, int), (double, (int, int))> timetoRoads = new Dictionary<(int, int), (double, (int, int))>();
        HashSet<(int, int)> toSet = new HashSet<(int, int)>();
        foreach ((int,int) a in Roads.Keys)
        {
            waittimetoRoads.Add(a, 0.00001f);
            timetoRoads.Add(a, (double.MaxValue,(0,0)));
        }
        for (int i = 0; i < from.Count; i++)
        {
            (int, int) tmp = from[i];
            if (timetoRoads.ContainsKey(tmp))
            {
                now0.Add(tmp);
                timetoRoads[tmp] = (0d, (0, 0));
            }
        }
        for(int i=0; i<to.Count;i++)
        {
            (int, int) tmp = to[i];
            if (timetoRoads.ContainsKey(tmp))
            {
                toSet.Add(tmp);
            }
        }
        (int, int) end=(0,0);
        double MinWay = double.MaxValue;
        bool Now0 = true;
        HashSet<(int, int)> now, nownew;
        while (Now0 && now0.Count!=0||!Now0 && now1.Count!=0)
        {
            now = Now0 ? now0 : now1;
            nownew = Now0 ? now1 : now0;
            foreach ((int, int) Apos in now)
            {
                double nowTimeTo = timetoRoads[Apos].Item1 + waittimetoRoads[Apos];
                if (nowTimeTo > MinWay) continue;
                if (toSet.Contains(Apos))
                {
                    end = Apos;
                    MinWay = nowTimeTo;
                    continue;
                }
                foreach ((int, int) b in waysFromRoads[Apos])
                {
                    if (timetoRoads[b].Item1 > nowTimeTo)
                    {
                        timetoRoads[b] = (nowTimeTo, Apos);
                        if (!nownew.Contains(b))
                            nownew.Add(b);
                    }
                }
            }
            now.Clear();
            Now0 = !Now0;
        }
        if (MinWay == double.MaxValue) return null;
        List<(int, int)> ans = new List<(int, int)>();
        ans.Add(end);
        (int,int) nowPos = end;
        while (!from.Contains(nowPos))
        {
            nowPos = timetoRoads[nowPos].Item2;
            ans.Add(nowPos);
        }
        ans.Reverse();
        return ans;
    }
    public Cell GetCell((int, int) Position)
    {
        if (Map.ContainsKey(Position))
            return Map[Position];
        else return null;
    }

    public void CreateNewTile((int,int) Position, ThingsInCell type)
    {
        if (type == ThingsInCell.HousePeople || type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact)
        {
            Map.Add(Position, new CellWithHouse(this, houseControlles, Position, type));
            AddHouseToTime(Position);
        }
        else if (type == ThingsInCell.RoadForCars)
        {
            Map.Add(Position, new CellWithRoad(this, houseControlles, Position, type));
            Roads.Add(Position, Map[Position] as CellWithRoad);
            waysFromRoads.Add(Position, new List<(int, int)>());
            AddRoadToTime(Position);
        }
       
    }
    public void UniteTiles((int,int) PositionFrom, List<(int, int)> PositionTo)
    {
        if (Roads.ContainsKey(PositionFrom))
        {
            Roads[PositionFrom].AddRoad(PositionFrom, PositionTo);
            waysFromRoads[PositionFrom] = Roads[PositionFrom].GetNearRoadsWays();
            //(Map[PositionTo] as CellWithRoad).AddRoad(PositionTo, PositionFrom, false);
            //UpdateSystem();
        }
        /*else if (Map[PositionFrom] is CellWithHouse && Map[PositionTo] is CellWithHouse)
        {
            (Map[PositionFrom] as CellWithHouse).UniteHouse(PositionFrom, PositionTo, true, ForOA);
            (Map[PositionTo] as CellWithHouse).UniteHouse(PositionTo, PositionFrom, false, ForOA);
        }*/
    }

    private void AddHouseToTime((int, int) position)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>(), startpos = new HashSet<(int, int)>();
        foreach((int,int) a in Map[position].GetNearTiles())
        {
            if (Roads.ContainsKey(a))
            {
                startpos.Add(a);
                nowpos.Add(a);
            }
        }
        Dictionary<(int, int), (int, int)> usedRoads = new Dictionary<(int, int), (int, int)>();
        Dictionary<(int, int), List<(int, int)>> HousesAround = new Dictionary<(int, int), List<(int, int)>>();
        HashSet<(int, int)> housesreached = new HashSet<(int, int)>();
        foreach((int,int) a in Map.Keys)
        {
            if (!Roads.ContainsKey(a))
            {
                foreach((int,int) b in Map[a].GetNearTiles())
                {
                    if (Roads.ContainsKey(b))
                    {
                        if (!HousesAround.ContainsKey(b)) HousesAround.Add(b, new List<(int, int)>());
                        HousesAround[b].Add(a);
                    }
                }
            }
        }
        while (nowpos.Count != 0)
        {
            foreach((int,int) a in nowpos)
            {
                if (HousesAround.ContainsKey(a))
                {
                    //Добрались до дома
                    foreach((int,int) b in HousesAround[a])
                    {
                        if (b != position&& !housesreached.Contains(b))
                        {
                            housesreached.Add(b);
                            List<(int, int)> tmpway = new List<(int, int)>();
                            (int, int) last = a;
                            while (!startpos.Contains(last))
                            {
                                tmpway.Add(last);
                                last = usedRoads[last];
                            }
                            tmpway.Add(last);
                            WaysFromTo.Add((b, position), tmpway);
                            tmpway.Reverse();
                            WaysFromTo.Add((position, b), tmpway);
                        }
                    }
                    
                }
                //Продолжили движение по дороге
                foreach ((int, int) b in Roads[a].GetNearRoadsWays())
                {
                    if (!usedRoads.ContainsKey(b))
                    {
                        usedRoads.Add(b, a);
                        newpos.Add(b);
                    }
                }
            }
            nowpos.Clear();
            foreach ((int, int) b in newpos) nowpos.Add(b);
            newpos.Clear();
        }
    }
    private void AddRoadToTime((int, int) position)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>();
        nowpos.Add(position);
        Dictionary<(int, int), (int, (int, int))> usedRoads = new Dictionary<(int, int), (int, (int, int))>();
        Dictionary<(int, int), List<(int, int)>> RoadsAround = new Dictionary<(int, int), List<(int, int)>>();
        foreach ((int, int) a in Map.Keys)
        {
            if (!Roads.ContainsKey(a))
            {
                foreach ((int, int) b in Map[a].GetNearTiles())
                {
                    if (Roads.ContainsKey(b))
                    {
                        if (!RoadsAround.ContainsKey(a)) RoadsAround.Add(a, new List<(int, int)>());
                        RoadsAround[a].Add(b);
                    }
                }
            }
        }
        int timer = 1;
        while (nowpos.Count != 0)
        {
            foreach ((int, int) a in nowpos)
            {
                //Продолжили движение по дороге
                foreach ((int, int) b in Roads[a].GetNearRoadsWays())
                {
                    if (!usedRoads.ContainsKey(b))
                    {
                        usedRoads.Add(b, (timer,a));
                        newpos.Add(b);
                    }
                }
            }
            nowpos.Clear();
            foreach ((int, int) b in newpos) nowpos.Add(b);
            newpos.Clear();
            timer++;
        }
        foreach((int,int) a in RoadsAround.Keys)
        {
            foreach ((int, int) b in RoadsAround.Keys)
            {
                if (a != b)
                {
                    int min1 = int.MaxValue, min2=int.MaxValue;
                    (int,int) posa=(0,0), posb=(0,0);
                    foreach((int,int) c in RoadsAround[a])
                    {
                        if (usedRoads.ContainsKey(c))
                        {
                            if (min1 > usedRoads[c].Item1)
                            {
                                posa = c;
                                min1 = usedRoads[c].Item1;
                            }
                        }
                    }
                    foreach ((int, int) c in RoadsAround[b])
                    {
                        if (usedRoads.ContainsKey(c))
                        {
                            if (min2 > usedRoads[c].Item1)
                            {
                                posb = c;
                                min2 = usedRoads[c].Item1;
                            }
                        }
                    }
                    if (!WaysFromTo.ContainsKey((a, b)) || min1+min2+1 < WaysFromTo[(a, b)].Count&&min1!=int.MaxValue&&min2!=int.MaxValue)
                    {
                        HashSet<(int, int)> nowposition = new HashSet<(int, int)>(), newpositions = new HashSet<(int, int)>();
                        nowposition.Add(posa);
                        Dictionary<(int, int), (int, int)> USED2 = new Dictionary<(int, int), (int, int)>();
                        while (nowposition.Count != 0)
                        {
                            foreach((int,int) f in nowposition)
                            {
                                if (f == posb)
                                {
                                    break;
                                }
                                foreach((int,int) c in Roads[f].GetNearRoadsWays())
                                {
                                    if (!USED2.ContainsKey(c))
                                    {
                                        newpositions.Add(c);
                                        USED2.Add(c, f);
                                    }
                                }
                            }
                            nowposition.Clear();
                            foreach ((int, int) m in newpositions) nowposition.Add(m);
                            newpositions.Clear();
                        }
                        List<(int, int)> tmpway = new List<(int, int)>();
                        (int, int) last = posb;
                        while (last!=posa)
                        {
                            tmpway.Add(last);
                            last = USED2[last];
                        }
                        tmpway.Add(posa);
                        WaysFromTo.Add((b, a), tmpway);
                        tmpway.Reverse();
                        WaysFromTo.Add((a, b), tmpway);
                    }
                }
            }
        }
    }
    public void Optimize()
    {
        a.Optimization(this);
    }
    public void RemoveTileAt((int,int) position)
    {
        if (Map[position] is CellWithHouse)
        {
            tilemap.SetTile(new Vector3Int(position.Item1, position.Item2, -1), null);
            houseControlles.RemoveHouse(Map[position] as CellWithHouse);
            Map.Remove(position);
        }
        else if (Map[position] is CellWithRoad)
        {
            (Map[position] as CellWithRoad).Remove();
            tilemap.SetTile(new Vector3Int(position.Item1, position.Item2, -1), null);
            tilemap.SetTile(new Vector3Int(position.Item1, position.Item2, -1), null);
            Map.Remove(position);
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) <= 1 && Map.ContainsKey((position.Item1 + i, position.Item2 + j)))
                    {
                        if (Map[(position.Item1 + i, position.Item2 + j)] as CellWithRoad != null)
                        {
                            //(Map[new Vector3Int(position.x + i, position.y + j, 0)] as CellWithRoad).RemoveRoad(new Vector3Int(position.x + i, position.y + j, 0), position);
                        }
                    }
                }
            }
        }
    }
    public List<(int, int)> PositionsRoadAround((int,int) position)
    {
        List<(int,int)> ans = new List<(int, int)>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (Math.Abs(i) + Math.Abs(j) <= 1 && Map.ContainsKey((position.Item1 + i, position.Item2 + j)))
                {
                    if (Map[(position.Item1 + i, position.Item2 + j)] as CellWithRoad != null)
                    {
                        ans.Add((position.Item1 + i, position.Item2 + j));
                    }
                }
            }
        }
        return ans;
    }
}
