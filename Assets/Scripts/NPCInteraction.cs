using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public string promptText = "E — поговорить";
    public string speakerName = "Страж";
    [TextArea(3, 8)]
    public string[] dialogueLines = new string[]
    {
        "Ты снова здесь. Значит, цикл не закончен.",
        "Иди. Клетка ждёт."
    };

    private bool playerInRange = false;
    private bool dialogueOpen = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!dialogueOpen)
                InteractionPrompt.Instance?.Show(promptText);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            InteractionPrompt.Instance?.Hide();
        }
    }

    void Update()
    {
        if (playerInRange && !dialogueOpen && Keyboard.current.eKey.wasPressedThisFrame)
            OpenDialogue();
    }

    void OpenDialogue()
    {
        dialogueOpen = true;
        InteractionPrompt.Instance?.Hide();
        DialogueUI.Instance?.Open(speakerName, dialogueLines, OnDialogueClosed);
    }

    void OnDialogueClosed()
    {
        dialogueOpen = false;
        if (playerInRange)
            InteractionPrompt.Instance?.Show(promptText);
    }
}