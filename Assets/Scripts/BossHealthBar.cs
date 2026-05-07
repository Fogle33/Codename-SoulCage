using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance;

    private Slider slider;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;

        slider = GetComponent<Slider>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Hide();
    }

    public void Show(float maxHP)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        slider.maxValue = maxHP;
        slider.value = maxHP;
    }

    public void UpdateHP(float current)
    {
        slider.value = current;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}