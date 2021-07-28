using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHouseLines : MonoBehaviour
{
    public CreateHomeButtonFunc HomeButton;
    private List<Collider2D> CollisionsInLine = new List<Collider2D>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "House" || collision.gameObject.tag == "HouseLine")
        {
            HomeButton.OnLineCroosed(this);
            CollisionsInLine.Add(collision);
            GetComponent<LineRenderer>().startColor = Color.red;
            GetComponent<LineRenderer>().endColor = Color.red;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "House" || collision.gameObject.tag == "HouseLine")
        {
            CollisionsInLine.Remove(collision);
            if (CollisionsInLine.Count == 0)
            {
                HomeButton.OnLineExist(this);
                GetComponent<LineRenderer>().startColor = Color.black;
                GetComponent<LineRenderer>().endColor = Color.black;
            }
        }
    }
    private List<Vector2> CalculateColliderPoints(List<Vector3> positions)
    {
        float width = 0.01f;
        float m = (positions[1].y - positions[0].y) / (positions[1].x - positions[0].x);
        float deltaX = (width / 2f) * (m / Mathf.Pow(m * m + 1, 0.5f));
        float deltaY = (width / 2f) * (1 / Mathf.Pow(m * m + 1, 0.5f));
        Vector3[] offsets = new Vector3[2];
        offsets[0] = new Vector3(-deltaX, deltaY);
        offsets[1] = new Vector3(deltaX, -deltaY);
        return new List<Vector2>
        {
            positions[0]+offsets[0],
            positions[1]+offsets[0],
            positions[1]+offsets[1],
            positions[0]+offsets[1]
        };
    }
    public void ChangePoints(Vector3 a, Vector3 b, float PointsRadius)
    {
        List<Vector3> NewLinePositions = new List<Vector3>
        {
            a + (b-a).normalized * 1.2f * PointsRadius,
            b + (a-b).normalized * 1.2f * PointsRadius
        };
        GetComponent<LineRenderer>().SetPositions(new List<Vector3> { a, b }.ToArray());
        GetComponent<PolygonCollider2D>().SetPath(0, CalculateColliderPoints(NewLinePositions));
    }
}
