using UnityEngine;

public class StoveClick : MonoBehaviour
{
    public GameObject targetCanvas; // Drag your canvas here

    void OnMouseDown()
    {
        targetCanvas.SetActive(false);
    }
}
