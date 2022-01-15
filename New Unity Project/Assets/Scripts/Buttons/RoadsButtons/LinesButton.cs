using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LinesButton : MonoBehaviour
{
    public int N;
    public Button Current, ThisButton;
    public GridFunc Redactor;
    void Start()
    {
        void Select()
        {
            Redactor.SetMode(ThingsInCell.RoadForCars);
            Redactor.Lines = N;
            Current.GetComponent<Image>().sprite = ThisButton.GetComponent<Image>().sprite;
            Current.GetComponent<Image>().color = ThisButton.GetComponent<Image>().color;
        }
        ThisButton.onClick.AddListener(Select);
    }
}
