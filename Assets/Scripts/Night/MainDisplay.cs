using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainDisplay : MonoBehaviour
{
    public static MainDisplay Instance;
    [SerializeField] TextMeshProUGUI nightTMP;
    [SerializeField] Image mainVisual;
    [SerializeField] TextMeshProUGUI notchTMP;
    [SerializeField] GameObject mainTextBox;
    [SerializeField] TextMeshProUGUI mainTMP;
    [SerializeField] TextMeshProUGUI walkTMP;
    [SerializeField] TextMeshProUGUI turnTMP;
    [SerializeField] TextMeshProUGUI timeTMP;



    [SerializeField] GameObject resultWindow;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateTime();
    }

    void UpdateTime()
    {
        TimeSpan elapsed;
        if (GameData.Instance.EndTime != null)
        {
            elapsed = (DateTime)GameData.Instance.EndTime-GameData.Instance.StartTime;
        }
        else
        {
            elapsed = DateTime.Now - GameData.Instance.StartTime;
        }
        int hours = (int)elapsed.TotalHours;    // 通算時間の整数部分
        int minutes = elapsed.Minutes;          // 0～59
        int seconds = elapsed.Seconds;          // 0～59
        timeTMP.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }
    public void UpdateNight()
    {
        nightTMP.text = $"Night:{NightSession.Instance.CurrentSize.ToString()}";
    }
    public void UpdateBottomCount()
    {
        walkTMP.text = $"Walk:{GameData.Instance.WalkCount}";
        turnTMP.text = $"Turn:{GameData.Instance.TurnCount}";
    }


    public void UpdateDisplay()
    {
        if (GameData.Instance.EndTime == null)
        {
            UpdateInGame();
        }
        else
        {
            if (ParsonalManager.Instance.Name != null)
            {
                UpdateInHome();
            }
            else
            {
                UpdateNoNameInHome();
            }
        }

    }
    void UpdateInGame()
    {
        // 1. 次の位置を計算
        var nextPos = GameData.Instance.NextPos;
        var nextTile = GameData.Instance.Map[nextPos];

        // 2. MainVisualをUIに反映
        mainVisual.sprite = nextTile.GetMainVisual();

        // 2.5. ノッチ表示
        if (nextTile is TreeTile tree && tree.GetFrontMark() != 0)
        {
            notchTMP.text = tree.GetFrontMark().ToString();
        }
        else
        {
            notchTMP.text = "";
        }

        // 3. MainTextBoxに反映
        mainTMP.text = nextTile.GetMainText();

        // 4. 中央コマンドを反映
        var command = nextTile.GetCommond();
        MainCommondsManager.Instance.SetCommond(command, 1);

        // 5. MainCounts
        UpdateBottomCount();

        // 5. デバッグ用
        DebugMapManager.Instance.UpdateOverlay();
    }
    public void UpdateInHome()
    {

        mainVisual.sprite = AssetManager.Instance.Visual.InHome;

        mainTextBox.SetActive(false);

        // 4. コマンドを反映
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.Diary, 0);
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.None, 1);
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.Sleep, 2);

        // 5. MainCounts
        UpdateBottomCount();

        // 6.firebase送信

        // 7. デバッグ用
        DebugMapManager.Instance.UpdateOverlay();

        resultWindow.SetActive(true);
        ResultMapManager.Instance.CreateBaseGrid();
        StartCoroutine(ResultMapManager.Instance.PlayReplay());
    }
    public void UpdateNoNameInHome()
    {

        mainVisual.sprite = AssetManager.Instance.Visual.InHome;

        mainTextBox.SetActive(false);

        // 4. コマンドを反映
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.None, 0);
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.None, 1);
        MainCommondsManager.Instance.SetCommond(AssetManager.Instance.Commond.Sleep, 2);

        // 5. MainCounts
        UpdateBottomCount();

        // 6.firebase送信はSleepで

        // 7. デバッグ用
        DebugMapManager.Instance.UpdateOverlay();

        resultWindow.SetActive(true);
        ResultMapManager.Instance.CreateBaseGrid();
        StartCoroutine(ResultMapManager.Instance.PlayReplay());

    }
}
