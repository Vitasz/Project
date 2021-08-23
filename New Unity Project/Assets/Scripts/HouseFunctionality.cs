using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseFunctionality : MonoBehaviour
{
    public int Type; // 1 - House, 2 - com, 3 - fac;
    public GameObject HumanPrefab;
    private int totalHumans, maxHumans;
    private Dictionary<(int, int), GameObject> Tiles = new Dictionary<(int, int), GameObject>();
    private HouseControlles HouseController;
    private RoadsControlles RoadController;
    public void CreateHouse(int type, Dictionary<(int,int), GameObject> TilesForHouse, HouseControlles housecontroller, RoadsControlles RoadController)
    {
        HouseController = housecontroller;
        this.RoadController = RoadController;
        foreach ((int, int) a in TilesForHouse.Keys) Tiles.Add(a, TilesForHouse[a]);
        Type = type;
        if (type == 1)
        {
            maxHumans = TilesForHouse.Count * 5;
            totalHumans = maxHumans;
            foreach (GameObject a in TilesForHouse.Values) a.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if (type == 2) foreach (GameObject a in TilesForHouse.Values) a.GetComponent<SpriteRenderer>().color = Color.blue;
        else if (type == 3) foreach (GameObject a in TilesForHouse.Values) a.GetComponent<SpriteRenderer>().color = Color.yellow;
        StartCoroutine("SpawnHumans");
    }
    private IEnumerator SpawnHumans()
    {
        while (true)
        {
            if (totalHumans > 0)
            {
                CreateHuman();
                totalHumans--;
            }
            yield return new WaitForSeconds(1);
        }
    }
    public void GetHuman() => totalHumans += 1;
    private void CreateHuman()
    {
        int GoIndex = 1;
        if (Type == 1) GoIndex = UnityEngine.Random.Range(2, 4);
        if (HouseController.HousesTypes.ContainsKey(GoIndex))
        {
            int IndexHouse = UnityEngine.Random.Range(0, HouseController.HousesTypes[GoIndex].Count);
            GameObject End = HouseController.HousesTypes[GoIndex][IndexHouse];
            SortedSet<(int, int)> from = new SortedSet<(int, int)>(), to = new SortedSet<(int, int)>();
            foreach ((int, int) a in Tiles.Keys) from.Add(a);
            foreach ((int, int) a in End.GetComponent<HouseFunctionality>().Tiles.Keys) to.Add(a);
            List<(int, int)> way = RoadController.FindWay(from, to);
            if (way != null)
            {
                GameObject NewHuman = Instantiate(HumanPrefab);
                HumanFunctionality Humanf = NewHuman.GetComponent<HumanFunctionality>();
                Humanf.StartGo(way, End, HouseController.Grid);
                return;
            }
        }
        totalHumans++;
    }
}
