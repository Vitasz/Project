using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RoadsControlles : MonoBehaviour
{
    public GridFunc Grid;
    public Dictionary<(int, int), RoadTile> Roads = new Dictionary<(int, int), RoadTile>();
    public void AddRoad(List<(int,int)> Positions)
    {
        for (int i = 0; i < Positions.Count; i++)
        {
            if (Roads.ContainsKey(Positions[i]))
            {
                int totalroads = 0;
                if (i - 1 >= 0&& GetIndex(Positions[i], Positions[i - 1])!=-1) Roads[Positions[i]].NearRoads[GetIndex(Positions[i], Positions[i - 1])] = 1;
                if (i + 1 < Positions.Count&& GetIndex(Positions[i], Positions[i + 1])!=-1) Roads[Positions[i]].NearRoads[GetIndex(Positions[i], Positions[i + 1])] = 1;
                foreach (int a in Roads[Positions[i]].NearRoads) if (a == 1) totalroads++;
                Roads[Positions[i]].roads = totalroads;
                ChangeRoad(Positions[i]);
            }
            else
            {
                int totalroads = 0;
                Roads[Positions[i]] = new RoadTile();
                if (i - 1 >= 0&& GetIndex(Positions[i], Positions[i - 1])!=-1) Roads[Positions[i]].NearRoads[GetIndex(Positions[i], Positions[i - 1])] = 1;
                if (i + 1 < Positions.Count&& GetIndex(Positions[i], Positions[i + 1])!=-1) Roads[Positions[i]].NearRoads[GetIndex(Positions[i], Positions[i + 1])] = 1;
                foreach (int a in Roads[Positions[i]].NearRoads) if (a == 1) totalroads++;
                Roads[Positions[i]].roads = totalroads;
                CreateTile(Positions[i]);
            }
        }
    }
    private int GetIndex((int,int) from, (int, int) to)
    {
        if (from.Item1 - 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 0;
        if (from.Item1 == to.Item1 && from.Item2 + 1 == to.Item2) return 1;
        if (from.Item1 + 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 2;
        if (from.Item1 + 1 == to.Item1 && from.Item2 == to.Item2) return 3;
        if (from.Item1 + 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 4;
        if (from.Item1 == to.Item1 && from.Item2 - 1 == to.Item2) return 5;
        if (from.Item1 - 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 6;
        if (from.Item1 - 1 == to.Item1 && from.Item2 == to.Item2) return 7;
        return -1;
    }
    private (int, int) IndexToPos((int, int) pos, int index)
    {
        if (index == 0) return (pos.Item1 - 1, pos.Item2 + 1);
        if (index == 1) return (pos.Item1, pos.Item2 + 1);
        if (index == 2) return (pos.Item1 + 1, pos.Item2 + 1);
        if (index == 3) return (pos.Item1 + 1, pos.Item2);
        if (index == 4) return (pos.Item1 + 1, pos.Item2 - 1);
        if (index == 5) return (pos.Item1, pos.Item2 - 1);
        if (index == 6) return (pos.Item1 - 1, pos.Item2 - 1);
        if (index == 7) return (pos.Item1 - 1, pos.Item2);
        return (-1, -1);
    }
    private bool nearTile((int, int) pos, (int, int) Tile)=>
         Math.Abs(pos.Item1 - Tile.Item1) <= 1 && Math.Abs(pos.Item2 - Tile.Item2) <= 1;
    public void CreateTile((int,int) position)
    {
        string swap(string a, int count)
        {
            string ans = a;
            for (int i = 0; i < a.Length; i++)
                ans = ans.Substring(0, (i + count) % a.Length) + a[i] + ans.Substring((i + count) % a.Length + 1);
            return ans;
        }
        void Create(string path, int rotation)
        {
            GameObject gameObject = new GameObject();
            Roads[position].Tile = gameObject;
            gameObject.transform.parent = transform;
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.localScale = new Vector2(Grid.SizeCell + Grid._linesWidth, Grid.SizeCell + Grid._linesWidth);
            gameObject.transform.localPosition = Grid.PositionCell(position);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
        }
        int[] tiles = Roads[position].NearRoads;
        string s = "";
        for (int i = 0; i < 8; i++)
        {
            if (tiles[i] == 1 && i % 2 == 1) s += '1';
            else s += '0';
        }
        string FilePath = Application.dataPath + "/Resources/Sprites/Tiles/Road";
        if (File.Exists(FilePath + "/basetile" + s + ".png"))
            Create("Sprites/Tiles/Road/basetile" + s, 0);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 2) + ".png"))
            Create("Sprites/Tiles/Road/basetile" + swap(s, 2), 90);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 4) + ".png"))
            Create("Sprites/Tiles/Road/basetile" + swap(s, 4), 180);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 6) + ".png"))
            Create("Sprites/Tiles/Road/basetile" + swap(s, 6), 270);
        else Debug.LogWarning("File " + s + " not found in roads' tiles");
    }
    private void ChangeRoad((int,int) position)
    {
        string swap(string a, int count)
        {
            string ans = a;
            for (int i = 0; i < a.Length; i++)
                ans = ans.Substring(0, (i + count) % a.Length) + a[i] + ans.Substring((i + count) % a.Length + 1);
            return ans;
        }
        void Change(string path, int rotation)
        {
            GameObject gameObject = Roads[position].Tile;
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
        }
        int[] tiles = Roads[position].NearRoads;
        string s = "";
        for (int i = 0; i < 8; i++)
        {
            if (tiles[i] == 1 && i % 2 == 1) s += '1';
            else s += '0';
        }
        string FilePath = Application.dataPath + "/Resources/Sprites/Tiles/Road";
        if (File.Exists(FilePath + "/basetile" + s + ".png"))
            Change("Sprites/Tiles/Road/basetile" + s, 0);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 2) + ".png"))
            Change("Sprites/Tiles/Road/basetile" + swap(s, 2), 90);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 4) + ".png"))
            Change("Sprites/Tiles/Road/basetile" + swap(s, 4), 180);
        else if (File.Exists(FilePath + "/basetile" + swap(s, 6) + ".png"))
            Change("Sprites/Tiles/Road/basetile" + swap(s, 6), 270);
        else Debug.LogWarning("File " + s + " not found in roads' tiles");
    }
    public List<(int,int)> FindWay(SortedSet<(int, int)> from, SortedSet<(int, int)> to)
    {
        List<((int, int), int)> now = new List<((int, int), int)>();
        foreach((int,int) a in from) now.Add((a, 0));
        List< ((int, int), int)> newnow = new List<((int, int), int)>();
        //List<List<(int, int)>> savenow = new List<List<(int, int)>>() { new List<(int, int)> { from } };
        Dictionary<(int, int), int> MinTimeToTile = new Dictionary<(int, int), int>();
        foreach((int,int) temp in from) MinTimeToTile[temp] = 0;
        SortedSet<(int, int)> VisitedTiles = new SortedSet<(int, int)>();
        foreach ((int, int) a in from) VisitedTiles.Add(a);
        (int, int) toposition = (-1, -1);
        while (now.Count != 0)
        {
            foreach(((int, int), int) a in now)
            {
                //Debug.Log(a.Item1);
                if (to.Contains(a.Item1))
                {
                    toposition = a.Item1;
                    newnow.Clear();
                    break;
                }
                if (Roads.ContainsKey(a.Item1))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (Roads[a.Item1].NearRoads[i] == 1 && (!VisitedTiles.Contains(IndexToPos(a.Item1, i))||MinTimeToTile[IndexToPos(a.Item1, i)] >a.Item2+Roads[a.Item1].TimeOnTile))
                        {
                            newnow.Add((IndexToPos(a.Item1, i), a.Item2+Roads[a.Item1].TimeOnTile));
                            VisitedTiles.Add(IndexToPos(a.Item1, i));
                            MinTimeToTile[IndexToPos(a.Item1, i)] = a.Item2 + Roads[a.Item1].TimeOnTile;
                        }
                    }
                }
            }
            now.Clear();
            foreach (((int, int), int) a in newnow) now.Add(a);
            newnow.Clear();
        }
        List<(int, int)> ans = new List<(int, int)>();
        //Debug.Log(toposition);
        if (toposition!=(-1, -1)) {
            (int, int) nowposition = toposition;
            while (!from.Contains(nowposition))
            {
                ans.Add(nowposition);
                for (int i = 1; i < 8; i += 2)
                {
                    (int, int) CheckPosition = IndexToPos(nowposition, i);
                    if (VisitedTiles.Contains(CheckPosition) && !ans.Contains(CheckPosition) &&Roads[nowposition].NearRoads[i]==1 && MinTimeToTile[CheckPosition] + Roads[CheckPosition].TimeOnTile==MinTimeToTile[nowposition])
                    {
                        nowposition = IndexToPos(nowposition, i);
                        break;
                    }
                }
            }
            ans.Add(nowposition);
            ans.Reverse();
            return ans;
        }
        return null;
    }
}
