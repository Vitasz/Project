﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class CellWithRoad : Cell
{
    private List<Vector3Int> roadsFromCell = new List<Vector3Int>();
    //private List<Vector3Int> CanMoveTo = new List<Vector3Int>();
    private int[] roadsfromCellOnIndex = new int[4];
    private string name = "0000";
    protected bool isEmpty = true;
    List<bool> EmptyLastFrames = new List<bool>(100);
    public float WaitTime = 1f;
    HumanFunctionality HumanInCell;
    private bool todel = false;
    private bool visible = true;
    Dictionary<Vector3, HumanFunctionality> humansInCell = new Dictionary<Vector3, HumanFunctionality>();
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position,ThingsInCell type) : base(grid, houseControlles, position, type) {
        grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, 0), Resources.Load<Tile>("Tiles/Roads/0000"));
        Vector3 Cellposition = GetCellPosition();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                humansInCell.Add(new Vector3(Cellposition.x + 0.25f * j +0.1f, Cellposition.y - 0.25f * i + 0.9f, 0), null);
            }
        }
    }
    /*public void RemoveRoad(Vector3Int from, Vector3Int to)
    {
        if (roadsFromCell.Contains(to))
        {
            roadsFromCell.Remove(to);
            roadsfromCellOnIndex[GetIndexNearCell(from, to)] = 0;
            name = name.Substring(0, 4);
            for (int i = 0; i < 4; i++)
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
    }*/
    public void AddRoad(Vector3Int from, List<Vector3Int> roads)
    {
        foreach (Vector3Int to in roads)
        {
            if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to) != -1)
            {
                roadsFromCell.Add(to);
                name = "";
                roadsfromCellOnIndex[GetIndexNearCell(from, to) / 2] = 1;
                for (int i = 0; i < 4; i++)
                {
                    if (roadsfromCellOnIndex[i] == 1) name += '1';
                    else name += '0';
                }
            }
        }
        UpdateTile();
    }
    public List<Vector3> GetWayInTheCell(Vector3Int from, Vector3Int to)
    {
        int fromInd = GetIndexNearCell(positioninTileMap, from)/2, toInd = GetIndexNearCell(positioninTileMap, to)/2;
        List<List<Vector3>> Allpositions = new List<List<Vector3>>();
        Vector3Int Cellposition = GetCellPosition();
        for (int i = 0; i < 4; i++)
        {
            Allpositions.Add(new List<Vector3>());
            for (int j = 0; j < 4; j++)
            {
                Allpositions[Allpositions.Count - 1].Add(new Vector3(Cellposition.x + 0.25f * j + 0.1f, Cellposition.y - 0.25f * i + 0.9f, 0));
            }
        }
        if (fromInd == 0)
        {
            if (toInd == 0)
            {
                return new List<Vector3>() { Allpositions[0][1], Allpositions[1][1], Allpositions[2][1], Allpositions[2][2], Allpositions[1][2], Allpositions[0][2] };
            }
            else if (toInd == 1)
            {
                return new List<Vector3>() { Allpositions[0][1], Allpositions[1][1], Allpositions[2][1], Allpositions[2][2], Allpositions[2][3]};
            }
            else if (toInd == 2)
            {
                return new List<Vector3>() { Allpositions[0][1], Allpositions[1][1], Allpositions[2][1], Allpositions[3][1]};
            }
            else if (toInd == 3)
            {
                return new List<Vector3>() { Allpositions[0][1], Allpositions[1][1], Allpositions[1][0]};
            }
        }
        else if (fromInd == 1)
        {
            if (toInd == 0)
            {
                return new List<Vector3>() { Allpositions[1][3], Allpositions[1][2], Allpositions[0][2]};
            }
            else if (toInd == 1)
            {
                return new List<Vector3>() { Allpositions[1][3], Allpositions[1][2], Allpositions[1][1], Allpositions[2][1], Allpositions[2][2], Allpositions[2][3] };
            }
            else if (toInd == 2)
            {
                return new List<Vector3>() { Allpositions[1][3], Allpositions[1][2], Allpositions[1][1], Allpositions[2][1], Allpositions[3][1] };
            }
            else if (toInd == 3)
            {
                return new List<Vector3>() { Allpositions[1][3], Allpositions[1][2], Allpositions[1][1], Allpositions[1][0]};
            }
        }
        else if (fromInd == 2)//OK
        {
            if (toInd == 0)
            {
                return new List<Vector3>() { Allpositions[3][2], Allpositions[2][2], Allpositions[1][2], Allpositions[0][2] };
            }
            else if (toInd == 1)
            {
                return new List<Vector3>() { Allpositions[3][2], Allpositions[2][2], Allpositions[2][3]};
            }
            else if (toInd == 2)
            {
                return new List<Vector3>() { Allpositions[3][2], Allpositions[2][2], Allpositions[1][2], Allpositions[1][1], Allpositions[2][1], Allpositions[3][1] };
            }
            else if (toInd == 3)
            {
                return new List<Vector3>() { Allpositions[3][2], Allpositions[2][2], Allpositions[1][2], Allpositions[1][1], Allpositions[1][0] };
            }
        }
        else if (fromInd == 3)//OK
        {
            if (toInd == 0)
            {
                return new List<Vector3>() { Allpositions[2][0], Allpositions[2][1], Allpositions[2][2], Allpositions[1][2], Allpositions[0][2]};
            }
            else if (toInd == 1)
            {
                return new List<Vector3>() { Allpositions[2][0], Allpositions[2][1], Allpositions[2][2], Allpositions[2][3]};
            }
            else if (toInd == 2)
            {
                return new List<Vector3>() { Allpositions[2][0], Allpositions[2][1], Allpositions[3][1]};
            }
            else if (toInd == 3)
            {
                return new List<Vector3>() { Allpositions[2][0], Allpositions[2][1], Allpositions[2][2], Allpositions[1][2], Allpositions[1][1], Allpositions[1][0] };
            }
        }
        return null;
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
            if (name != "0000")
            {
                if (WaitTime < 5f) color = Color.green;
                else if (WaitTime < 10f) color = Color.yellow;
                else color = Color.red;
                // Debug.Log(WaitTime);
                grid.tilemap.SetColor(positioninTileMap, color);
            }
        }
    }
    public HumanFunctionality CanMove(Vector3 position)
    {
        return humansInCell[position];
    }
    public void MoveToThis(HumanFunctionality who, Vector3 position)
    {
        humansInCell[position] = who;
    }
    public void MoveOutThis(HumanFunctionality who, Vector3 position)
    {
        if (who == humansInCell[position])
        {
            humansInCell[position] = null;
        }
    }
    public void UpdateWaitTime()
    {
        /*EmptyLastFrames.Add(isEmpty);
        if (EmptyLastFrames.Count>15)EmptyLastFrames.RemoveAt(0);
        int notwasEmpty = 0;
        for (int i = 0; i < EmptyLastFrames.Count; i++)
        {
            if (!EmptyLastFrames[i]) notwasEmpty++;
        }
        WaitTime = (float)notwasEmpty;*/
        UpdateTile();
    }
    public void Remove()
    {
        todel = true;
    }
}
