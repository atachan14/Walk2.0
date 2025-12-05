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
            NameInputPanel.SetActive(true);

            NameField.text = "";
            NameField.ActivateInputField();
            NameField.Select();

            bool decided = false;
            OkButton.onClick.RemoveAllListeners();
            OkButton.onClick.AddListener(() => decided = true);

            yield return new WaitUntil(() => decided);

            input = NameField.text.Trim();

            // --- ローカルチェック ---
            string error = ValidateNameLocal(input);
            if (error != null)
            {
                yield return CurtainManager.Instance.MiddleText(error, "");
                NameInputPanel.SetActive(false);
                continue; // もっかい
            }

            // --- Firebase チェック（非同期） ---
            var checkTask = FirebaseManager.Instance.IsNameAlreadyUsed(input);
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result)
            {
                yield return CurtainManager.Instance.MiddleText("その名前もう使われてるし？他のにして？", "");
                NameInputPanel.SetActive(false);
                continue;
            }

            // 全チェック通過
            break;
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
    private string ValidateNameLocal(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "名前が空っぽなんだけど？";

        if (name.Length < 1)
            return "短すぎ。1文字以上のはず。";

        if (name.Length > 10)
            return "長すぎ。10文字以内のはず。";

        return null; // ローカルチェックOK
    }

}
