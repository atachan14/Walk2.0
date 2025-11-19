using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CommondType
{
    None,
    LeftTurn,
    Walk,
    RightTurn,
    Notch,
    GoHome,

    Sleep,
    Diary
}

public class MainCommondsManager : MonoBehaviour
{
    public static MainCommondsManager Instance;
    [SerializeField] CommondData[] datas = new CommondData[3];
    [SerializeField] Button[] btms = new Button[3];
    [SerializeField] TextMeshProUGUI[] tmps = new TextMeshProUGUI[3];

    private void Awake()
    {
        Instance = this;
    }

    public void SetCommond(CommondData d, int i)
    {
        datas[i] = d;
        tmps[i].text = d.btnText;
        btms[i].onClick.RemoveAllListeners();
        btms[i].onClick.AddListener(() =>
        {
            if (datas[i] == null) return;
            StartCoroutine(Exe(datas[i].type));
        });
    }
    public void Init()
    {
        for (int i = 0; i < datas.Length; i++)
        {
            int index = i; // 👈 これが超重要！
            tmps[index].text = datas[index].btnText;
            btms[index].onClick.RemoveAllListeners();
            btms[index].onClick.AddListener(() =>
            {
                if (datas[index] == null) return;
                StartCoroutine(Exe(datas[index].type));
            });
        }
    }
    public IEnumerator Exe(CommondType ct)
    {
        switch (ct)
        {
            case CommondType.None:
                StartCoroutine(NoneExe());
                break;

            case CommondType.LeftTurn:
                StartCoroutine(LeftTurnExe());
                break;

            case CommondType.Walk:
                StartCoroutine(WalkExe());
                break;

            case CommondType.RightTurn:
                StartCoroutine(RightTurnExe());
                break;

            case CommondType.Notch:
                StartCoroutine(NotchExe());
                break;

            case CommondType.GoHome:
                StartCoroutine(GoHomeExe());
                break;

            case CommondType.Sleep:
                StartCoroutine(SleepExe());
                break;
            case CommondType.Diary:
                StartCoroutine(DiaryExe());
                break;
            default:
                yield break;
        }
    }
    IEnumerator NoneExe()
    {
        yield break;
    }

    IEnumerator LeftTurnExe()
    {
        yield return CurtainManager.Instance.FrontFlow("Now Turning..", "");
        GameData.Instance.AddLeftTurn();
        MainDisplay.Instance.UpdateDisplay();
        yield return CurtainManager.Instance.BackFlow("Now Turned.", "");
    }
    IEnumerator WalkExe()
    {
        yield return CurtainManager.Instance.FrontFlow("Now Walking..", "");
        GameData.Instance.AddWalk();
        MainDisplay.Instance.UpdateDisplay();
        yield return CurtainManager.Instance.BackFlow("Now Walked.", "");
    }
    IEnumerator RightTurnExe()
    {
        yield return CurtainManager.Instance.FrontFlow("Now Turning..", "");
        GameData.Instance.AddRightTurn();
        MainDisplay.Instance.UpdateDisplay();
        yield return CurtainManager.Instance.BackFlow("Now Turned.", "");
    }
    IEnumerator NotchExe()
    {
        GameData.Instance.AddNotch();
        MainDisplay.Instance.UpdateDisplay();
        yield break;
    }
    IEnumerator GoHomeExe()
    {
        yield return CurtainManager.Instance.FrontFlow("Now Walking..", "");
        GameData.Instance.AddWalk();
        GameData.Instance.EndTime = DateTime.Now;

        if (NameManager.Instance.Name != null) 
        {
            yield return FirebaseManager.Instance.AddClearRecordCoroutine();
            Debug.Log("GoHomeExe: Name exists, added clear record.");
        }

        MainDisplay.Instance.UpdateDisplay();
        yield return CurtainManager.Instance.BackFlow("Now Walked.", "");
    }
    IEnumerator SleepExe()
    {
       
        if (NameManager.Instance.Name == null)
        {
            yield return NameInputManager.Instance.InputName();
            yield return CurtainManager.Instance.MiddleText("now sleeping..", "");
            yield return FirebaseManager.Instance.AddClearRecordCoroutine();
        }
        else
        {
            yield return CurtainManager.Instance.FrontFlow("now sleeping..", "");
        }
            

        NightSession.Instance.CurrentSize++;
        yield return CurtainManager.Instance.GroupStayBackFlow("now sleeped..", "");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Night");
    }
    IEnumerator DiaryExe()
    {
        yield return DiaryManager.Instance.Open();
    }

}
