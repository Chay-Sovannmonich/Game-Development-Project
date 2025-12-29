using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelBarSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public float currentEXP = 0f;
    public float expToLevel2 = 100f; // Total EXP needed for Level 2
    public float expPerMoney = 20f; // EXP gained per money collected
    
    [Header("UI References")]
    public Image levelBarFill; // The fill image for the bar
    public TextMeshProUGUI levelText; // Shows "Level 1" or "Level 2"
    public TextMeshProUGUI expText; // Shows "20/100" or similar
    public TextMeshProUGUI percentageText; // Shows "20%"
    
    [Header("Visual Effects")]
    public ParticleSystem levelUpEffect;
    public AudioClip levelUpSound;
    public GameObject levelUpPopup; // Optional popup
    
    private bool isMaxLevel = false;
    
    void Start()
    {
        UpdateUI();
    }
    
    // Call this every time money is collected
    public void AddEXPMoneyCollected()
    {
        if (isMaxLevel) return;
        
        // Add EXP
        currentEXP += expPerMoney;
        
        // Check if we reached Level 2
        if (currentEXP >= expToLevel2)
        {
            LevelUp();
        }
        
        UpdateUI();
        Debug.Log($"Added {expPerMoney} EXP. Total: {currentEXP}/{expToLevel2}");
    }
    
    void LevelUp()
    {
        currentLevel = 2;
        currentEXP = expToLevel2; // Cap at max
        isMaxLevel = true;
        
        // Show level up effects
        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }
        
        if (levelUpSound != null)
        {
            AudioSource.PlayClipAtPoint(levelUpSound, Camera.main.transform.position);
        }
        
        if (levelUpPopup != null)
        {
            levelUpPopup.SetActive(true);
        }
        
        Debug.Log("LEVEL UP! You reached Level 2!");
    }
    
    void UpdateUI()
    {
        // Calculate fill amount (0 to 1)
        float fillAmount = currentEXP / expToLevel2;
        
        // Update level bar fill
        if (levelBarFill != null)
        {
            levelBarFill.fillAmount = fillAmount;
        }
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
            
            // Optional: Make level text bigger when leveling up
            if (currentLevel == 2)
            {
                levelText.fontSize = 36;
                levelText.color = Color.yellow;
            }
        }
        
        // Update EXP text
        if (expText != null)
        {
            expText.text = $"{currentEXP:F0}/{expToLevel2:F0}";
        }
        
        // Update percentage text
        if (percentageText != null)
        {
            int percentage = Mathf.RoundToInt(fillAmount * 100);
            percentageText.text = $"{percentage}%";
            
            // Optional: Change color when reaching 100%
            if (percentage >= 100)
            {
                percentageText.color = Color.green;
            }
        }
    }
    
    // Getters for other scripts
    public int GetCurrentLevel() => currentLevel;
    public float GetEXPPercentage() => currentEXP / expToLevel2;
    public bool IsMaxLevel() => isMaxLevel;
    
    [ContextMenu("Add EXP (Test)")]
    public void TestAddEXP()
    {
        AddEXPMoneyCollected();
    }
    
    [ContextMenu("Reset to Level 1")]
    public void ResetLevel()
    {
        currentLevel = 1;
        currentEXP = 0f;
        isMaxLevel = false;
        UpdateUI();
        Debug.Log("Reset to Level 1");
    }
    
    [ContextMenu("Set to Level 2")]
    public void SetToLevel2()
    {
        LevelUp();
        UpdateUI();
    }
}