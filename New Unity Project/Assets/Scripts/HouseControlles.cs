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
    
    public void AddHouse(Vector3Int Position, ThingsInCell type)
    {
        if (!HousesTypes.ContainsKey(type)) HousesTypes.Add(type, new List<Vector3Int>());
        if (!HousesTypes[type].Contains(Position)) {
            HousesTypes[type].Add(Position);
        }
    }
    public Cell GetRandomHouseCell(ThingsInCell type)
    {
        if (!HousesTypes.ContainsKey(type)) return null;
        int LengthSet = HousesTypes[type].Count;
        Vector3Int RandomCellPosition = HousesTypes[type][Mathf.CeilToInt(UnityEngine.Random.Range(0, LengthSet))];
        return Grid.GetCell(RandomCellPosition);
    }
}
