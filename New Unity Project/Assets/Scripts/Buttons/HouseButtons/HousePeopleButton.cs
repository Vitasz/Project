using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HousePeopleButton : MonoBehaviour
{
    public Button HouseButton;
    public GridFunc GridScript;
    public Button CurrentButton;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HousePeople);
            CurrentButton.GetComponent<Image>().sprite = HouseButton.GetComponent<Image>().sprite;
            CurrentButton.GetComponent<Image>().color = HouseButton.GetComponent<Image>().color;
        }
        HouseButton.onClick.AddListener(Select);
    }
    
}
