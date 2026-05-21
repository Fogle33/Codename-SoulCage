using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArenaUI : MonoBehaviour
{
    public static ArenaUI Instance;

    [Header("HP")]
    public Image hpBar;

    [Header("Wave info")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI wavesText;

    [Header("Cooldowns")]
    public Image dashCooldownIcon;
    public Image attackCooldownIcon;

    [Header("Resources (stable display, Hub)")]
    public TextMeshProUGUI soulsText;
    public TextMeshProUGUI scrapText;

    [Header("Scrap popup (Arena)")]
    public GameObject scrapPopupRoot;
    public TextMeshProUGUI scrapPopupText;
    public float scrapPopupDuration = 2f;

    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private Coroutine scrapPopupCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        GameState.OnSoulsChanged += UpdateSoulsDisplay;
        GameState.OnScrapChanged += UpdateScrapDisplay;
        GameState.OnScrapChanged += ShowScrapPopup;
    }

    void OnDisable()
    {
        GameState.OnSoulsChanged -= UpdateSoulsDisplay;
        GameState.OnScrapChanged -= UpdateScrapDisplay;
        GameState.OnScrapChanged -= ShowScrapPopup;
    }

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerCombat = player.GetComponent<PlayerCombat>();
        }

        UpdateSoulsDisplay();
        UpdateScrapDisplay();
        if (scrapPopupRoot != null) scrapPopupRoot.SetActive(false);
    }

    void Update()
    {
        if (dashCooldownIcon != null && playerMovement != null)
            dashCooldownIcon.fillAmount = 1f - playerMovement.DashCooldownProgress;
        if (attackCooldownIcon != null && playerCombat != null)
            attackCooldownIcon.fillAmount = 1f - playerCombat.AttackCooldownProgress;
    }

    public void UpdateHP(float current, float max)
    {
        if (hpBar) hpBar.fillAmount = current / max;
    }

    public void UpdateTimer(float seconds)
    {
        if (timerText)
        {
            if (seconds <= 0f)
                timerText.text = string.Empty;
            else
                timerText.text = Mathf.CeilToInt(seconds).ToString();
        }
    }

    public void UpdateWaves(int current, int max)
    {
        if (wavesText)
            wavesText.text = $"Wave {current}/{max}";
    }

    void UpdateSoulsDisplay()
    {
        if (soulsText) soulsText.text = GameState.Souls.ToString();
    }

    void UpdateScrapDisplay()
    {
        if (scrapText) scrapText.text = GameState.Scrap.ToString();
    }

    void ShowScrapPopup()
    {
        if (scrapPopupRoot == null || scrapPopupText == null) return;

        scrapPopupText.text = GameState.Scrap.ToString();
        scrapPopupRoot.SetActive(true);

        if (scrapPopupCoroutine != null)
            StopCoroutine(scrapPopupCoroutine);
        scrapPopupCoroutine = StartCoroutine(HideScrapPopupAfterDelay());
    }

    IEnumerator HideScrapPopupAfterDelay()
    {
        yield return new WaitForSeconds(scrapPopupDuration);
        if (scrapPopupRoot != null) scrapPopupRoot.SetActive(false);
        scrapPopupCoroutine = null;
    }
}