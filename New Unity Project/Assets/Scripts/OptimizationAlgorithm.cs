using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

public class OptimizationAlgorithm : MonoBehaviour
{
    List<CellWithHouse> GranHouse = new List<CellWithHouse>();
    List<CellWithRoad> GranRoad = new List<CellWithRoad>();
    //GridFunc grid;
    public int Deep = 1;
    //List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople, ThingsInCell.HouseCom, ThingsInCell.HouseFact, ThingsInCell.RoadForCars };
    Dictionary<(int,int), List<(int, int)>> Roads = new Dictionary<(int, int), List<(int, int)>>();
    Dictionary<(int, int), ThingsInCell> Houses = new Dictionary<(int, int), ThingsInCell>();
    Dictionary<(int,int), ThingsInCell> MapCopy = new Dictionary<(int, int), ThingsInCell>();
    Dictionary<((int, int), (int, int)), List<(int, int)>> WaysFrom = new Dictionary<((int, int), (int, int)), List<(int, int)>>();
    
    List<Task> tasks = new List<Task>();
    List<((int, int), ((int, int), ThingsInCell, List<(int, int)>))> variants = new List<((int, int), ((int, int), ThingsInCell, List<(int, int)>))>();
    (int, int) bestPosition = (0,0);
    int bestEfficienty = int.MaxValue;
    int maxway = int.MaxValue;
    ThingsInCell bestVariant;
    List<(int, int)> bestRoads= new List<(int, int)>();
    public int HODS = 0;
    private int total = 0;
    private long timepreparing = 0;
    private bool IsGran((int,int) position)
    {
        return !(MapCopy.ContainsKey((position.Item1 - 1, position.Item2)) &&
            MapCopy.ContainsKey((position.Item1, position.Item2 - 1)) &&
            MapCopy.ContainsKey((position.Item1 + 1, position.Item2)) &&
            MapCopy.ContainsKey((position.Item1, position.Item2 + 1)));
    }
    public void Optimization(GridFunc grid)
    {

        StartCoroutine(F(grid));
    }
    IEnumerator F(GridFunc grid)
    {
        int cntHousePeople = 0, cntHouseCom = 0, cntHouseFact = 0, cntRoads = 0;
        List<(int,int)> GetEmptyTiles((int, int) a)
        {
            List<(int, int)> ans2 = new List<(int, int)>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    (int, int) now = (a.Item1 + i, a.Item2 + j);
                    if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1&&!MapCopy.ContainsKey(now)) ans2.Add(now);
                }
            }
            return ans2;
        }
       
        long TOTALTIME = 0;
        
        while (HODS != 0)
        {
            variants.Clear();
            timepreparing = 0;
            total = 0;
            cntHousePeople = 0;
            cntHouseCom = 0;
            cntHouseFact = 0;
            Stopwatch optimization = new Stopwatch();
            optimization.Start();
            foreach((int,int) a in grid.Map.Keys)
            {
                MapCopy.Add(a, grid.Map[a].GetTypeCell());
                if (MapCopy[a] == ThingsInCell.HouseCom) cntHouseCom++;
                if (MapCopy[a] == ThingsInCell.HouseFact) cntHouseFact++;
                if (MapCopy[a] == ThingsInCell.HousePeople) cntHousePeople++;
                if (MapCopy[a] == ThingsInCell.RoadForCars) Roads.Add(a, grid.Roads[a].GetNearRoadsWays());
                else Houses.Add(a, MapCopy[a]);
            }
            WaysFrom = grid.WaysFromTo;
            cntRoads = Roads.Count;
            Dictionary<ThingsInCell, List<(int, int)>> Positions = new Dictionary<ThingsInCell, List<(int, int)>>();
            Positions.Add(ThingsInCell.HousePeople, new List<(int, int)>());
            Positions.Add(ThingsInCell.HouseCom, new List<(int, int)>());
            Positions.Add(ThingsInCell.HouseFact, new List<(int, int)>());
            Positions.Add(ThingsInCell.RoadForCars, new List<(int, int)>());
            foreach((int,int) a in Roads.Keys)
            {
                if (IsGran(a))
                {
                    foreach((int,int) b in GetEmptyTiles(a))
                    {
                        if (!Positions[ThingsInCell.HousePeople].Contains(b))
                        {
                            Positions[ThingsInCell.HousePeople].Add(b);
                            Positions[ThingsInCell.HouseCom].Add(b);
                            Positions[ThingsInCell.HouseFact].Add(b);
                            Positions[ThingsInCell.RoadForCars].Add(b);
                        }
                    }
                }
            }
            Dictionary<ThingsInCell, int> Indexs = new Dictionary<ThingsInCell, int>();
            Indexs.Add(ThingsInCell.HousePeople, 0);
            Indexs.Add(ThingsInCell.HouseCom, 0);
            Indexs.Add(ThingsInCell.HouseFact, 0);
            Indexs.Add(ThingsInCell.RoadForCars, 0);
            Stopwatch hodtime = new Stopwatch();
            hodtime.Start();
            Hod(ref Positions, new List<((int, int), ThingsInCell, List<(int, int)>)>(), Deep, cntRoads, cntHousePeople, cntHouseCom, cntHouseFact,Indexs);
            Task.WaitAll(tasks.ToArray());
            foreach(((int,int), ((int, int), ThingsInCell, List<(int, int)>)) a in variants)
            {
                if (bestEfficienty > a.Item1.Item1&&a.Item1.Item1!=-1)
                {
                    bestEfficienty = a.Item1.Item1;
                    maxway = a.Item1.Item2;
                    bestPosition = a.Item2.Item1;
                    bestVariant = a.Item2.Item2;
                    bestRoads = a.Item2.Item3;
                }
                else if (bestEfficienty == a.Item1.Item1)
                {
                    if (a.Item1.Item2 < maxway)
                    {
                        bestEfficienty = a.Item1.Item1;
                        maxway = a.Item1.Item2;
                        bestPosition = a.Item2.Item1;
                        bestVariant = a.Item2.Item2;
                        bestRoads = a.Item2.Item3;
                    }
                    else if (a.Item1.Item2 == maxway)
                    {
                        if (a.Item2.Item2 == ThingsInCell.RoadForCars && a.Item2.Item3.Count > bestRoads.Count)
                        {
                            bestEfficienty = a.Item1.Item1;
                            maxway = a.Item1.Item2;
                            bestPosition = a.Item2.Item1;
                            bestVariant = a.Item2.Item2;
                            bestRoads = a.Item2.Item3;
                        }
                    }
                }
            }
            hodtime.Stop();
            //UnityEngine.Debug.Log("TOTAL VARIANTS: " + Convert.ToString(total));
            UnityEngine.Debug.Log("Time counting (ms): " + Convert.ToString((float)timepreparing / total));
            UnityEngine.Debug.Log("Time per variant (ms): " + Convert.ToString((float)hodtime.ElapsedMilliseconds/total) + " Total var: "+ Convert.ToString(total));
            
            HODS--;
            if (bestEfficienty != int.MaxValue)
            {
                grid.CreateNewTile(bestPosition, bestVariant);
                if (bestVariant == ThingsInCell.RoadForCars)
                {
                    grid.UniteTiles(bestPosition, bestRoads);
                    foreach ((int, int) a in bestRoads)
                    {
                        grid.UniteTiles(a, new List<(int, int)>() { bestPosition });
                    }
                }
            }
            else HODS = 0;
            bestEfficienty = int.MaxValue;
            GranRoad.Clear();
            GranHouse.Clear();
            MapCopy.Clear();
            Roads.Clear();
            Houses.Clear();
            tasks.Clear();
            optimization.Stop();
            TOTALTIME += optimization.ElapsedMilliseconds;
            yield return new WaitForEndOfFrame();
        }
        UnityEngine.Debug.Log("TOTAL TIME: " + Convert.ToString(TOTALTIME));
        yield return null;
    }

    private List<List<(int, int)>> GetRoadsVariants((int, int) Position, List<(int,int)>RoadsAround)
    {
        List<List< (int, int) >> res = new List<List<(int, int)>>();
        for (int i = 0; i < RoadsAround.Count; i++)
        {
            int cnt = res.Count;
            for (int j = 0; j < cnt; j++)
            {
                res.Add(new List<(int, int)>());
                foreach ((int, int) a in res[j]) res[res.Count - 1].Add(a);
                res[res.Count - 1].Add(RoadsAround[i]);
            }
            res.Add(new List<(int, int)>() { RoadsAround[i] });
        }
        return res;
    }
    private void Hod(ref Dictionary<ThingsInCell, List<(int, int)>> Positions, List<((int,int), ThingsInCell, List<(int,int)>)>TilesToAdd, int deep,
        int cntroads, int cnthousePeople, int cnthouseCom, int cnthouseFact, Dictionary<ThingsInCell, int> Indexs)
    {
        if (deep == 0)
        {
            
            List<((int, int), ThingsInCell, List<(int, int)>)> tilestoaddnew = new List<((int, int), ThingsInCell, List<(int, int)>)>();
            for (int i = 0; i < TilesToAdd.Count; i++)
            {
                tilestoaddnew.Add(TilesToAdd[i]);
            }
            variants.Add(((-1, -1), ((-1, -1), ThingsInCell.Nothing, null)));
            int tmp = total;
            //TestSystem(tilestoaddnew, tmp);
            tasks.Add(Task.Run(() => TestSystem(tilestoaddnew, tmp)));
            total++;
            return;
        }
        ThingsInCell whatadd;
        if (cntroads <= (cnthouseCom + cnthousePeople + cnthouseFact)*1) whatadd = ThingsInCell.RoadForCars;
        else if (cnthouseFact <= cnthouseCom && cnthouseFact <= cnthousePeople) whatadd = ThingsInCell.HouseFact;
        else if (cnthouseCom <= cnthouseFact && cnthouseCom <= cnthousePeople) whatadd = ThingsInCell.HouseCom;
        else  whatadd = ThingsInCell.HousePeople;

        List<(int,int)> GetEmptyTiles((int, int)pos){
            List<(int, int)> ans = new List<(int, int)>();
            for (int x = -1; x < 2; x += 2)
            {
                (int, int) tmp = (pos.Item1 + x, pos.Item2);
                if (!Roads.ContainsKey(tmp) && !Houses.ContainsKey(tmp))
                {
                    bool notok = false;
                    for (int j = 0; j < TilesToAdd.Count; j++)
                    {
                        if (TilesToAdd[j].Item1 == pos)
                        {
                            notok = true;
                            break;
                        }
                    }
                    if (notok) continue;
                    ans.Add(tmp);
                }
            }
            for (int y = -1; y < 2; y += 2)
            {
                (int, int) tmp = (pos.Item1, pos.Item2 + y);
                if (!Roads.ContainsKey(tmp) && !Houses.ContainsKey(tmp))
                {
                    bool notok = false;
                    for (int j = 0; j < TilesToAdd.Count; j++)
                    {
                        if (TilesToAdd[j].Item1 == pos)
                        {
                            notok = true;
                            break;
                        }
                    }
                    if (notok) continue;
                    ans.Add(tmp);
                }
            }

            return ans;
        }
        int save = Indexs[whatadd];
        for (int i = Indexs[whatadd]; i < Positions[whatadd].Count; i++)
        {
            
            Indexs[whatadd] = i + 1;
            (int, int) Position = Positions[whatadd][i];
            bool notok = false;
            for (int j = 0; j < TilesToAdd.Count; j++)
            {
                if (TilesToAdd[j].Item1 == Position)
                {
                    notok = true;
                    break;
                }
            }
            if (notok) continue;
            if (whatadd == ThingsInCell.RoadForCars)
            {
                List<(int, int)> RoadsAround = new List<(int, int)>();
                for (int x = -1; x < 2; x += 2)
                {
                    (int, int) tmp = (Position.Item1 + x, Position.Item2);
                    if (Roads.ContainsKey(tmp))
                    {
                        RoadsAround.Add(tmp);
                    }
                    else
                    {
                        foreach (((int, int), ThingsInCell, List<(int, int)>) a in TilesToAdd)
                        {
                            if (a.Item1 == tmp && a.Item2 == ThingsInCell.RoadForCars)
                            {
                                RoadsAround.Add(tmp);
                                continue;
                            }
                        }
                    }
                }
                for (int y = -1; y < 2; y += 2)
                {
                    (int, int) tmp = (Position.Item1, Position.Item2 + y);
                    if (Roads.ContainsKey(tmp))
                    {
                        RoadsAround.Add(tmp);
                    }
                    else
                    {
                        foreach (((int, int), ThingsInCell, List<(int, int)>) a in TilesToAdd)
                        {
                            if (a.Item1 == tmp && a.Item2 == ThingsInCell.RoadForCars)
                            {
                                RoadsAround.Add(tmp);
                                continue;
                            }
                        }
                    }
                }
                int cnt = 0;
                foreach ((int, int) b in GetEmptyTiles(Position))
                {
                    if (!Positions[ThingsInCell.HousePeople].Contains(b))
                    {
                        Positions[ThingsInCell.HousePeople].Add(b);
                        Positions[ThingsInCell.HouseCom].Add(b);
                        Positions[ThingsInCell.HouseFact].Add(b);
                        Positions[ThingsInCell.RoadForCars].Add(b);
                        cnt++;
                    }
                }
                
                foreach (List<(int, int)> a in GetRoadsVariants(Position, RoadsAround))
                {
                    TilesToAdd.Add((Position, ThingsInCell.RoadForCars, a));

                    Hod(ref Positions, TilesToAdd, deep - 1, cntroads + 1, cnthousePeople, cnthouseCom, cnthouseFact, Indexs);
                    
                    TilesToAdd.RemoveAt(TilesToAdd.Count - 1);
                }
                for (int j = 0; j < cnt; j++)
                {
                    Positions[ThingsInCell.HousePeople].RemoveAt(Positions[ThingsInCell.HousePeople].Count - 1);
                    Positions[ThingsInCell.HouseCom].RemoveAt(Positions[ThingsInCell.HouseCom].Count - 1);
                    Positions[ThingsInCell.HouseFact].RemoveAt(Positions[ThingsInCell.HouseFact].Count - 1);
                    Positions[ThingsInCell.RoadForCars].RemoveAt(Positions[ThingsInCell.RoadForCars].Count - 1);
                }
            }
            else
            {
                TilesToAdd.Add((Position, whatadd, null));
                if (whatadd == ThingsInCell.HousePeople) cnthousePeople++;
                if (whatadd == ThingsInCell.HouseCom) cnthouseCom++;
                if (whatadd == ThingsInCell.HouseFact) cnthouseFact++;

                Hod(ref Positions, TilesToAdd, deep - 1, cntroads, cnthousePeople, cnthouseCom, cnthouseFact, Indexs);

                if (whatadd == ThingsInCell.HousePeople) cnthousePeople--;
                if (whatadd == ThingsInCell.HouseCom) cnthouseCom--;
                if (whatadd == ThingsInCell.HouseFact) cnthouseFact--;
                TilesToAdd.RemoveAt(TilesToAdd.Count - 1);
            }

        }
        Indexs[whatadd] = save;

    }

    private void TestSystem(List<((int, int), ThingsInCell, List<(int, int)>)> TilesToAdd, int number)
    {
        Dictionary<(int, int), HashSet<(int, int)>> roads = new Dictionary<(int, int), HashSet<(int, int)>>();
        HashSet<(int, int)> houses = new HashSet<(int, int)>();
        foreach ((int, int) a in Roads.Keys)
        {
            roads.Add(a, new HashSet<(int, int)>());
            foreach ((int, int) b in Roads[a])
            {
                roads[a].Add(b);
            }
        }
        foreach ((int, int) a in Houses.Keys) houses.Add(a);
        Dictionary<((int, int), (int, int)), List<(int, int)>> nowWays = new Dictionary<((int, int), (int, int)), List<(int, int)>>();
        foreach (((int, int), (int, int)) a in WaysFrom.Keys)
        {
            nowWays.Add(a, WaysFrom[a]);
        }
        foreach (((int, int), ThingsInCell, List<(int, int)>) b in TilesToAdd)
        {
            if (b.Item2 == ThingsInCell.RoadForCars)
            {
                roads.Add(b.Item1, new HashSet<(int, int)>());
                foreach ((int, int) c in b.Item3)
                {
                    roads[c].Add(b.Item1);
                    roads[b.Item1].Add(c);
                }
                AddRoadToTime(b.Item1, nowWays, houses, roads);
            }
            else
            {
                houses.Add(b.Item1);
                AddHouseToTime(b.Item1, nowWays, houses, roads);
            }
        }
        if (nowWays.Keys.Count != (houses.Count - 1) * houses.Count) {
            UnityEngine.Debug.LogError("Can't find way");
            return;
        }
        List<HashSet<(int, int)>> nowTime = new List<HashSet<(int, int)>>();
        int errors=0, length = 0, waymax=0;
        foreach(List<(int,int)> a in nowWays.Values)
        {
            for (int i = 0; i < a.Count; i++)
            {
                if (i == nowTime.Count)
                {
                    nowTime.Add(new HashSet<(int, int)>());
                }
                if (nowTime[i].Contains(a[i])) errors++;
                else nowTime[i].Add(a[i]);
            }
            length += a.Count;
            waymax = Math.Max(waymax, a.Count);
        }
        (int, int) nowefficiency = (errors+length, waymax);
        variants[number] = (nowefficiency, TilesToAdd[0]);
    }
    private void AddHouseToTime((int, int) position, Dictionary<((int, int), (int, int)), List<(int, int)>> WaysFromTo, HashSet<(int, int)> Houses, Dictionary<(int, int), HashSet<(int, int)>> Roads)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>(), startpos = new HashSet<(int, int)>();
        List<(int, int)> GetNearTiles((int, int) position)
        {
            List<(int, int)> ans = new List<(int, int)>();
            ans.Add((position.Item1 - 1, position.Item2));
            ans.Add((position.Item1 + 1, position.Item2));
            ans.Add((position.Item1, position.Item2 - 1));
            ans.Add((position.Item1, position.Item2+1));
            return ans;
        }
        foreach ((int, int) a in GetNearTiles(position))
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
        foreach ((int, int) a in Houses)
        {
            foreach ((int, int) b in GetNearTiles(a))
            {
                if (Roads.ContainsKey(b))
                {
                    if (!HousesAround.ContainsKey(b)) HousesAround.Add(b, new List<(int, int)>());
                    HousesAround[b].Add(a);
                }
            }
        }
        while (nowpos.Count != 0)
        {
            foreach ((int, int) a in nowpos)
            {
                if (HousesAround.ContainsKey(a))
                {
                    //Добрались до дома
                    foreach ((int, int) b in HousesAround[a])
                    {
                        if (b != position && !housesreached.Contains(b))
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
                foreach ((int, int) b in Roads[a])
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
    private void AddRoadToTime((int, int) position, Dictionary<((int, int), (int, int)), List<(int, int)>> WaysFromTo, HashSet<(int,int)>Houses, Dictionary<(int,int), HashSet<(int,int)>> Roads)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>();
        nowpos.Add(position);
        Dictionary<(int, int), (int, (int, int))> usedRoads = new Dictionary<(int, int), (int, (int, int))>();
        Dictionary<(int, int), List<(int, int)>> RoadsAround = new Dictionary<(int, int), List<(int, int)>>();
        List<(int, int)> RoadsAroundKeys = new List<(int, int)>();
        List<(int, int)> GetNearTiles((int, int) position)
        {
            List<(int, int)> ans = new List<(int, int)>();
            ans.Add((position.Item1 - 1, position.Item2));
            ans.Add((position.Item1 + 1, position.Item2));
            ans.Add((position.Item1, position.Item2 - 1));
            ans.Add((position.Item1, position.Item2 + 1));
            return ans;
        }
        
        
        foreach ((int, int) a in Houses)
        {
            foreach ((int, int) b in GetNearTiles(a))
            {
                if (Roads.ContainsKey(b))
                {
                    if (!RoadsAround.ContainsKey(a))
                    {
                        RoadsAround.Add(a, new List<(int, int)>());
                        RoadsAroundKeys.Add(a);
                    }
                    RoadsAround[a].Add(b);

                }
            }
        }
        usedRoads.Add(position, (0, (0, 0)));
        int timer = 1;
        while (nowpos.Count != 0)
        {
            foreach ((int, int) a in nowpos)
            {
                //Продолжили движение по дороге
                foreach ((int, int) b in Roads[a])
                {
                    if (!usedRoads.ContainsKey(b))
                    {
                        usedRoads.Add(b, (timer, a));
                        newpos.Add(b);
                    }
                }
            }
            nowpos.Clear();
            foreach ((int, int) b in newpos) nowpos.Add(b);
            newpos.Clear();
            timer++;
        }
        for (int i = 0; i < RoadsAroundKeys.Count; i++)
        {
            (int, int) a = RoadsAroundKeys[i];
            for (int j = i+1; j < RoadsAroundKeys.Count; j++)
            {
                (int, int) b = RoadsAroundKeys[j];
                int min1 = int.MaxValue, min2 = int.MaxValue;
                (int, int) posa = (0, 0), posb = (0, 0);
                foreach ((int, int) c in RoadsAround[a])
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
                if (min1 != int.MaxValue && min2 != int.MaxValue && (!WaysFromTo.ContainsKey((a, b)) || min1 + min2 + 1 < WaysFromTo[(a, b)].Count))
                {
                    List<(int, int)> tmpway = new List<(int, int)>();
                    (int, int) last = posb;
                    while (last != position)
                    {
                        tmpway.Add(last);
                        last = usedRoads[last].Item2;
                    }
                    tmpway.Add(last);
                    List<(int, int)> tmpway2 = new List<(int, int)>();
                    last = posa;
                    while (last != position)
                    {
                        tmpway2.Add(last);
                        last = usedRoads[last].Item2;
                    }
                    for (int i1 = tmpway2.Count-1; i1 >=0; i1--)
                    {
                        tmpway.Add(tmpway2[i1]);
                    }
                    if (WaysFromTo.ContainsKey((b, a)))
                    {
                        WaysFromTo[(b, a)] = tmpway;
                        tmpway.Reverse();
                        WaysFromTo[(a,b)] = tmpway;
                    }
                    else
                    {
                        WaysFromTo.Add((b, a), tmpway);
                        tmpway.Reverse();
                        WaysFromTo.Add((a, b), tmpway);
                    }
                }

            }
        }
                    
    }
}
