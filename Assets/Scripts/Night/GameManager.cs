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
        yield return StartCoroutine(CurtainManager.Instance
            .OpeningFrontFlow());

        yield return StartCoroutine(MapGenerator.Instance.Generate());

        MainCommondsManager.Instance.Init();
        MainDisplay.Instance.UpdateNight();
        MainDisplay.Instance.UpdateDisplay();

        yield return StartCoroutine(CurtainManager.Instance
            .OpeningBackFlow());
    }


    



}
