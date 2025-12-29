using UnityEngine;

public class CanvasSelfDestruct : MonoBehaviour
{
    [Header("Destruction Settings")]
    public float destroyDelay = 20f; // Time until destruction in seconds
    
    void Start()
    {
        // Start the destruction timer
        Invoke("DestroyCanvas", destroyDelay);
        
        Debug.Log("Canvas will self-destruct in " + destroyDelay + " seconds");
    }
    
    void DestroyCanvas()
    {
        // Destroy the canvas GameObject
        Destroy(gameObject);
        Debug.Log("Canvas destroyed after " + destroyDelay + " seconds");
    }
    
    // Optional: Destroy immediately with a method
    public void DestroyNow()
    {
        Destroy(gameObject);
    }
    
    // Optional: Cancel the destruction timer
    public void CancelDestruction()
    {
        CancelInvoke("DestroyCanvas");
        Debug.Log("Canvas destruction cancelled");
    }
    
    // Optional: Restart the destruction timer
    public void RestartTimer(float newDelay = -1f)
    {
        CancelInvoke("DestroyCanvas");
        
        if (newDelay > 0)
            destroyDelay = newDelay;
            
        Invoke("DestroyCanvas", destroyDelay);
        Debug.Log("Destruction timer restarted: " + destroyDelay + " seconds");
    }
    
    [ContextMenu("Destroy Now")]
    void DestroyNowEditor()
    {
        DestroyNow();
    }
    
    [ContextMenu("Test 2 Second Timer")]
    void TestShortTimer()
    {
        CancelInvoke("DestroyCanvas");
        Invoke("DestroyCanvas", 2f);
        Debug.Log("Testing with 2 second timer");
    }
}