using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public GameObject breakfastPanel;
    public GameObject lunchPanel;
    public GameObject dinnerPanel;
    public GameObject dessertPanel;

    public void ShowBreakfast()
    {
        breakfastPanel.SetActive(true);
        lunchPanel.SetActive(false);
        dinnerPanel.SetActive(false);
        dessertPanel.SetActive(false);
    }

    public void ShowLunch()
    {
        breakfastPanel.SetActive(false);
        lunchPanel.SetActive(true);
        dinnerPanel.SetActive(false);
        dessertPanel.SetActive(false);
    }

    public void ShowDinner()
    {
        breakfastPanel.SetActive(false);
        lunchPanel.SetActive(false);
        dinnerPanel.SetActive(true);
        dessertPanel.SetActive(false);
    }

    public void ShowDessert()
    {
        breakfastPanel.SetActive(false);
        lunchPanel.SetActive(false);
        dinnerPanel.SetActive(false);
        dessertPanel.SetActive(true);
    }
}