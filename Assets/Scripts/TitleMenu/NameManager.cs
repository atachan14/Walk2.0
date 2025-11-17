using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NameManager : MonoBehaviour
{
    public static NameManager Instance;
    public string Name { get; set; } = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 二重召喚を即処刑
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(TryGetName());
    }

    private IEnumerator TryGetName()
    {
        // Firebase側に問い合わせ
        var task = FirebaseManager.Instance.GetName();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("名前取得でエラー出た：" + task.Exception);
            Name = null;
            yield break;
        }

        // 名前が存在しない場合は null が返る
        Name = task.Result;

        Debug.Log("NameManager: 取得した名前 → " + (Name ?? "null"));
    }

    
}
