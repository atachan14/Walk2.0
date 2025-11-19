using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    FirebaseFirestore db;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 二重召喚を即処刑
            return;
        }
        Instance = this;
        db = FirebaseFirestore.DefaultInstance;
    }

    // 名前取得（null = 未設定）
    public async Task<string> GetName()
    {
        var uid = SystemInfo.deviceUniqueIdentifier;

        var doc = db.Collection("ParsonalData").Document(uid);
        var snap = await doc.GetSnapshotAsync();

        if (!snap.Exists || !snap.ContainsField("name"))
            return null;

        return snap.GetValue<string>("name");
    }

    public async Task SetName(string name)
    {
        var uid = SystemInfo.deviceUniqueIdentifier;

        var doc = db.Collection("ParsonalData").Document(uid);

        var data = new
        {
            name,
            created = DateTime.Now
        };

        await doc.SetAsync(data, SetOptions.MergeAll);
    }

    // クリア記録追加
    public IEnumerator AddClearRecordCoroutine()
    {
        Debug.Log("AddClearRecordCoroutine Start");
        var task = AddClearRecord();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
    }
    public async Task AddClearRecord()
    {
        Debug.Log("AddClearRecord Start");
        try
        {
            var elapsed = DateTime.Now - GameData.Instance.StartTime;
            long timeSec = (long)elapsed.TotalSeconds;

            var data = new Dictionary<string, object>
        {
            { "uid",  SystemInfo.deviceUniqueIdentifier},
            { "name", NameManager.Instance.Name },
            { "mapSize", NightSession.Instance.CurrentSize },
            { "walkCount", GameData.Instance.WalkCount },
            { "turnCount", GameData.Instance.TurnCount },
            { "timeSec", timeSec },
            { "startTime", Timestamp.FromDateTime(GameData.Instance.StartTime.ToUniversalTime()) }
        };

            await db.Collection("ClearRecords").AddAsync(data);
            Debug.Log("ClearRecord保存成功");
        }
        catch (Exception e)
        {
            Debug.LogError("ClearRecord保存失敗: " + e);
            throw; // 呼び出し元にも知らせたいならそのまま再スロー
        }
    }

}