using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class HouseControlles : MonoBehaviour
{
    public GridFunc Grid;
    public GameObject HousePrefab, HumanPrefab;
    public RoadsControlles RoadsController;
    public Dictionary<ThingsInCell, List<Vector3Int>> HousesTypes = new Dictionary<ThingsInCell, List<Vector3Int>>();
    public Clock clock;
    private List<CellWithHouse> CellsWithHumans = new List<CellWithHouse>();
    private List<CellWithHouse> CellsWithHouses = new List<CellWithHouse>();
    public bool CanSpawn = true;
    public HumanController humanController;
    public bool CoroutineWork = false;
    private void Start()
    {
       // StartCoroutine("SpawnHuman");
    }
    public void AddHouse(Vector3Int Position, ThingsInCell type, CellWithHouse house)
    {
        if (!HousesTypes.ContainsKey(type)) HousesTypes.Add(type, new List<Vector3Int>());
        if (!HousesTypes[type].Contains(Position)) {
            HousesTypes[type].Add(Position);
        }
        CellsWithHouses.Add(house);
    }
    public CellWithHouse GetRandomHouseCell(ThingsInCell NotUse)
    {
        List<Vector3Int> temp = new List<Vector3Int>();
        foreach(ThingsInCell a in HousesTypes.Keys)
        {
            if (a != NotUse)
            {
                foreach (Vector3Int b in HousesTypes[a]) temp.Add(b);
            }
        }
        int length = temp.Count;
        if (length == 0) return null;
        return Grid.GetCell(temp[Mathf.CeilToInt(UnityEngine.Random.Range(0, length))]) as CellWithHouse;
    }
    public CellWithHouse GetRandomHouseWithHuman()
    {
        int length = CellsWithHumans.Count;
        if (length == 0) return null;
        return Grid.GetCell(CellsWithHumans[Mathf.CeilToInt(UnityEngine.Random.Range(0, length))].GetCellPosition()) as CellWithHouse;
    }
    public IEnumerator SpawnHuman()
    {
        while (true)
        {
            if (CanSpawn && CellsWithHouses.Count>1)
            {
                CellWithHouse HouseFrom = CellsWithHouses[UnityEngine.Random.Range(0, CellsWithHouses.Count)];
                if (HouseFrom != null)
                {
                    List<CellWithHouse> houseswithout = new List<CellWithHouse>();
                    foreach (CellWithHouse a in CellsWithHouses) if (HouseFrom != a) houseswithout.Add(a);
                    
                    CellWithHouse HouseTo = houseswithout[UnityEngine.Random.Range(0, houseswithout.Count)];
                    if (HouseTo != null)
                    {
                        List<Vector3Int> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                        if (way != null)
                        {
                            //Create Human
                            HouseFrom.HumanInCellHouse--;
                            clock.totalHumans++;
                            clock.totalWays += way.Count;
                            if (HouseFrom.HumanInCellHouse == 0) CellsWithHumans.Remove(HouseFrom);
                            HumanFunctionality human = Instantiate(HumanPrefab, transform).GetComponent<HumanFunctionality>();
                            
                            human.StartGo(way, HouseTo, Grid, HouseFrom);
                            humanController.AddHuman(human);
                        }
                    }
                }
            }
             yield return new WaitForFixedUpdate();
        }
    }
    public void SpawnHumanNotInf()
    {
        int maxHumans = 100;
        clock.totalHumans = 0;
        clock.totalTimes = 0;
        while (maxHumans != 0)
        {
            if (CanSpawn && CellsWithHouses.Count > 1)
            {
                CellWithHouse HouseFrom = CellsWithHouses[UnityEngine.Random.Range(0, CellsWithHouses.Count)];
                if (HouseFrom != null)
                {
                    List<CellWithHouse> houseswithout = new List<CellWithHouse>();
                    foreach (CellWithHouse a in CellsWithHouses) if (HouseFrom != a) houseswithout.Add(a);

                    CellWithHouse HouseTo = houseswithout[UnityEngine.Random.Range(0, houseswithout.Count)];
                    if (HouseTo != null)
                    {
                        List<Vector3Int> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                        if (way != null)
                        {
                            //Create Human
                            maxHumans--;
                            HouseFrom.HumanInCellHouse--;
                            clock.totalHumans++;
                            clock.totalWays += way.Count;
                            if (HouseFrom.HumanInCellHouse == 0) CellsWithHumans.Remove(HouseFrom);
                            HFforOA human = new HFforOA();
                            human.StartGo(way, HouseTo, Grid, HouseFrom);
                            humanController.AddHumanForOA(human);
                        }
                    }
                }
            }
        }
        humanController.GoNotInf();
    }
    public void AddCellWithHumans(CellWithHouse temp) => CellsWithHumans.Add(temp);
    public void RemoveHouse(Vector3Int position)
    {
        CellsWithHouses.Remove(Grid.GetCell(position) as CellWithHouse);
    }
}
