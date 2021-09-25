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
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position) : base(grid, houseControlles, position) { UpdateTile();}
    public void AddRoad(Vector3Int from, Vector3Int to, bool fromthissell)
    {
        if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to)!=-1)
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
        Debug.Log(name);
        grid.tilemap.SetTile(positioninTileMap, Resources.Load<Tile>("Tiles/Roads/" + name));
    }
    public bool CanMove() => isEmpty;
    public bool MoveToThis() => isEmpty = false;
    public bool MoveOutThis() => isEmpty = true;

}
