using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class SaveFile : MonoBehaviour
{

    public InputField filename;
    public Button saveButton;
    public GridFunc grid;
    public GameObject tilemap;
    public void Start()
    {
        saveButton.onClick.AddListener(saveTilemap);
    }
    private void saveTilemap()
    {
        var map = grid.Map;
        List<string> whatSave = new List<string>();
        foreach ((int,int) a in map.Keys)
        {
            string tmp="";
            tmp += a.Item1.ToString() + " " + a.Item2.ToString() + " ";

            if (map[a] is CellWithRoad) {
                tmp += "cellwithroad " + (map[a] as CellWithRoad).GetNearRoadsWays().Count.ToString()+" ";
                foreach ((int, int) b in (map[a] as CellWithRoad).GetNearRoadsWays()) tmp += b.Item1.ToString()+" "+b.Item2.ToString()+" ";
            }
            else if (map[a] is CellWithHouse)
            {
                tmp +=  (map[a] as CellWithHouse).GetTypeCell().ToString() ;
            }
            whatSave.Add(tmp);
        }
        File.WriteAllLines("Saves/ " + filename.text + ".txt", whatSave);
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        for (int x = 0; x < texture.width; ++x)
        {
            for (int y = 0; y <200; ++y)
            {
                texture.SetPixel(texture.width - x, y, Color.clear);
            }
        }
        texture.Apply();
        RenderTexture rt = new RenderTexture(100, 100, 100);
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        Texture2D result = new Texture2D(100, 100);
        result.ReadPixels(new Rect(0, 0, 100, 100), 0, 0);
        result.Apply();
        byte[] bytes = result.EncodeToPNG();
        File.WriteAllBytes("Saves/ " + filename.text + "IMG.png", bytes);
        filename.text = "Success!";
    }
}
