using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
            int cntroads = 0, cnthousesPeople = 0, cnthousesCom=0, cnthousesFact=0;
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
            if (PositionsToCheck.Count != 0) Hod(ref PositionsToCheck, temp, Deep, new List<Vector3Int>(), cntroads,cnthousesPeople,cnthousesCom,cnthousesFact);
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
    private void Hod(ref List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nowdeep,List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd, int deep, List<Vector3Int> PosToadd, int cntroads,int cnthousePeople,int cnthouseCom, int cnthouseFact)
    {
        ThingsInCell whatadd;
        if (cntroads <= (cnthouseFact + cnthouseCom + cnthousePeople) * 3)
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
        if (deep==Deep)UnityEngine.Debug.Log(whatadd);
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
                    tasks.Add(Task.Run(() => TestSystem(tilesToAddCopy, RoadsCopy, HousesCopy)));
                    total++;
                }
                else
                {
                    List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nextdeep = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> a in nowdeep) nextdeep.Add(a);
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> b in BonusFor(nowdeep[i][j])) nextdeep.Add(b);
                    Hod(ref nextdeep, TilesToAdd, deep - 1, PosToadd, cntroads, cnthousePeople,cnthouseCom,cnthouseFact);
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
    List<List<Vector3Int>> GetWaysToEachOther(ref Dictionary<Vector3Int, List<Vector3Int>> roads,ref Dictionary<Vector3Int, ThingsInCell> houses)
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
        foreach (Vector3Int House in houses.Keys)
        {
            foreach(Vector3Int a in GetNearTiles(House))
            {
                if (roads.ContainsKey(a))
                {
                    if (!roads[a].Contains(House)) roads[a].Add(House);
                }
            }
        }
        foreach (Vector3Int startPos in houses.Keys)
        {
            List<Vector3Int> startpositions = new List<Vector3Int>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector3Int now = new Vector3Int(startPos.x + i, startPos.y + j, startPos.z);
                    if ((i != 0 || j != 0) && Math.Abs(i) + Math.Abs(j) <= 1 && roads.ContainsKey(now)) startpositions.Add(now);
                }
            }
            int HousesFind = 0;
            Dictionary<Vector3Int, bool> was = new Dictionary<Vector3Int, bool>();
            was.Add(startPos, false);
            Dictionary<Vector3Int, List<Vector3Int>> positions = new Dictionary<Vector3Int, List<Vector3Int>>(), newpositions = new Dictionary<Vector3Int, List<Vector3Int>>();
            foreach (Vector3Int a in startpositions)
            {
                positions.Add(a, new List<Vector3Int>());
            }
            while (positions.Count != 0 && HousesFind != houses.Count - 1)
            {
                foreach (Vector3Int a in positions.Keys)
                {
                    was.Add(a, false);
                    if (houses.ContainsKey(a))
                    {
                        if (houses[a] != houses[startPos])
                        {
                            ans.Add(positions[a]);
                        }
                        HousesFind++;
                    }
                    else
                    {
                        List<Vector3Int> way = positions[a];
                        way.Add(a);
                        foreach (Vector3Int b in roads[a])
                        {
                            if (!was.ContainsKey(b)&&!newpositions.ContainsKey(b))newpositions.Add(b, way);
                        }
                    }
                }
                positions.Clear();
                foreach (Vector3Int a in newpositions.Keys)
                    positions.Add(a, newpositions[a]);
                newpositions.Clear();
            }
            if (houses.Count - 1 != HousesFind) return null;
        }
        return ans;

    }
    private void TestSystem(List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd,
        Dictionary<Vector3Int, List<Vector3Int>> roads, Dictionary<Vector3Int, ThingsInCell> houses)
    {
        /*int indexOk = -1;
        for (int i = 0; i < TilesToAdd.Count-1; i++)
        {
            if (StartVariants.Contains(TilesToAdd[i].Item2))
            {
                indexOk = i;
            }
        }
        if (indexOk == -1) return;*/
        double GetEfficiencyOfSystem()
        {
            List<List<Vector3Int>> WaysTo = GetWaysToEachOther(ref roads, ref houses);
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
        if (GetEfficiencyOfSystem() == -1f) return;
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
                bestPosition = TilesToAdd[0].Item1;
                bestVariant = TilesToAdd[0].Item2;
                bestRoadsFrom = TilesToAdd[0].Item3;
                bestRoadsTo = TilesToAdd[0].Item4;
    
            }
        }
    }
}
