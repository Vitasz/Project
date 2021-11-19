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
    List<bool> EmptyLastFrames = new List<bool>(100);
    public float WaitTime = 1f;
    HumanFunctionality HumanInCell;
    Vector3Int NextCellHuman;
    private bool todel = false;
    private bool visible = true;
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position,ThingsInCell type, List<Vector3Int> RoadsFrom) : base(grid, houseControlles, position, type) {
        grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, -1), Resources.Load<Tile>("Tiles/Roads/0000000000000000"));
        roadsFromCell = RoadsFrom;
        foreach (Vector3Int a in RoadsFrom)
        {
            roadsfromCellOnIndex[GetIndexNearCell(position, a)] = 1;
        }

        name = name.Substring(0, 8);

        for (int i = 0; i < 8; i++)
        {
            if (roadsfromCellOnIndex[i] == 1) name += '1';
            else name += '0';
        }

        if(RoadsFrom.Count != 0) UpdateTile();

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
    public void AddRoad(Vector3Int from, Vector3Int to, bool fromthissell)
    {
        if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to) != -1 && name[GetIndexNearCell(from, to)] == '0' && name[GetIndexNearCell(from, to) + 8] == '0')
        {
            if (fromthissell)
            {
                roadsFromCell.Add(to);
                name = name.Substring(0, 8);
                roadsfromCellOnIndex[GetIndexNearCell(from, to)] = 1;
                for (int i = 0; i < 8; i++)
                {
                    if (roadsfromCellOnIndex[i] == 1) name += '1';
                    else name += '0';
                }
                //Debug.Log("Add");
            }
            else
            {
                name = name.Substring(0, GetIndexNearCell(from, to)) + '1' + name.Substring(GetIndexNearCell(from, to) + 1);
            }
            UpdateTile();
        }
    }
    protected override void UpdateTileWithNewNeighboors()
    {
        //SetTile();
    }
    public List<Vector3Int> GetNearRoadsWays()
    {
        List<Vector3Int> ans = new List<Vector3Int>();
        foreach (Vector3Int a in roadsFromCell) ans.Add(a);
        return ans;
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
        isEmpty = false;
        HumanInCell = who;
    }
    public void MoveOutThis(HumanFunctionality who)
    {
        if (who == HumanInCell)
        {
            HumanInCell = null;
            isEmpty = true;
        }
    }
    public bool IsEmpty() => isEmpty;
    public void UpdateWaitTime()
    {
        EmptyLastFrames.Add(isEmpty);
        if (EmptyLastFrames.Count>15)EmptyLastFrames.RemoveAt(0);
        int notwasEmpty = 0;
        for (int i = 0; i < EmptyLastFrames.Count; i++)
        {
            if (!EmptyLastFrames[i]) notwasEmpty++;
        }
        WaitTime = (float)notwasEmpty;
        UpdateTile();
    }
    public void Remove()
    {
        todel = true;
    }
}
