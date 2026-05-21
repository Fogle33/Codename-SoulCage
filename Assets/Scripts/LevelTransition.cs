using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public string targetSceneName = "Arena";
    public string promptText = "E — войти";

    private bool playerInRange = false;
    private SceneTransition sceneTransition;

    void Start()
    {
        sceneTransition = FindObjectOfType<SceneTransition>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
            EnterLevel();
    }

    void EnterLevel()
    {
        InteractionPrompt.Instance?.Hide();
        if (sceneTransition != null)
            sceneTransition.LoadScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }
}