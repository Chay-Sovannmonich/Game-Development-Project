using UnityEngine;

public class Camera2Enforcer : MonoBehaviour
{
    void Awake()
    {
        // Force Camera2 to be INACTIVE at the very beginning
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Debug.Log($"CAMERA2 ENFORCER: Forced {name} inactive in Awake()");
        }
    }
    
    void Start()
    {
        Debug.Log($"CAMERA2 ENFORCER: {name} is {(gameObject.activeSelf ? "ACTIVE" : "INACTIVE")}");
        
        // If somehow active, disable
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Debug.LogWarning($"CAMERA2 ENFORCER: Had to force disable {name} in Start()");
        }
    }
}