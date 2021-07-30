using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HouseFunc : MonoBehaviour
{
    public Material MaterialForHouseLines;
    public void CreateHouse(params Vector2[] points)
    {
        GetComponent<PolygonCollider2D>().SetPath(0, points.ToArray());
        var result1 = Triangulation.GetResult(points.ToList<Vector2>(), true);
        var result2 = Triangulation.GetResult(points.ToList<Vector2>(), false);
        var result = result1.Count > result2.Count ? result1 : result2;
        var verticles = result.Select(v => new Vector3(v.x, v.y, 0)).ToList();
        var triangles = new int[verticles.Count];
        for (int i = 0; i < verticles.Count; i++)
            triangles[i] = i;
        var mesh = new Mesh { vertices = verticles.ToArray(), triangles = triangles.ToArray(), uv = result.ToArray() };
        mesh.RecalculateNormals();
        var f = GetComponent<MeshFilter>();
        if (f.sharedMesh != null)
            DestroyImmediate(f.sharedMesh);
        f.sharedMesh = mesh;
        for (int i = 0; i < points.Length; i++)
        {
            CreateLineForHouse(points[i], points[(i + 1) % points.Length]);
        }
    }
    private void CreateLineForHouse(Vector3 from, Vector3 to)
    {
        GameObject NewLine = new GameObject("HouseLine");
        NewLine.transform.parent = transform;
        NewLine.AddComponent<LineRenderer>();
        NewLine.GetComponent<LineRenderer>().startWidth = 0.1f;
        NewLine.GetComponent<LineRenderer>().endWidth = 0.1f;
        NewLine.GetComponent<LineRenderer>().startColor = Color.black;
        NewLine.GetComponent<LineRenderer>().endColor = Color.black;
        NewLine.GetComponent<LineRenderer>().material = MaterialForHouseLines;
        NewLine.GetComponent<LineRenderer>().SetPositions(new List<Vector3> { from, to }.ToArray());
    }
}
