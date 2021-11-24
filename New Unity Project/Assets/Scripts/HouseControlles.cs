using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
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
    Dictionary<CellWithHouse, List<HumanFunctionality>> HumansInHouses = new Dictionary<CellWithHouse, List<HumanFunctionality>>();
    private void Start()
    {
        StartCoroutine("SpawnHuman");
    }
    public void AddHouse(Vector3Int Position, ThingsInCell type, CellWithHouse house)
    {
        //if (!HousesTypes.ContainsKey(type)) HousesTypes.Add(type, new List<Vector3Int>());
        //if (!HousesTypes[type].Contains(Position)) {
          //  HousesTypes[type].Add(Position);
        //}
        void AddHuman()
        {
            HumanFunctionality human = Instantiate(HumanPrefab, transform).GetComponent<HumanFunctionality>();
            human.houseControlles = this;
            human.grid = Grid;
            human.transform.localPosition = new Vector3(house.GetCellPosition().x+0.5f, house.GetCellPosition().y+0.5f, 0);
            HumansInHouses[house].Add(human);
        }
        CellsWithHouses.Add(house);
        HumansInHouses.Add(house, new List<HumanFunctionality>());
        if (type == ThingsInCell.HousePeople)
        {

            AddHuman();
        }
        
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
                        Stopwatch wayfind = new Stopwatch();
                        wayfind.Start();
                        List<Vector3Int> way = new List<Vector3Int>() ;
                        bool threadwork = true;
                        Thread _thread = new Thread(
                            ()=>
                            {
                                way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                                threadwork = false;
                            });
                        _thread.Start();
                        while (threadwork)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        wayfind.Stop();
                        if (way != null)
                        {
                            way.Insert(0, HouseFrom.GetCellPosition());
                            way.Add(HouseTo.GetCellPosition());
                            //Create Human
                            List<HumanFunctionality> NowHouses = HumansInHouses[HouseFrom];
                            HumanFunctionality human = NowHouses[0];
                            NowHouses.RemoveAt(0);
                            clock.totalHumans++;
                            clock.totalWays += way.Count;
                            if (NowHouses.Count == 0) CellsWithHumans.Remove(HouseFrom);
                            human.StartGo(way, HouseTo);
                            humanController.AddHuman(human);
                        }
                    }
                }
            }
             yield return new WaitForEndOfFrame();
        }
    }
    public void AddCellWithHumans(CellWithHouse temp) => CellsWithHumans.Add(temp);
    public void RemoveHouse(CellWithHouse what)
    {
        CellsWithHouses.Remove(what);
    }
    public void AddHumanToHouse(HumanFunctionality human, CellWithHouse where)
    {
        HumansInHouses[where].Add(human);
        if (!CellsWithHumans.Contains(where)) CellsWithHumans.Add(where);
    }
}
