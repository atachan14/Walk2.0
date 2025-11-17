using System.Collections;
using UnityEngine;
using TMPro;

public class CurtainManager : MonoBehaviour
{
    public static CurtainManager Instance;

    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI centerTMP;
    [SerializeField] private TextMeshProUGUI bottomTMP;
    [SerializeField] private float fadeTime = 0.8f;

    private void Awake()
    {
        Instance = this;
        group.alpha = 1;       // 開始時は閉じた状態
        centerTMP.alpha = 0;
        bottomTMP.alpha = 0;
    }

    // --- フロント演出（フェードイン方向） ---
    public IEnumerator OpeningFrontFlow()
    {
 
        // ①セットアップ（groupはアルファ0でシーンに配置しとく）
        SetAlpha(centerTMP, 0);
        SetAlpha(bottomTMP, 0);
        centerTMP.text = "目が覚めたら夜だった。\n\nお家に帰らなきゃ！";
        bottomTMP.text = "";

        //②CenterTMPをフェードイン
        yield return StartCoroutine(FadeText(centerTMP, true)); // フェードイン

    }

    // --- バック演出（フェードアウト方向） ---
    public IEnumerator OpeningBackFlow()
    {
        bottomTMP.text = "tap anywhere..";
        SetAlpha(bottomTMP, 0);

        // ③BottomTMPをフェードインしながら
        StartCoroutine(FadeText(bottomTMP, true));

        // ④タップ待機
        yield return StartCoroutine(WaitForTap());

        // ⑤全体をフェードアウト
        yield return StartCoroutine(FadeGroup(false));
    }

    public IEnumerator FrontFlow(string c,string b)
    {
        SetAlpha(centerTMP, 1);
        SetAlpha(bottomTMP, 1);
        centerTMP.text = c;
        bottomTMP.text = b;

        yield return StartCoroutine(FadeGroup(true));
    }
    public IEnumerator MiddleText(string c, string b)
    {
        Debug.Log("MiddleText Start");
        yield return StartCoroutine(FadeText(centerTMP,false));
        Debug.Log("Center FadeOut Complete");
        centerTMP.text = c;
        bottomTMP.text = b;
        yield return StartCoroutine(FadeText(centerTMP, true));
        Debug.Log("Center FadeIn Complete");
    }
    public IEnumerator GroupStayTapAnywhere()
    {
        bottomTMP.text = "tap anywhere..";
        SetAlpha(bottomTMP, 0);

        // ③BottomTMPをフェードインしながら
        StartCoroutine(FadeText(bottomTMP, true));

        // ④タップ待機
        yield return StartCoroutine(WaitForTap());
    }

    public IEnumerator BackFlow(string c, string b)
    {
        centerTMP.text = c;
        bottomTMP.text = b;
        yield return StartCoroutine(FadeGroup(false));
    }
    public IEnumerator GroupStayBackFlow(string c, string b)
    {
        centerTMP.text = c;
        bottomTMP.text = b;
        yield return StartCoroutine(FadeText(centerTMP, false));
    }

    // --- 共通Fade処理 ---
    private IEnumerator FadeGroup(bool fadeIn)
    {
        group.blocksRaycasts = fadeIn;
        float start = fadeIn ? 0 : 1;
        float end = fadeIn ? 1 : 0;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, t / fadeTime);
            yield return null;
        }
        group.alpha = end;
    }

    private IEnumerator FadeText(TMP_Text tmp, bool fadeIn)
    {
        float start = fadeIn ? 0 : 1;
        float end = fadeIn ? 1 : 0;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(start, end, t / fadeTime);
            yield return null;
        }
        tmp.alpha = end;
    }

    private IEnumerator FadeTextSkippable(TMP_Text tmp)
    {
        float t = 0;
        while (t < fadeTime)
        {
            if (InputManager.Instance.tapAnywhere)
            {
                tmp.alpha = 1;
                InputManager.Instance.tapAnywhere = false;
                yield break;
            }
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }
        tmp.alpha = 1;
    }

    private IEnumerator WaitForTap()
    {
        while (!InputManager.Instance.tapAnywhere)
            yield return null;
        InputManager.Instance.tapAnywhere = false;
    }

    private void SetAlpha(TMP_Text tmp, float a)
    {
        var c = tmp.color;
        c.a = a;
        tmp.color = c;
    }
}
