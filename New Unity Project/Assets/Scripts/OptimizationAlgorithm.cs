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
    Dictionary<Vector3Int, List<Vector3Int>> HousesAndRoadsAround = new Dictionary<Vector3Int, List<Vector3Int>>();
    Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> NowSystem;
    Dictionary<Vector3Int, HashSet<int>> RoadsToHouses;
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
        int cntHousePeople = 0, cntHouseCom = 0, cntHouseFact = 0, cntRoads = 0;

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
        long TOTALTIME = 0;
        while (HODS != 0)
        {
            foreach (Vector3Int a in grid.Map.Keys)
            {
                if (grid.Map[a] is CellWithRoad)
                {
                    Roads.Add(a, (grid.Map[a] as CellWithRoad).GetNearRoadsWays());
                }
                else
                {
                    ThingsInCell temp = (grid.Map[a] as CellWithHouse).typeHouse;
                    Houses.Add(a, temp);
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
            foreach (Vector3Int a in Roads.Keys)
            {
                if (IsGran(a))
                {
                    Positions.Add((a.x, a.y));
                }
            }

            Hod(ref Positions, Deep, cntRoads, cntHousePeople, cntHouseCom, cntHouseFact);

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
            HousesAndRoadsAround.Clear();
            yield return new WaitForEndOfFrame();
        }
        UnityEngine.Debug.Log("TOTAL TIME: " + Convert.ToString(TOTALTIME));
        yield return null;
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
    private void Hod(ref HashSet<(int, int)> Positions, int deep,
        int cntroads, int cnthousePeople, int cnthouseCom, int cnthouseFact)
    {
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
        
        //UnityEngine.Debug.Log(nowdeep[whatadd]);
        
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
        List<Vector3Int> HousesInList = new List<Vector3Int>();
        List<Vector3Int> BonusHousesInList = new List<Vector3Int>();
        Dictionary<Vector3Int, List<(int, List<Vector3Int>)>> BonusroadsandWaysFromThem = new Dictionary<Vector3Int, List<(int, List<Vector3Int>)>>();
        Dictionary<Vector3Int, HashSet<int>> Bonusroadstohouses = new Dictionary<Vector3Int, HashSet<int>>();
        Dictionary<Vector3Int, List<int>> RoadsWithHouses = new Dictionary<Vector3Int, List<int>>();
        foreach (Vector3Int a in Houses.Keys) HousesInList.Add(a);
        foreach (Vector3Int a in bonushouses.Keys) BonusHousesInList.Add(a);
        /*foreach(Vector3Int a in NowSystem.Keys)
        {
            roadstohouses.Add(a, new List<int>());
            roadsandWaysFromThem.Add(a, new List<(int, List<Vector3Int>)>());
            foreach((int,List<Vector3Int>) c in NowSystem[a])
            {
                roadstohouses[a].Add(c.Item1);
                roadsandWaysFromThem[a].Add(c);
            }
        }*/
        for (int i = 0; i < HousesInList.Count; i++)
        {
            foreach (Vector3Int a in HousesAndRoadsAround[HousesInList[i]])
            {
                if (!RoadsWithHouses.ContainsKey(a))
                {
                    RoadsWithHouses.Add(a, new List<int>());
                }
                RoadsWithHouses[a].Add(i);
            }
            if (bonushousesandroadsAround.ContainsKey(HousesInList[i]))
            {
                foreach (Vector3Int a in bonushousesandroadsAround[HousesInList[i]])
                {
                    if (!RoadsWithHouses.ContainsKey(a))
                    {
                        RoadsWithHouses.Add(a, new List<int>());
                    }
                    RoadsWithHouses[a].Add(i);
                }
            }
        }
        for (int i = HousesInList.Count; i < BonusHousesInList.Count + HousesInList.Count; i++)
        {
            foreach (Vector3Int a in bonushousesandroadsAround[BonusHousesInList[i - HousesInList.Count]])
            {
                if (!RoadsWithHouses.ContainsKey(a))
                {
                    RoadsWithHouses.Add(a, new List<int>());
                }
                RoadsWithHouses[a].Add(i);
            }
        }
        //Дома, к которым можно доехать из этой клетки
        foreach (Vector3Int a in NotChecked)
        {
            List<(Vector3Int, List<Vector3Int>)> nowpos = new List<(Vector3Int, List<Vector3Int>)>() { (a, new List<Vector3Int>()) }, newpos = new List<(Vector3Int, List<Vector3Int>)>();
            List<Vector3Int> Used = new List<Vector3Int>();
            bool end = false;
            if (!BonusroadsandWaysFromThem.ContainsKey(a))
            {
                BonusroadsandWaysFromThem.Add(a, new List<(int, List<Vector3Int>)>());
                Bonusroadstohouses.Add(a, new HashSet<int>());
            }
            while (nowpos.Count != 0 && !end)
            {
                foreach ((Vector3Int, List<Vector3Int>) b in nowpos)
                {
                    Used.Add(b.Item1);
                    b.Item2.Add(b.Item1);
                    //Добавление маршрута до дома
                    if (RoadsWithHouses.ContainsKey(b.Item1))
                    {
                        List<Vector3Int> copy = new List<Vector3Int>();
                        foreach (Vector3Int c in b.Item2) copy.Add(c);
                        foreach (int c in RoadsWithHouses[b.Item1])
                        {
                            if (RoadsToHouses.ContainsKey(a) && RoadsToHouses[a].Contains(c) || Bonusroadstohouses[a].Contains(c)) continue;
                            BonusroadsandWaysFromThem[a].Add((c, copy));
                            Bonusroadstohouses[a].Add(c);
                            if (Bonusroadstohouses[a].Count == Houses.Count + bonushouses.Count) end = true;
                        }
                    }
                    if (Roads.ContainsKey(b.Item1))
                    {
                        foreach (Vector3Int f in Roads[b.Item1])
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
                    if (bonusroads.ContainsKey(b.Item1))
                    {
                        foreach (Vector3Int f in bonusroads[b.Item1])
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
                }
                nowpos.Clear();
                foreach ((Vector3Int, List<Vector3Int>) b in newpos) nowpos.Add(b);
                newpos.Clear();
            }
        }

        Dictionary<Vector3Int, List<Vector3Int>> ReverseRoads = new Dictionary<Vector3Int, List<Vector3Int>>();
        foreach (Vector3Int a in Roads.Keys)
        {
            foreach (Vector3Int b in Roads[a])
            {
                if (!ReverseRoads.ContainsKey(b)) ReverseRoads.Add(b, new List<Vector3Int>());
                ReverseRoads[b].Add(a);
            }
        }
        foreach (Vector3Int a in bonusroads.Keys)
        {
            foreach (Vector3Int b in bonusroads[a])
            {
                if (!ReverseRoads.ContainsKey(b)) ReverseRoads.Add(b, new List<Vector3Int>());
                ReverseRoads[b].Add(a);
            }
        }
        //Дома, куда можно доехать благодаря этой клетки
        foreach (Vector3Int a in NotChecked)
        {
            List<(Vector3Int, int, List<Vector3Int>)> nowpos = new List<(Vector3Int, int, List<Vector3Int>)>(), newpos = new List<(Vector3Int, int, List<Vector3Int>)>();
            if (ReverseRoads.ContainsKey(a))
            {
                if (RoadsToHouses.ContainsKey(a))
                {
                    foreach ((int, List<Vector3Int>) b in NowSystem[a])
                    {
                        foreach (Vector3Int k in ReverseRoads[a])
                            nowpos.Add((k, b.Item1, b.Item2));
                    }
                }
                if (Bonusroadstohouses.ContainsKey(a))
                {
                    foreach ((int, List<Vector3Int>) b in BonusroadsandWaysFromThem[a])
                    {
                        foreach (Vector3Int k in ReverseRoads[a])
                            nowpos.Add((k, b.Item1, b.Item2));
                    }
                }
            }

            while (nowpos.Count != 0)
            {
                foreach ((Vector3Int, int, List<Vector3Int>) k in nowpos)
                {
                    List<Vector3Int> copy = new List<Vector3Int>();
                    foreach (Vector3Int f in k.Item3) copy.Add(f);
                    copy.Insert(0, k.Item1);
                    if (RoadsToHouses.ContainsKey(k.Item1) && !RoadsToHouses[k.Item1].Contains(k.Item2))
                    {
                        if (!Bonusroadstohouses.ContainsKey(k.Item1))
                        {
                            Bonusroadstohouses.Add(k.Item1, new HashSet<int>());
                            BonusroadsandWaysFromThem.Add(k.Item1, new List<(int, List<Vector3Int>)>());
                        }
                        if (!Bonusroadstohouses[k.Item1].Contains(k.Item2))
                        {
                            Bonusroadstohouses[k.Item1].Add(k.Item2);
                            BonusroadsandWaysFromThem[k.Item1].Add((k.Item2, copy));
                            if (ReverseRoads.ContainsKey(k.Item1))
                            {
                                foreach (Vector3Int f in ReverseRoads[k.Item1])
                                {
                                    newpos.Add((f, k.Item2, copy));
                                }
                            }
                        }
                    }
                    else
                    {
                        int index = 0;
                        bool ok = true;
                        if (BonusroadsandWaysFromThem.ContainsKey(k.Item1))
                        {
                            int index1 = 0;
                            foreach ((int, List<Vector3Int>) f in BonusroadsandWaysFromThem[k.Item1])
                            {
                                if (f.Item1 == k.Item2)
                                {
                                    if (f.Item2.Count > copy.Count)
                                    {
                                        BonusroadsandWaysFromThem[k.Item1][index1] = (f.Item1, copy);
                                    }
                                    ok = false;
                                    break;
                                }
                                index1++;
                            }
                        }
                        if (ok && NowSystem.ContainsKey(k.Item1))
                        {
                            foreach ((int, List<Vector3Int>) f in NowSystem[k.Item1])
                            {
                                if (f.Item1 == k.Item2)
                                {
                                    if (f.Item2.Count > copy.Count)
                                    {
                                        if (!BonusroadsandWaysFromThem.ContainsKey(k.Item1))
                                        {
                                            BonusroadsandWaysFromThem.Add(k.Item1, new List<(int, List<Vector3Int>)>());
                                            Bonusroadstohouses.Add(k.Item1, new HashSet<int>());
                                        }
                                        BonusroadsandWaysFromThem[k.Item1].Add((f.Item1, copy));
                                        Bonusroadstohouses[k.Item1].Add(f.Item1);
                                    }
                                    break;
                                }
                                index++;
                            }
                        }
                    }
                }

                nowpos.Clear();
                foreach ((Vector3Int, int, List<Vector3Int>) s in newpos) nowpos.Add(s);
                newpos.Clear();
            }
        }
        List<Dictionary<Vector3Int, int>> ways = new List<Dictionary<Vector3Int, int>>();
        int ans = 0, total = 0;
        for (int i = 0; i < HousesInList.Count; i++)
        {
            Dictionary<int, List<Vector3Int>> temp = new Dictionary<int, List<Vector3Int>>();
            foreach (Vector3Int a in HousesAndRoadsAround[HousesInList[i]])
            {
                if (BonusroadsandWaysFromThem.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = BonusroadsandWaysFromThem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
                if (NowSystem.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = NowSystem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
            }
            if (bonushousesandroadsAround.ContainsKey(HousesInList[i]))
            {
                foreach (Vector3Int a in bonushousesandroadsAround[HousesInList[i]])
                {
                    List<(int, List<Vector3Int>)> now = BonusroadsandWaysFromThem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
            }
            if (temp.Count != Houses.Count + bonushouses.Count)
                return -1f;
            foreach (int a in temp.Keys)
            {
                if (a != i)
                {
                    total++;
                    List<Vector3Int> b = temp[a];
                    for (int j = 0; j < b.Count; j++)
                    {
                        if (ways.Count > j && ways[j].ContainsKey(b[j]))
                        {
                            ans += ways[j][b[j]];
                            ways[j][b[j]]++;
                        }
                        else if (ways.Count <= j)
                        {
                            ways.Add(new Dictionary<Vector3Int, int>());
                            ways[j].Add(b[j], 1);
                        }
                        else
                        {
                            ways[j].Add(b[j], 1);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < BonusHousesInList.Count; i++)
        {
            Dictionary<int, List<Vector3Int>> temp = new Dictionary<int, List<Vector3Int>>();
            foreach (Vector3Int a in bonushousesandroadsAround[BonusHousesInList[i]])
            {
                if (BonusroadsandWaysFromThem.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = BonusroadsandWaysFromThem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
                if (NowSystem.ContainsKey(a))
                {
                    List<(int, List<Vector3Int>)> now = NowSystem[a];
                    foreach ((int, List<Vector3Int>) b in now)
                    {
                        if (!temp.ContainsKey(b.Item1)) temp.Add(b.Item1, b.Item2);
                        else if (temp[b.Item1].Count > b.Item2.Count) temp[b.Item1] = b.Item2;
                    }
                }
            }
            if (temp.Count != Houses.Count + bonushouses.Count) return -1f;
            foreach (int a in temp.Keys)
            {
                if (a != i)
                {
                    total++;
                    List<Vector3Int> b = temp[a];
                    for (int j = 0; j < b.Count; j++)
                    {
                        if (ways.Count > j && ways[j].ContainsKey(b[j]))
                        {
                            ans += ways[j][b[j]];
                            ways[j][b[j]]++;
                        }
                        else if (ways.Count <= j)
                        {
                            ways.Add(new Dictionary<Vector3Int, int>());
                            ways[j].Add(b[j], 1);
                        }
                        else
                        {
                            ways[j].Add(b[j], 1);
                        }
                    }
                }
            }
        }
        return (double)ans / ways.Count;
    }

    private void TestSystem(List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd)
    {
        Dictionary<Vector3Int, List<Vector3Int>> BonusRoads = new Dictionary<Vector3Int, List<Vector3Int>>();
        Dictionary<Vector3Int, ThingsInCell> BonusHouses = new Dictionary<Vector3Int, ThingsInCell>();
        Dictionary<Vector3Int, List<Vector3Int>> BonushousesandroadsAround = new Dictionary<Vector3Int, List<Vector3Int>>();
        List<Vector3Int> tocheck = new List<Vector3Int>();
        for (int f = 0; f < TilesToAdd.Count; f++)
        {
            Vector3Int NowPosition = TilesToAdd[f].Item1;
            if (TilesToAdd[f].Item2 == ThingsInCell.RoadForCars)
            {

                BonusRoads.Add(NowPosition, new List<Vector3Int>());
                foreach (Vector3Int a in TilesToAdd[f].Item3)
                    BonusRoads[NowPosition].Add(a);
                foreach (Vector3Int a in TilesToAdd[f].Item4)
                    if (BonusRoads.ContainsKey(a)) BonusRoads[a].Add(NowPosition);
                    else BonusRoads.Add(a, new List<Vector3Int>() { NowPosition });
                if (!tocheck.Contains(NowPosition))
                    tocheck.Add(NowPosition);
                List<Vector3Int> ans2 = new List<Vector3Int>();
                ans2.Add(new Vector3Int(NowPosition.x - 1, NowPosition.y, 0));
                ans2.Add(new Vector3Int(NowPosition.x + 1, NowPosition.y, 0));
                ans2.Add(new Vector3Int(NowPosition.x, NowPosition.y - 1, 0));
                ans2.Add(new Vector3Int(NowPosition.x, NowPosition.y + 1, 0));
                foreach (Vector3Int a in ans2)
                    if (Houses.ContainsKey(a) || BonusHouses.ContainsKey(a))
                    {
                        if (BonushousesandroadsAround.ContainsKey(a)) BonushousesandroadsAround[a].Add(NowPosition);
                        else BonushousesandroadsAround.Add(a, new List<Vector3Int>() { NowPosition });
                    }
            }
            else
            {
                BonusHouses.Add(TilesToAdd[f].Item1, TilesToAdd[f].Item2);
                List<Vector3Int> NearRoads = new List<Vector3Int>();
                BonushousesandroadsAround.Add(TilesToAdd[f].Item1, NearRoads);
                List<Vector3Int> ans2 = new List<Vector3Int>();
                ans2.Add(new Vector3Int(NowPosition.x - 1, NowPosition.y, 0));
                ans2.Add(new Vector3Int(NowPosition.x + 1, NowPosition.y, 0));
                ans2.Add(new Vector3Int(NowPosition.x, NowPosition.y - 1, 0));
                ans2.Add(new Vector3Int(NowPosition.x, NowPosition.y + 1, 0));
                foreach (Vector3Int a in ans2) if (Roads.ContainsKey(a) || BonusRoads.ContainsKey(a))
                    {
                        if (!tocheck.Contains(a))
                            tocheck.Add(a);
                        NearRoads.Add(a);
                    }
            }
        }
        double nowEfficiency = GetWaysToEachOtherFor1(ref BonusRoads, ref BonusHouses, ref tocheck, ref BonushousesandroadsAround);
        // UnityEngine.Debug.Log(nowEfficiency);
        if (nowEfficiency != -1f)
        {
            if (bestEfficienty == nowEfficiency && nowEfficiency != -1)
            {
                if (bestVariant == ThingsInCell.RoadForCars)
                {
                    if (bestRoadsFrom.Count + bestRoadsTo.Count > TilesToAdd[0].Item3.Count + TilesToAdd[0].Item4.Count)
                    {
                        bestPosition = TilesToAdd[0].Item1;
                        bestVariant = TilesToAdd[0].Item2;
                        bestRoadsFrom = TilesToAdd[0].Item3;
                        bestRoadsTo = TilesToAdd[0].Item4;
                    }
                }
            }
            else if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
            {
                bestEfficienty = nowEfficiency;
                bestPosition = TilesToAdd[0].Item1;
                bestVariant = TilesToAdd[0].Item2;
                bestRoadsFrom = TilesToAdd[0].Item3;
                bestRoadsTo = TilesToAdd[0].Item4;
            }
        }
    }
}
