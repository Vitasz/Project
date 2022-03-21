using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadFile : MonoBehaviour
{
    public HorizontalLayoutGroup content;
    public GameObject SaveMapPrefab;
    public GridFunc grid;
    public Button thisButton;
    public void PreLoad()
    {
        string[] tmp = Directory.GetFiles("Saves");
        foreach (Transform child in content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (string a in tmp)
        {
            if (a.EndsWith(".txt"))
            {
                GameObject newObject = Instantiate(SaveMapPrefab, content.transform);
                OnClickLoadFile newObjectOC = newObject.GetComponent<OnClickLoadFile>();
                newObjectOC.filename = a;
                newObjectOC.grid = grid;
                newObjectOC.init();
            }
        }
    }
    public void Start()
    {
        thisButton.onClick.AddListener(PreLoad);
        PreLoad();
    }
}
