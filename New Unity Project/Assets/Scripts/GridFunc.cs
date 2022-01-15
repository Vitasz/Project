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
    public int Lines = 1;
    public HouseControlles houseControlles;
    public GameControlls GameController;
    public Camera nowCamera;
    public Tilemap tilemap;
    public Clock clock;
    public HumanController HumanControlles;
    public Dictionary<(int,int), Cell> Map = new Dictionary<(int, int), Cell>();
    public Dictionary<(int, int), CellWithRoad> Roads = new Dictionary<(int, int), CellWithRoad>();
    public OptimizationAlgorithm a;
    private readonly Dictionary<(int, int), List<(int, int)>> waysFromRoads = new Dictionary<(int, int), List<(int, int)>>();
    //FOR OPTIMIZATION
    public Dictionary<(int, int), Dictionary<(int, int), ((int, int),int)>> WaysFromRoadsToHouses = new Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>>();

    private int ModeRedactor = 0;
    Vector3Int prevpositionClick = new Vector3Int(), nowpositionClick = new Vector3Int();
    private bool hasfirstclick = false;
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            if (tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition)) != prevpositionClick || !hasfirstclick)
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
            AddWaysFromHouse(Position);
        }
        else if (type == ThingsInCell.RoadForCars)
        {
            Map.Add(Position, new CellWithRoad(this, Position, type, Lines));
            Roads.Add(Position, Map[Position] as CellWithRoad);
            waysFromRoads.Add(Position, new List<(int, int)>());
            AddWaysFromRoad(Position);
        }
       
    }
    public void UniteTiles((int,int) PositionFrom, List<(int, int)> PositionTo)
    {
        if (Roads.ContainsKey(PositionFrom))
        {
            Roads[PositionFrom].AddRoad(PositionTo);
            waysFromRoads[PositionFrom] = Roads[PositionFrom].GetNearRoadsWays();
            AddWaysFromRoad(PositionFrom);
            //(Map[PositionTo] as CellWithRoad).AddRoad(PositionTo, PositionFrom, false);
            //UpdateSystem();
        }
        /*else if (Map[PositionFrom] is CellWithHouse && Map[PositionTo] is CellWithHouse)
        {
            (Map[PositionFrom] as CellWithHouse).UniteHouse(PositionFrom, PositionTo, true, ForOA);
            (Map[PositionTo] as CellWithHouse).UniteHouse(PositionTo, PositionFrom, false, ForOA);
        }*/
    }

    private void AddWaysFromHouse((int, int) position)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>();
        HashSet<(int, int)> usedRoads = new HashSet<(int, int)>();
        int timer = 1;
        foreach ((int,int) a in Map[position].GetNearTiles())
        {
            if (Roads.ContainsKey(a))
            {
                nowpos.Add(a);
                if (!WaysFromRoadsToHouses.ContainsKey(a)) WaysFromRoadsToHouses.Add(a, new Dictionary<(int, int), ((int, int), int)>());
                WaysFromRoadsToHouses[a][position] = (position, 1);
                usedRoads.Add(a);
            }
        }
        
        while (nowpos.Count != 0)
        {
            timer++;
            foreach((int,int) a in nowpos)
            {
                //Продолжили движение по дороге
                foreach ((int, int) b in waysFromRoads[a])
                {
                    if (!usedRoads.Contains(b))
                    {
                        if (!WaysFromRoadsToHouses.ContainsKey(b))
                            WaysFromRoadsToHouses.Add(b, new Dictionary<(int, int), ((int, int), int)>());
                        WaysFromRoadsToHouses[b][position] = (a, timer);
                        usedRoads.Add(b);
                        newpos.Add(b);
                    }
                }
            }
            nowpos.Clear();
            foreach ((int, int) b in newpos) nowpos.Add(b);
            newpos.Clear();
        }
    }
    private void AddWaysFromRoad((int, int) position)
    {
        //ДОМА ВОКРУГ ДОРОГИ
        Dictionary<(int, int), ((int, int), int)> dictposition;
        if (!WaysFromRoadsToHouses.ContainsKey(position))
        {
            dictposition = new Dictionary<(int, int), ((int, int), int)>();
            WaysFromRoadsToHouses.Add(position, dictposition);
        }
        else dictposition = WaysFromRoadsToHouses[position];
        foreach ((int,int) a in Map[position].GetNearTiles())
        {
            if (Map.ContainsKey(a) && !Roads.ContainsKey(a))
            {
                dictposition[a] = (a, 1);
            }
        }

        //ДОРОГИ ВОКРУГ ТАЙЛА
        //Перезаписываем все дома в этот тайл
        foreach ((int, int) b in waysFromRoads[position])
        {
            Dictionary<(int, int), ((int, int), int)> dictb = WaysFromRoadsToHouses[b];
            foreach ((int, int) c in dictb.Keys)
            {
                if (!dictposition.ContainsKey(c))
                {
                    dictposition.Add(c, (b, dictb[c].Item2 + 1));
                }
                else if (dictposition[c].Item2 > dictb[c].Item2 + 1)
                {
                    dictposition[c] = (b, dictb[c].Item2 + 1);
                }
            }
        }
        foreach ((int,int) a in waysFromRoads[position])
        {
            Dictionary<(int, int), ((int, int), int)> dicta = WaysFromRoadsToHouses[a];
            List<((int, int),int)> housesneedtochange = new List<((int, int), int)>();//Дома, которые надо перезаписать, при помощи поиска в глубину
            foreach((int,int) house in dictposition.Keys)
            {
                if (!dicta.ContainsKey(house) || dicta[house].Item2 > dictposition[house].Item2 + 1)
                {
                    housesneedtochange.Add((house, dictposition[house].Item2 + 1));
                }
            }
            //Запускаем обход в глубину, перезаписывая те дома, которые необходимо
            if (housesneedtochange.Count != 0)
            {
                for (int i = 0; i < housesneedtochange.Count; i++)
                {
                    (int, int) nowhouse = housesneedtochange[i].Item1;
                    HashSet<(int, int)> nowpositions = new HashSet<(int, int)>(), nextpositions = new HashSet<(int, int)>();
                    nowpositions.Add(position);
                    int timer = dictposition[nowhouse].Item2;
                    while (nowpositions.Count != 0)
                    {
                        timer++;
                        foreach((int,int) f in nowpositions)
                        {
                            foreach((int,int) m in waysFromRoads[f])
                            {
                                Dictionary<(int, int), ((int, int), int)> dictm = WaysFromRoadsToHouses[m];
                                if (!dictm.ContainsKey(nowhouse) || dictm[nowhouse].Item2 > timer)
                                {
                                    nextpositions.Add(m);
                                    if (!dictm.ContainsKey(nowhouse)) dictm.Add(nowhouse, (f, timer));
                                    else dictm[nowhouse] = (f, timer);
                                }
                            }
                        }
                        nowpositions.Clear();
                        foreach ((int, int) f in nextpositions) nowpositions.Add(f);
                        nextpositions.Clear();
                    }
                }
            }
        }
    }
    public void Optimize()
    {
        a.Optimization(this);
    }
    public void RemoveHouseAt((int,int) position)
    {
        tilemap.SetTile(new Vector3Int(position.Item1, position.Item2, 1), null);
        houseControlles.RemoveHouse(Map[position] as CellWithHouse);
        Map.Remove(position);
        foreach ((int, int) a in WaysFromRoadsToHouses.Keys) WaysFromRoadsToHouses[a].Remove(position);
    }
    public void RemoveRoadAt((int, int) position)
    {
        tilemap.SetTile(new Vector3Int(position.Item1, position.Item2, 1), null);
        Roads.Remove(position);
        
        Map.Remove(position);
        HashSet<(int, int)> housesneedtorec = new HashSet<(int, int)>();
        foreach((int,int) a in waysFromRoads[position])
        {
            Roads[a].RemoveRoad(position);
            foreach((int,int) b in WaysFromRoadsToHouses[a].Keys)
            {
                if (WaysFromRoadsToHouses[a][b].Item1 == position) housesneedtorec.Add(b);
            }
            waysFromRoads[a] = Roads[a].GetNearRoadsWays();
        }
        waysFromRoads.Remove(position);
        WaysFromRoadsToHouses.Remove(position);
        foreach((int,int) a in WaysFromRoadsToHouses.Keys)
        {
            foreach((int,int) c in housesneedtorec)
            {
                WaysFromRoadsToHouses[a].Remove(c);
            }
        }
        foreach ((int, int) z in housesneedtorec) AddWaysFromHouse(z);
    }
}
