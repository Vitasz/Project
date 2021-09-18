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
    public CellWithHouse GetRandomHouseCell(ThingsInCell type)
    {
        if (!HousesTypes.ContainsKey(type)) return null;
        int LengthSet = HousesTypes[type].Count;
        Vector3Int RandomCellPosition = HousesTypes[type][Mathf.CeilToInt(UnityEngine.Random.Range(0, LengthSet))];
        return Grid.GetCell(RandomCellPosition) as CellWithHouse;
    }
    public IEnumerator SpawnHuman()
    {
        while (true)
        {
            CellWithHouse HouseFrom = GetRandomHouseCell(ThingsInCell.HousePeople);

            if (HouseFrom != null)
            {
                if (HouseFrom.HumanInCellHouse != 0)
                {
                    Debug.Log("House from " + HouseFrom.GetCellPosition());
                    CellWithHouse HouseTo = GetRandomHouseCell(ThingsInCell.HouseCom);
                    if (HouseTo != null)
                    {
                        Debug.Log("House to " + HouseTo.GetCellPosition());
                        Vector3Int positionTo = HouseTo.GetCellPosition();
                        List<Vector3Int> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                        Debug.Log(way);
                        if (way != null)
                        {
                            //Create Human
                        }
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
