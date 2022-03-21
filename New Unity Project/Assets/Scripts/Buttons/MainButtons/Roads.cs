using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roads : MonoBehaviour
{
    public GameObject HousePanel, RoadsPanel, OptimizePanel, GeneratorPanel, SavePanel, LoadPanel;
    public Button HouseButton, RoadsButton, OptimizeButton, GButton, SaveButton, LoadButton, currentButton;

    void Start()
    {
        void SelectThis()
        {
            HousePanel.SetActive(false);
            RoadsPanel.SetActive(true);
            OptimizePanel.SetActive(false);
            GeneratorPanel.SetActive(false);
            SavePanel.SetActive(false);
            LoadPanel.SetActive(false);
            currentButton.gameObject.SetActive(true);
            RoadsButton.transform.GetComponent<Image>().color = new Color(194f / 255f, 194f / 255f, 194f / 255f, 100f / 255f);
            HouseButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            OptimizeButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            GButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            LoadButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            SaveButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
        }
        RoadsButton.onClick.AddListener(SelectThis);
    }
}
