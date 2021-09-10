using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems ;
public class GridFunc : MonoBehaviour
{
    public EventSystem EventSystemManager;
    public int SizeX, SizeY, SizeCell;
    public GameControlls GameController;
    public Material MaterialForLines;
    public BoxCollider2D Collider;
    public readonly float _linesWidth = 0.3f;
    private List<List<Cell>> Map = new List<List<Cell>>();
    private bool isRedactorActive = false;
    private List<(int, int, int, int)> CellsClicked = new List<(int, int, int, int)>();
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
        for (int i = 0; i < SizeX; i++)
        {
            Map.Add(new List<Cell>());
            for (int j = 0; j < SizeY; j++)
            {
                Map[i].Add(new Cell(this, PositionCell((i, j))));
            }
        }
        for (int i = 0; i <= SizeX; i++)
        {
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
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
            if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
            int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
            if (!isRedactorActive)
            {
                if (X >= 0 && Y >= 0 && X < SizeX && Y < SizeY)
                    GameController.ClickOnGrid((X, Y));
            }
            else
            {
                
                if (Mathf.Abs(nowPositionRedactor.Item1-X)<=1 && Mathf.Abs(nowPositionRedactor.Item2 - Y) <= 1)
                {
                    worldPosition -= new Vector2(PositionCell((X, Y)).x - (float)SizeCell /2, PositionCell((X, Y)).y - (float)SizeCell /2); //left conner of cell
                    int PosInCellX = Mathf.CeilToInt(worldPosition.x / ((float)SizeCell / 4))+1, PosInCellY = Mathf.CeilToInt(worldPosition.y / ((float)SizeCell / 4))+1;
                    if (CellsClicked.Count == 0 || CellsClicked[CellsClicked.Count-1] != (X, Y, PosInCellX, PosInCellY)) CellsClicked.Add((X, Y, PosInCellX, PosInCellY));
                }
            }
        }
    }
    private void OnMouseDrag()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
            if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
            if (isRedactorActive)
            {
                int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
                if (Mathf.Abs(nowPositionRedactor.Item1 - X) <= 1 && Mathf.Abs(nowPositionRedactor.Item2 - Y) <= 1)
                {
                    worldPosition -= new Vector2(PositionCell((X, Y)).x - (float)SizeCell / 2, PositionCell((X, Y)).y - (float)SizeCell / 2); //left conner of cell
                    int PosInCellX = Mathf.CeilToInt(worldPosition.x / ((float)SizeCell / 4)) + 1, PosInCellY = Mathf.CeilToInt(worldPosition.y / ((float)SizeCell / 4)) + 1;
                    if (CellsClicked.Count==0 || CellsClicked[CellsClicked.Count - 1] != (X, Y, PosInCellX, PosInCellY)) CellsClicked.Add((X, Y, PosInCellX, PosInCellY));
                }
            }
        }
    }
    private void OnMouseUp()
    {
        if (isRedactorActive)
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
            if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
            int X = Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX / 2, Y = Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2;
            if (Mathf.Abs(nowPositionRedactor.Item1 - X) <= 1 && Mathf.Abs(nowPositionRedactor.Item2 - Y) <= 1)
            {
                worldPosition -= new Vector2(PositionCell((X, Y)).x - (float)SizeCell / 2, PositionCell((X, Y)).y - (float)SizeCell / 2); //left conner of cell
                int PosInCellX = Mathf.CeilToInt(worldPosition.x / ((float)SizeCell / 4)) + 1, PosInCellY = Mathf.CeilToInt(worldPosition.y / ((float)SizeCell / 4)) + 1;
                if (CellsClicked[CellsClicked.Count - 1] != (X, Y, PosInCellX, PosInCellY)) CellsClicked.Add((X, Y, PosInCellX, PosInCellY));
            }
            if (CellsClicked.Count > 1)
            {
                for (int i = 0; i < CellsClicked.Count-1; i++)
                {
                    (int, int) from = (CellsClicked[i].Item1, CellsClicked[i].Item2);
                    (int, int) prevto = (CellsClicked[i + 1].Item1, CellsClicked[i + 1].Item2);
                    (int, int) to;
                    if (from == prevto) to = (CellsClicked[i + 1].Item3, CellsClicked[i + 1].Item4);
                    else
                    {
                        if (from.Item1==prevto.Item1-1) to = (5, CellsClicked[i + 1].Item4);
                        else if (from.Item1==prevto.Item1+1)to = (-1, CellsClicked[i + 1].Item4);
                        else if (from.Item2==prevto.Item2-1)to = (CellsClicked[i + 1].Item3, 5);
                        else to = (CellsClicked[i + 1].Item3, -1);
                    }
                    Map[CellsClicked[i].Item1][CellsClicked[i].Item2].AddRoad((CellsClicked[i].Item3, CellsClicked[i].Item4), to, (int)ThingsInCell.RoadForCars);
                }
            }
            CellsClicked.Clear();
        }
    }
    public Vector3 PositionCell((int,int) cell)
    {
        Vector3 toret = new Vector3(0, 0,-100);
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + cell.Item1 * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + cell.Item2 * SizeCell;
        return toret;
    }
    (int, int) nowPositionRedactor;
    public void OpenRedactorCell((int, int) Position) {
        nowPositionRedactor = Position;
        for (int i = -1; i < 2; i++)
            if (Position.Item1 - i >= 0 && Position.Item1 - i < SizeX)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (Position.Item2 - j >= 0 && Position.Item2 - j < SizeY)
                    {
                        Map[Position.Item1 - i][Position.Item2 - j].ShowMiniCells();
                    }
                }
            }
        isRedactorActive = true;
    }
    public void CloseRedactorCell()
    {
        for (int i = -1; i < 2; i++)
            if (nowPositionRedactor.Item1 - i >= 0 && nowPositionRedactor.Item1 - i < SizeX)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (nowPositionRedactor.Item2 - j >= 0 && nowPositionRedactor.Item2 - j < SizeY)
                    {
                        Map[nowPositionRedactor.Item1 - i][nowPositionRedactor.Item2 - j].StopShowMiniCells();
                    }
                }
            }
        isRedactorActive = false;
    }
}
