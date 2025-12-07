using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TabType
{
    my, every, night
}
public enum SortType
{
    size, walk, turn, time, date
}
public readonly struct ClearRecord
{
    public string Uid { get; }
    public int Size { get; }
    public string UserName { get; }
    public int Walk { get; }
    public int Turn { get; }
    public long Time { get; }
    public DateTime Date { get; }

    public ClearRecord(
        string uid,
        int size,
        string userName,
        int walk,
        int turn,
        long time,
        DateTime date)
    {
        Uid = uid;
        Size = size;
        UserName = userName;
        Walk = walk;
        Turn = turn;
        Time = time;
        Date = date;
    }
}
public class DieryManager : MonoBehaviour
{
    public static DieryManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public CanvasGroup DieryUI;
    public Image DieryBG;
    public TextMeshProUGUI myTabText;
    public Color myColor;
    public Color everyColor;
    public Color mapColor;

    public List<DieryRow> DieryRows = new();
    public List<ClearRecord> AllClearRecords;
    public List<ClearRecord> SelectedClearRecords;

    TabType currentTab = TabType.my;
    int currentSize = 0;
    SortType currentSort = SortType.date;
    bool isName = false;

    int currentPage = 0;
    int pageRows = 10;
    public TextMeshProUGUI pageText;

    // nightTab
    public GameObject nightTab;
    public TMP_InputField currentSizeField;
    public Button currentSizeDown;
    public Button currentSizeUp;


    public void OnMyTabButton() => CurrentTab = TabType.my;
    public void OnEveryTabButton() => CurrentTab = TabType.every;
    public void OnMapTabButton() => CurrentTab = TabType.night;
    public void OnSizeButton() => CurrentSort = SortType.size;
    public void OnWalkButton() => CurrentSort = SortType.walk;
    public void OnTurnButton() => CurrentSort = SortType.turn;
    public void OnTimeButton() => CurrentSort = SortType.time;
    public void OnDateButton() => CurrentSort = SortType.date;
    public void OnNameToggle() => IsName = !IsName;
    public void OnSizeSelect(int size)
    {
        currentSize = size;
        Debug.Log($"Selected size: {currentSize}");
        CurrentTab = TabType.night;
    }
    public void OnNextPageButton() => NextPage();
    public void OnPrevPageButton() => PrevPage();

    // nightTab
    public void OnCurrentSizeFieldChanged(string text)
    {
        if (int.TryParse(currentSizeField.text, out int size))
        {
            currentSize = size;
            CurrentSizeCheck();
            ResetPage();
            UpdateDiery();
        }
    }
    public void OnCurrentSizeDownButton()
    {
        currentSize = currentSize - 1;
        currentSizeField.text = currentSize.ToString();
        CurrentSizeCheck();
        ResetPage();
        UpdateDiery();
    }
    public void OnCurrentSizeUpButton()
    {
        currentSize = currentSize + 1;
        currentSizeField.text = currentSize.ToString();
        CurrentSizeCheck();
        ResetPage();
        UpdateDiery();
    }

    void CurrentSizeCheck()
    {
        currentSizeDown.interactable = currentSize > 3;
    }
    public void OnPlayCurrentSizeButton()
    {
        StartCoroutine(PlayCurrentSizeCoroutine());
    }
    IEnumerator PlayCurrentSizeCoroutine()
    { 
        yield return CurtainManager.Instance.FrontFlow("now sleeping..", "");
        yield return FirebaseManager.Instance.ClearSaveDataCoroutine();
        NightSession.Instance.CurrentSize = currentSize;
        yield return CurtainManager.Instance.GroupStayBackFlow("now sleeped..", "");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Night");
    }

    int MaxPage
    {
        get
        {
            if (SelectedClearRecords == null || SelectedClearRecords.Count == 0)
            {
                return 0;
            }
            return (SelectedClearRecords.Count - 1) / pageRows;
        }
    }


    public TabType CurrentTab
    {
        get { return currentTab; }
        set
        {
            if (currentTab != value)
            {
                currentTab = value;
                ResetPage();
                UpdateDiery();
            }
        }
    }
    public bool IsName
    {
        get { return isName; }
        set
        {
            if (isName != value)
            {
                isName = value;
                ResetPage();
                UpdateDiery();
            }
        }
    }
    public SortType CurrentSort
    {
        get { return currentSort; }
        set
        {
            if (currentSort != value)
            {
                currentSort = value;
                ResetPage();
                UpdateDiery();
            }
        }
    }
    public IEnumerator OpenDiery()
    {
        yield return FirebaseManager.Instance.LoadClearRecordsCoroutine();
        DieryUI.alpha = 1;
        DieryUI.blocksRaycasts = true;
        DieryUI.interactable = true;

        // 自分の最新サイズを取得
        currentSize = AllClearRecords
            .Where(r => r.Uid == SystemInfo.deviceUniqueIdentifier) // 自分の記録だけ抽出
            .OrderByDescending(r => r.Date)                         // 日付降順にソート
            .Select(r => r.Size)                                    // サイズだけ取り出す
            .FirstOrDefault();                                      // 最新のサイズ、なければ 0

        ResetPage();
        UpdateDiery();
    }

    public void CloseDiery()
    {
        DieryUI.alpha = 0;
        DieryUI.blocksRaycasts = false;
        DieryUI.interactable = false;
    }

    void UpdateDiery()
    {
        if (AllClearRecords == null || AllClearRecords.Count == 0)
        {
            Debug.Log("No Clear Records");
            return;
        }

        // ① タブでフィルタリング
        SelectedClearRecords = new(AllClearRecords);

        switch (currentTab)
        {
            case TabType.my:
                myTabText.text = $"{ParsonalManager.Instance.Name}\nの記録";
                SelectedClearRecords = SelectedClearRecords.FindAll(
                    x => x.Uid == SystemInfo.deviceUniqueIdentifier);
                DieryBG.color = myColor;
                nightTab.SetActive(false);
                break;


            case TabType.every:
                DieryBG.color = everyColor;
                nightTab.SetActive(false);
                break;

            case TabType.night:
                SelectedClearRecords = SelectedClearRecords.FindAll(
                    x => x.Size == currentSize);
                DieryBG.color = mapColor;
                nightTab.SetActive(true);
                currentSizeField.text = currentSize.ToString();
                break;

        }

        // ② ソート
        SelectedClearRecords.Sort(currentSort switch
        {
            SortType.size => (a, b) => b.Size.CompareTo(a.Size),
            SortType.walk => (a, b) => a.Walk.CompareTo(b.Walk),
            SortType.turn => (a, b) => a.Turn.CompareTo(b.Turn),
            SortType.time => (a, b) => a.Time.CompareTo(b.Time),
            SortType.date => (a, b) => b.Date.CompareTo(a.Date),
            _ => (a, b) => 0
        });

        // ③ 同 uid の 2 件目以降を削除（isName フラグ）
        if (isName)
        {
            HashSet<string> seen = new();
            SelectedClearRecords = SelectedClearRecords.FindAll(r =>
            {
                if (seen.Contains(r.Uid)) return false;
                seen.Add(r.Uid);
                return true;
            });
        }

        // ④ ページング（ここ超重要）
        int start = currentPage * pageRows;
        int end = Mathf.Min(start + pageRows, SelectedClearRecords.Count);
        pageText.text = $"{currentPage + 1} / {MaxPage + 1}";

        // ⑤ UI 更新
        for (int i = 0; i < DieryRows.Count; i++)
        {
            int idx = start + i;

            if (idx < end)
            {
                DieryRows[i].Set(SelectedClearRecords[idx]);
            }
            else
            {
                DieryRows[i].Hide();
            }
        }
    }

    public void NextPage()
    {
        if (currentPage < MaxPage)
        {
            currentPage++;
            UpdateDiery();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateDiery();
        }
    }

    public void ResetPage()
    {
        currentPage = 0;
    }



}
