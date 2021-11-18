using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GridFunc grid;
    public void Start()
    {
        GenerateCity(200);
    }
    /// <summary>
    /// Генерирует город с Count домами
    /// </summary>
    /// <param name="Count">Количество домов</param>
    public void GenerateCity(int Count)
    {
        Dictionary<Vector3Int, ThingsInCell> Houses = new Dictionary<Vector3Int, ThingsInCell>();
        Dictionary<Vector3Int, List<Vector3Int>> Roads = new Dictionary<Vector3Int, List<Vector3Int>>();
        List<Vector3Int> HousesPositions = new List<Vector3Int>();
        void CreateRoadsBetweenHouses(Vector3Int from, Vector3Int to)
        {
            List<Vector3Int> nowPosition = new List<Vector3Int>();
            List<Vector3Int> newPosition = new List<Vector3Int>();
            Dictionary<Vector3Int, Vector3Int> USED = new Dictionary<Vector3Int, Vector3Int>();

            bool ok = false;
            USED.Add(from, new Vector3Int());
            nowPosition.Add(from);

            while (nowPosition.Count != 0)
            {
                foreach (Vector3Int a in nowPosition)
                {
                    if (a == to)
                    {
                        ok = true;
                        newPosition.Clear();
                        break;
                    }
                    if (Houses.ContainsKey(a) && a != from)
                    {
                        continue;
                    }
                    if (Roads.ContainsKey(a))
                    {
                        if (!USED.ContainsKey(new Vector3Int(a.x - 1, a.y, 0)))
                        {
                            newPosition.Insert(0, new Vector3Int(a.x - 1, a.y, 0));
                            USED.Add(new Vector3Int(a.x - 1, a.y, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x + 1, a.y, 0)))
                        {
                            newPosition.Insert(0, new Vector3Int(a.x + 1, a.y, 0));
                            USED.Add(new Vector3Int(a.x + 1, a.y, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x, a.y - 1, 0)))
                        {
                            newPosition.Insert(0, new Vector3Int(a.x, a.y - 1, 0));
                            USED.Add(new Vector3Int(a.x, a.y - 1, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x, a.y + 1, 0)))
                        {
                            newPosition.Insert(0, new Vector3Int(a.x, a.y + 1, 0));
                            USED.Add(new Vector3Int(a.x, a.y + 1, 0), a);
                        }
                    }
                    else
                    {
                        if (!USED.ContainsKey(new Vector3Int(a.x - 1, a.y, 0)))
                        {
                            newPosition.Add(new Vector3Int(a.x - 1, a.y, 0));
                            USED.Add(new Vector3Int(a.x - 1, a.y, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x + 1, a.y, 0)))
                        {
                            newPosition.Add(new Vector3Int(a.x + 1, a.y, 0));
                            USED.Add(new Vector3Int(a.x + 1, a.y, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x, a.y - 1, 0)))
                        {
                            newPosition.Add(new Vector3Int(a.x, a.y - 1, 0));
                            USED.Add(new Vector3Int(a.x, a.y - 1, 0), a);
                        }
                        if (!USED.ContainsKey(new Vector3Int(a.x, a.y + 1, 0)))
                        {
                            newPosition.Add(new Vector3Int(a.x, a.y + 1, 0));
                            USED.Add(new Vector3Int(a.x, a.y + 1, 0), a);
                        }
                    }




                }
                nowPosition.Clear();
                foreach (Vector3Int a in newPosition)
                {
                    nowPosition.Add(a);
                }
                newPosition.Clear();
            }
            if (!ok)
            {
                Debug.LogError("Can't find way");
            }
            else
            {
                //to = USED[to];
                while (USED[to] != from)
                {
                    if (Roads.ContainsKey(USED[to]))
                    {
                        if (!Roads[USED[to]].Contains(to))
                        {
                            Roads[USED[to]].Add(to);
                        }
                        to = USED[to];
                    }
                    else
                    {
                        Roads.Add(USED[to], new List<Vector3Int>() { to });
                        to = USED[to];
                    }
                }
            }
        }

        int cntHousePeople = 1, cntHouseCom = 0, cntHouseFact = 0;

        Houses.Add(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
        HousesPositions.Add(new Vector3Int(0, 0, 0));

        while (cntHousePeople + cntHouseFact + cntHouseCom != Count)
        {
            List<Vector3Int> canBePositions = new List<Vector3Int>();
            Vector3Int randomHouse = HousesPositions[UnityEngine.Random.Range(0, HousesPositions.Count)];
            for (int i = -5; i <= 5; i++)
            {
                for (int j = -5; j <= 5; j++)
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
        foreach (Vector3Int a in Houses.Keys)
        {
            grid.CreateNewTile(a, Houses[a], false);
        }
        foreach (Vector3Int a in Roads.Keys)
        {
            grid.CreateNewTile(a, ThingsInCell.RoadForCars, false);
        }
        foreach (Vector3Int a in Roads.Keys)
        {
            foreach (Vector3Int b in Roads[a])
            {
                grid.UniteTiles(a, b, ThingsInCell.RoadForCars, false);
            }
        }
    }
}

