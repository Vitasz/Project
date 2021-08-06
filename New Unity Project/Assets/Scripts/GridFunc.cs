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
    public readonly float _linesWidth = 0.3f;
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
        Collider.size = new Vector2(SizeX * SizeCell, SizeY * SizeCell);
    }
    private void OnMouseDown()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
        if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
        int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
        if (X >= 0 && Y >= 0 && X < SizeX && Y < SizeY)
            GameController.ClickOnGrid(X, Y);
    }
    private void OnMouseDrag()
    {
       // if (GameController.Mode == 1)
       // {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
            if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
            int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
            if (X >= 0 && Y >= 0 && X < SizeX && Y < SizeY)
                GameController.ClickOnGrid(X, Y);
      //  }

    }
    private void OnMouseUp()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
        if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
        int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
        if (X >= 0 && Y >= 0 && X < SizeX && Y < SizeY)
            GameController.StopClickOnGrid(X, Y);
        else GameController.StopClickOnGrid(-1, -1);
    }
    public void SetSquare(List<(int,int)> Positions, int what)
    {
        foreach((int, int) a in Positions)
                Map[a.Item1][a.Item2] = what;
    }
    public bool TestSquare(List<(int, int)> Positions)
    {
        foreach ((int, int) a in Positions)
            if (Map[a.Item1][a.Item2] != 0) return false;
        return true;
    }
    public Vector2 PositionCell(int x, int y)
    {
        Vector2 toret = new Vector2();
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + x * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + y * SizeCell;
        return toret;
    }
    public string CountSameTiles(int X, int Y)
    {
        string total = "";
        if (X > 0 && Y + 1 < SizeY) total += Convert.ToString(Map[X - 1][Y + 1] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (Y + 1 < SizeY) total += Convert.ToString(Map[X][Y + 1] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (X + 1 < SizeX && Y + 1 < SizeY) total += Convert.ToString(Map[X + 1][Y + 1] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (X + 1 < SizeX) total += Convert.ToString(Map[X + 1][Y] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (X + 1 < SizeX && Y > 0) total += Convert.ToString(Map[X + 1][Y - 1] == Map[X][Y]  ? 1 : 0);
        else total += '0';
        if (Y > 0) total += Convert.ToString(Map[X][Y - 1] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (X > 0 && Y > 0) total += Convert.ToString(Map[X - 1][Y - 1] == Map[X][Y] ? 1 : 0);
        else total += '0';
        if (X > 0) total += Convert.ToString(Map[X - 1][Y] == Map[X][Y] ? 1 : 0);
        else total += '0';
        return total;
    }
}
