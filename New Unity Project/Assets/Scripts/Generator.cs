using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

public class Generator : MonoBehaviour
{
    public GridFunc grid;

    public int r, N;
    public bool MinimumRoads;
    public void Start()
    {
        GenerateCity(N);
    }

    /// <summary>
    /// Генерирует город с Count домами
    /// </summary>
    /// <param name="Count">Количество домов</param>
    public void GenerateCity(int Count)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        List<Vector3Int> canBePositions = new List<Vector3Int>();
        List<Vector3Int> wasPositions = new List<Vector3Int>();
        Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
        Dictionary<Vector3Int, List<Vector3Int>> Roads = new Dictionary<Vector3Int, List<Vector3Int>>();
        List<Vector3Int> newRoads = new List<Vector3Int>();
        bool CreateRoadsBetweenHouses(Vector3Int from, Vector3Int to)
        {
            Dictionary<Vector3Int, int> PositionIND0 = new Dictionary<Vector3Int, int>();
            Dictionary<Vector3Int, int> PositionIND1 = new Dictionary<Vector3Int, int>();
            Dictionary<Vector3Int, (Vector3Int, int)> USED = new Dictionary<Vector3Int, (Vector3Int, int)>();

            bool ok = false;
            int minWay = int.MaxValue;
            USED.Add(from, (new Vector3Int(), 0));
            PositionIND0.Add(from, 0);
            Vector3Int last = to;
            bool nowind = false;
            while (PositionIND0.Count != 0&&!nowind||PositionIND1.Count!=0&&nowind)
            {
                Dictionary<Vector3Int, int> nowPosition = nowind?PositionIND1:PositionIND0, newPosition = nowind?PositionIND0:PositionIND1;
                foreach (Vector3Int a in nowPosition.Keys)
                {
                    Vector3Int aPos = a;
                    int NewRoads = nowPosition[a];
                    if (NewRoads > minWay)
                    {
                        continue;
                    }
                    if (aPos == to&&Roads.Count==0)
                    {
                        ok = true;
                        if (minWay>NewRoads)
                        {
                            minWay = NewRoads;
                            last = aPos;
                        }
                        continue;
                    }
                    if (Roads.ContainsKey(a))
                    {
                        ok = true;
                        if (minWay > NewRoads)
                        {
                            minWay = NewRoads;
                            last = aPos;
                        }
                        continue;
                    }
                    if (Houses.ContainsKey(aPos) && aPos != from)
                    {
                        continue;
                    }
                    if (!ok||MinimumRoads)
                    {
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(aPos.x + i, aPos.y, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > NewRoads + roads)
                            {
                                if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads + roads);
                                else newPosition[temp] = NewRoads + roads;
                                if (USED.ContainsKey(temp))
                                {
                                    USED[temp] = (aPos, NewRoads + roads);
                                }
                                else USED.Add(temp, (aPos, NewRoads));
                            }
                        }
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(aPos.x, aPos.y + i, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;
                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > NewRoads + roads)
                            {
                                if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads + roads);
                                else newPosition[temp] = NewRoads + roads;
                                if (USED.ContainsKey(temp))
                                {
                                    USED[temp] = (aPos, NewRoads + roads);
                                }
                                else USED.Add(temp, (aPos, NewRoads));
                            }
                        }
                    }
                }
                nowPosition.Clear();
                nowind = !nowind;
            }
            if (!ok)
            {
                //UnityEngine.Debug.LogError("Can't find way");
                return false;
            }
            else
            {
                Vector3Int tosave = to;
                to = last;
                Vector3Int nowpos = USED[last].Item1;
                while (nowpos != from)
                {
                    if (to != tosave)
                    {
                         if (Roads.ContainsKey(nowpos))
                         {
                            Roads[nowpos].Add(to);
                         }
                         else
                         {
                            Roads.Add(nowpos, new List<Vector3Int>() { to });
                            if (!newRoads.Contains(nowpos))newRoads.Add(nowpos);
                         }
                         if (Roads.ContainsKey(to))
                         {
                             Roads[to].Add(nowpos);
                         }
                         else
                         {
                            Roads.Add(to, new List<Vector3Int>() { nowpos });
                            if (!newRoads.Contains(to)) newRoads.Add(to);
                         }
                    }
                    to = nowpos;
                    if (!USED.ContainsKey(to)) break;
                    nowpos = USED[to].Item1;
                }
                return true;
            }
        }

        void GetBonusPositions(Vector3Int position)
        {
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    Vector3Int NowPosition = new Vector3Int(position.x + i, position.y + j, 0);
                    if (!Houses.ContainsKey(NowPosition) &&
                        !Roads.ContainsKey(NowPosition)&&!wasPositions.Contains(NowPosition))
                    {
                         if (!canBePositions.Contains(NowPosition))canBePositions.Add(NowPosition);
                    }
                }
            }
        }
        void GetBonusPositionsForRoad(Vector3Int position)
        {
            for (int i = -1; i <= 1; i+=2)
            {
                for (int j = -1; j <= 1; j+=2)
                {
                    Vector3Int NowPosition = new Vector3Int(position.x + i, position.y + j, 0);
                    if (!Houses.ContainsKey(NowPosition) &&
                        !Roads.ContainsKey(NowPosition)&&!wasPositions.Contains(NowPosition))
                    {
                        if (!canBePositions.Contains(NowPosition)) canBePositions.Add(NowPosition);
                    }
                }
            }
        }
        int cntHousePeople = 1, cntHouseCom = 0, cntHouseFact = 0;
        Houses.Add(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
        GetBonusPositions(new Vector3Int(0, 0, 0));
        while (Houses.Count != Count)
        {
            if (canBePositions.Count != 0)
            {
                Vector3Int Position = canBePositions[UnityEngine.Random.Range(0, canBePositions.Count)];
                ThingsInCell whatadd;
                canBePositions.Remove(Position);
                wasPositions.Add(Position);
                bool ok = CreateRoadsBetweenHouses(Position, new Vector3Int(0, 0, 0));
                if (ok)
                {
                    if (cntHouseCom <= cntHouseFact && cntHousePeople >= cntHouseCom)
                    {
                        whatadd = ThingsInCell.HouseCom;
                        cntHouseCom++;
                    }
                    else if (cntHouseFact <= cntHouseCom && cntHousePeople >= cntHouseFact)
                    {
                        whatadd = ThingsInCell.HouseFact;
                        cntHouseFact++;
                    }
                    else
                    {
                        whatadd = ThingsInCell.HousePeople;
                        cntHousePeople++;
                    }
                    Houses.Add(Position, whatadd);
                    GetBonusPositions(Position);
                    for (int i = 0; i < canBePositions.Count; i++)
                    {
                        if (newRoads.Contains(canBePositions[i]))
                        {
                            canBePositions.RemoveAt(i);
                            i--;
                        }
                    }
                }
                else
                {
                    //UnityEngine.Debug.LogError(Position);
                    foreach (Vector3Int a in newRoads) Roads.Remove(a);
                }
                foreach (Vector3Int c in newRoads)
                {
                    canBePositions.Remove(c);
                    GetBonusPositionsForRoad(c);
                }
                newRoads.Clear();
            }
            else
            {
                UnityEngine.Debug.LogError("END PLACE");
                break;
            }
        }
        UnityEngine.Debug.Log(Houses.Count);
        timer.Stop();
        UnityEngine.Debug.Log(timer.ElapsedMilliseconds);
        Stopwatch timeVisible = new Stopwatch();
        timeVisible.Start();
        foreach (Vector3Int a in Houses.Keys)
        {
            grid.CreateNewTile(a, Houses[a]);
        }
        foreach (Vector3Int a in Roads.Keys)
        {
            grid.CreateNewTile(a, ThingsInCell.RoadForCars);
        }
        foreach (Vector3Int a in Roads.Keys)
        {
            List<Vector3Int> RoadsCell = Roads[a];
            grid.UniteTiles(a, RoadsCell);
        }
        timeVisible.Stop();
        UnityEngine.Debug.Log("TIME DRAW: " + Convert.ToString(timeVisible.ElapsedMilliseconds));
    }
}

