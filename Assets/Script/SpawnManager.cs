using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("New Prefabs To Spawn")]
    public GameObject newBoardPrefab;
    public GameObject newRiceBallPrefab;
    public GameObject newExtraPrefab;

    [Header("Old Objects To Destroy")]
    public GameObject oldBoardA;
    public GameObject oldRiceBallA;

    [Header("Spawn Positions")]
    public Vector3 boardPos = new Vector3(3.9f, 1.5f, 11f);
    public Vector3 riceBallPos = new Vector3(3.9f, 1.5f, 11f);
    public Vector3 extraPos = new Vector3(-0.5f, 2.5f, 11.5f);

    [Header("Bubble Respawn")]
    public GameObject bubbleToRespawn;
    public float respawnDelay = 2f;
    public bool unpauseGameOnRespawn = true;
    public bool forceUnpauseAfterSpawn = true; // Force unpause the game

    [Header("Bubble Persistence")]
    public bool respectBubblePersistence = true; // NEW: Check if bubble should stay hidden

    [Header("Worker AI Reference")]
    public WorkerAI workerAI; // Drag your WorkerAI GameObject here in the Inspector
    
    [Header("Game Flow Options")]
    public bool requireWorkerForGameplay = false; // Set to false to allow game without worker

    public Quaternion spawnRotation = Quaternion.identity;
    private bool hasSpawned = false;

    public void TransformObjects()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        DestroyOldObjects();
        List<GameObject> spawnedFood = SpawnNewObjects();
        SpawnExtraObject();

        Debug.Log("SpawnManager: Finished all spawns.");

        // FORCE UNPAUSE THE GAME IMMEDIATELY
        if (forceUnpauseAfterSpawn)
        {
            UnpauseGame();
        }

        // Check if worker exists, is active, and is enabled
        bool canUseWorker = workerAI != null && 
                           workerAI.gameObject.activeInHierarchy && 
                           workerAI.enabled;

        // Only notify worker if one exists, is active, and we want worker gameplay
        if (canUseWorker)
        {
            Debug.Log("SpawnManager: EMERGENCY - Food spawned! Calling worker IMMEDIATELY!");
            
            // Force worker to drop everything and get food
            StartCoroutine(EmergencyCallWorker(spawnedFood));
        }
        else if (requireWorkerForGameplay)
        {
            Debug.LogError("SpawnManager: No active WorkerAI assigned! Worker won't pick up food!");
        }
        else
        {
            if (workerAI != null && (!workerAI.gameObject.activeInHierarchy || !workerAI.enabled))
            {
                Debug.LogWarning("SpawnManager: WorkerAI exists but is inactive or disabled. Game continues without worker.");
            }
            else
            {
                Debug.LogWarning("SpawnManager: No WorkerAI assigned, but game continues without worker.");
            }
            
            // Optional: Add alternative gameplay logic here
            OnFoodSpawnedWithoutWorker(spawnedFood);
        }

        // Only respawn bubble if it's not permanently hidden
        if (respectBubblePersistence)
        {
            CheckAndRespawnBubble();
        }
        else
        {
            RespawnBubble();
        }
    }

    // NEW METHOD: Check if bubble should be respawned
    private void CheckAndRespawnBubble()
    {
        if (bubbleToRespawn != null)
        {
            // Check if bubble has been permanently clicked
            BubbleClick bubbleScript = bubbleToRespawn.GetComponent<BubbleClick>();
            if (bubbleScript != null)
            {
                // Try to check if bubble is marked as permanently clicked
                // Since BubbleClick saves to PlayerPrefs, we need to check the same key
                string bubbleKey = "BubbleClicked_CookingBubble"; // Default key
                
                // If you made the bubbleID serialized, you might need reflection or a public method
                // For now, using the default key pattern
                int clickedState = PlayerPrefs.GetInt(bubbleKey, 0);
                
                if (clickedState == 1)
                {
                    Debug.Log("SpawnManager: Bubble is permanently clicked. NOT respawning.");
                    return; // Don't respawn!
                }
            }
            
            // If not permanently clicked, proceed with normal respawn
            StartCoroutine(RespawnBubbleAfterDelay());
        }
    }

    // NEW METHOD: Force unpause the game
    private void UnpauseGame()
    {
        Time.timeScale = 1f;
        Debug.Log("SpawnManager: Force unpausing game. Time.timeScale = " + Time.timeScale);
    }

    private IEnumerator EmergencyCallWorker(List<GameObject> spawnedFood)
    {
        // Wait one frame to ensure food is properly spawned
        yield return null;
        
        try
        {
            // Double-check worker is still active before calling
            if (workerAI != null && workerAI.gameObject.activeInHierarchy && workerAI.enabled)
            {
                // Call emergency food pickup
                workerAI.EmergencyFoodPickup();
                
                // Also notify about each food item
                foreach (GameObject food in spawnedFood)
                {
                    if (food != null)
                    {
                        workerAI.OnFoodSpawned(food);
                    }
                }
                
                Debug.Log("SpawnManager: Emergency call to worker completed!");
            }
            else
            {
                Debug.LogWarning("SpawnManager: Worker became inactive before emergency call. Switching to no-worker mode.");
                OnFoodSpawnedWithoutWorker(spawnedFood);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SpawnManager: Error calling worker: {e.Message}");
            Debug.LogWarning("SpawnManager: Switching to no-worker mode due to error.");
            OnFoodSpawnedWithoutWorker(spawnedFood);
        }
    }

    // New method to handle food spawning when there's no worker
    private void OnFoodSpawnedWithoutWorker(List<GameObject> spawnedFood)
    {
        Debug.Log("SpawnManager: Food spawned without worker. Game continues normally.");
        
        // Optional: Add any alternative logic here, such as:
        // 1. Auto-destroy food after some time
        // 2. Trigger alternative game mechanics
        // 3. Log player score or progress
        // 4. Spawn alternative NPCs
        
        // Example: Auto-destroy food after 30 seconds if no worker picks it up
        StartCoroutine(AutoCleanupFood(spawnedFood, 30f));
    }

    // Optional: Auto-cleanup food if no worker is available
    private IEnumerator AutoCleanupFood(List<GameObject> foodItems, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        foreach (GameObject food in foodItems)
        {
            if (food != null)
            {
                Debug.Log($"SpawnManager: Auto-destroying {food.name} (no worker available)");
                Destroy(food);
            }
        }
    }

    private void DestroyOldObjects()
    {
        if (oldBoardA != null) Destroy(oldBoardA);
        if (oldRiceBallA != null) Destroy(oldRiceBallA);
    }

    // Spawn new objects and return list
    private List<GameObject> SpawnNewObjects()
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        if (newBoardPrefab != null)
        {
            GameObject board = Instantiate(newBoardPrefab, boardPos, spawnRotation);
            board.tag = "Board"; // Tag the spawned board
            spawnedObjects.Add(board);
            Debug.Log($"SpawnManager: Spawned Board at {boardPos}");
        }

        if (newRiceBallPrefab != null)
        {
            GameObject riceBall = Instantiate(newRiceBallPrefab, riceBallPos, spawnRotation);
            riceBall.tag = "RiceBall"; // Tag the spawned rice ball
            spawnedObjects.Add(riceBall);
            Debug.Log($"SpawnManager: Spawned RiceBall at {riceBallPos}");
        }

        return spawnedObjects;
    }

    private void SpawnExtraObject()
    {
        if (newExtraPrefab != null)
        {
            GameObject extra = Instantiate(newExtraPrefab, extraPos, spawnRotation);
            extra.tag = "Extra"; // Tag the spawned extra object
            Debug.Log($"SpawnManager: Spawned Extra object at {extraPos}");
        }
    }

    private void RespawnBubble()
    {
        if (bubbleToRespawn != null)
        {
            StartCoroutine(RespawnBubbleAfterDelay());
        }
    }

    private IEnumerator RespawnBubbleAfterDelay()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);
        
        try
        {
            BubbleClick bubbleScript = bubbleToRespawn.GetComponent<BubbleClick>();
            if (bubbleScript != null)
            {
                if (unpauseGameOnRespawn)
                {
                    bubbleScript.RespawnBubble();
                }
                else
                {
                    bubbleScript.ShowBubbleOnly();
                }
            }
            else
            {
                // Ensure game is unpaused when bubble respawns
                Time.timeScale = 1f;
                if (bubbleToRespawn != null)
                {
                    bubbleToRespawn.SetActive(true);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SpawnManager: Error in bubble respawn: {e.Message}");
            // Still unpause the game even if bubble respawn fails
            Time.timeScale = 1f;
        }
        
        Debug.Log("Bubble respawn process complete!");
    }

    // Public method to manually call for food pickup
    public void CallWorkerForFoodPickup()
    {
        try
        {
            // Check if worker exists and is active
            if (workerAI != null && workerAI.gameObject.activeInHierarchy && workerAI.enabled)
            {
                Debug.Log("SpawnManager: EMERGENCY - Calling worker for food pickup!");
                workerAI.EmergencyFoodPickup();
            }
            else if (requireWorkerForGameplay)
            {
                Debug.LogError("SpawnManager: No active WorkerAI assigned!");
            }
            else
            {
                Debug.LogWarning("SpawnManager: No active WorkerAI assigned. Food pickup not available.");
                // Optional: Add alternative pickup logic here
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SpawnManager: Error in CallWorkerForFoodPickup: {e.Message}");
        }
    }

    // NEW: Optional cleanup when disabled
    void OnDisable()
    {
        // Ensure game is unpaused if SpawnManager is disabled
        if (forceUnpauseAfterSpawn)
        {
            Time.timeScale = 1f;
        }
    }
}