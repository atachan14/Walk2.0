using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputManager : MonoBehaviour
{
    public static NameInputManager Instance;

    [SerializeField] GameObject NameInputPanel;
    [SerializeField] TMP_InputField NameField;
    [SerializeField] Button OkButton;

    private void Awake()
    {
        Instance = this;
    }
    public IEnumerator InputName()
    {
        yield return CurtainManager.Instance.FrontFlow("ていうか、\n\nぼくの名前はなんだっけ？", "");
        yield return CurtainManager.Instance.GroupStayTapAnywhere();

        // 入力UI表示
        NameInputPanel.SetActive(true);

        // 入力欄をフォーカス → スマホIMEが出る
        NameField.text = "";
        NameField.ActivateInputField();
        NameField.Select();

        // 決定押すまで待つ
        bool decided = false;
        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(() => decided = true);

        yield return new WaitUntil(() => decided);

        // 入力値取得
        string input = NameField.text.Trim();
        if (string.IsNullOrEmpty(input))
            input = "noname"; // 空の場合の適当救済

        NameManager.Instance.Name = input;

        // 保存
        var task = FirebaseManager.Instance.SetName(NameManager.Instance.Name);
        yield return new WaitUntil(() => task.IsCompleted);

        // UI閉じる
        NameInputPanel.SetActive(false);

        yield return CurtainManager.Instance.MiddleText($"{NameManager.Instance.Name} memorizing..", "");
        yield return CurtainManager.Instance.GroupStayBackFlow($"{NameManager.Instance.Name} memorized..", "");
    }
}
