using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RoadsControlles : MonoBehaviour
{
    public GridFunc Grid;
    public void AddRoad(int X, int Y)
    {
        Debug.Log("Add road at: X: " + Convert.ToString(X) + " Y: " + Convert.ToString(Y));
    }

    public void CreateTile(int X, int Y)
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
            gameObject.transform.parent = transform;
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.localScale = new Vector2(Grid.SizeCell + Grid._linesWidth, Grid.SizeCell + Grid._linesWidth);
            gameObject.transform.localPosition = Grid.PositionCell(X, Y);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
            spriteRenderer.color = Color.red;
        }
        string s = Grid.CountSameTiles(X, Y);
        string FilePath = Application.dataPath + "/Resources/Sprites/Tiles/Roads";
        if (File.Exists(FilePath + "/base" + s + ".png"))
            Create("Sprites/Tiles/Roads/base" + s, 0);
        else if (File.Exists(FilePath + "/base" + swap(s, 2) + ".png"))
            Create("Sprites/Tiles/Roads/base" + swap(s, 2), 90);
        else if (File.Exists(FilePath + "/base" + swap(s, 4) + ".png"))
            Create("Sprites/Tiles/Roads/base" + swap(s, 4), 180);
        else if (File.Exists(FilePath + "/base" + swap(s, 6) + ".png"))
            Create("Sprites/Tiles/Roads/base" + swap(s, 6), 270);
        else Debug.LogWarning("File " + s + " not found in houses' tiles");
    }
}
