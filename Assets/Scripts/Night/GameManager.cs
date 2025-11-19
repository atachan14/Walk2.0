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
        yield return CurtainManager.Instance.OpeningFrontFlow();

        bool hasSave = false;

        // SaveData の有無だけを受け取る
        yield return FirebaseManager.Instance.LoadSaveDataCoroutine(result =>
        {
            hasSave = result;
        });

        if (hasSave)
        {
            Debug.Log("セーブデータあったので続きから開始");
        }
        else
        {
            Debug.Log("セーブデータ無し。新規開始");
            yield return MapGenerator.Instance.Generate();
        }


        MainCommondsManager.Instance.Init();
        MainDisplay.Instance.UpdateNight();
        MainDisplay.Instance.UpdateDisplay();

        yield return StartCoroutine(CurtainManager.Instance
            .OpeningBackFlow());
    }






}
