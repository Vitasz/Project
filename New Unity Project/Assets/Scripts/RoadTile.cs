using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class RoadTile
{
    public GameObject Tile;
    public int[] NearRoads = new int[8];
    private List<List<bool>> HumanInTiles= new List<List<bool>>(){
        new List<bool>(){false, false,false,false },
        new List<bool>(){false, false,false,false },
        new List<bool>(){false, false,false,false },
        new List<bool>(){false, false,false,false } };
    public int totalHumans = 0, roads =0;
    public int TimeOnTile = 1;
    private bool HasMainRoad = false;
    public void HumanInTile()
    {
        totalHumans++;
        SetColorTile();
    }
    public void HumanOutTile()
    {
        totalHumans--;
        SetColorTile();
    }
    private void SetColorTile()
    {
        if (totalHumans < 10)
        {
            Tile.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if (totalHumans < 20)
        {
            Tile.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (totalHumans < 30)
        {
            Tile.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            Tile.GetComponent<SpriteRenderer>().color = new Color(176, 0, 0);
        }
    }
    public bool CanMove((int, int) to)
    {
        if (!HasMainRoad)
        {
            int near = to.Item1 - 1;
            if (near == -1) near = 3;
            int count1 = 0;
            if (HumanInTiles[0][2]) count1++;
            if (HumanInTiles[1][2]) count1++;
            if (HumanInTiles[2][2]) count1++;
            if (HumanInTiles[3][2]) count1++;
            return !HumanInTiles[to.Item1][to.Item2] && (!HumanInTiles[near][2] || roads==2 || to.Item2!=2 || count1 == roads);
        }
        else return false;
    }
    public void SetHumanInCell((int, int) where, bool what)
    {
        if (what) totalHumans++;
        else totalHumans--;
        SetColorTile();
        HumanInTiles[where.Item1][where.Item2] = what;
    }

}
