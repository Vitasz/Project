using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

public class OptimizationAlgorithm : MonoBehaviour
{
    public int Deep = 1;
    public float k = 1.2f;
    public bool CanRemoveHouses = false;
    public bool CanRemoveRoads = false;
    public int HODS = 0;

    Dictionary<(int,int), List<(int, int)>> Roads = new Dictionary<(int, int), List<(int, int)>>();
    Dictionary<(int, int), ThingsInCell> Houses = new Dictionary<(int, int), ThingsInCell>();
    Dictionary<(int,int), ThingsInCell> MapCopy = new Dictionary<(int, int), ThingsInCell>();
    
    HashSet<(int, int)> Grans = new HashSet<(int, int)>();
    Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> WaysFromRoadsToHouses = new Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>>();
    HashSet<(int, int)> addedHouses = new HashSet<(int, int)>();
    HashSet<(int, int)> addedRoads = new HashSet<(int, int)>();
    List<Task> tasks = new List<Task>();
    List<((int, int), List<((int, int), ThingsInCell, List<(int, int)>)>,(int,int), (int,int))> variants = new List<((int, int), List<((int, int), ThingsInCell, List<(int, int)>)>, (int, int), (int, int))>();
    int bestEfficienty = int.MaxValue;
    int maxway = int.MaxValue;
   
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
        addedHouses.Clear();
        addedRoads.Clear();
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
            if (addedHouses.Count == 0 && addedRoads.Count == 0 || !CanRemoveHouses && !CanRemoveRoads) Hod(ref Positions, new List<((int, int), ThingsInCell, List<(int, int)>)>(), Deep, cntRoads, cntHousePeople + cntHouseCom + cntHouseFact, Indexs, (int.MaxValue, int.MaxValue), (int.MaxValue, int.MaxValue));
            else
            {
                if (CanRemoveHouses)
                {
                    foreach ((int, int) a in addedHouses)
                    {
                       
                        foreach (List<(int, int)> b in GetRoadsVariants(a, GetRoadsAround(a, new List<((int, int), ThingsInCell, List<(int, int)>)>(), (int.MaxValue, int.MaxValue))))
                        {
                            List<((int, int), ThingsInCell, List<(int, int)>)> tmp = new List<((int, int), ThingsInCell, List<(int, int)>)>();
                            tmp.Add((a, ThingsInCell.RoadForCars, b));
                            Hod(ref Positions, tmp, Deep, cntRoads + 1, cntHousePeople + cntHouseCom + cntHouseFact - 1, Indexs, a, (int.MaxValue, int.MaxValue));
                        }
                        
                    }
                }
                if (CanRemoveRoads)
                {
                    foreach ((int, int) a in addedRoads)
                    {
                        List<((int, int), ThingsInCell, List<(int, int)>)> tmp = new List<((int, int), ThingsInCell, List<(int, int)>)>();
                        tmp.Add((a, ThingsInCell.HousePeople, null));
                        Hod(ref Positions, tmp, Deep, cntRoads-1, cntHousePeople + cntHouseCom + cntHouseFact+1, Indexs, (int.MaxValue, int.MaxValue), a);

                    }
                }
            }
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
                }
                else if (bestEfficienty == nowefficiency)
                {
                    if (waymax < maxway)
                    {
                        bestvar = i;
                        bestEfficienty = nowefficiency;
                        maxway = waymax;
                    }
                }
            }
            int toadd = Deep+1;
            HODS-= toadd;
            if (bestvar!=-1)
            {
                //addedHouses.Clear();
                //addedRoads.Clear();
                if (variants[bestvar].Item3.Item1 != int.MaxValue)
                {
                    ThingsInCell typehouse = grid.GetCell(variants[bestvar].Item3).GetTypeCell();
                    grid.RemoveHouseAt(variants[bestvar].Item3);
                    Houses.Remove(variants[bestvar].Item3);
                    addedHouses.Remove(variants[bestvar].Item3);
                    MapCopy.Remove(variants[bestvar].Item3);
                    if (typehouse == ThingsInCell.HousePeople) cntHousePeople--;
                    if (typehouse == ThingsInCell.HouseCom) cntHouseCom--;
                    if (typehouse == ThingsInCell.HouseFact) cntHouseFact--;
                    HODS++;
                }
                if (variants[bestvar].Item4.Item1 != int.MaxValue)
                {
                    grid.RemoveRoadAt(variants[bestvar].Item4);
                    foreach((int,int) a in Roads[variants[bestvar].Item4])
                    {
                        Roads[a].Remove(variants[bestvar].Item4);
                    }
                    Roads.Remove(variants[bestvar].Item4);
                    addedRoads.Remove(variants[bestvar].Item4);
                    MapCopy.Remove(variants[bestvar].Item4);
                    if (Grans.Contains(variants[bestvar].Item4)) Grans.Remove(variants[bestvar].Item4);
                    cntRoads--;
                    HODS++;
                }
                foreach (((int, int), ThingsInCell, List<(int, int)>) a in variants[bestvar].Item2)
                {
                    ThingsInCell whatadd=a.Item2;
                    if (whatadd != ThingsInCell.RoadForCars)
                    {
                        if (cntHouseFact <= cntHouseCom && cntHouseFact <= cntHousePeople) whatadd = ThingsInCell.HouseFact;
                        else if (cntHouseCom <= cntHouseFact && cntHouseCom <= cntHousePeople) whatadd = ThingsInCell.HouseCom;
                        else whatadd = ThingsInCell.HousePeople;
                    }
                    grid.CreateNewTile(a.Item1, whatadd);
                    MapCopy.Add(a.Item1, whatadd);
                    if (whatadd == ThingsInCell.RoadForCars)
                    {
                        grid.UniteTiles(a.Item1, a.Item3);
                        foreach ((int, int) b in a.Item3)
                        {
                            Roads[b].Add(a.Item1);
                        }
                        foreach ((int, int) b in a.Item3)
                        {
                            grid.UniteTiles(b, new List<(int, int)>() { a.Item1 });
                        }
                        Roads.Add(a.Item1, a.Item3);
                        addedRoads.Add(a.Item1);
                        cntRoads++;
                        
                    }
                    else
                    {
                        addedHouses.Add(a.Item1);
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
                foreach ((int, int) b in GetNearTiles(variants[bestvar].Item3))
                {
                    if (!Grans.Contains(b) && IsGran(b) && Roads.ContainsKey(b)) Grans.Add(b);
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
        int cntroads, int cntHouses, Dictionary<ThingsInCell, int> Indexs, (int,int)housetoremove, (int,int) roadtoremove)
    {
        if (deep == 0)
        {
            if (Positions[ThingsInCell.HousePeople].Count>Deep)
            {
                List<((int, int), ThingsInCell, List<(int, int)>)> tilestoaddnew = new List<((int, int), ThingsInCell, List<(int, int)>)>();
               
                for (int i = 0; i < TilesToAdd.Count; i++)
                {
                    //if (TilesToAdd[i].Item2 == ThingsInCell.RoadForCars && TilesToAdd[i].Item3 == null) UnityEngine.Debug.LogError("");
                    tilestoaddnew.Add(TilesToAdd[i]);
                }
                variants.Add(((int.MaxValue, int.MaxValue), tilestoaddnew, (int.MaxValue, int.MaxValue), (int.MaxValue, int.MaxValue)));
                int tmp = total;
                //TestSystem(tilestoaddnew, housetoremove, roadtoremove, tmp);
                tasks.Add(Task.Run(() => TestSystem(tilestoaddnew, housetoremove, roadtoremove, tmp)));
                total++;
            }
            return;
        }
        ThingsInCell whatadd;
        
        if (cntroads <= cntHouses * k) whatadd = ThingsInCell.RoadForCars;
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
                
                foreach (List<(int, int)> a in GetRoadsVariants(Position, GetRoadsAround(Position, TilesToAdd, roadtoremove)))
                {
                    TilesToAdd.Add((Position, ThingsInCell.RoadForCars, a));

                    Hod(ref Positions, TilesToAdd, deep - 1, cntroads + 1, cntHouses, Indexs, housetoremove, roadtoremove);
                    
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
                cntHouses++;

                Hod(ref Positions, TilesToAdd, deep - 1, cntroads, cntHouses, Indexs, housetoremove, roadtoremove);
                
                cntHouses--;
                TilesToAdd.RemoveAt(TilesToAdd.Count - 1);
            }

        }
        Indexs[whatadd] = save;

    }

    List<(int,int)> GetRoadsAround((int, int) position, List<((int, int), ThingsInCell, List<(int, int)>)> TilesToAdd, (int,int)roadtoremove)
    {
        List<(int, int)> RoadsAround = new List<(int, int)>();
        for (int x = -1; x < 2; x += 2)
        {
            (int, int) tmp = (position.Item1 + x, position.Item2);
            if (tmp != roadtoremove)
            {
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
        }
        for (int y = -1; y < 2; y += 2)
        {
            (int, int) tmp = (position.Item1, position.Item2 + y);
            if (tmp != roadtoremove)
            {
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
        }
        return RoadsAround;
    }
    
    
    private void TestSystem(List<((int, int), ThingsInCell, List<(int, int)>)> TilesToAdd, (int,int)housetoremove, (int, int) roadtoremove, int number)
    {
        Dictionary<(int, int), HashSet<(int, int)>> newroads = new Dictionary<(int, int), HashSet<(int, int)>>();
        List<(int, int)> newhouses = new List<(int, int)>();
        Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWays = new Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>>();
        int newlength = 0, eco = 0, newwaymax = 0;
        if (housetoremove.Item1 != int.MaxValue)
        {
            eco+= RemoveHouse(housetoremove);
        }
        if (roadtoremove.Item1 != int.MaxValue)
        {
            if (!RemoveRoad(roadtoremove, newroads, newWays, housetoremove))return;
        }
        int cntnewroads = 0;
        foreach (((int, int), ThingsInCell, List<(int, int)>) b in TilesToAdd)
        {
            if (b.Item2 == ThingsInCell.RoadForCars)
            {
                cntnewroads++;
                newroads.Add(b.Item1, new HashSet<(int, int)>());
                foreach ((int, int) c in b.Item3)
                {
                    newroads[b.Item1].Add(c);
                    if (!newroads.ContainsKey(c)) newroads.Add(c, new HashSet<(int, int)>());
                    newroads[c].Add(b.Item1);
                }
                
                eco+=AddWaysFromRoad(b.Item1, newhouses, newroads, newWays, housetoremove, roadtoremove);
            }
            else
            {
                newhouses.Add(b.Item1);
                int t = AddWaysFromHouse(b.Item1, newroads, newWays, roadtoremove, cntnewroads);
                if (t == -1) return;
                newlength += t;
                newwaymax = Math.Max(t, newwaymax);
            }
        }
        
        (int, int) nowefficiency = (newlength-eco, newwaymax);
        bestEfficienty = Math.Min(nowefficiency.Item1, bestEfficienty);
        variants[number] = (nowefficiency, TilesToAdd, housetoremove, roadtoremove);
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
    private int AddWaysFromHouse((int, int) position, Dictionary<(int, int), HashSet<(int, int)>> newRoads, Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWaysFromRoadsToHouses, (int, int) roadtoremove, int cntnewroads)
    {
        HashSet<(int, int)> nowpos = new HashSet<(int, int)>(), newpos = new HashSet<(int, int)>();
        HashSet<(int, int)> usedRoads = new HashSet<(int, int)>();
        
        int timer = 1;
        int ans = 0;
        foreach ((int, int) a in GetNearTiles(position))
        {
            if (a == roadtoremove) continue;
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
                if (Roads.ContainsKey(a)&&a!=roadtoremove)
                {
                    foreach ((int, int) b in Roads[a])
                    {
                        if (!usedRoads.Contains(b)&&b!=roadtoremove)
                        {
                            if (!newWaysFromRoadsToHouses.ContainsKey(b))
                                newWaysFromRoadsToHouses.Add(b, new Dictionary<(int, int), ((int, int), int)>());
                            newWaysFromRoadsToHouses[b][position] = (a, timer);
                            usedRoads.Add(b);
                            newpos.Add(b);
                        }
                    }
                }
                if (newRoads.ContainsKey(a)&&a!=roadtoremove)
                {
                    foreach((int, int) b in newRoads[a])
                    {
                        if (!usedRoads.Contains(b)&&b!=roadtoremove)
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
        //UnityEngine.Debug.Log(Convert.ToString(usedRoads.Count) + ' ' + Convert.ToString(newRoads.Count + Roads.Count - (roadtoremove.Item1 == int.MaxValue ? 0 : 1)));
        return usedRoads.Count== cntnewroads + Roads.Count-(roadtoremove.Item1==int.MaxValue?0:1)?ans:-1;
    }
    /// <summary>
    /// Добавляет новую дорогу в граф
    /// </summary>
    /// <param name="position">Позиция дороги</param>
    /// <param name="Houses">Позиции домов</param>
    /// <param name="Roads">Граф дорог</param>
    /// <param name="WaysFromRoadsToHouses">Граф города</param>
    /// <returns>Возвращает "экономию" расстояния между домами</returns>
    private int AddWaysFromRoad((int, int) position, List<(int,int)> newHouses, Dictionary<(int,int), HashSet<(int,int)>> newRoads, Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWaysFromRoadsToHouses, (int,int)housetoremove, (int, int) roadtoremove)
    {
        //ДОМА ВОКРУГ ДОРОГИ
        Dictionary<(int, int), ((int, int), int)> dictposition;
        dictposition = new Dictionary<(int, int), ((int, int), int)>();
        newWaysFromRoadsToHouses.Add(position, dictposition);

        foreach ((int, int) a in GetNearTiles(position))
        {
            if (a != housetoremove)
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
        }

        //ДОРОГИ ВОКРУГ ТАЙЛА
        //Перезаписываем все дома в этот тайл  
        foreach ((int, int) b in newRoads[position])
        {
            if (b == roadtoremove) continue;
            Dictionary<(int, int), ((int, int), int)> dictbNEW, dictbOLD;
            if (newWaysFromRoadsToHouses.ContainsKey(b))
            {
                dictbNEW = newWaysFromRoadsToHouses[b];
                foreach ((int, int) c in dictbNEW.Keys)
                {
                    if (c != housetoremove)
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
            }
            if (WaysFromRoadsToHouses.ContainsKey(b))
            {
                dictbOLD = WaysFromRoadsToHouses[b];
                foreach ((int, int) c in dictbOLD.Keys)
                {
                    if (c != housetoremove)
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
            
        }
        
        //ПЕРЕСТРОЕНИЕ ГРАФА И РАССЧЁТ ЭКОНОМИИ
        int ans = 0;
        foreach ((int, int) a in newRoads[position])
        {
            if (a == roadtoremove) continue;
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
    private int RemoveHouse((int, int) position)
    {
        int ans = 0;
        foreach((int,int) a in Roads.Keys)
        {
            if(WaysFromRoadsToHouses[a].ContainsKey(position)) ans += WaysFromRoadsToHouses[a][position].Item2;
        }
        return ans;
    }
    private bool RemoveRoad((int,int) position, Dictionary<(int, int), HashSet<(int, int)>> newRoads, Dictionary<(int, int), Dictionary<(int, int), ((int, int), int)>> newWaysFromRoadsToHouses, (int, int) housetoremove)
    {
        HashSet<(int, int)> housesneedtorec = new HashSet<(int, int)>();
        foreach ((int, int) a in Roads[position])
        {
            foreach ((int, int) b in WaysFromRoadsToHouses[a].Keys)
            {
                if (b!=housetoremove&&WaysFromRoadsToHouses[a][b].Item1 == position) housesneedtorec.Add(b);
            }
        }
        foreach ((int, int) z in housesneedtorec)
        {
            int t = AddWaysFromHouse(z, newRoads, newWaysFromRoadsToHouses, position, 0);
            if (t == -1) return false;
        }
        return true;
    }
    private bool checkSystem(List<(int, int)> newHouses, Dictionary<(int, int), HashSet<(int, int)>> newRoads, (int, int) housetoremove, (int,int)roadtoremove)
    {
        Dictionary<(int, int), List<(int, int)>> housesAround = new Dictionary<(int, int), List<(int, int)>>();
        int cnth = 0, cntr = 0;
        foreach((int,int) a in Roads.Keys)
        {
            if (a != roadtoremove)
            {
                housesAround.Add(a, new List<(int, int)>());
                foreach((int,int) b in GetNearTiles(a))
                {
                    if (b != housetoremove)
                        if (Houses.ContainsKey(b)||newHouses.Contains(b)) housesAround[a].Add(b);
                }
            }
        }
        foreach ((int, int) a in newRoads.Keys)
        {
            if (!housesAround.ContainsKey(a))
            {
                housesAround.Add(a, new List<(int, int)>());
                foreach ((int, int) b in GetNearTiles(a))
                {
                    if (b != housetoremove)
                        if (Houses.ContainsKey(b) || newHouses.Contains(b)) housesAround[a].Add(b);
                }
            }
        }
        HashSet<(int, int)> nowpositions = new HashSet<(int, int)>() { Roads.First().Key }, newpositions = new HashSet<(int, int)>();
        HashSet<(int, int)> reached = new HashSet<(int, int)>(), usedroads=  new HashSet<(int, int)>() { Roads.First().Key };
        while (nowpositions.Count != 0)
        {
            foreach((int,int) a in nowpositions)
            {
                if (a == roadtoremove) continue;
                cntr++;
                foreach ((int,int) b in housesAround[a])
                {
                    if (!reached.Contains(b))
                    {
                        cnth++;
                        reached.Add(b);
                    }
                }
                if (Roads.ContainsKey(a)){
                    foreach ((int, int) b in Roads[a])
                    {
                        if (!usedroads.Contains(b))
                        {
                            usedroads.Add(b);
                            newpositions.Add(b);
                        }
                    }
                }
                if (newRoads.ContainsKey(a))
                {
                    foreach ((int, int) b in newRoads[a])
                    {
                        if (!usedroads.Contains(b))
                        {
                            usedroads.Add(b);
                            newpositions.Add(b);
                        }
                    }
                }
            }
            nowpositions.Clear();
            foreach ((int, int) a in newpositions) nowpositions.Add(a);
            newpositions.Clear();
        }

        return cnth == newHouses.Count + Houses.Count - (housetoremove.Item1!=int.MaxValue ? 1 : 0) && cntr == housesAround.Count;
    }
}
