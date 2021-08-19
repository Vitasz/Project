using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class HouseControlles : MonoBehaviour
{
    public GridFunc Grid;
    public GameObject HousePrefab;
    public RoadsControlles RoadsController;
    Dictionary<GameObject, List<(int,int)>> Houses = new Dictionary<GameObject, List<(int, int)>>();
    public void CreateHouse(List<(int,int)> Positions)
    {
        if (Grid.TestSquare(Positions))
        {
            Grid.SetSquare(Positions, transform.childCount + 1);
            GameObject NewHouse = new GameObject("House " + Convert.ToString(transform.childCount + 1));
            Houses.Add(NewHouse, new List<(int,int)>());
            NewHouse.transform.parent = transform;
            foreach ((int, int) a in Positions) CreateTile(NewHouse, a.Item1, a.Item2);
        }
    }
    private void CreateTile(GameObject House, int X, int Y)
    {
        string transformInput(string s1){
            for (int i = 0; i < s1.Length; i+=2)
            {
                int nowprev = i - 1 == -1 ? s1.Length - 1 : i - 1;
                int nownext = i + 1 == s1.Length ? 0 : i + 1;
                if (s1[i] == '1' && (s1[nowprev] == '0' && s1[nownext] == '0')) s1 = s1.Substring(0, i) + '0' + s1.Substring(i + 1);
                if (s1[nowprev]=='1' && s1[nownext]=='1') s1 = s1.Substring(0, i) + '1' + s1.Substring(i + 1);
            }
            return s1;
        }
        string swap(string a, int count)
        {
            string ans = a;
            for (int i = 0; i < a.Length; i++)
                ans = ans.Substring(0, (i + count)%a.Length) + a[i] + ans.Substring((i + count) % a.Length + 1);
            return ans;
        }
        void Create(string path, int rotation)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.parent = House.transform;
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(path);
            gameObject.transform.localScale = new Vector2(Grid.SizeCell + Grid._linesWidth, Grid.SizeCell + Grid._linesWidth);
            gameObject.transform.localPosition = Grid.PositionCell(X, Y);
            gameObject.transform.eulerAngles = new Vector3(0, 0, rotation);
            spriteRenderer.color = Color.red;
            spriteRenderer.sortingLayerName = "House";
        }
        string s = transformInput(Grid.CountSameTiles(X, Y));
        if (s.Contains("0"))
        {
            RoadsController.AddRoad(new List<(int, int)> { (X, Y) });
            Houses[House].Add((X, Y));
        }
        string FilePath = Application.dataPath + "/Resources/Sprites/Tiles/House";
        if (File.Exists(FilePath + "/base" + s + ".png"))
            Create("Sprites/Tiles/House/base" + s,0);
        else if (File.Exists(FilePath + "/base" + swap(s, 2) + ".png"))
            Create("Sprites/Tiles/House/base" + swap(s, 2), 90);
        else if (File.Exists(FilePath + "/base" + swap(s, 4) + ".png"))
            Create("Sprites/Tiles/House/base" + swap(s, 4), 180);
        else if (File.Exists(FilePath + "/base" + swap(s, 6) + ".png"))
            Create("Sprites/Tiles/House/base" + swap(s, 6), 270);
        else Debug.LogError("File " + s + " not found in houses' tiles");
    }
}
