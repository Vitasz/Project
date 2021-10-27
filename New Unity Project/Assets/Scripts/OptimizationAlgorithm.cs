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
    private int Deep = 2;
    ThingsInCell bestVariant;
    List<Vector3Int> bestRoadsFrom = new List<Vector3Int>();
    List<Vector3Int> bestRoadsTo = new List<Vector3Int>();
    List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople, ThingsInCell.HouseCom, ThingsInCell.HouseFact, ThingsInCell.RoadForCars };

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
            List<Vector3Int> PositionsToCheck = new List<Vector3Int>(), PosToCheckCopy = new List<Vector3Int>();
            foreach (CellWithRoad a in GranRoad)
                foreach (Vector3Int b in GetGrans(a.GetCellPosition()))
                    if (!PositionsToCheck.Contains(b))
                    {
                        PositionsToCheck.Add(b);
                        PosToCheckCopy.Add(b);
                    }


            Debug.Log("To Check:" + Convert.ToString(PositionsToCheck.Count));
            List<Vector3Int> wasStart = new List<Vector3Int>();

            int s = 0;
            foreach (Vector3Int b in PositionsToCheck)
            {
                foreach (ThingsInCell c in Variants)
                {
                    if (c == ThingsInCell.HouseCom || c == ThingsInCell.HouseFact || c == ThingsInCell.HousePeople)
                    {
                        grid.CreateNewTile(b, c, true);

                        Hod(PosToCheckCopy, Deep-1, b, c, null, null);
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
                                foreach (Vector3Int a in GetGrans(b)) if (!PosToCheckCopy.Contains(a)) PosToCheckCopy.Add(a);
                                foreach (Vector3Int position in RoadsFrom)
                                    grid.UniteTiles(b, position, ThingsInCell.RoadForCars,true);
                                foreach (Vector3Int position in RoadsTo)
                                    grid.UniteTiles(position, b, ThingsInCell.RoadForCars,true);
                                Hod(PosToCheckCopy, Deep-1, b, c, RoadsFrom, RoadsTo);
                                PosToCheckCopy.Clear();
                                foreach (Vector3Int a in PositionsToCheck) if (!wasStart.Contains(a)) PosToCheckCopy.Add(a);
                                grid.RemoveTileAt(b);
                                yield return new WaitForEndOfFrame();
                            }
                        }

                    }
                }
                Debug.Log(s);
                s++;
                PosToCheckCopy.Remove(b);
                wasStart.Add(b);
                yield return new WaitForEndOfFrame();
            }
            Debug.Log(bestVariant);
            grid.CreateNewTile(bestPosition, bestVariant, false);
            if (bestVariant == ThingsInCell.RoadForCars)
            {
                foreach(Vector3Int a in bestRoadsTo)
                    grid.UniteTiles(a, bestPosition, ThingsInCell.RoadForCars,false);
                foreach (Vector3Int a in bestRoadsFrom)
                    grid.UniteTiles(bestPosition, a, ThingsInCell.RoadForCars,false);
            }
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
            yield return new WaitForEndOfFrame();
        }
    }
    private void Hod(List<Vector3Int>PosToCheck, int deep, Vector3Int start, ThingsInCell startthing, List<Vector3Int>startRoadsFrom, List<Vector3Int> startRoadsTo)
    {
        
        if (deep != 0)
        {
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
                            PosToCheckCopy.Remove(b);
                            Hod(PosToCheckCopy, deep - 1, b, c, startRoadsFrom, startRoadsTo);
                            grid.RemoveTileAt(b);
                            PosToCheckCopy.Add(b);
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
                                    Hod(PosToCheckCopy, deep-1, b, c, RoadsFrom, RoadsTo);
                                    PosToCheckCopy.Clear();
                                    foreach (Vector3Int a in PosToCheck) PosToCheckCopy.Add(a);
                                    grid.RemoveTileAt(b);
                                }
                            }

                        }
                    }
                }
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
