using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
public class GetEfficiency
{
    
}
public class OptimizationAlgorithm : MonoBehaviour
{
    List<CellWithHouse> GranHouse = new List<CellWithHouse>();
    List<CellWithRoad> GranRoad = new List<CellWithRoad>();
    //GridFunc grid;
    public int Deep = 1;
    List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople,/* ThingsInCell.HouseCom, ThingsInCell.HouseFact,*/ ThingsInCell.RoadForCars };
    List<ThingsInCell> StartVariants = new List<ThingsInCell>();
    Dictionary<Vector3Int, List<Vector3Int>> Roads = new Dictionary<Vector3Int, List<Vector3Int>>();
    Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
    Dictionary<Vector3Int, ThingsInCell> MapCopy = new Dictionary<Vector3Int, ThingsInCell>();
    List<Task> tasks = new List<Task>();
    Vector3Int bestPosition = new Vector3Int();
    double bestEfficienty = -1;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    public bool AddRoad, AddHouse;
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
        foreach (Vector3Int a in grid.Map.Keys)
        {
            Cell cell= grid.Map[a];
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
            }
        }
        StartCoroutine(F(grid));
    }
    IEnumerator F(GridFunc grid)
    {
        while (true)
        {
            List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> PositionsToCheck = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();//{Position, Type, RoadsFrom, RoadsTo, Bonus grans(if type is road))}
            List<Vector3Int> was = new List<Vector3Int>();
            if (AddRoad) StartVariants.Add(ThingsInCell.RoadForCars);
            if (AddHouse) StartVariants.Add(ThingsInCell.HousePeople);
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
            if (PositionsToCheck.Count != 0) Hod(ref PositionsToCheck, temp, -1, Deep, new List<Vector3Int>());
            UnityEngine.Debug.Log(tasks.Count);
            yield return new WaitForEndOfFrame();
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            UnityEngine.Debug.Log("Total variants:" + Convert.ToString(total)+" Time: "+Convert.ToString(stopwatch.ElapsedMilliseconds));
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
            StartVariants.Clear();
            GranRoad.Clear();
            GranHouse.Clear();
            MapCopy.Clear();
            Roads.Clear();
            Houses.Clear();
            tasks.Clear();
            break;
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
                        res[res.Count-1].Add((PositionGran, type, null, null));
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
    private void Hod(ref List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nowdeep,List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd,int prevpos, int deep, List<Vector3Int> PosToadd)
    {
        if (nowdeep.Count==0)
        {
            List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> tilesToAddCopy = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>();
            foreach((Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>) a in TilesToAdd)
                tilesToAddCopy.Add(a);
            Dictionary<Vector3Int, List<Vector3Int>> RoadsCopy = new Dictionary<Vector3Int, List<Vector3Int>>();
            Dictionary<Vector3Int, ThingsInCell> HousesCopy = new Dictionary<Vector3Int, ThingsInCell>();
            
            foreach (Vector3Int a in Roads.Keys)
            {
                RoadsCopy.Add(a, new List<Vector3Int>());
                foreach(Vector3Int b in Roads[a])
                {
                    RoadsCopy[a].Add(b);
                }
            }
            foreach(Vector3Int a in Houses.Keys)
            {
                HousesCopy.Add(a, Houses[a]);
            }
            tasks.Add(Task.Run(()=>TestSystem(tilesToAddCopy, RoadsCopy, HousesCopy)));
            total++;
            return;
        }
        for (int i = prevpos+1; i < nowdeep.Count; i++)
        {
            for (int j = 0; j < nowdeep[i].Count; j++)
            {
                if (PosToadd.Contains(nowdeep[i][j].Item1)) continue;
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
                    tasks.Add(Task.Run(() => TestSystem(tilesToAddCopy, RoadsCopy, HousesCopy)));
                    total++;
                }
                else
                {
                    List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nextdeep = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> a in nowdeep) nextdeep.Add(a);
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> b in BonusFor(nowdeep[i][j])) nextdeep.Add(b);
                    Hod(ref nextdeep, TilesToAdd, i, deep - 1, PosToadd);
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
                    if (MapCopy[new Vector3Int(position.x + i, position.y + j, 0)]==ThingsInCell.RoadForCars)
                    {
                        ans.Add(new Vector3Int(position.x + i, position.y + j, 0));
                    }
                }
            }
        }
        return ans;
    }
    private void TestSystem(List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd,
        Dictionary<Vector3Int, List<Vector3Int>> roads, Dictionary<Vector3Int, ThingsInCell> houses)
    {
        int indexOk = -1;
        for (int i = 0; i < TilesToAdd.Count; i++)
        {
            if (StartVariants.Contains(TilesToAdd[i].Item2))
            {
                indexOk = i;
            }
        }
        if (indexOk == -1) return;
        double GetEfficiencyOfSystem()
        {
            List<Vector3Int> GetNearRoads(Vector3Int a)
            {
                List<Vector3Int> ans = new List<Vector3Int>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Vector3Int now = new Vector3Int(a.x + i, a.y + j, a.z);
                        if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1 && roads.ContainsKey(now)) ans.Add(now);
                    }
                }
                return ans;
            }
            List<Vector3Int> GetNearTiles(Vector3Int a)
            {
                List<Vector3Int> ans = new List<Vector3Int>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Vector3Int now = new Vector3Int(a.x + i, a.y + j, a.z);
                        if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1) ans.Add(now);
                    }
                }
                return ans;
            }
            List<List<Vector3Int>> WaysTo = new List<List<Vector3Int>>();
            bool Colorize(List<Vector3Int> startpositions, Vector3Int startPos)
            {
                int HousesFind = 0;
                Dictionary<Vector3Int, List<Vector3Int>> was = new Dictionary<Vector3Int, List<Vector3Int>>();
                was.Add(startPos, new List<Vector3Int>());
                List<(Vector3Int, List<Vector3Int>)> positions = new List<(Vector3Int, List<Vector3Int>)>(), newpositions = new List<(Vector3Int, List<Vector3Int>)>();
                foreach (Vector3Int a in startpositions)
                {
                    positions.Add((a, new List<Vector3Int>()));
                }
                while (positions.Count != 0 && HousesFind != houses.Count - 1)
                {
                    foreach ((Vector3Int, List<Vector3Int>) a in positions)
                    {
                        if (!was.ContainsKey(a.Item1))
                        {
                            was.Add(a.Item1, a.Item2);
                            a.Item2.Add(a.Item1);
                            foreach (Vector3Int b in GetNearTiles(a.Item1))
                            {
                                if (houses.ContainsKey(b) && !was.ContainsKey(b))
                                {
                                    if (houses[b] != houses[startPos])
                                    {
                                        WaysTo.Add(a.Item2);
                                    }
                                    was.Add(b, a.Item2);
                                    HousesFind++;
                                }
                            }
                            foreach (Vector3Int b in roads[a.Item1])
                            {
                                newpositions.Add((b, a.Item2));
                            }
                        }
                    }
                    positions.Clear();
                    foreach ((Vector3Int, List<Vector3Int>) a in newpositions)
                    {
                        positions.Add(a);
                    }
                    newpositions.Clear();
                }
                return houses.Count - 1 == HousesFind;
            }


            foreach (Vector3Int a in houses.Keys)
            {
                bool ok = Colorize(GetNearRoads(a), a);
                if (!ok) return -1f;
            }
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
        
        for (int f = 0; f < TilesToAdd.Count; f++)
        {
            if (TilesToAdd[f].Item2 == ThingsInCell.RoadForCars)
            {
                roads.Add(TilesToAdd[f].Item1, new List<Vector3Int>());
                foreach (Vector3Int a in TilesToAdd[f].Item3)
                    roads[TilesToAdd[f].Item1].Add(a);
                foreach (Vector3Int a in TilesToAdd[f].Item4)
                    roads[a].Add(TilesToAdd[f].Item1);
            }
            else houses.Add(TilesToAdd[f].Item1, TilesToAdd[f].Item2);
        }
        double nowEfficiency = GetEfficiencyOfSystem();
        //UnityEngine.Debug.Log(nowEfficiency);
        if (nowEfficiency != -1f)
        {
            if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
            {
                //UnityEngine.Debug.Log(TilesToAdd.Count);
                bestEfficienty = nowEfficiency;
                bestPosition = TilesToAdd[indexOk].Item1;
                bestVariant = TilesToAdd[indexOk].Item2;
                bestRoadsFrom = TilesToAdd[indexOk].Item3;
                bestRoadsTo = TilesToAdd[indexOk].Item4;
    
            }
        }
    }
}
