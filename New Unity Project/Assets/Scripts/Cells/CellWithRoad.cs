﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using COLORS_CONST;
public class CellWithRoad : Cell
{
    private List<(int, int)> roadsFromCell = new List<(int, int)>();
    //private List<Vector3Int> CanMoveTo = new List<Vector3Int>();
    private int[] roadsfromCellOnIndex = new int[4];
    private string name = "0000";
    protected bool isEmpty = true;
    List<float> EmptyLastFrames = new List<float>();
    public float WaitTime = 1f;
    private float humansInCelllast100frames = 0;
    private float humansNow = 0;
    private float humansPlaces = 4;
    private bool todel = false;
    private bool visible = true;
    Dictionary<Vector3, HumanFunctionality> humansInCell = new Dictionary<Vector3, HumanFunctionality>();
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, (int,int) position,ThingsInCell type) : base(grid, houseControlles, position, type) {
        //grid.tilemap.SetTile(new Vector3Int(positioninTileMap.Item1, positioninTileMap.Item2, 0), Resources.Load<Tile>("Tiles/Roads/0000"));
        (int, int) Cellposition = GetCellPosition();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                humansInCell.Add(new Vector3(Cellposition.Item1 + 0.25f * j +0.1f, Cellposition.Item2 - 0.25f * i + 0.9f, 0), null);
            }
        }
    }
    public void RemoveRoad((int,int) from, (int, int) to)
    {
        if (roadsFromCell.Contains(to))
        {
            roadsFromCell.Remove(to);
            roadsfromCellOnIndex[GetIndexNearCell(from, to)/2] = 0;
            name = "";
            for (int i = 0; i < 4; i++)
            {
                if (roadsfromCellOnIndex[i] == 1) name += '1';
                else name += '0';
            }
        }
        UpdateTile();
    }
    public void AddRoad((int, int) from, List<(int, int)> roads)
    {
        foreach ((int, int) to in roads)
        {
            if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to) != -1)
            {
                roadsFromCell.Add(to);
                humansPlaces += 2;
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
    public List<Vector3> GetWayInTheCell((int, int) from, (int, int) to)
    {
        int fromInd = GetIndexNearCell(positioninTileMap, from)/2, toInd = GetIndexNearCell(positioninTileMap, to)/2;
        List<List<Vector3>> Allpositions = new List<List<Vector3>>();
        (int, int) Cellposition = GetCellPosition();
        for (int i = 0; i < 4; i++)
        {
            Allpositions.Add(new List<Vector3>());
            for (int j = 0; j < 4; j++)
            {
                Allpositions[Allpositions.Count - 1].Add(new Vector3(Cellposition.Item1 + 0.25f * j + 0.1f, Cellposition.Item2 - 0.25f * i + 0.9f, 0));
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
    public List<(int,int)> GetNearRoadsWays()
    {
        List<(int, int)> ans = new List<(int, int)>();
        foreach ((int, int) a in roadsFromCell) ans.Add(a);
        return ans;
    }
    protected override void UpdateTile()
    {
        // Debug.Log(name);
        if (!todel&&visible)
        {
            Vector3Int tmp = new Vector3Int(positioninTileMap.Item1, positioninTileMap.Item2, 1);
            grid.tilemap.SetTile(tmp, Resources.Load<Tile>("Tiles/Roads/" + name));
            grid.tilemap.SetTileFlags(tmp, TileFlags.None);
            Color color=new Color(0,0,0,1f);
            Color from, to;
            float percents = 0;
            if (WaitTime < 4f)
            {
                from = to = COLORS.ColorRoad1;
                percents = 0f;
            }
            else if (WaitTime < 8f)
            {
                from = COLORS.ColorRoad1;
                to = COLORS.ColorRoad2;
                percents = (WaitTime-4f)/4f;
            }
            else if (WaitTime < 12f)
            {
                from = COLORS.ColorRoad2;
                to = COLORS.ColorRoad3;
                percents = (WaitTime - 8f) / 4f;
            }
            else
            {
                from = to = COLORS.ColorRoad3;
                percents = 0f;
            }
            color.r = Mathf.Lerp(from.r, to.r, percents);
            color.g = Mathf.Lerp(from.g, to.g, percents);
            color.b = Mathf.Lerp(from.b, to.b, percents);
            grid.tilemap.SetColor(tmp, color);
        }
    }
    public HumanFunctionality CanMove(Vector3 position)
    {
        return humansInCell[position];
    }
    public void MoveToThis(HumanFunctionality who, Vector3 position)
    {
        humansInCell[position] = who;
        humansNow++;
    }
    public void MoveOutThis(HumanFunctionality who, Vector3 position)
    {
        if (who == humansInCell[position])
        {
            humansInCell[position] = null;
        }
        humansNow--;
    }
    public void UpdateWaitTime()
    {
        EmptyLastFrames.Add(humansNow/humansPlaces);
        humansInCelllast100frames += humansNow / humansPlaces;
        if (EmptyLastFrames.Count > 15)
        {
            humansInCelllast100frames -= EmptyLastFrames[0];
            EmptyLastFrames.RemoveAt(0);
        }
        humansInCelllast100frames = Math.Max(0.00001f, humansInCelllast100frames);
        WaitTime = (float)(humansInCelllast100frames);
        UpdateTile();
    }
    public void Remove()
    {
        todel = true;
    }
}
