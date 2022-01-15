using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FactoryButton : MonoBehaviour
{
    public Button Factory;
    public GridFunc GridScript;
    void Start()
    {
        void Select()
        {
            GridScript.SetMode(ThingsInCell.HouseFact);

        }
        Factory.onClick.AddListener(Select);
    }
}
