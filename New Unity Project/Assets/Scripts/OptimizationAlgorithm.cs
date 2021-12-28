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
    HashSet<(int, int)> Grans = new HashSet<(int, int)>();
    Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> WaysFromRoadsToHouses = new Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>>();

    List<Task> tasks = new List<Task>();
    List<((int, int), List<((int, int), ThingsInCell, List<(int, int)>)>)> variants = new List<((int, int), List<((int, int), ThingsInCell, List<(int, int)>)>)>();
    //(int, int) bestPosition = (0,0);
    int bestEfficienty = int.MaxValue;
    int maxway = int.MaxValue;
    //ThingsInCell bestVariant;
    //List<(int, int)> bestRoads= new List<(int, int)>();
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
        int cntHousePeople = 0, cntHouseCom = 0, cntHouseFact = 0, cntRoads;
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
        Roads.Clear();
        Houses.Clear();
        MapCopy.Clear();
        Grans.Clear();
        foreach ((int, int) a in grid.Map.Keys)
        {
            MapCopy.Add(a, grid.Map[a].GetTypeCell());
            if (MapCopy[a] == ThingsInCell.HouseCom) cntHouseCom++;
            if (MapCopy[a] == ThingsInCell.HouseFact) cntHouseFact++;
            if (MapCopy[a] == ThingsInCell.HousePeople) cntHousePeople++;
            if (MapCopy[a] == ThingsInCell.RoadForCars)
            {
                Roads.Add(a, grid.Roads[a].GetNearRoadsWays());
            }
            else Houses.Add(a, MapCopy[a]);
        }
        foreach((int,int)a in Roads.Keys)
        {
            if (IsGran(a)) Grans.Add(a);
        }
        while (HODS > 0)
        {
            variants.Clear();
            timepreparing = 0;
            total = 0;
            bestEfficienty = int.MaxValue;
            WaysFromRoadsToHouses = grid.WaysFromRoadsToHouses;
            cntRoads = Roads.Count;
            Dictionary<ThingsInCell, List<(int, int)>> Positions = new Dictionary<ThingsInCell, List<(int, int)>>();
            Positions.Add(ThingsInCell.HousePeople, new List<(int, int)>());
            Positions.Add(ThingsInCell.RoadForCars, new List<(int, int)>());
            foreach((int,int) a in Grans)
            {
                foreach ((int, int) b in GetEmptyTiles(a))
                {
                    if (!Positions[ThingsInCell.HousePeople].Contains(b))
                    {
                        Positions[ThingsInCell.HousePeople].Add(b);
                        Positions[ThingsInCell.RoadForCars].Add(b);
                    }
                }
            }
            
            Dictionary<ThingsInCell, int> Indexs = new Dictionary<ThingsInCell, int>();
            Indexs.Add(ThingsInCell.HousePeople, 0);
            Indexs.Add(ThingsInCell.RoadForCars, 0);
            Stopwatch hodtime = new Stopwatch();
            hodtime.Start();
            Hod(ref Positions, new List<((int, int), ThingsInCell, List<(int, int)>)>(), Deep, cntRoads, cntHousePeople, cntHouseCom, cntHouseFact,Indexs);
            Task.WaitAll(tasks.ToArray());
            
            hodtime.Stop();
            //UnityEngine.Debug.Log("TOTAL VARIANTS: " + Convert.ToString(total));
            //UnityEngine.Debug.Log("Time counting (ms): " + Convert.ToString((float) timepreparing / total));
            UnityEngine.Debug.Log("Time per variant (ms): " + Convert.ToString((float)hodtime.ElapsedMilliseconds/total) + " Total var: "+ Convert.ToString(total));
            bestEfficienty = int.MaxValue;
            int bestvar = -1;
            for (int i = 0; i < variants.Count; i++)
            {
                int nowefficiency = variants[i].Item1.Item1;
                int waymax = variants[i].Item1.Item2;
                if (bestEfficienty > nowefficiency && nowefficiency != int.MaxValue)
                {
                    bestvar = i;
                    bestEfficienty = nowefficiency;
                    maxway = waymax;
                    //bestPosition = a.Item2.Item1;
                    //bestVariant = a.Item2.Item2;
                    //bestRoads = a.Item2.Item3;
                }
                else if (bestEfficienty == nowefficiency)
                {
                    if (waymax < maxway)
                    {
                        bestvar = i;
                        bestEfficienty = nowefficiency;
                        maxway = waymax;
                       //bestPosition = a.Item2.Item1;
                       // bestVariant = a.Item2.Item2;
                       // bestRoads = a.Item2.Item3;
                    }
                    /*else if (a.Item1.Item2 == maxway)
                    {
                        if (a.Item2.Item2 == ThingsInCell.RoadForCars && a.Item2.Item3.Count > bestRoads.Count)
                        {
                            bestEfficienty = a.Item1.Item1;
                            maxway = a.Item1.Item2;
                            bestPosition = a.Item2.Item1;
                            bestVariant = a.Item2.Item2;
                            bestRoads = a.Item2.Item3;
                        }
                    }*/
                }
            }
            int toadd = Deep / 2 + 1;
            HODS-= toadd;
            if (bestvar!=-1)
            {
                variants[bestvar].Item2.RemoveRange(toadd, Deep-toadd);
                
                foreach (((int, int), ThingsInCell, List<(int, int)>) a in variants[bestvar].Item2)
                {
                    ThingsInCell whatadd;
                    if (cntRoads < (cntHouseCom + cntHousePeople + cntHouseFact)) whatadd = ThingsInCell.RoadForCars;
                    else if (cntHouseFact <= cntHouseCom && cntHouseFact <= cntHousePeople) whatadd = ThingsInCell.HouseFact;
                    else if (cntHouseCom <= cntHouseFact && cntHouseCom <= cntHousePeople) whatadd = ThingsInCell.HouseCom;
                    else whatadd = ThingsInCell.HousePeople;
                    grid.CreateNewTile(a.Item1, whatadd);
                    MapCopy.Add(a.Item1, whatadd);
                    if (whatadd == ThingsInCell.RoadForCars)
                    {
                        grid.UniteTiles(a.Item1, a.Item3);
                        foreach ((int, int) b in a.Item3)
                        {
                            grid.UniteTiles(b, new List<(int, int)>() { a.Item1 });
                        }
                        Roads.Add(a.Item1, a.Item3);
                        cntRoads++;
                        
                    }
                    else
                    {
                        Houses.Add(a.Item1, whatadd);
                        if (whatadd == ThingsInCell.HouseCom) cntHouseCom++;
                        if (whatadd == ThingsInCell.HouseFact) cntHouseFact++;
                        if (whatadd == ThingsInCell.HousePeople) cntHousePeople++;
                    }
                }
                foreach (((int, int), ThingsInCell, List<(int, int)>) a in variants[bestvar].Item2)
                {
                    if (a.Item2 == ThingsInCell.RoadForCars)
                    {
                        if (Grans.Contains(a.Item1) && !IsGran(a.Item1)) Grans.Remove(a.Item1);
                        else if (!Grans.Contains(a.Item1) && IsGran(a.Item1)) Grans.Add(a.Item1);
                    }
                    foreach ((int, int) b in GetNearTiles(a.Item1))
                    {
                        if (Grans.Contains(b) && !IsGran(b)) Grans.Remove(b);
                    }
                }
            }
            else HODS = 0;
            
            tasks.Clear();
            
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
            bool ok = true;
            foreach(ThingsInCell a in Indexs.Keys)
            {
               // UnityEngine.Debug.Log(a);
               // UnityEngine.Debug.Log(Positions[a].Count);
               // UnityEngine.Debug.Log(Indexs[a]);
                ok &= Positions[a].Count > Indexs[a];
            }
            if (ok)
            {
                List<((int, int), ThingsInCell, List<(int, int)>)> tilestoaddnew = new List<((int, int), ThingsInCell, List<(int, int)>)>();
                for (int i = 0; i < TilesToAdd.Count; i++)
                {
                    tilestoaddnew.Add(TilesToAdd[i]);
                }
                variants.Add(((int.MaxValue, int.MaxValue), new List<((int, int), ThingsInCell, List<(int, int)>)>() { ((int.MaxValue, int.MaxValue), ThingsInCell.Nothing, null) }));
                int tmp = total;
                //TestSystem(tilestoaddnew, tmp);
                tasks.Add(Task.Run(() => TestSystem(tilestoaddnew, tmp)));
                total++;
            }
            return;
        }
        ThingsInCell whatadd;
        if (cntroads < (cnthouseCom + cnthousePeople + cnthouseFact)) whatadd = ThingsInCell.RoadForCars;
        //else if (cnthouseFact <= cnthouseCom && cnthouseFact <= cnthousePeople) whatadd = ThingsInCell.HouseFact;
        //else if (cnthouseCom <= cnthouseFact && cnthouseCom <= cnthousePeople) whatadd = ThingsInCell.HouseCom;
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
        Dictionary<(int, int), HashSet<(int, int)>> newroads = new Dictionary<(int, int), HashSet<(int, int)>>();
        List<(int, int)> newhouses = new List<(int, int)>();
        Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWays = new Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>>();
        int newlength = 0, eco = 0, newwaymax = 0;
        foreach (((int, int), ThingsInCell, List<(int, int)>) b in TilesToAdd)
        {
            if (b.Item2 == ThingsInCell.RoadForCars)
            {
                newroads.Add(b.Item1, new HashSet<(int, int)>());
                foreach ((int, int) c in b.Item3)
                {
                    newroads[b.Item1].Add(c);
                }
                eco+=AddWaysFromRoad(b.Item1, newhouses, newroads, newWays);
            }
            else
            {
                newhouses.Add(b.Item1);
                int t =AddWaysFromHouse(b.Item1, newroads, newWays);
                newlength += t;
                newwaymax = Math.Max(t, newwaymax);
            }
        }
        (int, int) nowefficiency = (newlength-eco, newwaymax);
        bestEfficienty = Math.Min(nowefficiency.Item1, bestEfficienty);
        variants[number] = (nowefficiency, TilesToAdd);
    }
    List<(int,int)>GetNearTiles((int,int) position)
    {
        List<(int, int)> ans = new List<(int, int)>();
        ans.Add((position.Item1 + 1, position.Item2));
        ans.Add((position.Item1 - 1, position.Item2));
        ans.Add((position.Item1, position.Item2 + 1));
        ans.Add((position.Item1, position.Item2 - 1));
        return ans;
    }
    /// <summary>
    /// Добавляет в граф путей новый дом
    /// </summary>
    /// <param name="position">Позиция дома</param>
    /// <param name="Roads">Граф дорог</param>
    /// <param name="WaysFromRoadsToHouses">Граф города</param>
    /// <returns>Возвращает максимальный маршрут</returns>
    private int AddWaysFromHouse((int, int) position, Dictionary<(int, int), HashSet<(int, int)>> newRoads, Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWaysFromRoadsToHouses)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>();
        HashSet<(int, int)> usedRoads = new HashSet<(int, int)>();
        
        int timer = 1;
        int ans = 0;
        foreach ((int, int) a in GetNearTiles(position))
        {
            if (Roads.ContainsKey(a)||newRoads.ContainsKey(a))
            {
                nowpos.Add(a);
                if (!newWaysFromRoadsToHouses.ContainsKey(a)) newWaysFromRoadsToHouses.Add(a, new Dictionary<(int, int), ((int, int), int)>());
                newWaysFromRoadsToHouses[a][position] = (position, 1);
                ans++;
                usedRoads.Add(a);
            }
        }
        
        while (nowpos.Count != 0)
        {
            timer++;
            foreach ((int, int) a in nowpos)
            {
                //Продолжили движение по дороге
                if (Roads.ContainsKey(a))
                {
                    foreach ((int, int) b in Roads[a])
                    {
                        if (!usedRoads.Contains(b))
                        {
                            if (!newWaysFromRoadsToHouses.ContainsKey(b))
                                newWaysFromRoadsToHouses.Add(b, new Dictionary<(int, int), ((int, int), int)>());
                            newWaysFromRoadsToHouses[b][position] = (a, timer);
                            usedRoads.Add(b);
                            newpos.Add(b);
                        }
                    }
                }
                if (newRoads.ContainsKey(a))
                {
                    foreach((int, int) b in newRoads[a])
                    {
                        if (!usedRoads.Contains(b))
                        {
                            if (!newWaysFromRoadsToHouses.ContainsKey(b))
                                newWaysFromRoadsToHouses.Add(b, new Dictionary<(int, int), ((int, int), int)>());
                            newWaysFromRoadsToHouses[b][position] = (a, timer);
                            usedRoads.Add(b);
                            newpos.Add(b);
                            ans += timer;
                        }
                    }
                }
            }
            nowpos.Clear();
            foreach ((int, int) b in newpos) nowpos.Add(b);
            newpos.Clear();
        }
        return ans;
    }
    /// <summary>
    /// Добавляет новую дорогу в граф
    /// </summary>
    /// <param name="position">Позиция дороги</param>
    /// <param name="Houses">Позиции домов</param>
    /// <param name="Roads">Граф дорог</param>
    /// <param name="WaysFromRoadsToHouses">Граф города</param>
    /// <returns>Возвращает "экономию" расстояния между домами</returns>
    private int AddWaysFromRoad((int, int) position, List<(int,int)> newHouses, Dictionary<(int,int), HashSet<(int,int)>> newRoads, Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWaysFromRoadsToHouses)
    {
        //ДОМА ВОКРУГ ДОРОГИ
        Dictionary<(int, int), ((int, int), int)> dictposition;
        dictposition = new Dictionary<(int, int), ((int, int), int)>();
        newWaysFromRoadsToHouses.Add(position, dictposition);

        foreach ((int, int) a in GetNearTiles(position))
        {
            if (Houses.ContainsKey(a))
            {
                dictposition[a] = (a, 1);
            }
            else if (newHouses.Contains(a))
            {
                dictposition[a] = (a, 1);
            }
        }

        //ДОРОГИ ВОКРУГ ТАЙЛА
        //Перезаписываем все дома в этот тайл  
        foreach ((int, int) b in newRoads[position])
        {
            Dictionary<(int, int), ((int, int), int)> dictbNEW, dictbOLD;
            if (newWaysFromRoadsToHouses.ContainsKey(b))
            {
                dictbNEW = newWaysFromRoadsToHouses[b];
                foreach ((int, int) c in dictbNEW.Keys)
                {
                    if (!dictposition.ContainsKey(c))
                    {
                        dictposition.Add(c, (b, dictbNEW[c].Item2 + 1));
                    }
                    else if (dictposition[c].Item2 > dictbNEW[c].Item2 + 1)
                    {
                        dictposition[c] = (b, dictbNEW[c].Item2 + 1);
                    }
                }
            }
            if (WaysFromRoadsToHouses.ContainsKey(b))
            {
                dictbOLD = WaysFromRoadsToHouses[b];
                foreach ((int, int) c in dictbOLD.Keys)
                {
                    if (!dictposition.ContainsKey(c))
                    {
                        dictposition.Add(c, (b, dictbOLD[c].Item2 + 1));
                    }
                    else if (dictposition[c].Item2 > dictbOLD[c].Item2 + 1)
                    {
                        dictposition[c] = (b, dictbOLD[c].Item2 + 1);
                    }
                }
            }
            
        }
        
        //ПЕРЕСТРОЕНИЕ ГРАФА И РАССЧЁТ ЭКОНОМИИ
        int ans = 0;
        foreach ((int, int) a in newRoads[position])
        {
            HashSet<(int, int)> housesskipped = new HashSet<(int, int)>();
            List<((int, int), int)> housesneedtochange = new List<((int, int), int)>();//Дома, которые надо перезаписать, при помощи поиска в глубину
            if (newWaysFromRoadsToHouses.ContainsKey(a))
            {
                Dictionary<(int, int), ((int, int), int)> dicta = newWaysFromRoadsToHouses[a];
                foreach ((int, int) house in dictposition.Keys)
                {
                    if (!dicta.ContainsKey(house))
                    {
                        housesneedtochange.Add((house, dictposition[house].Item2 + 1));
                    }
                    else if (dicta[house].Item2 > dictposition[house].Item2 + 1)
                    {
                        housesneedtochange.Add((house, dictposition[house].Item2 + 1));
                        ans += dicta[house].Item2 - dictposition[house].Item2 - 1;
                    }
                    housesskipped.Add(house);
                }
            }
            if (WaysFromRoadsToHouses.ContainsKey(a)) {
                Dictionary<(int, int), ((int, int), int)> dicta = WaysFromRoadsToHouses[a];
                foreach ((int, int) house in dictposition.Keys)
                {
                    if (!housesskipped.Contains(house))
                    {
                        if (!dicta.ContainsKey(house))
                        {
                            housesneedtochange.Add((house, dictposition[house].Item2 + 1));
                        }
                        else if (dicta[house].Item2 > dictposition[house].Item2 + 1)
                        {
                            housesneedtochange.Add((house, dictposition[house].Item2 + 1));
                            ans += dicta[house].Item2 - dictposition[house].Item2 - 1;
                        }
                    }
                }
            }



            //Запускаем обход в глубину, перезаписывая те дома, которые необходимо
            /*if (housesneedtochange.Count != 0)
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
                        foreach ((int, int) f in nowpositions)
                        {
                            if (Roads.ContainsKey(f))
                                foreach ((int, int) m in Roads[f])
                                {

                                    Dictionary<(int, int), ((int, int), int)> dictm;
                                    if (!newWaysFromRoadsToHouses.ContainsKey(m))
                                    {
                                        dictm = new Dictionary<(int, int), ((int, int), int)>();
                                        newWaysFromRoadsToHouses.Add(m, new Dictionary<(int, int), ((int, int), int)>());
                                    }
                                    else dictm = newWaysFromRoadsToHouses[m];
                                    if (!dictm.ContainsKey(nowhouse) || dictm[nowhouse].Item2 > timer)
                                    {
                                        nextpositions.Add(m);
                                        if (!dictm.ContainsKey(nowhouse)) dictm.Add(nowhouse, (f, timer));
                                        else dictm[nowhouse] = (f, timer);
                                    }
                                }
                            if (newRoads.ContainsKey(f))
                            {
                                foreach ((int, int) m in newRoads[f])
                                {

                                    Dictionary<(int, int), ((int, int), int)> dictm;
                                    if (!newWaysFromRoadsToHouses.ContainsKey(m))
                                    {
                                        dictm = new Dictionary<(int, int), ((int, int), int)>();
                                        newWaysFromRoadsToHouses.Add(m, new Dictionary<(int, int), ((int, int), int)>());
                                    }
                                    else dictm = newWaysFromRoadsToHouses[m];
                                    if (!dictm.ContainsKey(nowhouse) || dictm[nowhouse].Item2 > timer)
                                    {
                                        nextpositions.Add(m);
                                        if (!dictm.ContainsKey(nowhouse)) dictm.Add(nowhouse, (f, timer));
                                        else dictm[nowhouse] = (f, timer);
                                    }
                                }
                            }
                        }
                        nowpositions.Clear();
                        foreach ((int, int) f in nextpositions) nowpositions.Add(f);
                        nextpositions.Clear();
                    }
                }
            }*/
        }
        return ans;
    }
}
