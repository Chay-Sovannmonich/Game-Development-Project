using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIStatsManager : MonoBehaviour
{
    [Header("Stats")]
    public int coins;
    public float exp;       // current exp
    public float maxExp;    // exp required to level up
    public float happiness; // 0 to 100

    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public Image expFill;
    public Image happinessFill;

    void UpdateUI()
    {
        coinText.text = coins.ToString();

        expFill.fillAmount = exp / maxExp;
        happinessFill.fillAmount = happiness / 100f;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public void AddExp(float amount)
    {
        exp += amount;
        if (exp > maxExp) exp = maxExp;
        UpdateUI();
    }

    public void SetHappiness(float value)
    {
        happiness = Mathf.Clamp(value, 0, 100);
        UpdateUI();
    }

    private void Start()
    {
        UpdateUI();
    }
}
