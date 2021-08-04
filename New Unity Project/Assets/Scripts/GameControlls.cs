using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameControlls : MonoBehaviour
{
    public void ClickOnGrid(int X, int Y)
    {
        Debug.Log("Coordinates: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
    }
}
