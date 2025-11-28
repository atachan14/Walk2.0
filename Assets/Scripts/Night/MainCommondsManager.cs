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
            StartCoroutine(Exe(datas[i]));
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
                StartCoroutine(Exe(datas[index]));
            });
        }
    }
    public IEnumerator Exe(CommondData cd)
    {
        switch (cd.type)
        {
            case CommondType.None:
                yield return NoneExe();
                break;

            case CommondType.LeftTurn:
                yield return LeftTurnExe(); break;

            case CommondType.Walk:
                yield return WalkExe();
                break;

            case CommondType.RightTurn:
                yield return RightTurnExe();
                break;

            case CommondType.Notch:
                yield return NotchExe();
                break;

            case CommondType.GoHome:
                yield return GoHomeExe();
                break;

            case CommondType.Sleep:
                yield return SleepExe();
                break;
            case CommondType.Diary:
                yield return DiaryExe();
                break;
            default:
                yield break;
        }
        if (!cd.dontSave)
        {
            yield return FirebaseManager.Instance.SetSaveDataCoroutine();
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

        if (ParsonalManager.Instance.Name != null)
        {
            yield return FirebaseManager.Instance.AddClearRecordCoroutine();
        }

        MainDisplay.Instance.UpdateDisplay();
        yield return CurtainManager.Instance.BackFlow("Now Walked.", "");
    }
    IEnumerator SleepExe()
    {

        if (ParsonalManager.Instance.Name == null)
        {
            yield return NameInputManager.Instance.InputName();
            yield return CurtainManager.Instance.MiddleText("now sleeping..", "");
            yield return FirebaseManager.Instance.AddClearRecordCoroutine();

        }
        else
        {
            yield return CurtainManager.Instance.FrontFlow("now sleeping..", "");
        }

        yield return FirebaseManager.Instance.ClearSaveDataCoroutine();
        NightSession.Instance.CurrentSize = ParsonalManager.Instance.MaxSize + 1;
        yield return CurtainManager.Instance.GroupStayBackFlow("now sleeped..", "");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Night");
    }
    IEnumerator DiaryExe()
    {
        yield return DieryManager.Instance.OpenDiery();
    }

}
