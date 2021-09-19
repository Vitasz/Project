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
    private List<CellWithHouse> CellsWithHumans = new List<CellWithHouse>();
    private void Start()
    {
        StartCoroutine("SpawnHuman");
    }
    public void AddHouse(Vector3Int Position, ThingsInCell type)
    {
        if (!HousesTypes.ContainsKey(type)) HousesTypes.Add(type, new List<Vector3Int>());
        if (!HousesTypes[type].Contains(Position)) {
            HousesTypes[type].Add(Position);
        }
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
            CellWithHouse HouseFrom = GetRandomHouseWithHuman();
            if (HouseFrom != null)
            {
                CellWithHouse HouseTo = GetRandomHouseCell(HouseFrom.typeHouse);
                if (HouseTo != null)
                {
                    List<Vector3Int> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                    if (way != null)
                    {
                        //Create Human
                        HouseFrom.HumanInCellHouse--;
                        if (HouseFrom.HumanInCellHouse == 0) CellsWithHumans.Remove(HouseFrom);
                        HumanFunctionality human = Instantiate(HumanPrefab, transform).GetComponent<HumanFunctionality>();
                        human.StartGo(way, HouseTo, Grid, HouseFrom);
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public void AddCellWithHumans(CellWithHouse temp) => CellsWithHumans.Add(temp);
}
