using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchaseScreen : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI soulsText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI feedbackText;
    public Button healButton;
    public Button damageButton;
    public Button speedButton;
    public Button continueButton;

    [Header("Цены (скрап)")]
    public int healCost = 3;
    public int damageCost = 6;
    public int speedCost = 5;

    [Header("Значения улучшений")]
    public float healAmount = 0.01f;
    public float damageBonus = 0.10f;
    public float speedBonus = 0.0015f;

    [Header("Лимиты (макс. покупок каждого)")]
    public int maxHealPurchases = 3;
    public int maxDamagePurchases = 3;
    public int maxSpeedPurchases = 3;
    [Header("Текущие статы")]
    public TextMeshProUGUI currentHealText;
    public TextMeshProUGUI currentDamageText;
    public TextMeshProUGUI currentSpeedText;
    void Start()
    {
        RefreshUI();
        healButton?.onClick.AddListener(BuyHeal);
        damageButton?.onClick.AddListener(BuyDamage);
        speedButton?.onClick.AddListener(BuySpeed);
        continueButton?.onClick.AddListener(Continue);
        UpdateButtonStates();
    }

    void RefreshUI()
    {
        if (soulsText) soulsText.text = $"Души: {GameState.Souls}";
        if (scrapText) scrapText.text = $"Лом: {GameState.Scrap}";

        if (currentHealText)
            currentHealText.text = $"Сейчас: {PlayerUpgrades.HealMultiplier * 100:F0}%";
        if (currentDamageText)
            currentDamageText.text = $"Сейчас: x{PlayerUpgrades.DamageMultiplier:F2}";
        if (currentSpeedText)
            currentSpeedText.text = $"Сейчас: +{PlayerUpgrades.SpeedBonus:F0}";
    }

    void Feedback(string msg)
    {
        if (feedbackText) feedbackText.text = msg;
    }

    void UpdateButtonStates()
    {
        if (healButton)
            healButton.interactable = PlayerUpgrades.HealPurchases < maxHealPurchases;
        if (damageButton)
            damageButton.interactable = PlayerUpgrades.DamagePurchases < maxDamagePurchases;
        if (speedButton)
            speedButton.interactable = PlayerUpgrades.SpeedPurchases < maxSpeedPurchases;
    }

    void BuyHeal()
    {
        if (PlayerUpgrades.HealPurchases >= maxHealPurchases)
        {
            Feedback("Максимум куплено"); return;
        }
        if (!GameState.SpendScrap(healCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.HealMultiplier += healAmount;
        PlayerUpgrades.HealPurchases++;
        Feedback($"Хил +{healAmount * 100}%");
        RefreshUI();
        UpdateButtonStates();
    }

    void BuyDamage()
    {
        if (PlayerUpgrades.DamagePurchases >= maxDamagePurchases)
        {
            Feedback("Максимум куплено");
            return;
        }
        if (!GameState.SpendScrap(damageCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.DamageMultiplier += damageBonus;
        PlayerUpgrades.DamagePurchases++;
        Feedback($"Урон +{damageBonus * 100}% куплено");
        RefreshUI();
        UpdateButtonStates();
    }

    void BuySpeed()
    {
        if (PlayerUpgrades.SpeedPurchases >= maxSpeedPurchases)
        {
            Feedback("Максимум куплено");
            return;
        }
        if (!GameState.SpendScrap(speedCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.SpeedBonus += speedBonus;
        PlayerUpgrades.SpeedPurchases++;
        Feedback($"Скорость +{speedBonus} куплено");
        RefreshUI();
        UpdateButtonStates();
    }

    void Continue()
    {
        FindObjectOfType<SceneTransition>()?.LoadScene(GameState.NextScene);
    }
}