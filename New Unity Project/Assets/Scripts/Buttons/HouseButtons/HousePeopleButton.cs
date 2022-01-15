using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HousePeopleButton : MonoBehaviour
{
    public Button HouseButton;
    public GridFunc GridScript;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HousePeople);

        }
        HouseButton.onClick.AddListener(Select);
    }
    
}
