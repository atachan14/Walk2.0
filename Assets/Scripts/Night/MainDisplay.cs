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
    public void UpdateNight()
    {
        nightTMP.text = $"Night:{NightSession.Instance.CurrentSize.ToString()}";
    }
    public void UpdateBottomCount()
    {
        walkTMP.text = $"Walk:{GameData.Instance.WalkCount}";
        turnTMP.text = $"Turn:{GameData.Instance.TurnCount}";
        timeTMP.text = $"Time:未実装";
    }


    public void UpdateDisplay()
    {
        if (!GameData.Instance.InHome)
        {
            UpdateInGame();
        }
        else
        {
            if (NameManager.Instance.Name != null)
            {
                UpdateInHome();
                Debug.Log("名前ありのホーム表示");
            }
            else
            {
                UpdateNoNameInHome();
                Debug.Log("名前なしのホーム表示");
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
