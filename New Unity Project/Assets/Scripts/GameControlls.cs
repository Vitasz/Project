using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameControlls : MonoBehaviour
{
    public int Mode = 1;//1 - Create House, 2 - Create Road;
    public HouseControlles HouseController;
    public RoadsControlles RoadsController;
    public Button HouseButton, RoadButton;
    List<(int, int)> ClickedPositions = new List<(int, int)>();
    void Start()
    {
        void HouseButtonClick() => Mode = 1;
        void RoadButtonClick() => Mode = 2;
        HouseButton.onClick.AddListener(HouseButtonClick);
        RoadButton.onClick.AddListener(RoadButtonClick);
        StartCoroutine("testwayfinder");
    }
    public IEnumerator testwayfinder()
    {
        while (true)
        {
            List<(int, int)> way = RoadsController.FindWay((0, 0),new SortedSet<(int, int)>() { (3, 3), (3, 4), (4, 4), (4, 3) });
            if (way != null)
            {
                Debug.Log("Way: " + Convert.ToString(way.Count));
                //foreach ((int, int) a in way) Debug.Log(a);
            }
            else Debug.Log("NO WAY FOUND");
            yield return new WaitForSeconds(1);
        }
    }
    public void ClickOnGrid(int X, int Y)
    {
        //Debug.Log(" Start Coordinates: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
        if (!ClickedPositions.Contains((X, Y))) ClickedPositions.Add((X, Y));
    }
    public void StopClickOnGrid(int X, int Y)
    {
        if (X!=-1&&Y!=-1 && !ClickedPositions.Contains((X, Y))) ClickedPositions.Add((X, Y));
       // Debug.Log("Stop Coordinates: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
        if (Mode == 1) HouseController.CreateHouse(ClickedPositions);
        else RoadsController.AddRoad(ClickedPositions);
        ClickedPositions.Clear();
    }

}
