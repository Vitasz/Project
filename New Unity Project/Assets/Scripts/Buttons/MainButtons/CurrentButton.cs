using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentButton : MonoBehaviour
{
    public Button Current;
    public GridFunc Redactor;
    public Sprite Defolt;
    void Start()
    {
        void OnClick()
        {
            Redactor.SetMode(ThingsInCell.Nothing);
            Current.transform.GetComponent<Image>().sprite = Defolt;
            Current.transform.GetComponent<Image>().color = new Color(88f / 255f, 88f / 255f, 88f / 255f, 100f / 255f);
        }
        Current.onClick.AddListener(OnClick);
    }
}
