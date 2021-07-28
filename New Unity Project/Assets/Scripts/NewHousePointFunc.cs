using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHousePointFunc : MonoBehaviour
{
    public CreateHomeButtonFunc NewHomeButton;
    public Vector3 PositionPoint;
    public void OnMouseDown()
    {
        NewHomeButton.ClickOnPoint(PositionPoint);
    }
    public void OnMouseDrag()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0;
        if (NewHomeButton.Mode == 3 && newPosition != PositionPoint)
        {
            NewHomeButton.MovePoint(PositionPoint, newPosition);
        }
    }
}
