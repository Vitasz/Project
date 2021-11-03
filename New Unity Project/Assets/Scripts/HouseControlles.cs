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
   // public Dictionary<ThingsInCell, List<Vector3Int>> HousesTypes = new Dictionary<ThingsInCell, List<Vector3Int>>();
    public Clock clock;
    private List<CellWithHouse> CellsWithHumans = new List<CellWithHouse>();
    private List<CellWithHouse> CellsWithHouses = new List<CellWithHouse>();
    public HumanController humanController;
    private void Start()
    {
        StartCoroutine("SpawnHuman");
    }
    public void AddHouse(Vector3Int Position, ThingsInCell type, CellWithHouse house, bool ForOA)
    {
        //if (!HousesTypes.ContainsKey(type)) HousesTypes.Add(type, new List<Vector3Int>());
        //if (!HousesTypes[type].Contains(Position)) {
          //  HousesTypes[type].Add(Position);
        //}
        CellsWithHouses.Add(house);
    }
    public IEnumerator SpawnHuman()
    {
        while (true)
        {
           // Debug.Log(CellsWithHumans.Count);
            if (CellsWithHumans.Count>=1)
            {
                CellWithHouse HouseFrom = CellsWithHumans[UnityEngine.Random.Range(0, CellsWithHumans.Count)];
                if (HouseFrom != null)
                {
                    List<CellWithHouse> houseswithout = new List<CellWithHouse>();
                    foreach (CellWithHouse a in CellsWithHouses) if (HouseFrom != a) houseswithout.Add(a);
                    if (houseswithout.Count == 0)
                    {
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                    CellWithHouse HouseTo = houseswithout[UnityEngine.Random.Range(0, houseswithout.Count)];
                    if (HouseTo != null)
                    {
                        List<Vector3Int> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                        
                        if (way != null)
                        {
                            way.Insert(0, HouseFrom.GetCellPosition());
                            way.Add(HouseTo.GetCellPosition());
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
    public float SpawnHumanNotInf()
    {
        List<List<Vector3Int>> WaysTo = new List<List<Vector3Int>>();
        
        for (int i = 0; i < CellsWithHouses.Count; i++)
        {
            List<List<Vector3Int>> temp = new List<List<Vector3Int>>();
            for (int j = 0; j < CellsWithHouses.Count; j++)
            {
                if (i != j)
                {
                    List<Vector3Int> NowWay = Grid.FindWay(CellsWithHouses[i].GetNearTiles(), CellsWithHouses[j].GetNearTiles());
                    if (NowWay == null)
                    {
                        return -1f;
                    }
                    else
                    {
                        if (temp.Count<5)
                        temp.Add(NowWay);
                        else
                        {
                            int nowpos = 4;
                            while (nowpos!=-1&&temp[nowpos].Count < NowWay.Count) nowpos--;
                            nowpos++;
                            if (nowpos != 5)
                            {
                                temp.RemoveAt(temp.Count - 1);
                                temp.Insert(nowpos, NowWay);
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < temp.Count; k++)
            {
                WaysTo.Add(temp[k]);
            }
        }
        return  humanController.GetEfficiencySystem(WaysTo);
    }
    public void AddCellWithHumans(CellWithHouse temp) => CellsWithHumans.Add(temp);
    public void RemoveHouse(CellWithHouse what)
    {
        CellsWithHouses.Remove(what);
    }
}
