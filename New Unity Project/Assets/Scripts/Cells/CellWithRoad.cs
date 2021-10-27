using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class CellWithRoad : Cell
{
    private List<Vector3Int> roadsFromCell = new List<Vector3Int>();
    //private List<Vector3Int> CanMoveTo = new List<Vector3Int>();
    private int[] roadsfromCellOnIndex = new int[8];
    private string name = "0000000000000000";
    protected bool isEmpty = true;
    public float WaitTime = 1f;
    HumanFunctionality HumanInCell;
    HFforOA HumanInCellforOA;
    Vector3Int NextCellHuman;
    private bool todel = false;
    private bool visible = false;
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position, bool ForOA) : base(grid, houseControlles, position) {
        if (!ForOA)grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, -1), Resources.Load<Tile>("Tiles/Roads/0000000000000000"));
        visible = !ForOA;
        //UpdateTile();
    }
    public void RemoveRoad(Vector3Int from, Vector3Int to)
    {
        if (roadsFromCell.Contains(to))
        {
            roadsFromCell.Remove(to);
            roadsfromCellOnIndex[GetIndexNearCell(from, to)] = 0;
            name = name.Substring(0, 8);
            for (int i = 0; i < 8; i++)
            {
                if (roadsfromCellOnIndex[i] == 1) name += '1';
                else name += '0';
            }
            UpdateTile();
        }
        else
        {
            name = name.Substring(0, GetIndexNearCell(from, to)) + '0' + name.Substring(GetIndexNearCell(from, to) + 1);
            UpdateTile();
        }
    }
    public void AddRoad(Vector3Int from, Vector3Int to, bool fromthissell, bool ForOA)
    {
        if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to)!=-1 && name[GetIndexNearCell(from,to)]=='0'&&name[GetIndexNearCell(from,to)+8]=='0')
        {
            if (fromthissell)
            {
                roadsFromCell.Add(to);
                if (!ForOA)
                {
                    name = name.Substring(0, 8);
                    roadsfromCellOnIndex[GetIndexNearCell(from, to)] = 1;
                    for (int i = 0; i < 8; i++)
                    {
                        if (roadsfromCellOnIndex[i] == 1) name += '1';
                        else name += '0';
                    }
                }
                //Debug.Log("Add");
            }
            else {
                if (!ForOA)
                    name = name.Substring(0, GetIndexNearCell(from, to)) + '1' + name.Substring(GetIndexNearCell(from, to) + 1);
            }
            if (!ForOA) UpdateTile();
        }
    }
    protected override void UpdateTileWithNewNeighboors()
    {
        //SetTile();
    }
    public List<Vector3Int> GetNearRoadsWays()
    {
        return roadsFromCell;
    }
    protected override void UpdateTile()
    {
        // Debug.Log(name);
        if (!todel&&visible)
        {
            grid.tilemap.SetTile(positioninTileMap, Resources.Load<Tile>("Tiles/Roads/" + name));
            grid.tilemap.SetTileFlags(positioninTileMap, TileFlags.None);
            Color color;
            if (name != "0000000000000000")
            {
                if (WaitTime < 5f) color = Color.green;
                else if (WaitTime < 10f) color = Color.yellow;
                else color = Color.red;
                // Debug.Log(WaitTime);
                grid.tilemap.SetColor(positioninTileMap, color);
            }
        }
    }
    public HumanFunctionality CanMove()
    {
        if (isEmpty)
        {
            return null;
        }
        else return HumanInCell;
    }
    public void MoveToThis(HumanFunctionality who)
    {
        HumanInCell = who;
        isEmpty = false;
    }
    public void MoveToThis(HFforOA who)
    {
        HumanInCellforOA = who;
        isEmpty = false;
    }
    public void MoveOutThis()
    {
        HumanInCell = null;
        HumanInCellforOA = null;
        isEmpty = true;
        WaitTime = 0.0001f;
    }
    public bool IsEmpty() => isEmpty;
    public void UpdateWaitTime()
    {
        WaitTime = (WaitTime * 2 + (isEmpty ? 0 : 5)) / 2;
        WaitTime = Math.Max(0.0001f, WaitTime);
        UpdateTile();
    }
    public void Remove()
    {
        todel = true;
    }
    public HFforOA CanMoveForOA()
    {
        if (isEmpty)
        {
            return null;
        }
        else return HumanInCellforOA;
    }
}
