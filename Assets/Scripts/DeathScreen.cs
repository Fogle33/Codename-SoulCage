using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance;
    public GameObject panel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    public void Show()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Сначала выключаем хитбокс напрямую
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.ForceStopAttack(); // Новый метод
                combat.enabled = false;
            }

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
        }

        ArenaUI ui = FindObjectOfType<ArenaUI>();
        if (ui != null) ui.GetComponent<Canvas>().enabled = false;

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }
}