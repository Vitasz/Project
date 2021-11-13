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
    List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople, ThingsInCell.HouseCom, ThingsInCell.HouseFact, ThingsInCell.RoadForCars };
    Dictionary<Vector3Int, List<Vector3Int>> Roads = new Dictionary<Vector3Int, List<Vector3Int>>();
    Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
    Dictionary<Vector3Int, ThingsInCell> MapCopy = new Dictionary<Vector3Int, ThingsInCell>();
    List<Task> tasks = new List<Task>();
    Vector3Int bestPosition = new Vector3Int();
    double bestEfficienty = -1;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    public int HODS = 0;
    private int total = 0;
    private bool IsGran(Vector3Int position)
    {
        return !(MapCopy.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z)) &&
            MapCopy.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z)) &&
            MapCopy.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z)) &&
            MapCopy.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z)));
    }
    private List<Vector3Int> GetGrans(Vector3Int position)
    {
        List<Vector3Int> Grans = new List<Vector3Int>();
        if (!MapCopy.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x - 1, position.y, position.z));
        if (!MapCopy.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x + 1, position.y, position.z));
        if (!MapCopy.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z))) Grans.Add(new Vector3Int(position.x, position.y - 1, position.z));
        if (!MapCopy.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z))) Grans.Add(new Vector3Int(position.x, position.y + 1, position.z));
        return Grans;
    }
    public void Optimization(GridFunc grid)
    {
        //this.grid = grid;
        //if (grid.Map.Count == 0) grid.CreateNewTile(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople, false);

        StartCoroutine(F(grid));
    }
    IEnumerator F(GridFunc grid)
    {

        while (HODS != 0)
        {
            int cntroads = 0, cnthousesPeople = 0, cnthousesCom = 0, cnthousesFact = 0;
            foreach (Vector3Int a in grid.Map.Keys)
            {
                Cell cell = grid.Map[a];
                if (IsGran(a))
                {
                    if (cell is CellWithHouse) GranHouse.Add(cell as CellWithHouse);
                    else if (cell is CellWithRoad) GranRoad.Add(cell as CellWithRoad);
                }
                MapCopy.Add(a, cell.GetTypeCell());
                if (cell.GetTypeCell() == ThingsInCell.RoadForCars)
                {
                    Roads.Add(a, (cell as CellWithRoad).GetNearRoadsWays());
                }
                else
                {
                    Houses.Add(a, cell.GetTypeCell());
                    if (cell.GetTypeCell() == ThingsInCell.HousePeople) cnthousesPeople++;
                    else if (cell.GetTypeCell() == ThingsInCell.HouseCom) cnthousesCom++;
                    else cnthousesFact++;
                }
            }
            //List < List < Vector3Int >> temp3 = GetWaysToEachOtherFor1(ref Roads, ref Houses);
            // UnityEngine.Debug.Log(temp2.Count, temp3.Count);
            HODS -= 1;
            cntroads = Roads.Count;
            List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> PositionsToCheck = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();//{Position, Type, RoadsFrom, RoadsTo, Bonus grans(if type is road))}
            List<Vector3Int> was = new List<Vector3Int>();
            foreach (CellWithRoad cell in GranRoad)
                foreach (Vector3Int PositionGran in GetGrans(cell.GetCellPosition()))
                {
                    if (!was.Contains(PositionGran)) was.Add(PositionGran);
                    else continue;
                    PositionsToCheck.Add(new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>());
                    foreach (ThingsInCell type in Variants)
                        if (type == ThingsInCell.HouseCom || type == ThingsInCell.HousePeople || type == ThingsInCell.HouseFact)
                            PositionsToCheck[PositionsToCheck.Count - 1].Add((PositionGran, type, null, null));
                        else if (type == ThingsInCell.RoadForCars)
                        {
                            foreach ((List<Vector3Int>, List<Vector3Int>) a in GetRoadsVariants(PositionGran))
                                PositionsToCheck[PositionsToCheck.Count - 1].Add((PositionGran, type, a.Item1, a.Item2));
                        }
                    if (PositionsToCheck[PositionsToCheck.Count - 1].Count == 0) PositionsToCheck.RemoveAt(PositionsToCheck.Count - 1);
                }
            total = 0;
            List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> temp = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (PositionsToCheck.Count != 0) Hod(ref PositionsToCheck, temp, Deep, new List<Vector3Int>(), cntroads,cnthousesPeople,cnthousesCom,cnthousesFact, grid.NowSystem);
            UnityEngine.Debug.Log(tasks.Count);
            yield return new WaitForEndOfFrame();
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            UnityEngine.Debug.Log("Total variants:" + Convert.ToString(total) + " Time: " + Convert.ToString(stopwatch.ElapsedMilliseconds));
            if (bestEfficienty != -1) grid.CreateNewTile(bestPosition, bestVariant, false);
            else UnityEngine.Debug.Log("NO VARIANTS");
            if (bestVariant == ThingsInCell.RoadForCars)
            {
                foreach (Vector3Int a in bestRoadsTo)
                    grid.UniteTiles(a, bestPosition, ThingsInCell.RoadForCars, false);
                foreach (Vector3Int a in bestRoadsFrom)
                    grid.UniteTiles(bestPosition, a, ThingsInCell.RoadForCars, false);
            }
            bestEfficienty = -1;
            GranRoad.Clear();
            GranHouse.Clear();
            MapCopy.Clear();
            Roads.Clear();
            Houses.Clear();
            tasks.Clear();
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    private List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> BonusFor((Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>) what)
    {
        List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> res = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();
        if (what.Item2 == ThingsInCell.RoadForCars)
            foreach (Vector3Int PositionGran in GetGrans(what.Item1))
            {
                res.Add(new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>());
                foreach (ThingsInCell type in Variants)
                    if (type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact || type == ThingsInCell.HouseFact)
                        res[res.Count - 1].Add((PositionGran, type, null, null));
                    else if (type == ThingsInCell.RoadForCars)
                    {
                        foreach ((List<Vector3Int>, List<Vector3Int>) a in GetRoadsVariants(PositionGran))
                            res[res.Count - 1].Add((PositionGran, type, a.Item1, a.Item2));
                    }
                if (res[res.Count - 1].Count == 0) res.RemoveAt(res.Count - 1);
            }
        return res;
    }

    private List<(List<Vector3Int>, List<Vector3Int>)> GetRoadsVariants(Vector3Int Position)
    {
        List<(List<Vector3Int>, List<Vector3Int>)> res = new List<(List<Vector3Int>, List<Vector3Int>)>();
        List<Vector3Int> CanBeRoadsFrom = PositionsRoadAround(Position);
        int st1 = 1;
        for (int i = 0; i < CanBeRoadsFrom.Count; i++)
            st1 *= 2;

        for (int i = 0; i < st1; i++)
        {
            int now1 = i;
            List<Vector3Int> RoadsFrom = new List<Vector3Int>();
            int pos = 0;
            List<Vector3Int> CanBeRoadsTo = new List<Vector3Int>();
            foreach (Vector3Int a in CanBeRoadsFrom)
            {
                CanBeRoadsTo.Add(a);
            }
            while (now1 != 0)
            {
                if (now1 % 2 == 1)
                {
                    RoadsFrom.Add(CanBeRoadsFrom[pos]);
                    CanBeRoadsTo.Remove(CanBeRoadsFrom[pos]);
                }
                now1 /= 2;
                pos++;
            }
            int st2 = 1;

            for (int j = 0; j < CanBeRoadsTo.Count; j++)
            {
                st2 *= 2;
            }
            for (int j = 0; j < st2; j++)
            {
                if (i == 0 && j == 0) continue;
                int now2 = j;
                List<Vector3Int> RoadsTo = new List<Vector3Int>();
                int pos2 = 0;
                while (now2 != 0)
                {
                    if (now2 % 2 == 1) RoadsTo.Add(CanBeRoadsTo[pos2]);
                    now2 /= 2;
                    pos2++;
                }
                res.Add((RoadsFrom, RoadsTo));
            }
        }
        return res;
    }
    private void Hod(ref List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nowdeep, List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd, int deep, List<Vector3Int> PosToadd, 
        int cntroads, int cnthousePeople, int cnthouseCom, int cnthouseFact, Dictionary<Vector3Int, List<(int,List<Vector3Int>)>> NowSystem)
    {
        ThingsInCell whatadd;
        if (cnthouseCom <= cnthouseFact && cnthousePeople >= cnthouseCom)
        {
            whatadd = ThingsInCell.HouseCom;
            cnthouseCom++;
        }
        else if (cnthouseFact <= cnthouseCom && cnthousePeople >= cnthouseFact)
        {
            whatadd = ThingsInCell.HouseFact;
            cnthouseFact++;
        }
        else if (cntroads <= (cnthouseFact + cnthouseCom + cnthousePeople) * 2)
        {
            whatadd = ThingsInCell.RoadForCars;
            cntroads++;
        }
        else
        {
            whatadd = ThingsInCell.HousePeople;
            cnthousePeople++;
        }
        for (int i = 0; i < nowdeep.Count; i++)
        {
            for (int j = 0; j < nowdeep[i].Count; j++)
            {
                if (PosToadd.Contains(nowdeep[i][j].Item1)) continue;
                if (deep == 1)
                {
                    if (nowdeep[i][j].Item2 == ThingsInCell.RoadForCars)
                        continue;
                }
                else if (nowdeep[i][j].Item2 != whatadd) continue;
                TilesToAdd.Add(nowdeep[i][j]);
                PosToadd.Add(nowdeep[i][j].Item1);
                if (deep == 1)
                {
                    List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> tilesToAddCopy = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>();
                    foreach ((Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>) a in TilesToAdd)
                        tilesToAddCopy.Add(a);
                    Dictionary<Vector3Int, List<Vector3Int>> RoadsCopy = new Dictionary<Vector3Int, List<Vector3Int>>();
                    Dictionary<Vector3Int, ThingsInCell> HousesCopy = new Dictionary<Vector3Int, ThingsInCell>();

                    foreach (Vector3Int a in Roads.Keys)
                    {
                        RoadsCopy.Add(a, new List<Vector3Int>());
                        foreach (Vector3Int b in Roads[a])
                        {
                            RoadsCopy[a].Add(b);
                        }
                    }
                    foreach (Vector3Int a in Houses.Keys)
                    {
                        HousesCopy.Add(a, Houses[a]);
                    }
                    tasks.Add(Task.Run(() => TestSystem(tilesToAddCopy, RoadsCopy, HousesCopy, NowSystem)));
                    total++;
                }
                else
                {
                    List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nextdeep = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> a in nowdeep) nextdeep.Add(a);
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> b in BonusFor(nowdeep[i][j])) nextdeep.Add(b);
                    Hod(ref nextdeep, TilesToAdd, deep - 1, PosToadd, cntroads, cnthousePeople, cnthouseCom, cnthouseFact, NowSystem);
                }
                TilesToAdd.RemoveAt(TilesToAdd.Count - 1);
                PosToadd.RemoveAt(PosToadd.Count - 1);
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
                if (Math.Abs(i) + Math.Abs(j) <= 1 && MapCopy.ContainsKey(new Vector3Int(position.x + i, position.y + j, 0)))
                {
                    if (MapCopy[new Vector3Int(position.x + i, position.y + j, 0)] == ThingsInCell.RoadForCars)
                    {
                        ans.Add(new Vector3Int(position.x + i, position.y + j, 0));
                    }
                }
            }
        }
        return ans;
    }
    List<List<Vector3Int>> GetWaysToEachOtherFor2(ref Dictionary<Vector3Int, List<Vector3Int>> roads, ref Dictionary<Vector3Int, ThingsInCell> houses)
    {
        List<List<Vector3Int>> ans = new List<List<Vector3Int>>();
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
        foreach (Vector3Int a in houses.Keys) HousesInList.Add(a);
        for (int i = 0; i < HousesInList.Count; i++)
        {
            foreach (Vector3Int a in GetNearTiles(HousesInList[i]))
            {
                if (roads.ContainsKey(a))
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
                        if (!roadstohouses.ContainsKey(a)) {
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
        for (int i = 0; i < HousesInList.Count; i++)
        {
            Dictionary<int, List<Vector3Int>> temp = new Dictionary<int, List<Vector3Int>>();
            foreach (Vector3Int a in GetNearTiles(HousesInList[i]))
            {
                if (roads.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = roadsandWaysFromThem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
            }
            if (temp.Count != houses.Count) return null;
            foreach (int a in temp.Keys)
            {
                if (a != i)
                    ans.Add(temp[a]);
            }
        }
        return ans;
    }

    List<List<Vector3Int>> GetWaysToEachOtherFor1(ref Dictionary<Vector3Int, List<Vector3Int>> roads, ref Dictionary<Vector3Int, ThingsInCell> houses, ref List<Vector3Int> NotChecked,
        ref Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> nowSystem)
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
        foreach (Vector3Int a in houses.Keys) HousesInList.Add(a);
        foreach(Vector3Int a in nowSystem.Keys)
        {
            roadstohouses.Add(a, new List<int>());
            roadsandWaysFromThem.Add(a, new List<(int, List<Vector3Int>)>());
            foreach((int,List<Vector3Int>) c in nowSystem[a])
            {
                roadstohouses[a].Add(c.Item1);
                roadsandWaysFromThem[a].Add(c);
            }
        }
        
        for (int i = 0; i < HousesInList.Count; i++)
        {
            foreach (Vector3Int a in GetNearTiles(HousesInList[i]))
            {
                if (roads.ContainsKey(a))
                {
                    if (!RoadsWithHouses.ContainsKey(a))
                    {
                        RoadsWithHouses.Add(a, new List<int>());
                    }
                    if (!RoadsWithHouses[a].Contains(i))
                    {
                        RoadsWithHouses[a].Add(i);
                    }
                }
            }
        }
        //Дома, к которым можно доехать из этой клетки
        foreach (Vector3Int a in NotChecked)
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
        
        Dictionary<Vector3Int, List<Vector3Int>> ReverseRoads = new Dictionary<Vector3Int, List<Vector3Int>>();
        foreach(Vector3Int a in roads.Keys)
        {
            foreach(Vector3Int b in roads[a])
            {
                if (!ReverseRoads.ContainsKey(b)) ReverseRoads.Add(b, new List<Vector3Int>());
                ReverseRoads[b].Add(a);
            }
        }
        //Дома, куда можно доехать благодаря этой клетки
        foreach (Vector3Int a in NotChecked)
        {
            List<(Vector3Int, int, List<Vector3Int>)> nowpos = new List<(Vector3Int, int, List<Vector3Int>)>(), newpos = new List<(Vector3Int, int, List<Vector3Int>)>();
            if (ReverseRoads.ContainsKey(a) && roadsandWaysFromThem.ContainsKey(a))
            {
                foreach ((int, List<Vector3Int>) b in roadsandWaysFromThem[a])
                {
                    foreach (Vector3Int k in ReverseRoads[a])
                        nowpos.Add((k, b.Item1, b.Item2));
                }
            }
            while (nowpos.Count != 0)
            {
                foreach((Vector3Int, int, List<Vector3Int>) k in nowpos)
                {
                    List<Vector3Int> copy = new List<Vector3Int>();
                    foreach (Vector3Int f in k.Item3) copy.Add(f);
                    copy.Insert(0, k.Item1);
                    if (roadstohouses.ContainsKey(k.Item1)&&!roadstohouses[k.Item1].Contains(k.Item2))
                    {
                        roadstohouses[k.Item1].Add(k.Item2);
                        roadsandWaysFromThem[k.Item1].Add((k.Item2, copy));
                        if (ReverseRoads.ContainsKey(k.Item1))
                        {
                            foreach (Vector3Int f in ReverseRoads[k.Item1])
                            {
                                newpos.Add((f, k.Item2, copy));
                            }
                        }
                    }
                    else
                    {
                        int index = 0;
                        if (roadsandWaysFromThem.ContainsKey(k.Item1))
                        foreach ((int, List<Vector3Int>) f in roadsandWaysFromThem[k.Item1])
                        {
                            if (f.Item1 == k.Item2)
                            {
                                if (f.Item2.Count > copy.Count)
                                {
                                    roadsandWaysFromThem[k.Item1][index] = (f.Item1, copy);
                                }
                                break;
                            }
                            index++;
                        }

                    }
                }

                nowpos.Clear();
                foreach ((Vector3Int, int, List<Vector3Int>) s in newpos) nowpos.Add(s);
                newpos.Clear();
            }
        }
        List<List<Vector3Int>> ans = new List<List<Vector3Int>>();
        for(int i =0; i<HousesInList.Count;i++)
        {
            Dictionary<int, List<Vector3Int>> temp = new Dictionary<int, List<Vector3Int>>();
            foreach (Vector3Int a in GetNearTiles(HousesInList[i]))
            {
                if (roads.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = roadsandWaysFromThem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
            }
            if (temp.Count != houses.Count) return null;
            foreach (int a in temp.Keys)
            {
                if (a!=i)
                    ans.Add(temp[a]);
            }
        }

        return ans;
    }

    bool equals(List<List<Vector3Int>> a, List<List<Vector3Int>>b)
    {
        foreach(List<Vector3Int> s in a)
        {
            bool ok = false;
            foreach(List<Vector3Int>k in b)
            {
                if (k.Count == s.Count)
                {
                    bool temp = true;
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (k[i] != s[i])
                        {
                            temp = false;
                            break;
                        }
                    }
                    if (temp)
                    {
                        ok = true;
                        break;
                    }
                }
            }
            if (!ok) return false;
        }
        return true;
    }

    private void TestSystem(List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd,
        Dictionary<Vector3Int, List<Vector3Int>> roads, Dictionary<Vector3Int, ThingsInCell> houses, Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> nowSystem)
    {
        List<Vector3Int> tocheck = new List<Vector3Int>();
        double GetEfficiencyOfSystem()
        {
            //List<List<Vector3Int>> temp3 = GetWaysToEachOtherFor2(ref roads, ref houses);
            List<List<Vector3Int>> WaysTo = GetWaysToEachOtherFor1(ref roads, ref houses, ref tocheck, ref nowSystem);
           // bool ok = equals(WaysTo, temp3);
          /*  if (!ok)
            {
                UnityEngine.Debug.Log(WaysTo);
                UnityEngine.Debug.Log(temp3);
                UnityEngine.Debug.LogError("NOT OK");
            }*/
            if (WaysTo == null) return -1f;
            int totalTimes = 0, totalWays = 0;
            List<List<Vector3Int>> busyCells = new List<List<Vector3Int>>() { new List<Vector3Int>() };
            for (int i = 0; i < WaysTo.Count; i++)
            {
                int nowtime = 0;
                int prevtime = 0;
                for (int j = 0; j < WaysTo[i].Count; j++)
                {
                    if (busyCells[nowtime].Contains(WaysTo[i][j]))
                    {
                        nowtime++;
                        if (busyCells.Count == nowtime) busyCells.Add(new List<Vector3Int>());
                        while (busyCells[nowtime].Contains(WaysTo[i][j]))
                        {
                            nowtime++;
                            if (busyCells.Count == nowtime) busyCells.Add(new List<Vector3Int>());
                        }
                    }
                    if (busyCells.Count == nowtime) busyCells.Add(new List<Vector3Int>());
                    busyCells[nowtime].Add(WaysTo[i][j]);
                    totalTimes += nowtime - prevtime;
                    prevtime = nowtime;
                    nowtime++;
                    if (busyCells.Count == nowtime) busyCells.Add(new List<Vector3Int>());
                }
                totalWays += WaysTo[i].Count;
            }
            return (double)totalTimes / totalWays;
        }
        //if (GetEfficiencyOfSystem() == -1f) return;
        for (int f = 0; f < TilesToAdd.Count; f++)
        {
            if (TilesToAdd[f].Item2 == ThingsInCell.RoadForCars)
            {
                roads.Add(TilesToAdd[f].Item1, new List<Vector3Int>());
                foreach (Vector3Int a in TilesToAdd[f].Item3)
                    roads[TilesToAdd[f].Item1].Add(a);
                foreach (Vector3Int a in TilesToAdd[f].Item4)
                    roads[a].Add(TilesToAdd[f].Item1);
                tocheck.Add(TilesToAdd[f].Item1);
            }
            else
            {
                houses.Add(TilesToAdd[f].Item1, TilesToAdd[f].Item2);
                List<Vector3Int> ans2 = new List<Vector3Int>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Vector3Int now = new Vector3Int(TilesToAdd[f].Item1.x + i, TilesToAdd[f].Item1.y + j, TilesToAdd[f].Item1.z);
                        if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1) ans2.Add(now);
                    }
                }
                foreach (Vector3Int a in ans2) if (roads.ContainsKey(a)) tocheck.Add(a);
            }
        }
        double nowEfficiency = GetEfficiencyOfSystem();
        if (nowEfficiency != -1f)
        {
            if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
            {
                //UnityEngine.Debug.Log(TilesToAdd.Count);
                bestEfficienty = nowEfficiency;
                bestPosition = TilesToAdd[0].Item1;
                bestVariant = TilesToAdd[0].Item2;
                bestRoadsFrom = TilesToAdd[0].Item3;
                bestRoadsTo = TilesToAdd[0].Item4;
    
            }
        }
    }
}
