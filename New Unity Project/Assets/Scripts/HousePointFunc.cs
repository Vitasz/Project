using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHousePointFunc : MonoBehaviour
{
    public CreateHomeButtonFunc NewHomeButton;
    public Vector3 PositionPoint;
    public void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
        NewHomeButton.ClickOnPoint(PositionPoint);
    }
}
