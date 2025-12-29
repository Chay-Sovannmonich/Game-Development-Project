using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class EXPManager : MonoBehaviour
{
    [Header("EXP Settings")]
    public float currentEXP = 0f;
    public float expPerMoneyIncrease = 20f; // 20% per money increase
    public float expToNextLevel = 100f;
    public int currentLevel = 1;
    
    [Header("UI References")]
    public Image expBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI percentageText;
    
    [Header("Level Up Effects")]
    public ParticleSystem levelUpParticles;
    public AudioClip levelUpSound;
    public GameObject levelUpEffect;
    
    // Events
    public event Action<int> OnLevelUp;
    
    void Start()
    {
        UpdateUI();
    }
    
    // Call this when money increases
    public void AddEXPFromMoneyIncrease()
    {
        AddEXP(expPerMoneyIncrease);
    }
    
    public void AddEXP(float amount)
    {
        currentEXP += amount;
        
        // Check for level up
        while (currentEXP >= expToNextLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
        Debug.Log($"Added {amount} EXP. Current: {currentEXP}/{expToNextLevel}");
    }
    
    void LevelUp()
    {
        currentEXP -= expToNextLevel;
        currentLevel++;
        
        // Increase required EXP for next level (optional scaling)
        expToNextLevel = CalculateNextLevelEXP();
        
        // Trigger level up effects
        LevelUpEffects();
        
        // Invoke level up event
        OnLevelUp?.Invoke(currentLevel);
        
        Debug.Log($"Level Up! Now Level {currentLevel}");
    }
    
    float CalculateNextLevelEXP()
    {
        // Example scaling: increase by 50% each level
        return expToNextLevel * 1.5f;
    }
    
    void LevelUpEffects()
    {
        // Play particles
        if (levelUpParticles != null)
        {
            levelUpParticles.Play();
        }
        
        // Play sound
        if (levelUpSound != null)
        {
            AudioSource.PlayClipAtPoint(levelUpSound, Camera.main.transform.position);
        }
        
        // Show effect
        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }
    }
    
    void UpdateUI()
    {
        // Update EXP bar
        if (expBarFill != null)
        {
            float fillAmount = currentEXP / expToNextLevel;
            expBarFill.fillAmount = fillAmount;
        }
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
        
        // Update EXP text
        if (expText != null)
        {
            expText.text = $"{currentEXP:F0}/{expToNextLevel:F0} EXP";
        }
        
        // Update percentage text
        if (percentageText != null)
        {
            int percentage = Mathf.RoundToInt((currentEXP / expToNextLevel) * 100);
            percentageText.text = $"{percentage}%";
        }
    }
    
    // Getters
    public float GetCurrentEXP() => currentEXP;
    public float GetEXPToNextLevel() => expToNextLevel;
    public int GetCurrentLevel() => currentLevel;
    public float GetEXPPercentage() => currentEXP / expToNextLevel;
    
    [ContextMenu("Add 20 EXP")]
    void TestAddEXP()
    {
        AddEXP(20f);
    }
    
    [ContextMenu("Level Up")]
    void TestLevelUp()
    {
        AddEXP(expToNextLevel);
    }
    
    [ContextMenu("Reset EXP")]
    void ResetEXP()
    {
        currentEXP = 0f;
        currentLevel = 1;
        expToNextLevel = 100f;
        UpdateUI();
    }
}