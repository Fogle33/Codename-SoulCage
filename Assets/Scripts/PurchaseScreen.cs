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

    [Header("Следующая сцена")]
    public string nextScene = "Arena2";

    void Start()
    {
        RefreshUI();
        healButton?.onClick.AddListener(BuyHeal);
        damageButton?.onClick.AddListener(BuyDamage);
        speedButton?.onClick.AddListener(BuySpeed);
        continueButton?.onClick.AddListener(Continue);
    }

    void RefreshUI()
    {
        if (soulsText) soulsText.text = $"Души: {GameState.Souls}";
        if (scrapText) scrapText.text = $"Лом: {GameState.Scrap}";
    }

    void Feedback(string msg) { if (feedbackText) feedbackText.text = msg; }

    void BuyHeal()
    {
        if (!GameState.SpendScrap(healCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.HealOnNextSpawn = true;
        Feedback("Лечение куплено");
        RefreshUI();
    }

    void BuyDamage()
    {
        if (!GameState.SpendScrap(damageCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.DamageMultiplier += 0.25f;
        Feedback("Урон +25%");
        RefreshUI();
    }

    void BuySpeed()
    {
        if (!GameState.SpendScrap(speedCost)) { Feedback("Недостаточно лома"); return; }
        PlayerUpgrades.SpeedBonus += 1f;
        Feedback("Скорость +20%");
        RefreshUI();
    }

    void Continue()
    {
        FindObjectOfType<SceneTransition>()?.LoadScene(nextScene);
    }
}