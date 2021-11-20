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
    public void Start()
    {
        StartCoroutine(GenerateCity(N));
    }

    /// <summary>
    /// Генерирует город с Count домами
    /// </summary>
    /// <param name="Count">Количество домов</param>
    public IEnumerator GenerateCity(int Count)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        List<Vector3Int> canBePositions = new List<Vector3Int>();
        List<Vector3Int> wasPositions = new List<Vector3Int>();
        Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
        Dictionary<Vector3Int,int> housesInd = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, (List<Vector3Int>,List<Vector3Int>)> Roads = new Dictionary<Vector3Int, (List<Vector3Int>, List<Vector3Int>)>();
        Dictionary<Vector3Int, HashSet<int>> RoadsAndHousesFromThem = new Dictionary<Vector3Int, HashSet<int>>();
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
            int cnt = 0, toInd = housesInd[to];
            while (PositionIND0.Count != 0&&!nowind||PositionIND1.Count!=0&&nowind)
            {
                cnt++;
                if (cnt == 100) break;
                Dictionary<Vector3Int, int> nowPosition = nowind?PositionIND1:PositionIND0, newPosition = nowind?PositionIND0:PositionIND1;
                
                foreach (Vector3Int a in nowPosition.Keys)
                {
                    Vector3Int aPos = a;
                    int NewRoads = nowPosition[a];
                    if (NewRoads > minWay)
                    {
                        continue;
                    }
                    if (aPos == to)
                    {
                        ok = true;
                        if (minWay>NewRoads)
                        {
                            minWay = NewRoads;
                            last = aPos;
                        }
                        continue;
                    }
                    if (RoadsAndHousesFromThem.ContainsKey(aPos) && RoadsAndHousesFromThem[aPos].Contains(toInd))
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
                    if (Roads.ContainsKey(aPos))
                    {
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(aPos.x + i, aPos.y, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > NewRoads + roads)
                            {
                                if (!Roads.ContainsKey(temp))
                                {
                                    if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads + 1);
                                    else newPosition[temp] = NewRoads + 1;
                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (aPos, NewRoads + 1);
                                    }
                                    else USED.Add(temp, (aPos, NewRoads));
                                }
                                else if (!Roads[temp].Item1.Contains(aPos))
                                {
                                    if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads);
                                    else newPosition[temp] = NewRoads;
                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (aPos, NewRoads);
                                    }
                                    else USED.Add(temp, (aPos, NewRoads));
                                }
                            }

                        }
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(aPos.x, aPos.y + i, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > NewRoads + roads)
                            {
                                if (!Roads.ContainsKey(temp))
                                {
                                    if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads + 1);
                                    else newPosition[temp] = NewRoads + 1;
                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (aPos, NewRoads + 1);
                                    }
                                    else USED.Add(temp, (aPos, NewRoads));
                                }
                                else if (!Roads[temp].Item1.Contains(aPos))
                                {
                                    if (!newPosition.ContainsKey(temp)) newPosition.Add(temp, NewRoads);
                                    else newPosition[temp] = NewRoads;
                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (aPos, NewRoads);
                                    }
                                    else USED.Add(temp, (aPos, NewRoads));
                                }
                            }
                        }

                    }
                    else
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
                                if (!newPosition.ContainsKey(temp))newPosition.Add(temp, NewRoads + roads);
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
                List<Vector3Int> ans = new List<Vector3Int>();
                Vector3Int tosave = to;
                to = last;
                Vector3Int nowpos = USED[last].Item1;
                List<HashSet<int>> housesinway = new List<HashSet<int>>();
                while (nowpos != from)
                {
                    if (to != tosave)
                    {
                         if (RoadsAndHousesFromThem.ContainsKey(to))
                         {
                             RoadsAndHousesFromThem[to].Add(toInd);
                             foreach (HashSet<int> road in housesinway)
                                 foreach (int a in RoadsAndHousesFromThem[to])
                                 {
                                     road.Add(a);
                                 }
                         }
                         else RoadsAndHousesFromThem.Add(to, new HashSet<int>() { toInd });
                         housesinway.Add(RoadsAndHousesFromThem[to]);
                         if (Roads.ContainsKey(nowpos))
                         {
                             Roads[nowpos].Item1.Add(to);
                         }
                         else
                         {
                             Roads.Add(nowpos, (new List<Vector3Int>() { to }, new List<Vector3Int>()));
                         }
                         if (Roads.ContainsKey(to))
                         {
                             Roads[to].Item2.Add(nowpos);
                         }
                         else
                         {
                             Roads.Add(to, (new List<Vector3Int>(), new List<Vector3Int>() { nowpos }));
                         }
                    }
                    to = nowpos;
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
        int cntHousePeople = 1, cntHouseCom = 0, cntHouseFact = 0;
        Houses.Add(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
        housesInd.Add(new Vector3Int(0, 0, 0), 0);
        grid.CreateNewTile(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
        GetBonusPositions(new Vector3Int(0, 0, 0));
        int nowind = 0;
        while (Houses.Count != Count)
        {
            nowind++;
            if (canBePositions.Count != 0)
            {
                Vector3Int Position = canBePositions[UnityEngine.Random.Range(0, canBePositions.Count)];
                ThingsInCell whatadd;
                canBePositions.Remove(Position);
                wasPositions.Add(Position);
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
                bool ok = true;
                housesInd.Add(Position, nowind);
                foreach (Vector3Int a in Houses.Keys)
                {
                    if (a != Position)
                    {
                        bool d = CreateRoadsBetweenHouses(a, Position);
                        if (!d)
                        {
                            ok = false;
                            break;
                        }
                        d = CreateRoadsBetweenHouses(Position, a);
                        if (!d)
                        {
                            ok = false;
                            break;
                        }
                    }
                }
                if (ok)
                {
                    Houses.Add(Position, whatadd);
                    GetBonusPositions(Position);
                    for (int i = 0; i < canBePositions.Count; i++)
                    {
                        if (Roads.ContainsKey(canBePositions[i]))
                        {
                            canBePositions.RemoveAt(i);
                            i--;
                        }
                    }
                    grid.CreateNewTile(Position, whatadd);
                    yield return new WaitForEndOfFrame();
                }
            }
            else break;
        }

        timer.Stop();
        UnityEngine.Debug.Log(timer.ElapsedMilliseconds);
        Stopwatch timeVisible = new Stopwatch();
        timeVisible.Start();
       /* foreach (Vector3Int a in Houses.Keys)
        {
            grid.CreateNewTile(a, Houses[a]);
        }*/
        foreach (Vector3Int a in Roads.Keys)
        {
            grid.CreateNewTile(a, ThingsInCell.RoadForCars);
        }
        foreach (Vector3Int a in Roads.Keys)
        {
            List<Vector3Int> RoadsFromCell = Roads[a].Item1;
            List<Vector3Int> RoadsToCell = Roads[a].Item2;
            grid.UniteTiles(a, RoadsFromCell, ThingsInCell.RoadForCars, true);
            grid.UniteTiles(a, RoadsToCell, ThingsInCell.RoadForCars, false);
        }
        timeVisible.Stop();
        UnityEngine.Debug.Log("TIME DRAW: " + Convert.ToString(timeVisible.ElapsedMilliseconds));
    }
}

