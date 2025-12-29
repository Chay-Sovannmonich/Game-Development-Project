using UnityEngine;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    
    [Header("Time Settings")]
    public float globalTimeScale = 1.0f;
    public float gameplayTimeScale = 1.0f;
    public float uiTimeScale = 0.0f; // UI time scale when menu is open
    
    private bool isMenuOpen = false;
    private List<IPausable> pausableObjects = new List<IPausable>();
    private List<GameObject> pausedAnimators = new List<GameObject>();
    
    // Interface for pausable objects
    public interface IPausable
    {
        void OnPause(bool pause);
        bool ShouldPauseWithMenu { get; }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Handle global time scale based on menu state
        if (isMenuOpen)
        {
            // Gameplay continues, UI is responsive
            Time.timeScale = gameplayTimeScale; // Keep gameplay running at normal speed
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = globalTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
    
    public void SetMenuOpen(bool open)
    {
        if (isMenuOpen == open) return;
        
        isMenuOpen = open;
        
        if (isMenuOpen)
        {
            // Pause only specific objects that should pause with menu
            foreach (var obj in pausableObjects)
            {
                if (obj.ShouldPauseWithMenu)
                {
                    obj.OnPause(true);
                }
            }
            
            // Keep gameplay objects (cars, AI) running
            Debug.Log("Menu opened - gameplay continues");
        }
        else
        {
            // Unpause all objects
            foreach (var obj in pausableObjects)
            {
                obj.OnPause(false);
            }
            
            Debug.Log("Menu closed");
        }
    }
    
    // Register objects that might need to be paused
    public void RegisterPausable(IPausable pausable)
    {
        if (!pausableObjects.Contains(pausable))
        {
            pausableObjects.Add(pausable);
        }
    }
    
    public void UnregisterPausable(IPausable pausable)
    {
        pausableObjects.Remove(pausable);
    }
    
    // Get appropriate delta time based on object type
    public float GetDeltaTime(bool isGameplayObject)
    {
        if (isGameplayObject)
        {
            // Gameplay objects use unscaled time when menu is open
            return isMenuOpen ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        else
        {
            // UI objects use regular delta time
            return Time.deltaTime;
        }
    }
}