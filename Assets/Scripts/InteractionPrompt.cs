using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance;
    public GameObject root;
    public TextMeshProUGUI text;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        Hide();
    }

    public void Show(string message)
    {
        if (text) text.text = message;
        if (root) root.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }
}