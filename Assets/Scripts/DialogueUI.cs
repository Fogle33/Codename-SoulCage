using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public GameObject root;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI bodyText;
    public Button continueButton;

    [Header("Resource display (optional)")]
    public TextMeshProUGUI dialogueSoulsText;
    public TextMeshProUGUI dialogueScrapText;

    private string[] currentLines;
    private int currentIndex;
    private Action onClose;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        if (root) root.SetActive(false);
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Open(string speaker, string[] lines, Action onCloseCallback)
    {
        if (lines == null || lines.Length == 0) return;
        currentLines = lines;
        currentIndex = 0;
        onClose = onCloseCallback;
        if (speakerNameText) speakerNameText.text = speaker;
        if (root) root.SetActive(true);
        ShowCurrentLine();
        RefreshResources();
    }

    void ShowCurrentLine()
    {
        if (bodyText) bodyText.text = currentLines[currentIndex];
    }

    void OnContinueClicked()
    {
        currentIndex++;
        if (currentIndex >= currentLines.Length)
            Close();
        else
            ShowCurrentLine();
    }

    void Close()
    {
        if (root) root.SetActive(false);
        var cb = onClose;
        onClose = null;
        cb?.Invoke();
    }

    void RefreshResources()
    {
        if (dialogueSoulsText) dialogueSoulsText.text = GameState.Souls.ToString();
        if (dialogueScrapText) dialogueScrapText.text = GameState.Scrap.ToString();
    }
}