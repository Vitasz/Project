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
        GenerateCity(N);
    }

    /// <summary>
    /// ���������� ����� � Count ������
    /// </summary>
    /// <param name="Count">���������� �����</param>
    public void GenerateCity(int Count)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
        Dictionary<Vector3Int, List<Vector3Int>> Roads = new Dictionary<Vector3Int, List<Vector3Int>>();
        List<Vector3Int> HousesPositions = new List<Vector3Int>();
        /*List<Vector3Int>*/void CreateRoadsBetweenHouses(Vector3Int from, Vector3Int to)
        {
            List<(Vector3Int, int)> nowPosition = new List<(Vector3Int, int)>();
            List<(Vector3Int, int)> newPosition = new List<(Vector3Int, int)>();
            Dictionary<Vector3Int, (Vector3Int, int)> USED = new Dictionary<Vector3Int, (Vector3Int, int)>();

            bool ok = false;
            int minWay = int.MaxValue;
            USED.Add(from, (new Vector3Int(), 0));
            nowPosition.Add((from, 0));

            while (nowPosition.Count != 0)
            {
                foreach ((Vector3Int, int) a in nowPosition)
                {
                    if (a.Item2 > minWay)
                    {
                        continue;
                    }
                    if (a.Item1 == to)
                    {
                        ok = true;
                        minWay = Math.Min(minWay, a.Item2);
                        continue;
                    }
                    if (Houses.ContainsKey(a.Item1) && a.Item1 != from)
                    {
                        continue;
                    }
                    if (Roads.ContainsKey(a.Item1))
                    {
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(a.Item1.x + i, a.Item1.y, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > a.Item2 + roads)
                            {
                                if (!Roads.ContainsKey(temp))
                                {
                                    newPosition.Insert(0, (temp, a.Item2 + 1));

                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (a.Item1, a.Item2 + 1);
                                    }
                                    else USED.Add(temp, a);
                                }
                                else if (!Roads[temp].Contains(a.Item1))
                                {
                                    newPosition.Insert(0, (temp, a.Item2));

                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (a.Item1, a.Item2);
                                    }
                                    else USED.Add(temp, a);
                                }
                            }

                        }
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(a.Item1.x, a.Item1.y + i, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > a.Item2 + roads)
                            {
                                if (!Roads.ContainsKey(temp))
                                {
                                    newPosition.Insert(0, (temp, a.Item2 + 1));

                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (a.Item1, a.Item2 + 1);
                                    }
                                    else USED.Add(temp, a);
                                }
                                else if (!Roads[temp].Contains(a.Item1))
                                {
                                    newPosition.Insert(0, (temp, a.Item2));

                                    if (USED.ContainsKey(temp))
                                    {
                                        USED[temp] = (a.Item1, a.Item2);
                                    }
                                    else USED.Add(temp, a);
                                }
                            }
                        }

                    }
                    else
                    {
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector3Int temp = new Vector3Int(a.Item1.x + i, a.Item1.y, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > a.Item2 + roads)
                            {
                                newPosition.Add((temp, a.Item2 + roads));

                                if (USED.ContainsKey(temp))
                                {
                                    USED[temp] = (a.Item1, a.Item2 + roads);
                                }
                                else USED.Add(temp, a);

                            }
                        }
                        for (int i = -1; i < 2; i += 2)
                        {

                            Vector3Int temp = new Vector3Int(a.Item1.x, a.Item1.y + i, 0);
                            int roads = Roads.ContainsKey(temp) ? 0 : 1;

                            if (!USED.ContainsKey(temp) || USED[temp].Item2 > a.Item2 + roads)
                            {
                                newPosition.Add((temp, a.Item2 + roads));

                                if (USED.ContainsKey(temp))
                                {
                                    USED[temp] = (a.Item1, a.Item2 + roads);
                                }
                                else USED.Add(temp, a);

                            }
                        }
                    }
                }
                nowPosition.Clear();
                foreach ((Vector3Int, int) a in newPosition)
                {
                    nowPosition.Add(a);
                }
                newPosition.Clear();
            }
            if (!ok)
            {
                UnityEngine.Debug.LogError("Can't find way");
            }
            else
            {
                List<Vector3Int> ans = new List<Vector3Int>();
                //to = USED[to];
                while (USED[to].Item1 != from)
                {
                    if (Roads.ContainsKey(USED[to].Item1))
                    {
                        if (!Roads[USED[to].Item1].Contains(to))
                        {
                            Roads[USED[to].Item1].Add(to);
                        }
                        to = USED[to].Item1;
                    }
                    else
                    {
                        Roads.Add(USED[to].Item1, new List<Vector3Int>() { to });
                        to = USED[to].Item1;
                    }
                    /*ans.Add(USED[to].Item1);
                    to = USED[to].Item1;*/
                }
                /*ans.RemoveAt(ans.Count - 1);
                return ans;*/
            }
            //return null;
        }

        int cntHousePeople = 1, cntHouseCom = 0, cntHouseFact = 0;

        Houses.Add(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
        HousesPositions.Add(new Vector3Int(0, 0, 0));

        while (cntHousePeople + cntHouseFact + cntHouseCom != Count)
        {
            List<Vector3Int> canBePositions = new List<Vector3Int>();
            Vector3Int randomHouse = HousesPositions[UnityEngine.Random.Range(0, HousesPositions.Count)];
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    if (!Houses.ContainsKey(new Vector3Int(randomHouse.x + i, randomHouse.y + j, 0)) &&
                        !Roads.ContainsKey(new Vector3Int(randomHouse.x + i, randomHouse.y + j, 0)))
                    {
                        canBePositions.Add(new Vector3Int(randomHouse.x + i, randomHouse.y + j, 0));
                    }
                }
            }

            if (canBePositions.Count != 0)
            {
                Vector3Int Position = canBePositions[UnityEngine.Random.Range(0, canBePositions.Count)];
                ThingsInCell whatadd;

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
                HousesPositions.Add(Position);
                //List<List<Vector3Int>> 
                foreach (Vector3Int a in Houses.Keys)
                {
                    if (a != Position)
                    {
                        CreateRoadsBetweenHouses(a, Position);
                        CreateRoadsBetweenHouses(Position, a);
                    }
                }
            }
            else
            {
                HousesPositions.Remove(randomHouse);
                //Debug.Log(Houses.Count);

            }
        }

        timer.Stop();
        UnityEngine.Debug.Log(timer.ElapsedMilliseconds);
        List<Task> tasks = new List<Task>();

        foreach (Vector3Int a in Houses.Keys)
        {
            /*tasks.Add(Task.Run(() =>*/ grid.CreateNewTile(a, Houses[a], new List<Vector3Int>());
        }

        foreach (Vector3Int a in Roads.Keys)
        {
            /*tasks.Add(Task.Run(() =>*/ grid.CreateNewTile(a, ThingsInCell.RoadForCars, Roads[a]);
        }

        //Task.WaitAll(tasks.ToArray());
        //tasks.Clear();

        /*foreach (Vector3Int a in Roads.Keys)
        {
            foreach (Vector3Int b in Roads[a])
            {
                tasks.Add(Task.Run(() => grid.UniteTiles(a, b, ThingsInCell.RoadForCars)));
            }
        }*/

        //ask.WaitAll(tasks.ToArray());
        
    }
}

