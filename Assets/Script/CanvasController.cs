using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject targetCanvas;

    public void CloseCanvas()
    {
        targetCanvas.SetActive(false);
    }
}
