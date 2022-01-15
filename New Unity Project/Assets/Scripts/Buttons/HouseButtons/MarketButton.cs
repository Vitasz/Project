using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MarketButton : MonoBehaviour
{
    public Button Market;
    public GridFunc GridScript;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HouseCom);

        }
        Market.onClick.AddListener(Select);
    }
}
