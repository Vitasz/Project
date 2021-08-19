using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RoadsControlles : MonoBehaviour
{
    public GridFunc Grid;
    private Dictionary<(int, int), int[]> Roads = new Dictionary<(int, int), int[]>();
    private Dictionary<(int, int), GameObject> Tiles = new Dictionary<(int, int), GameObject>();
    public void AddRoad(List<(int,int)> Positions)
    {
        for (int i = 0; i < Positions.Count; i++)
        {
           // if (Grid.TestSquare(new List<(int, int)> { Positions[i] }))
          //  {
                if (Roads.ContainsKey(Positions[i]))
                {
                    if (i - 1 >= 0) Roads[Positions[i]][GetIndex(Positions[i], Positions[i - 1])] = 1;
                    if (i + 1 < Positions.Count) Roads[Positions[i]][GetIndex(Positions[i], Positions[i + 1])] = 1;
                    ChangeRoad(Positions[i]);
                }
                else
                {
                    Debug.Log(i);
                    Roads[Positions[i]] = new int[8];
                    if (i - 1 >= 0) Roads[Positions[i]][GetIndex(Positions[i], Positions[i - 1])] = 1;
                    Debug.Log(Positions.Count);
                    if (i + 1 < Positions.Count) Roads[Positions[i]][GetIndex(Positions[i], Positions[i + 1])] = 1;
                    CreateTile(Positions[i]);
                }
         //   }
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
            Tiles[position] = gameObject;
            gameObject.transform.parent = transform;
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.localScale = new Vector2(Grid.SizeCell + Grid._linesWidth, Grid.SizeCell + Grid._linesWidth);
            gameObject.transform.localPosition = Grid.PositionCell(position.Item1, position.Item2);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
        }
        int[] tiles = Roads[position];
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
            GameObject gameObject = Tiles[position];
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
        }
        int[] tiles = Roads[position];
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
}
