using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Optimize : MonoBehaviour
{
    public GameObject HousePanel, RoadsPanel, OptimizePanel, GeneratorPanel;
    public Button HouseButton, RoadsButton, OptimizeButton, GButton;

    void Start()
    {
        void SelectThis()
        {
            HousePanel.SetActive(false);
            RoadsPanel.SetActive(false);
            OptimizePanel.SetActive(true);
            GeneratorPanel.SetActive(false) ;
            OptimizeButton.transform.GetComponent<Image>().color = new Color(194f / 255f, 194f / 255f, 194f / 255f, 100f / 255f);
            HouseButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            RoadsButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            GButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
        }
        OptimizeButton.onClick.AddListener(SelectThis);
    }
}
