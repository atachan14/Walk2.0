using System.Collections;
using System.Collections.Generic;
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


        string input = null;

        while (true)
        {
            // 入力UI表示
            NameInputPanel.SetActive(true);

            NameField.text = "";
            NameField.ActivateInputField();
            NameField.Select();

            bool decided = false;
            OkButton.onClick.RemoveAllListeners();
            OkButton.onClick.AddListener(() => decided = true);

            // 決定待ち
            yield return new WaitUntil(() => decided);

            input = NameField.text.Trim();

            // チェック
            string error = ValidateName(input);
            if (error == null)
            {
                // OK
                break;
            }

            // NG → エラー表示して再入力
            yield return CurtainManager.Instance.MiddleText(error, "");
            NameInputPanel.SetActive(false);
        }

        // 通過してきた名前を採用
        ParsonalManager.Instance.Name = input;

        // 保存
        var task = FirebaseManager.Instance.SetName(input);
        yield return new WaitUntil(() => task.IsCompleted);

        // UI閉じる
        NameInputPanel.SetActive(false);

        yield return CurtainManager.Instance.MiddleText($"{input} memorizing..", "");
        yield return CurtainManager.Instance.GroupStayBackFlow($"{input} memorized..", "");
    }
    private string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "名前が空っぽなんだけど？";

        if (name.Length < 1)
            return "短すぎ。1文字以上のはず";

        if (name.Length > 10)
            return "長すぎ。10文字以内のはず";

        //if (existingNames.Contains(name))
        //    return "その名前もう使われてるし？かぶってんよ？";

        return null; // OK
    }
}
