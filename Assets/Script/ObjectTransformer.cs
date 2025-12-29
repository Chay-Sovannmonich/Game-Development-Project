using UnityEngine;

public class ObjectTransformer : MonoBehaviour
{
    public SpawnManager spawnManager;   // (Element 1)
    public bool destroyBubble = true;  // (Element 2)

    void OnMouseDown()
    {
        if (spawnManager != null)
            spawnManager.TransformObjects();

        if (destroyBubble)
            Destroy(gameObject);
    }
}
