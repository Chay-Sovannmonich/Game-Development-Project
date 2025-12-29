using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClickShowPanel : MonoBehaviour
{
    public GameObject panel;
    public Image image;

    private bool hasClicked = false;

    void Start()
    {
        panel.SetActive(false);
        image.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (hasClicked) return;

        hasClicked = true;
        StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        panel.SetActive(true);
        image.gameObject.SetActive(true);
    }
}
