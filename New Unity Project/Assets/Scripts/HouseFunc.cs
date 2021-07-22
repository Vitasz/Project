using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HouseFunc : MonoBehaviour
{
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
    }
}
