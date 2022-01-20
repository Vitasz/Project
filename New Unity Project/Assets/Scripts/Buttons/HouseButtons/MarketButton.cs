using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MarketButton : MonoBehaviour
{
    public Button Market;
    public GridFunc GridScript;
    public Button CurrentButton;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HouseCom);
            CurrentButton.GetComponent<Image>().sprite = Market.GetComponent<Image>().sprite;
            CurrentButton.GetComponent<Image>().color = Market.GetComponent<Image>().color;
        }
        Market.onClick.AddListener(Select);
    }
}
