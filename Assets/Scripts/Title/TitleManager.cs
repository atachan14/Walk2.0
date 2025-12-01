using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] float speed = 1f;

    private void Start()
    {
        var c = tmp.color;
        c.a = 0f;
        tmp.color = c;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // 0 Å® 1
        yield return StartCoroutine(Fade(0f, 1f));
        // 1 Å® 0
        yield return StartCoroutine(Fade(1f, 0f));
        // ÇªÇÃå„ÉVÅ[ÉìÇ‘Çøî≤Ç´
        SceneManager.LoadScene("Night");
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float a = Mathf.Lerp(from, to, t);
            var c = tmp.color;
            c.a = a;
            tmp.color = c;
            yield return null;
        }
    }
}
