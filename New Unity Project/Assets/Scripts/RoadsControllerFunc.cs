using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoadsControllerFunc : MonoBehaviour
{
    public GameObject RoadPrefab;
    private readonly List<RoadPointFunc> RoadPoints = new List<RoadPointFunc>();
    private readonly List<(Vector2, Vector2)> Roads = new List<(Vector2, Vector2)>();
    public void CreateRoad(List<RoadPointFunc> Points)
    {
        (Vector2, Vector2) NewRoadsPoint = (Points[0].transform.position, Points[1].transform.position);
        if (!Roads.Contains(NewRoadsPoint) && !Roads.Contains((NewRoadsPoint.Item2, NewRoadsPoint.Item1))){
            GameObject NewRoad = Instantiate(RoadPrefab, transform);
            NewRoad.GetComponent<LineRenderer>().SetPositions(
                Points.Select<RoadPointFunc, Vector3>(x => x.transform.localPosition).ToArray());
            Roads.Add(NewRoadsPoint);
        }
    }
    public void AddRoadPoint(RoadPointFunc Point) => RoadPoints.Add(Point);
    public void SetOnRoadsPoint()
    {
        foreach (RoadPointFunc a in RoadPoints) a.gameObject.SetActive(true);
    }
    public void SetOffRoadsPoint()
    {
        foreach (RoadPointFunc a in RoadPoints) a.gameObject.SetActive(false);
    }

}
