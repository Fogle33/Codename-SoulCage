using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Clicked Start Game") ;
        SceneManager.LoadScene("Hub");
    }

    public void QuitGame()
    {
        Debug.Log("Clicked Quit Game");
        Application.Quit();
    }
}