using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameControlls : MonoBehaviour
{
    public int Mode = 1;//1 - Create House, 2 - Create Road;
    public HouseControlles HouseController;
    private (int, int) ClickPosition;
    public void ClickOnGrid(int X, int Y)
    {
        Debug.Log(" Start Coordinates: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
        ClickPosition = (X, Y);
    }
    public void StopClickOnGrid(int X, int Y)
    {
        Debug.Log("Stop Coordinates: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
        if (Mode == 1)
        {
            HouseController.CreateHouse(ClickPosition, (X, Y));
        }
    }

}
