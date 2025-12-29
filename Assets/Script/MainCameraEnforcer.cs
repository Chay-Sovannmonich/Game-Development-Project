using UnityEngine;

public class MainCameraEnforcer : MonoBehaviour
{
    void Awake()
    {
        // Force this camera to be active in Awake (runs before Start)
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log($"MAIN CAMERA ENFORCER: Forced {name} active in Awake()");
        }
        
        // Tag as MainCamera if not already
        if (!gameObject.CompareTag("MainCamera"))
        {
            gameObject.tag = "MainCamera";
            Debug.Log($"MAIN CAMERA ENFORCER: Tagged {name} as MainCamera");
        }
    }
    
    void Start()
    {
        Debug.Log($"MAIN CAMERA ENFORCER: {name} is {(gameObject.activeSelf ? "ACTIVE" : "INACTIVE")}");
        
        // Double-check we're active
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.LogWarning($"MAIN CAMERA ENFORCER: Had to force activate {name} in Start()");
        }
    }
}