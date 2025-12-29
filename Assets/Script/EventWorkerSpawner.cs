using UnityEngine;

public class EventWorkerSpawner : MonoBehaviour
{
    [Header("=== SPAWN SETTINGS ===")]
    public GameObject workerPrefab;          // ⬅️ DRAG WORKERAI PREFAB HERE
    public bool spawnOnce = true;           // Only spawn once
    
    [Header("=== SPAWN POSITION ===")]
    public Vector3 spawnPositionOffset = Vector3.zero;
    public Vector3 spawnRotation = Vector3.zero;
    
    [Header("=== CAMERA REFERENCE ===")]
    public CameraSwitchButton cameraSwitchButton; // ⬅️ DRAG CAMERA SWITCH BUTTON HERE
    
    private bool hasSpawned = false;
    private GameObject spawnedWorker;
    
    void Start()
    {
        Debug.Log("=== EVENT WORKER SPAWNER STARTING ===");
        
        // Try to find CameraSwitchButton if not assigned
        if (cameraSwitchButton == null)
        {
            cameraSwitchButton = FindObjectOfType<CameraSwitchButton>();
            if (cameraSwitchButton != null)
            {
                Debug.Log($"✓ Found CameraSwitchButton: {cameraSwitchButton.gameObject.name}");
            }
        }
        
        if (cameraSwitchButton == null)
        {
            Debug.LogError("❌ CameraSwitchButton not found! Make sure it's in the scene.");
            return;
        }
        
        if (workerPrefab == null)
        {
            Debug.LogError("❌ No worker prefab assigned!");
            return;
        }
        
        // Subscribe to the camera switch event
        cameraSwitchButton.onSwitchedToCamera2 += OnCameraSwitchedToCamera2;
        
        Debug.Log("✓ EventWorkerSpawner ready - waiting for camera switch...");
        Debug.Log($"✓ Will spawn: {workerPrefab.name}");
        Debug.Log($"✓ Spawn position: {transform.position + spawnPositionOffset}");
    }
    
    void OnCameraSwitchedToCamera2()
    {
        Debug.Log("=== CAMERA SWITCHED TO CAMERA2! ===");
        
        if (hasSpawned && spawnOnce)
        {
            Debug.Log("Worker already spawned (spawnOnce = true)");
            return;
        }
        
        if (workerPrefab == null)
        {
            Debug.LogError("❌ No worker prefab to spawn!");
            return;
        }
        
        // Calculate spawn position
        Vector3 spawnPos = transform.position + spawnPositionOffset;
        Quaternion spawnRot = Quaternion.Euler(spawnRotation);
        
        // Spawn the worker
        spawnedWorker = Instantiate(workerPrefab, spawnPos, spawnRot);
        spawnedWorker.name = $"{workerPrefab.name} (Spawned)";
        
        hasSpawned = true;
        
        Debug.Log($"✅ SPAWNED WORKER: {workerPrefab.name}");
        Debug.Log($"✅ Position: {spawnPos}");
        Debug.Log($"✅ Rotation: {spawnRot.eulerAngles}");
    }
    
    void OnDestroy()
    {
        // Clean up event subscription
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnCameraSwitchedToCamera2;
        }
        
        // Clean up spawned worker
        if (spawnedWorker != null)
        {
            Destroy(spawnedWorker);
        }
    }
    
    [ContextMenu("Force Spawn Worker Now")]
    public void ForceSpawnNow()
    {
        Debug.Log("=== FORCE SPAWNING WORKER ===");
        
        if (workerPrefab == null)
        {
            Debug.LogError("No worker prefab assigned!");
            return;
        }
        
        Vector3 spawnPos = transform.position + spawnPositionOffset;
        Quaternion spawnRot = Quaternion.Euler(spawnRotation);
        
        spawnedWorker = Instantiate(workerPrefab, spawnPos, spawnRot);
        spawnedWorker.name = $"{workerPrefab.name} (Forced)";
        
        hasSpawned = true;
        
        Debug.Log($"✓ Forced spawn: {workerPrefab.name} at {spawnPos}");
    }
    
    [ContextMenu("Destroy Spawned Worker")]
    public void DestroySpawnedWorker()
    {
        if (spawnedWorker != null)
        {
            Destroy(spawnedWorker);
            hasSpawned = false;
            Debug.Log("Destroyed spawned worker");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn position
        Gizmos.color = Color.green;
        Vector3 spawnPos = transform.position + spawnPositionOffset;
        Gizmos.DrawSphere(spawnPos, 0.5f);
        Gizmos.DrawWireSphere(spawnPos, 0.7f);
        
        // Draw arrow showing forward direction
        Gizmos.color = Color.blue;
        Quaternion spawnRot = Quaternion.Euler(spawnRotation);
        Gizmos.DrawRay(spawnPos, spawnRot * Vector3.forward * 1f);
        
        // Draw connection to transform
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, spawnPos);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(spawnPos + Vector3.up * 0.5f, "Worker Spawn Point");
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
            $"EventWorkerSpawner\nPrefab: {(workerPrefab != null ? workerPrefab.name : "None")}\nSpawned: {hasSpawned}");
        #endif
    }
}