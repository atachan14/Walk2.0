using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public struct ParsonalDataResult
{
    public string name;
    public int maxSize;

    public ParsonalDataResult(string name, int maxSize)
    {
        this.name = name;
        this.maxSize = maxSize;
    }
}
public class ParsonalManager : MonoBehaviour
{
    public static ParsonalManager Instance;
    public string Name { get; set; } = null;
    public int MaxSize { get; set; } = 0;

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
        StartCoroutine(TryGetParsonalData());
    }

    private IEnumerator TryGetParsonalData()
    {
        var task = FirebaseManager.Instance.GetParsonalData();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("パーソナルデータ取得でエラー：" + task.Exception);
            Name = null;
            MaxSize = 0;
            yield break;
        }

        var result = task.Result;
        Name = result.name;
        MaxSize = result.maxSize;

        Debug.Log($"ParsonalManager: Name={Name ?? "null"}, MaxSize={MaxSize}");
    }


}
