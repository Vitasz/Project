using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OptimizationAlgorithm:MonoBehaviour
{
    List<CellWithHouse> GranHouse = new List<CellWithHouse>();
    List<CellWithRoad> GranRoad = new List<CellWithRoad>();
    GridFunc grid;
    public bool isCoroutineWorking = false;
    Vector3Int bestPosition = new Vector3Int();
    double bestEfficienty = -1;
    private readonly int Deep = 3;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople,/* ThingsInCell.HouseCom, ThingsInCell.HouseFact,*/ ThingsInCell.RoadForCars };

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
        if (!grid.Map.ContainsKey(new Vector3Int(position.x , position.y-1, position.z)) )Grans.Add(new Vector3Int(position.x, position.y - 1, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x, position.y+1, position.z))) Grans.Add(new Vector3Int(position.x, position.y+1, position.z));
        return Grans;
    }
    public void Optimization(GridFunc grid)
    {
        Debug.Log("OPTIMIZE");
        this.grid = grid;
        if (grid.Map.Count == 0) grid.CreateNewTile(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople, false);
        foreach(Vector3Int a in grid.Map.Keys)
        {
            if (IsGran(a))
            {
                if (grid.Map[a] is CellWithHouse) GranHouse.Add(grid.Map[a] as CellWithHouse);
                else if (grid.Map[a] is CellWithRoad) GranRoad.Add(grid.Map[a] as CellWithRoad);
            }
        }
        StartCoroutine("F");
    }
    IEnumerator F()
    {
        while (true)
        {
            List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>,List<Vector3Int>)> PositionsToCheck = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>, List<Vector3Int>)>();//{Position, Type, RoadsFrom, RoadsTo, Bonus grans(if type is road))}
            foreach (CellWithRoad cell in GranRoad)
                foreach (Vector3Int PositionGran in GetGrans(cell.GetCellPosition()))
                        foreach(ThingsInCell type in Variants)
                            if (type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact || type == ThingsInCell.HouseFact)
                                PositionsToCheck.Add((PositionGran, type, null, null, null));
                            else if (type==ThingsInCell.RoadForCars)
                            {
                                List<Vector3Int> Bonus = new List<Vector3Int>();
                                foreach (Vector3Int bonusgran in GetGrans(PositionGran)) Bonus.Add(bonusgran);
                                foreach ((List<Vector3Int>, List<Vector3Int>) a in GetRoadsVariants(PositionGran))
                                    PositionsToCheck.Add((PositionGran, type, a.Item1, a.Item2, Bonus));
                            }
            //List<int> indexs = new List<int>(Deep);
            //for (int i = 0; i < Deep; i++) //Start indexs is {0, 1, 2...}
            //    indexs[i] = i;
            int totalVariants = 0;
            for (int i = 0; i < PositionsToCheck.Count; i++)
            {
                List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>, List<Vector3Int>)> Bonus = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>,List<Vector3Int>)>();
                if (PositionsToCheck[i].Item2 == ThingsInCell.RoadForCars)
                    foreach (Vector3Int PositionGran in PositionsToCheck[i].Item5)
                        foreach (ThingsInCell type in Variants)
                            if (type == ThingsInCell.HouseCom || type == ThingsInCell.HouseFact || type == ThingsInCell.HouseFact)
                                Bonus.Add((PositionGran, type, null, null, null));
                            else if (type == ThingsInCell.RoadForCars)
                            {
                                List<Vector3Int> BonusNew = new List<Vector3Int>();
                                foreach (Vector3Int bonusgran in GetGrans(PositionGran)) BonusNew.Add(bonusgran);
                                foreach((List<Vector3Int>, List<Vector3Int>) a in GetRoadsVariants(PositionGran))
                                    Bonus.Add((PositionGran, type, a.Item1, a.Item2, BonusNew));
                            }

                List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)> TilesToAdd = new List<(Vector3Int, ThingsInCell, List<Vector3Int>, List<Vector3Int>)>();//{pos, type, roadsfrom, roadsto};
                TilesToAdd.Add((PositionsToCheck[i].Item1, PositionsToCheck[i].Item2, PositionsToCheck[i].Item3, PositionsToCheck[i].Item4));
                for (int j = i+1; j < PositionsToCheck.Count; j++)
                {
                    if (PositionsToCheck[j].Item1 == PositionsToCheck[i].Item1) continue;
                    TilesToAdd.Add((PositionsToCheck[j].Item1, PositionsToCheck[j].Item2, PositionsToCheck[j].Item3, PositionsToCheck[j].Item4));

                    for (int f = 0; f < TilesToAdd.Count; f++)
                    {
                        grid.CreateNewTile(TilesToAdd[f].Item1, TilesToAdd[f].Item2, true);
                        if (TilesToAdd[f].Item2 == ThingsInCell.RoadForCars)
                        {
                            foreach (Vector3Int a in TilesToAdd[f].Item3)
                                grid.UniteTiles(TilesToAdd[f].Item1, a, ThingsInCell.RoadForCars, true) ;
                            foreach (Vector3Int a in TilesToAdd[f].Item4)
                                grid.UniteTiles(a, TilesToAdd[f].Item1, ThingsInCell.RoadForCars, true);
                        }
                    }
                   // yield return new WaitForEndOfFrame();
                    double nowEfficiency = grid.StartSimmulation(this);
                    totalVariants++;
                    if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
                    {
                        bestEfficienty = nowEfficiency;
                        bestPosition = TilesToAdd[0].Item1;
                        bestVariant = TilesToAdd[0].Item2;
                        bestRoadsFrom = TilesToAdd[0].Item3;
                        bestRoadsTo = TilesToAdd[0].Item4;
                    }
                    for (int f = 0; f < TilesToAdd.Count; f++)
                    {
                        grid.RemoveTileAt(TilesToAdd[f].Item1);
                    }
                    TilesToAdd.RemoveAt(1);
                    
                }
                for (int j = 0; j < Bonus.Count; j++)
                {
                    if (Bonus[j].Item1 == PositionsToCheck[i].Item1) continue;
                    TilesToAdd.Add((Bonus[j].Item1, Bonus[j].Item2, Bonus[j].Item3, Bonus[j].Item4));

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
                    // yield return new WaitForEndOfFrame();
                    double nowEfficiency = grid.StartSimmulation(this);
                    totalVariants++;
                    if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
                    {
                        bestEfficienty = nowEfficiency;
                        bestPosition = TilesToAdd[0].Item1;
                        bestVariant = TilesToAdd[0].Item2;
                        bestRoadsFrom = TilesToAdd[0].Item3;
                        bestRoadsTo = TilesToAdd[0].Item4;
                    }
                    for (int f = 0; f < TilesToAdd.Count; f++)
                    {
                        grid.RemoveTileAt(TilesToAdd[f].Item1);
                    }
                    TilesToAdd.RemoveAt(1);

                }
                TilesToAdd.RemoveAt(0);
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("Total variants:" + Convert.ToString(totalVariants));
            grid.CreateNewTile(bestPosition, bestVariant, false);
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
            foreach (Vector3Int a in grid.Map.Keys)
            {
                if (IsGran(a))
                {
                    if (grid.Map[a] is CellWithHouse) GranHouse.Add(grid.Map[a] as CellWithHouse);
                    else if (grid.Map[a] is CellWithRoad) GranRoad.Add(grid.Map[a] as CellWithRoad);
                }
            }
            /*while (indexs[0] < PositionsToCheck.Count-Deep)
            {
                List<List<Vector3Int>> BonusesOnDeep = new List<List<Vector3Int>>(Deep);
                for (int i = 0; i < indexs.Count; i++)
                {
                    if (PositionsToCheck[i].Item2 == ThingsInCell.RoadForCars)
                    {
                        foreach (Vector3Int a in PositionsToCheck[i].Item5) BonusesOnDeep[i].Add(a);
                    }
                }
                for (int i = 0; i < indexs.Count; i++)
                {
                    grid.CreateNewTile(PositionsToCheck[indexs[i]].Item1, PositionsToCheck[indexs[i]].Item2, true);
                    if (PositionsToCheck[indexs[i]].Item2 == ThingsInCell.RoadForCars)
                    {
                        foreach (Vector3Int a in PositionsToCheck[indexs[i]].Item3)
                            grid.UniteTiles(a, PositionsToCheck[indexs[i]].Item1, ThingsInCell.RoadForCars, true);
                        foreach (Vector3Int a in PositionsToCheck[indexs[i]].Item4)
                            grid.UniteTiles(PositionsToCheck[indexs[i]].Item1, a, ThingsInCell.RoadForCars, true);
                    }
                }
                double nowEfficiency = grid.StartSimmulation(this);
                if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
                {
                    bestEfficienty = nowEfficiency;
                    bestPosition = PositionsToCheck[indexs[0]].Item1;
                    bestVariant = PositionsToCheck[indexs[0]].Item2;
                    bestRoadsFrom = PositionsToCheck[indexs[0]].Item3;
                    bestRoadsTo = PositionsToCheck[indexs[0]].Item4;
                }
                totalVariants++;
            }
            grid.CreateNewTile(bestPosition, bestVariant, false);
            if (bestVariant == ThingsInCell.RoadForCars)
            {
                foreach (Vector3Int a in bestRoadsTo)
                    grid.UniteTiles(a, bestPosition, ThingsInCell.RoadForCars, false);
                foreach (Vector3Int a in bestRoadsFrom)
                    grid.UniteTiles(bestPosition, a, ThingsInCell.RoadForCars, false);
            }*/

            /*foreach (Vector3Int b in PositionsToCheck)
            {
                foreach (ThingsInCell c in Variants)
                {
                    if (c == ThingsInCell.HouseCom || c == ThingsInCell.HouseFact || c == ThingsInCell.HousePeople)
                    {
                        grid.CreateNewTile(b, c, true);
                        Hod(PosToCheckWithoutWas, Deep-1, b, c, null, null);
                        grid.RemoveTileAt(b);
                    }
                    else if (c == ThingsInCell.RoadForCars)
                    {
                        List<Vector3Int> CanBeRoadsFrom = grid.PositionsRoadAround(b);
                        int st1 = 1;
                        for (int i = 0; i < CanBeRoadsFrom.Count; i++)
                        {
                            st1 *= 2;
                        }
                        for (int i = 0; i < st1; i++)
                        {
                            int now1 = i;
                            List<Vector3Int> RoadsFrom = new List<Vector3Int>();
                            int pos = 0;
                            List<Vector3Int> CanBeRoadsTo = new List<Vector3Int>();
                            foreach(Vector3Int a in CanBeRoadsFrom)
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
                                }
                                grid.CreateNewTile(b, c, true);
                                foreach (Vector3Int a in GetGrans(b)) if (!PosToCheckWithoutWas.Contains(a)) PosToCheckWithoutWas.Add(a);
                                foreach (Vector3Int position in RoadsFrom)
                                    grid.UniteTiles(b, position, ThingsInCell.RoadForCars,true);
                                foreach (Vector3Int position in RoadsTo)
                                    grid.UniteTiles(position, b, ThingsInCell.RoadForCars,true);
                                Hod(PosToCheckWithoutWas, Deep-1, b, c, RoadsFrom, RoadsTo);
                                PosToCheckWithoutWas.Clear();
                                foreach (Vector3Int a in PositionsToCheck) if (!wasStart.Contains(a)) PosToCheckWithoutWas.Add(a);
                                grid.RemoveTileAt(b);
                                yield return new WaitForEndOfFrame();
                            }
                        }

                    }
                }
                Debug.Log(s);
                s++;
                PosToCheckWithoutWas.Remove(b);
                wasStart.Add(b);
                yield return new WaitForEndOfFrame();
            }
            Debug.Log(bestVariant);
            
            foreach (Vector3Int a in grid.Map.Keys)
            {
                if (IsGran(a))
                {
                    if (grid.Map[a] is CellWithHouse) GranHouse.Add(grid.Map[a] as CellWithHouse);
                    else if (grid.Map[a] is CellWithRoad) GranRoad.Add(grid.Map[a] as CellWithRoad);
                }
            }
            wasStart.Clear();
            Debug.Log("BEST VARIANT FIND");
            bestEfficienty = -1;
            yield return new WaitForEndOfFrame();*/
        }
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
                }
                res.Add((RoadsFrom, RoadsTo));
            }
        }
        return res;
    }
    private void Hod(List<Vector3Int>PosToCheck, int deep, Vector3Int start, ThingsInCell startthing, List<Vector3Int>startRoadsFrom, List<Vector3Int> startRoadsTo)
    {
        
        if (deep != 0)
        {

            List<Vector3Int> wasStart = new List<Vector3Int>();
            List<Vector3Int> PosToCheckCopy = new List<Vector3Int>();
            foreach (Vector3Int b in PosToCheck) PosToCheckCopy.Add(b);
            foreach (Vector3Int b in PosToCheck)
            {
                if (!grid.Map.ContainsKey(b))
                {
                    foreach (ThingsInCell c in Variants)
                    {
                        if (c == ThingsInCell.HouseCom || c == ThingsInCell.HouseFact || c == ThingsInCell.HousePeople)
                        {

                            grid.CreateNewTile(b, c, true);
                            Hod(PosToCheckCopy, deep - 1, start, startthing, startRoadsFrom, startRoadsTo);
                            grid.RemoveTileAt(b);
                        }
                        else if (c == ThingsInCell.RoadForCars)
                        {
                            List<Vector3Int> CanBeRoadsFrom = grid.PositionsRoadAround(b);
                            int st1 = 1;
                            for (int i = 1; i < CanBeRoadsFrom.Count; i++)
                            {
                                st1 *= 2;
                            }
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
                                }
                                int st2 = 1;

                                for (int j = 1; j < CanBeRoadsTo.Count; j++)
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
                                    }

                                    grid.CreateNewTile(b, c, true);
                                    foreach (Vector3Int a in GetGrans(b)) if (!PosToCheckCopy.Contains(a)) PosToCheckCopy.Add(a);
                                    foreach (Vector3Int position in RoadsFrom)
                                        grid.UniteTiles(b, position, ThingsInCell.RoadForCars, true);
                                    foreach (Vector3Int position in RoadsTo)
                                        grid.UniteTiles(position, b, ThingsInCell.RoadForCars, true);
                                    Hod(PosToCheckCopy, deep-1, start, startthing, startRoadsFrom, startRoadsTo);
                                    PosToCheckCopy.Clear();
                                    foreach (Vector3Int a in PosToCheck) if (!wasStart.Contains(a)) PosToCheckCopy.Add(a);
                                    grid.RemoveTileAt(b);
                                }
                            }

                        }
                    }
                }
                PosToCheckCopy.Remove(b);
                wasStart.Add(b);
            }
        }
        else
        {
            double nowEfficiency = grid.StartSimmulation(this);
            if (bestEfficienty == -1 || bestEfficienty > nowEfficiency)
            {
                bestEfficienty = nowEfficiency;
                bestPosition = start;
                bestVariant = startthing;
                bestRoadsFrom = startRoadsFrom;
                bestRoadsTo = startRoadsTo;
            }
        }
    }
}
