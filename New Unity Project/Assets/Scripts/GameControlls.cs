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
