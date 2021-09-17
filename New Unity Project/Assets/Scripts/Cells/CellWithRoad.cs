using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CellWithRoad : Cell
{
    private List<Vector3Int> roadsFromCell = new List<Vector3Int>();
    private List<Vector3Int> CanMoveTo = new List<Vector3Int>();
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position) : base(grid, houseControlles, position) { }
    public void AddRoad(Vector3Int from, Vector3Int to, int type, CellWithRoad toCell)
    {
        if (!roadsFromCell.Contains(to) && from != to)
        {
            roadsFromCell.Add(to);
            UpdateTileWithNewNeighboors(); toCell.UpdateTileWithNewNeighboors();
            CanMoveTo.Add(to);
            // grid.GetComponent<Tilemap>().SetTile(from, Resources.Load<Tile>("Tiles/Roads/basetile" + roadsnear));
            // grid.GetComponent<Tilemap>().SetTile(to, Resources.Load<Tile>("Tiles/Roads/basetile00000000"));
        }
    }
    protected override void UpdateTileWithNewNeighboors()
    {
        
    }

    public List<Vector3Int> GetNearRoadsWays()
    {
        return roadsFromCell;

    }

}
