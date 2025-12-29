using UnityEngine;

// Helper component for cars to work with TimeManager
public class CarTimeHandler : MonoBehaviour, TimeManager.IPausable
{
    public bool ShouldPauseWithMenu => false; // Cars should NOT pause when menu is open
    
    public void OnPause(bool pause)
    {
        // Cars don't pause when menu opens, so we don't need to do anything here
        // If you want cars to pause, change ShouldPauseWithMenu to true
        // and handle pausing here
        if (pause)
        {
            Debug.Log($"[CarTimeHandler] {gameObject.name} paused (but shouldn't be called)");
        }
        else
        {
            Debug.Log($"[CarTimeHandler] {gameObject.name} unpaused");
        }
    }
    
    void OnDestroy()
    {
        // Unregister from TimeManager when destroyed
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.UnregisterPausable(this);
        }
    }
}