using System.Collections;
using UnityEngine;

public enum GamePhase
{
    main,
    result
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(GameFlowCoroutine());
    }

    IEnumerator GameFlowCoroutine()
    {
        yield return StartCoroutine(PlayOpening());


    }
    IEnumerator PlayOpening()
    {

        bool hasSave = false;

        // SaveData の有無だけを受け取る
        yield return FirebaseManager.Instance.LoadSaveDataCoroutine(result =>
        {
            hasSave = result;
        });

        if (hasSave)
        {
            yield return CurtainManager.Instance.OpeningFrontFlow(
                "Now Awaking..", "");

            MainCommondsManager.Instance.Init();
            MainDisplay.Instance.UpdateNight();
            MainDisplay.Instance.UpdateDisplay();

            yield return CurtainManager.Instance.BackFlow("Now Awaked..", "");
        }
        else
        {
            yield return CurtainManager.Instance.OpeningFrontFlow(
                "目が覚めたら夜だった。\n\nお家に帰らなきゃ！", "");

            yield return MapGenerator.Instance.Generate();
            MainCommondsManager.Instance.Init();
            MainDisplay.Instance.UpdateNight();
            MainDisplay.Instance.UpdateDisplay();

            yield return CurtainManager.Instance.OpeningBackFlow();

        }
    }






}
