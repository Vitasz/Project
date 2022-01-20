using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructuresButtons : MonoBehaviour
{
    public GameObject HousePanel, RoadsPanel, OptimizePanel;
    public Button HouseButton, RoadsButton, OptimizeButton;

    void Start()
    {
        void SelectThis()
        {
            HousePanel.SetActive(true);
            RoadsPanel.SetActive(false);
            OptimizePanel.SetActive(false);
            HouseButton.transform.GetComponent<Image>().color = new Color(194f / 255f, 194f / 255f, 194f / 255f, 100f / 255f);
            RoadsButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
            OptimizeButton.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
        }
        HouseButton.onClick.AddListener(SelectThis);
    }
}
