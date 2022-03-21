using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
public class OnClickLoadFile : MonoBehaviour
{
    public string filename;
    public Button thisButton;
    public GridFunc grid;
    public Image buttonImage;
    public Text text;
    public void Start()
    {
        void load()
        {
            grid.Load(filename);
        }
        thisButton.onClick.AddListener(load);
    }
    public void init()
    {
        
        string name = filename.Replace(".txt", "");
        text.text = name.Replace("Saves\\","");
        var tmp = File.ReadAllBytes(name + "IMG.png");
        var tmpColor = new  Texture2D(100, 100);
        tmpColor.LoadImage(tmp);
        Sprite sprite = Sprite.Create(tmpColor, new Rect(0, 0, tmpColor.width, tmpColor.height), Vector2.zero);
        buttonImage.sprite = sprite;
    }
}
