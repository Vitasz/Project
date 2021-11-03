using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class OptimizationAlgorithm : MonoBehaviour
{
    List<CellWithHouse> GranHouse = new List<CellWithHouse>();
    List<CellWithRoad> GranRoad = new List<CellWithRoad>();
    GridFunc grid;
    public bool isCoroutineWorking = false;
    Vector3Int bestPosition = new Vector3Int();
    double bestEfficienty = -1;
    public int Deep = 1;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    public bool AddRoad, AddHouse;
    List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople,/* ThingsInCell.HouseCom, ThingsInCell.HouseFact,*/ ThingsInCell.RoadForCars };
    private int total = 0;
    private bool IsGran(Vector3Int position)
    {
        return !(grid.Map.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z)));
    }
    private List<Vector3Int> GetGrans(Vector3Int position)
    {
        List<Vector3Int> Grans = new List<Vector3Int>();
        if (!grid.Map.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x - 1, position.y, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x + 1, position.y, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z))) Grans.Add(new Vector3Int(position.x, position.y - 1, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z))) Grans.Add(new Vector3Int(position.x, position.y + 1, position.z));
        return Grans;
    }
    public void Optimization(GridFunc grid)
    {
        this.grid = grid;
        if (grid.Map.Count == 0) grid.CreateNewTile(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople, false);
        foreach (Vector3Int a in grid.Map.Keys)
        {
            if (IsGran(a))
            {
                if (grid.Map[a] is CellWithHouse) GranHouse.Add(grid.Map[a] as CellWithHouse);
                else if (grid.Map[a] is CellWithRoad) GranRoad.Add(grid.Map[a] as CellWithRoad);
            }
        }
        //UnityEngine.Debug.Log(grid.StartSimmulation());
        StartCoroutine("F");
    }
    IEnumerator F()
    {
        while (true)
        {
            List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> PositionsToCheck = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();//{Position, Type, RoadsFrom, RoadsTo, Bonus grans(if type is road))}
            List<Vector3Int> was = new List<Vector3Int>();
            List<ThingsInCell> VariantsToAddStart = new List<ThingsInCell>();
            if (AddRoad) VariantsToAddStart.Add(ThingsInCell.RoadForCars);
            if (AddHouse) VariantsToAddStart.Add(ThingsInCell.HousePeople);
            foreach (CellWithRoad cell in GranRoad)
                foreach (Vector3Int PositionGran in GetGrans(cell.GetCellPosition()))
                {
                    if (!was.Contains(PositionGran)) was.Add(PositionGran);
                    else continue;
                    PositionsToCheck.Add(new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>());
                    foreach (ThingsInCell type in VariantsToAddStart)
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
            Hod(ref PositionsToCheck,ref temp, -1, Deep, new List<Vector3Int>());
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
    private void TestSystem(List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd)
    {
        for (int f = 0; f < TilesToAdd.Count; f++)
        {
            grid.CreateNewTile(TilesToAdd[f].Item1, TilesToAdd[f].Item2, true);
            if (TilesToAdd[f].Item2 == ThingsInCell.RoadForCars)
            {
                foreach (Vector3Int a in TilesToAdd[f].Item3)
                    grid.UniteTiles(TilesToAdd[f].Item1, a, ThingsInCell.RoadForCars, true);
                foreach (Vector3Int a in TilesToAdd[f].Item4)
                    grid.UniteTiles(a, TilesToAdd[f].Item1, ThingsInCell.RoadForCars, true);
            }
        }
        double nowEfficiency = grid.StartSimmulation();
        if (nowEfficiency != -1f)
        {
            if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
            {
                bestEfficienty = nowEfficiency;
                bestPosition = TilesToAdd[0].Item1;
                bestVariant = TilesToAdd[0].Item2;
                bestRoadsFrom = TilesToAdd[0].Item3;
                bestRoadsTo = TilesToAdd[0].Item4;
               // UnityEngine.Debug.Log(Convert.ToString(bestRoadsFrom.Count) + ' ' + Convert.ToString(bestRoadsTo.Count));
            }
        }
        for (int f = 0; f < TilesToAdd.Count; f++)
            grid.RemoveTileAt(TilesToAdd[f].Item1);
    }
    private List<(List<Vector3Int>, List<Vector3Int>)> GetRoadsVariants(Vector3Int Position)
    {
        List<(List<Vector3Int>, List<Vector3Int>)> res = new List<(List<Vector3Int>, List<Vector3Int>)>();
        List<Vector3Int> CanBeRoadsFrom = grid.PositionsRoadAround(Position);
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
    private void Hod(ref List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nowdeep,ref List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd,int prevpos, int deep, List<Vector3Int> PosToadd)
    {
        if (nowdeep.Count==0)
        {
            TestSystem(TilesToAdd);
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
                    TestSystem(TilesToAdd);
                    total++;
                }
                else
                {
                    List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>> nextdeep = new List<List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>>();
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> a in nowdeep) nextdeep.Add(a);
                    foreach (List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> b in BonusFor(nowdeep[i][j])) nextdeep.Add(b);
                    Hod(ref nextdeep,ref TilesToAdd, i, deep - 1, PosToadd);
                }
                TilesToAdd.RemoveAt(TilesToAdd.Count - 1);
                PosToadd.RemoveAt(PosToadd.Count - 1);
            }
        }
    }
}
