using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeOut()); // ← теперь метод существует
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    IEnumerator Transition(string sceneName)
    {
        yield return StartCoroutine(FadeIn());   // затемнение
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return StartCoroutine(FadeOut());  // разтёмнение
    }

    IEnumerator FadeIn()
    {
        yield return Fade(0, 1);
    }

    IEnumerator FadeOut()
    {
        yield return Fade(1, 0);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}