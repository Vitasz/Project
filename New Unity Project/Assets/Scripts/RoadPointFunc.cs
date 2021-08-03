using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPointFunc : MonoBehaviour
{
    public CreateRoadButtonFunc RoadButton;
    public int Type;
    private void OnMouseDown()
    {
        RoadButton.OnClickRoadPoint(this);
    }

}
