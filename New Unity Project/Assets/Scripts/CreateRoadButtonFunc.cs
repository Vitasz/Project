using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CreateRoadButtonFunc : MonoBehaviour
{
    public RoadsControllerFunc RoadsController;
    private int Mode = 0;
    private readonly List<RoadPointFunc> ClickedPoints = new List<RoadPointFunc>();
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnClickCreateRoadButton);
    }
    private void OnClickCreateRoadButton()
    {
        if (Mode == 0)
        {
            Mode = 1;
            RoadsController.SetOnRoadsPoint();
            transform.GetChild(0).GetComponent<Text>().text = "End Creating Road";
        }
        else
        {
            Mode = 0;
            RoadsController.SetOffRoadsPoint();
            transform.GetChild(0).GetComponent<Text>().text = "Create Road";
        }
    }
    public void OnClickRoadPoint(RoadPointFunc Point)
    {
        if (Mode==1 && !ClickedPoints.Contains(Point))
        {
            ClickedPoints.Add(Point);
            if (ClickedPoints.Count == 2)
            {
                RoadsController.CreateRoad(ClickedPoints);
                ClickedPoints.Clear();
            }
        }
    }
    
}
