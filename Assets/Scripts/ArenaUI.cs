using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ArenaUI : MonoBehaviour
{
    public static ArenaUI Instance;
    public Image hpBar;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI wavesText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
                timerText.text = string.Empty; // при нуле скрываем таймер
            else
                timerText.text = Mathf.CeilToInt(seconds).ToString();
        }
    }
    public void UpdateWaves(int current, int max)
    {
        if (wavesText)
            wavesText.text = $"Wave {current}/{max}";
    }

    
}
