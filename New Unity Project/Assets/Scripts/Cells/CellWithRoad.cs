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
    Vector3Int NextCellHuman;
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position) : base(grid, houseControlles, position) {
        grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, -1), Resources.Load<Tile>("Tiles/Roads/0000000000000000"));
        //UpdateTile();
    }
    public void AddRoad(Vector3Int from, Vector3Int to, bool fromthissell)
    {
        if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to)!=-1 && name[GetIndexNearCell(from,to)]=='0'&&name[GetIndexNearCell(from,to)+8]=='0')
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
            else {
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
        return roadsFromCell;
    }
    protected override void UpdateTile()
    {
       // Debug.Log(name);
        grid.tilemap.SetTile(positioninTileMap, Resources.Load<Tile>("Tiles/Roads/" + name));
        grid.tilemap.SetTileFlags(positioninTileMap, TileFlags.None);
        Color color;
        if (WaitTime <5f) color = Color.green;
        else if (WaitTime < 10f) color = Color.yellow;
        else color = Color.red;
       // Debug.Log(WaitTime);
        grid.tilemap.SetColor(positioninTileMap, color);
    }
    public bool CanMove(HumanFunctionality who, Vector3Int from)
    {
        if (isEmpty)
        {
            if (from == positioninTileMap) return true;
            int indexfrom = GetIndexNearCell(positioninTileMap, from);
            int indexcheck = (indexfrom + 6) % 8;
            if (name[indexcheck] != '0')
            {
                Vector3Int PositionCheck = GetPositionNearCell(positioninTileMap, indexcheck);
                CellWithRoad CellCheck = grid.GetCell(PositionCheck) as CellWithRoad;
                return CellCheck.IsEmpty() || CellCheck.NextCellHuman != positioninTileMap;
            }
            else return true;
        }
        else return false;
    }
    public void MoveToThis(HumanFunctionality who, Vector3Int NextCellHuman)
    {
        HumanInCell = who;
        isEmpty = false;
        this.NextCellHuman = NextCellHuman;
    }
    public void MoveOutThis()
    {
        HumanInCell = null;
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
}
