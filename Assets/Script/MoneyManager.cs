using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    [Header("Money Settings")]
    public int startingMoney = 500;
    public int currentMoney = 500;
    
    [Header("UI References")]
    public TextMeshProUGUI moneyTextUI;
    public TextMeshPro moneyText3D;
    public string moneyPrefix = "Money: $";
    
    [Header("Money Bar UI")]
    public Image moneyBarFill;
    public TextMeshProUGUI moneyPercentageText;
    public int maxMoney = 1000;
    
    [Header("EXP System")]
    public EXPManager expManager;
    
    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyDisplay();
        
        if (moneyBarFill != null)
        {
            UpdateMoneyBar();
        }
        
        // Find EXP Manager if not assigned
        if (expManager == null)
        {
            expManager = FindObjectOfType<EXPManager>();
        }
    }
    
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyDisplay();
        
        if (moneyBarFill != null)
        {
            UpdateMoneyBar();
        }
        
        // Trigger EXP increase
        if (expManager != null)
        {
            expManager.AddEXPFromMoneyIncrease();
        }
        
        Debug.Log($"Added ${amount}. Total: ${currentMoney}");
    }
    
    public void SetMoney(int amount)
    {
        currentMoney = amount;
        UpdateMoneyDisplay();
        
        if (moneyBarFill != null)
        {
            UpdateMoneyBar();
        }
    }
    
    void UpdateMoneyDisplay()
    {
        string moneyString = $"{moneyPrefix}{currentMoney}";
        
        if (moneyTextUI != null)
        {
            moneyTextUI.text = moneyString;
        }
        
        if (moneyText3D != null)
        {
            moneyText3D.text = moneyString;
        }
    }
    
    void UpdateMoneyBar()
    {
        if (moneyBarFill != null)
        {
            float fillAmount = Mathf.Clamp01((float)currentMoney / maxMoney);
            moneyBarFill.fillAmount = fillAmount;
            
            if (moneyPercentageText != null)
            {
                int percentage = Mathf.RoundToInt(fillAmount * 100);
                moneyPercentageText.text = $"{percentage}%";
            }
        }
    }
    
    public int GetCurrentMoney() => currentMoney;
    public string GetMoneyString() => $"{moneyPrefix}{currentMoney}";
    
    [ContextMenu("Add 50 Money")]
    void TestAdd50()
    {
        AddMoney(50);
    }
    
    [ContextMenu("Reset to 500")]
    void ResetMoney()
    {
        SetMoney(500);
    }
}