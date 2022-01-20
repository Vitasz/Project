using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FactoryButton : MonoBehaviour
{
    public Button Factory;
    public GridFunc GridScript;
    public Button CurrentButton;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HouseFact);
            CurrentButton.GetComponent<Image>().sprite = Factory.GetComponent<Image>().sprite;
            CurrentButton.GetComponent<Image>().color = Factory.GetComponent<Image>().color;
        }
        Factory.onClick.AddListener(Select);
    }
}
