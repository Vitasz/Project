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
                tmp += " cellwithroad " + (map[a] as CellWithRoad).GetNearRoadsWays().Count.ToString()+" ";
                foreach ((int, int) b in (map[a] as CellWithRoad).GetNearRoadsWays()) tmp += b.Item1.ToString()+" "+b.Item2.ToString()+" ";
            }
            else if (map[a] is CellWithHouse)
            {
                tmp +=  (map[a] as CellWithHouse).GetTypeCell().ToString() ;
            }
            whatSave.Add(tmp);
        }
        File.WriteAllLines("Saves / " + filename.text + ".txt", whatSave);
        filename.text = "Success!";
        ScreenCapture.CaptureScreenshot("Screenshot.png");
    }
}
