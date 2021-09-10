using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseFunctionality : MonoBehaviour
{
    public int Type; // 1 - House, 2 - com, 3 - fac;
    public GameObject HumanPrefab;
    public int totalHumans, maxHumans;
    private Dictionary<(int, int), GameObject> Tiles = new Dictionary<(int, int), GameObject>();
    private HouseControlles HouseController;
    private RoadsControlles RoadController;
    private int HumansPerTile = 200;
    public void CreateHouse(int type)
    {
        
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
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void GetHuman() => totalHumans += 1;
    private void CreateHuman()
    {
        int GoIndex = 1;
        if (Type == 1) GoIndex = UnityEngine.Random.Range(2, HouseController.HousesTypes.Keys.Count + 1);
        if (HouseController.HousesTypes.ContainsKey(GoIndex))
        {
            int IndexHouse = UnityEngine.Random.Range(0, HouseController.HousesTypes[GoIndex].Count);
            GameObject End = HouseController.HousesTypes[GoIndex][IndexHouse];
            SortedSet<(int, int)> from = new SortedSet<(int, int)>(), to = new SortedSet<(int, int)>();
            foreach ((int, int) a in Tiles.Keys) from.Add(a);
            foreach ((int, int) a in End.GetComponent<HouseFunctionality>().Tiles.Keys) to.Add(a);
            //List<(int, int)> way = RoadController.FindWay(from, to);
            /*if (way != null)
            {
                GameObject NewHuman = Instantiate(HumanPrefab);
                NewHuman.transform.localScale = new Vector3((float)HouseController.Grid.SizeCell * 2 / 10, (float)HouseController.Grid.SizeCell * 2 / 10, 0);
                HumanFunctionality Humanf = NewHuman.GetComponent<HumanFunctionality>();
                //Humanf.StartGo(way, End, HouseController.Grid, RoadController);
                return;
            }*/
        }
        totalHumans++;
    }
}
