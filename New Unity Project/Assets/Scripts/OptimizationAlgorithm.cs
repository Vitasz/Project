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
    Dictionary<(int,int), List<Vector3Int>> Roads = new Dictionary<(int, int), List<Vector3Int>>();
    Dictionary<(int, int), ThingsInCell> Houses = new Dictionary<(int, int), ThingsInCell>();
    Dictionary<(int,int), ThingsInCell> MapCopy = new Dictionary<(int, int), ThingsInCell>();
    //Dictionary<Vector3Int, List<Vector3Int>> HousesAndRoadsAround = new Dictionary<Vector3Int, List<Vector3Int>>();
    //Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> NowSystem;
    //Dictionary<Vector3Int, HashSet<int>> RoadsToHouses;
    List<Task> tasks = new List<Task>();
    Vector3Int bestPosition = new Vector3Int();
    double bestEfficienty = -1;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    public int HODS = 0;
    private int total = 0;
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
                    if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1) ans2.Add(now);
                }
            }
            return ans2;
        }
       
        long TOTALTIME = 0;
        while (HODS != 0)
        {

            foreach (Vector3Int a in grid.Map.Keys)
            {
                (int, int) now = (a.x, a.y);
                MapCopy.Add(now, grid.Map[a].GetTypeCell());
                if (grid.Map[a] is CellWithRoad)
                {
                    Roads.Add(now, (grid.Map[a] as CellWithRoad).GetNearRoadsWays());
                }
                else
                {
                    ThingsInCell temp = (grid.Map[a] as CellWithHouse).typeHouse;
                    Houses.Add(now, temp);
                    if (temp == ThingsInCell.HousePeople)
                    {
                        cntHousePeople++;
                    }
                    else if (temp == ThingsInCell.HouseCom)
                    {
                        cntHouseCom++;
                    }
                    else
                    {
                        cntHouseFact++;
                    }
                }
            }
            cntRoads = Roads.Keys.Count;
            Stopwatch OPTIMIZEWATCH = new Stopwatch();
            OPTIMIZEWATCH.Start();
            HashSet<(int, int)> Positions = new HashSet<(int, int)>();
            foreach ((int,int) a in Roads.Keys)
            {
                if (IsGran(a))
                {
                    foreach ((int, int) b in GetEmptyTiles(a))
                    {
                        Positions.Add(b);
                    }
                }
            }
            total = 0;
            UnityEngine.Debug.Log(Positions.Count);
            Hod(ref Positions, new List<((int, int), ThingsInCell)>(), new List<(int, int)>(),Deep, cntRoads, cntHousePeople, cntHouseCom, cntHouseFact);
            UnityEngine.Debug.Log("TOTAL VARIANTS" + Convert.ToString(total));
            OPTIMIZEWATCH.Stop();
            UnityEngine.Debug.Log(OPTIMIZEWATCH.ElapsedMilliseconds);
            if (bestEfficienty != -1) grid.CreateNewTile(bestPosition, bestVariant);
            else
            {
                UnityEngine.Debug.Log("NO VARIANTS");
                HODS = 0;
            }
            if (bestVariant == ThingsInCell.RoadForCars)
            {
                foreach (Vector3Int a in bestRoadsTo)
                {
                    grid.UniteTiles(a, new List<Vector3Int>() { bestPosition });
                    grid.UniteTiles(bestPosition, new List<Vector3Int>() { a });
                }
                foreach (Vector3Int a in bestRoadsFrom)
                {
                    grid.UniteTiles(bestPosition, new List<Vector3Int>() { a });
                    grid.UniteTiles(a, new List<Vector3Int>() { bestPosition });
                }
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
        UnityEngine.Debug.Log("TOTAL TIME: " + Convert.ToString(TOTALTIME));
        yield return null;
    }

    private List<List<(int, int)>> GetRoadsVariants((int, int) Position)
    {
        List<List< (int, int) >> res = new List<List<(int, int)>>();
        
        return res;
    }
    private void Hod(ref HashSet<(int, int)> Positions, List<((int,int), ThingsInCell)>TilesToAdd, List<(int,int)> BonusPositions, int deep,
        int cntroads, int cnthousePeople, int cnthouseCom, int cnthouseFact)
    {

        List<(int,int)> BonusFor((int,int) position)
        {
            List<(int,int)> ans2 = new List<(int,int)>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    (int, int) now = (position.Item1 + i, position.Item2 + j);
                    if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1&&!Houses.ContainsKey(now)&&!Roads.ContainsKey(now)) ans2.Add(now);
                }
            }
            return ans2;
        }


        if (deep == 0)
        {
            TestSystem(TilesToAdd);
            total++;
            return;
        }
        ThingsInCell whatadd;
        if (cntroads <= (cnthouseFact + cnthouseCom + cnthousePeople) * 1.5)
        {
            whatadd = ThingsInCell.RoadForCars;
            cntroads++;
        }
        else if (cnthouseCom <= cnthouseFact && cnthousePeople >= cnthouseCom)
        {
            whatadd = ThingsInCell.HouseCom;
            cnthouseCom++;
        }
        else if (cnthouseFact <= cnthouseCom && cnthousePeople >= cnthouseFact)
        {
            whatadd = ThingsInCell.HouseFact;
            cnthouseFact++;
        }
        else
        {
            whatadd = ThingsInCell.HousePeople;
            cnthousePeople++;
        }
        
        foreach((int, int) a in Positions)
        {
            bool ok = true;
            foreach (((int, int), ThingsInCell) b in TilesToAdd)
            {
                if (b.Item1 == a)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                TilesToAdd.Add((a, whatadd));
                if (whatadd == ThingsInCell.RoadForCars)
                {
                    List<(int, int)> bonus = BonusFor(a);
                    BonusPositions.AddRange(bonus);
                    Hod(ref Positions, TilesToAdd, BonusPositions, deep - 1, cntroads, cnthousePeople, cnthouseCom, cnthouseFact);
                    BonusPositions.RemoveRange(BonusPositions.Count - bonus.Count, bonus.Count);
                }
                else
                {
                    Hod(ref Positions, TilesToAdd, BonusPositions,deep - 1, cntroads, cnthousePeople, cnthouseCom, cnthouseFact);
                }
            }
        }
        //UnityEngine.Debug.Log(nowdeep[whatadd]);
        
    }
    public List<(int, int)> PositionsRoadAround((int,int) position)
    {
        List<(int, int)> ans = new List<(int, int)>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (Math.Abs(i) + Math.Abs(j) <= 1 && MapCopy.ContainsKey((position.Item1 + i, position.Item2 + j)))
                {
                    if (MapCopy[(position.Item1 + i, position.Item2 + j)] == ThingsInCell.RoadForCars)
                    {
                        ans.Add((position.Item1 + i, position.Item2 + j));
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

    double GetWaysToEachOtherFor1(ref Dictionary<Vector3Int, List<Vector3Int>> bonusroads, ref Dictionary<Vector3Int, ThingsInCell> bonushouses, ref List<Vector3Int> NotChecked,
        ref Dictionary<Vector3Int, List<Vector3Int>> bonushousesandroadsAround)
    {
        return 0f;
       
    }

    private void TestSystem(List<((int,int), ThingsInCell)> TilesToAdd)
    {

    }
}
