using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance;
    public GameObject root;
    public Button respawnButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        if (root) root.SetActive(false);
        if (respawnButton) respawnButton.onClick.AddListener(Respawn);
    }

    public void Show()
    {
        Time.timeScale = 0f;
        if (root) root.SetActive(true);
    }

    void Respawn()
    {
        Time.timeScale = 1f;
        PlayerUpgrades.Reset();
        FindObjectOfType<SceneTransition>()?.LoadScene("Hub");
    }
}