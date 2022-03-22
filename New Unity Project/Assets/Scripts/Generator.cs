using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
public class Generator : MonoBehaviour
{
    public GridFunc grid;

    public int r, N;
    /// <summary>
    /// Генерирует город с Count домами
    /// </summary>
    /// <param name="Count">Количество домов</param>
    public IEnumerator GenerateCity(int Count, Dictionary<(int, int), Cell> Map)
    {
        System.Random random = new System.Random();
        Stopwatch timer = new Stopwatch();
        timer.Start();
        HashSet<(int, int)> canBePositions = new HashSet<(int, int)>();
        HashSet<(int, int)> wasPositions = new HashSet<(int, int)>();
        Dictionary<(int,int), ThingsInCell> Houses = new Dictionary<(int, int), ThingsInCell>();
        Dictionary<(int, int), List<(int, int)>> Roads = new Dictionary<(int, int), List<(int, int)>>();
        List<(int, int)> newRoads = new List<(int, int)>();
        int cntHousePeople = 0, cntHouseCom = 0, cntHouseFact = 0;
        
        
        bool CreateRoadsBetweenHouses((int, int) from)
        {
            List<(int, int)> PositionIND0 = new List<(int, int)>();
            List<(int, int)> PositionIND1 = new List<(int, int)>();
            Dictionary<(int, int), (int, int)> USED = new Dictionary<(int, int), (int, int)>();

            bool ok = false;
            USED.Add(from, (0,0));
            PositionIND0.Add(from);
            (int, int) last = (0, 0);
            bool nowind = false;
            while (PositionIND0.Count != 0&&!nowind||PositionIND1.Count!=0&&nowind)
            {
                if (ok) break;
                List<(int, int)> nowPosition = nowind?PositionIND1:PositionIND0, newPosition = nowind?PositionIND0:PositionIND1;
                newPosition.Clear();
                foreach ((int, int) a in nowPosition)
                {
                    if (Roads.ContainsKey(a))
                    {
                        ok = true;
                        last = a;
                        newPosition.Clear();
                        break;
                    }
                    for (int i = -1; i < 2; i += 2)
                    {
                        (int, int) temp = (a.Item1 + i, a.Item2);
                        if (!USED.ContainsKey(temp) && !Houses.ContainsKey(temp))
                        {
                            newPosition.Add(temp);
                            USED.Add(temp, a);
                        }
                    }
                    for (int i = -1; i < 2; i += 2)
                    {
                        (int, int) temp = (a.Item1, a.Item2 + i);
                        if (!USED.ContainsKey(temp) && !Houses.ContainsKey(temp))
                        {
                            newPosition.Add(temp);
                            USED.Add(temp, a);
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
                (int, int) to;
                to = last;
                (int, int) nowpos = USED[last];
                while (nowpos != from)
                {
                    
                    if (Roads.ContainsKey(nowpos))
                    {
                        Roads[nowpos].Add(to);
                    }
                    else
                    {
                        Roads.Add(nowpos, new List<(int,int)>());
                        Roads[nowpos].Add(to);
                        if (!newRoads.Contains(nowpos))newRoads.Add(nowpos);
                    }
                    if (Roads.ContainsKey(to))
                    {
                        Roads[to].Add(nowpos);
                    }
                    else
                    {
                        Roads.Add(to, new List<(int, int)>() { nowpos });
                        if (!newRoads.Contains(to)) newRoads.Add(to);
                    }
                    
                    to = nowpos;
                    nowpos = USED[to];
                }

                return true;
            }
        }
        void GetBonusPositions((int, int) position)
        {
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    (int, int) NowPosition = (position.Item1 + i, position.Item2 + j);
                    if (!wasPositions.Contains(NowPosition))
                    {
                        if (!canBePositions.Contains(NowPosition))canBePositions.Add(NowPosition);
                    }
                }
            }
        }
        void GetBonusPositionsForRoad((int, int) position)
        {
            for (int i = -1; i <= 1; i+=2)
            {
                for (int j = -1; j <= 1; j+=2)
                {
                    (int,int) NowPosition = (position.Item1 + i, position.Item2 + j);
                    if (!wasPositions.Contains(NowPosition))
                    {
                        if (!canBePositions.Contains(NowPosition)) canBePositions.Add(NowPosition);
                    }
                }
            }
        }
        
        
        foreach ((int, int) position in Map.Keys)
        {
            wasPositions.Add(position);
            canBePositions.Remove(position);
            if (Map[position] is CellWithHouse)
            {
                GetBonusPositions(position);
                if ((Map[position] as CellWithHouse).GetTypeCell() == ThingsInCell.HousePeople) cntHousePeople++;
                if ((Map[position] as CellWithHouse).GetTypeCell() == ThingsInCell.HouseCom) cntHouseCom++;
                if ((Map[position] as CellWithHouse).GetTypeCell() == ThingsInCell.HouseFact) cntHouseFact++;
                Houses.Add(position, (Map[position] as CellWithHouse).GetTypeCell());
            }
        }
        if (Houses.Keys.Count != 0)
        {
            (int, int) firstroad = canBePositions.First();
            wasPositions.Add(firstroad);
            canBePositions.Remove(firstroad);
            Roads.Add(firstroad, new List<(int, int)>());
            GetBonusPositionsForRoad(firstroad);
            foreach ((int, int) position in Houses.Keys)
            {
                bool ok = CreateRoadsBetweenHouses(position);
                if (ok)
                {
                    foreach ((int, int) a in Roads.Keys)
                    {
                        if (!grid.Roads.ContainsKey(a))
                        {
                            grid.CreateNewTile(a, ThingsInCell.RoadForCars);
                            grid.UniteTiles(a, Roads[a]);
                            foreach ((int, int) b in Roads[a])
                            {
                                grid.UniteTiles(b, new List<(int, int)>() { a });
                            }
                        }

                    }
                    yield return new WaitForEndOfFrame();
                    GetBonusPositions(position);
                    foreach ((int, int) c in newRoads)
                    {
                        GetBonusPositionsForRoad(c);
                        canBePositions.Remove(c);
                        wasPositions.Add(c);
                    }
                }
            }
        }

        else if (canBePositions.Count == 0)
        {
            Houses.Add((0, 0), ThingsInCell.HousePeople);
            wasPositions.Add((0, 0));
            GetBonusPositions((0, 0));
            (int, int) firstroad = canBePositions.First();
            wasPositions.Add(firstroad);
            canBePositions.Remove(firstroad);
            Roads.Add(firstroad, new List<(int, int)>());
            GetBonusPositionsForRoad(firstroad);
            cntHousePeople++;
        }

        foreach ((int, int) position in Map.Keys)
        {
            wasPositions.Add(position);
            canBePositions.Remove(position);
            if (!Roads.ContainsKey(position) && Map[position] is CellWithRoad)
            {
                Roads.Add(position, (Map[position] as CellWithRoad).GetNearRoadsWays());
            }
            else if (Roads.ContainsKey(position) && Map[position] is CellWithRoad)
            {
                foreach((int,int) b in (Map[position] as CellWithRoad).GetNearRoadsWays())
                {
                    if (!Roads[position].Contains(b)) Roads[position].Add(b);
                }
            }
        }



        long totalct = 0;
        while (Count!=0)
        {
            if (canBePositions.Count != 0)
            {
                ThingsInCell whatadd;
                if (cntHousePeople < (cntHouseCom + cntHouseFact))
                {
                    whatadd = ThingsInCell.HousePeople;
                }
                else if (cntHouseCom <= cntHouseFact && cntHousePeople >= cntHouseCom)
                {
                    whatadd = ThingsInCell.HouseCom;
                }
                else
                {
                    whatadd = ThingsInCell.HouseFact;
                }
                
                (int, int) Position = canBePositions.ToArray()[random.Next(0, canBePositions.Count)];
                
                canBePositions.Remove(Position);
                wasPositions.Add(Position);
                Stopwatch createtime = new Stopwatch();
                createtime.Start();
                bool ok = CreateRoadsBetweenHouses(Position);
                createtime.Stop();
                totalct += createtime.ElapsedMilliseconds;
                if (ok)
                {
                    Count--;
                    if (whatadd == ThingsInCell.HouseCom)
                    {
                        cntHouseCom++;
                    }
                    else if (whatadd == ThingsInCell.HouseFact)
                    {
                        cntHouseFact++;
                    }
                    else
                    {
                        cntHousePeople++;
                    }
                    Houses.Add(Position, whatadd);
                    grid.CreateNewTile(Position, whatadd);
                    foreach((int,int) a in Roads.Keys)
                    {
                        if (!grid.Roads.ContainsKey(a))
                        {
                            grid.CreateNewTile(a, ThingsInCell.RoadForCars);
                            grid.UniteTiles(a, Roads[a]);
                            foreach ((int, int) b in Roads[a])
                            {
                                grid.UniteTiles(b, new List<(int, int)>() { a });
                            }
                        }
                        
                    }
                    yield return new WaitForEndOfFrame();
                    GetBonusPositions(Position);
                    foreach ((int, int) c in newRoads)
                    {
                        GetBonusPositionsForRoad(c);
                        canBePositions.Remove(c);
                        wasPositions.Add(c);
                    }
                }
                else
                {
                    foreach ((int, int) a in newRoads) Roads.Remove(a);
                }
                newRoads.Clear();
                
            }
            else
            {
                UnityEngine.Debug.LogError("END PLACE");
                break;
            }
            
        }
        UnityEngine.Debug.Log(totalct);
        timer.Stop();
        UnityEngine.Debug.Log(timer.ElapsedMilliseconds);
        grid.generatorActive = false;
        yield return null;
    }
}

