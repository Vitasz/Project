using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class GridFunc : MonoBehaviour
{
    public int SizeX, SizeY, SizeCell;
    public GameControlls GameController;
    public Material MaterialForLines;
    public BoxCollider2D Collider;
    private readonly float _linesWidth = 0.3f;
    private List<Vector2> HighlightedSquares = new List<Vector2>();
    private List<List<int>> Map;
    private void Start()
    {
        void CreateLine(Vector3[] Positions)
        {
            GameObject NewLineGrid = new GameObject();
            NewLineGrid.transform.parent = transform;
            LineRenderer NewLineGridLR = NewLineGrid.AddComponent<LineRenderer>();
            NewLineGridLR.startWidth = _linesWidth;
            NewLineGridLR.endWidth = _linesWidth;
            NewLineGridLR.startColor = Color.black;
            NewLineGridLR.endColor = Color.black;
            NewLineGridLR.material = MaterialForLines;
            NewLineGridLR.SetPositions(Positions);
        }
        Map = Enumerable.Repeat(new List<int>(), SizeX).ToList();
        for (int i = 0; i <= SizeX; i++)
        {
            Debug.Log(i);
            if (SizeX!=i)Map[i] = Enumerable.Repeat(0, SizeX).ToList();
            Vector3[] Positions = new Vector3[] { new Vector3((float)i * SizeCell - (float)SizeX / 2 * SizeCell, (float)SizeY / 2 * SizeCell),
               new Vector3((float)i * SizeCell - (float)SizeX / 2 * SizeCell, -(float)SizeY / 2 * SizeCell)
               };
            CreateLine(Positions);
        }
        for (int i = 0; i <= SizeY; i++)
        {
            Vector3[] Positions = new Vector3[] { new Vector3((float)SizeX / 2 * SizeCell, (float)i * SizeCell - (float)SizeY / 2 * SizeCell),
               new Vector3(-(float)SizeX / 2 * SizeCell, (float)i * SizeCell - (float)SizeY / 2 * SizeCell)
               };
            CreateLine(Positions);
        }
        Collider.size = new Vector2(SizeX * SizeCell * 2, SizeY * SizeCell * 2);
    }
    private void OnMouseDown()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
        if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
        GameController.ClickOnGrid(Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX/2, Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2);
    }
    private void OnMouseDrag()
    {
        if (GameController.Mode == 1)
        {

        }
    }
    private void OnMouseUp()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
        if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
        GameController.StopClickOnGrid(Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2);
    }
    public void SetSquare((int,int) from, (int, int) to, int what)
    {
        for (int i = from.Item1; i <= to.Item1; i++)
            for (int j = from.Item2; j <= to.Item2; j++)
                Map[i][j] = what;
    }
    public bool TestSquare((int, int) from, (int, int) to)
    {
        for (int i = Math.Min(from.Item1, to.Item1); i <= Math.Max(from.Item1, to.Item1); i++)
            for (int j = Math.Min(from.Item2, to.Item2); j <= Math.Max(from.Item2, to.Item2); j++)
                if (Map[i][j] != 0) return false;
        return true;
    }
    public Vector2 PositionCell(int x, int y)
    {
        Vector2 toret = new Vector2();
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + x * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + y * SizeCell;
        return toret;
    }
}
