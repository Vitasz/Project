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
    public GameObject HumanPrefab;
    public Clock clock;
    private List<CellWithHouse> CellsWithHumans = new List<CellWithHouse>();
    private List<CellWithHouse> CellsWithHouses = new List<CellWithHouse>();
    public HumanController humanController;
    public bool CanSpawn = true;
    Dictionary<CellWithHouse, List<HumanFunctionality>> HumansInHouses = new Dictionary<CellWithHouse, List<HumanFunctionality>>();
    private void Start()
    {
       StartCoroutine("SpawnHuman");
    }
    public void AddHouse(ThingsInCell type, CellWithHouse house)
    {
        void AddHuman()
        {
            HumanFunctionality human = Instantiate(HumanPrefab, transform).GetComponent<HumanFunctionality>();
            human.houseControlles = this;
            human.grid = Grid;
            human.transform.localPosition = new Vector3(house.GetCellPosition().Item1+0.5f, house.GetCellPosition().Item2+0.5f, 0);
            HumansInHouses[house].Add(human);
            human.transform.gameObject.SetActive(false);
        }
        CellsWithHouses.Add(house);
        HumansInHouses.Add(house, new List<HumanFunctionality>());
        if (type == ThingsInCell.HousePeople)
        {
            for (int i = 0; i < 4; i++)
            {
                AddHuman();
            }
        }
    }
    public IEnumerator SpawnHuman()
    {
        while (true)
        {
            if (CanSpawn)
            {
                if (CellsWithHumans.Count >= 1)
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
                            List<(int, int)> way = Grid.FindWay(HouseFrom.GetNearTiles(), HouseTo.GetNearTiles());
                            if (way != null)
                            {
                                way.Insert(0, HouseFrom.GetCellPosition());
                                way.Add(HouseTo.GetCellPosition());
                                //Create Human
                                List<HumanFunctionality> NowHouses = HumansInHouses[HouseFrom];
                                HumanFunctionality human = NowHouses[0];
                                NowHouses.RemoveAt(0);
                                if (NowHouses.Count == 0) CellsWithHumans.Remove(HouseFrom);
                                human.StartGo(way, HouseTo);
                                humanController.AddHuman(human);
                            }
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
        foreach (HumanFunctionality a in HumansInHouses[what])
        {
            Destroy(a.gameObject);
        }
    }
    public void AddHumanToHouse(HumanFunctionality human, CellWithHouse where)
    {
        HumansInHouses[where].Add(human);
        if (!CellsWithHumans.Contains(where)) CellsWithHumans.Add(where);
    }
}
