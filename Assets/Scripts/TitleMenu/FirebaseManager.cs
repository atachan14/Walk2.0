using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GameData;

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
        var task = AddClearRecord();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
    }
    public async Task AddClearRecord()
    {
        try
        {
            var elapsed = (DateTime)GameData.Instance.EndTime - GameData.Instance.StartTime;
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
    // コルーチン用ラッパー
    public IEnumerator SetSaveDataCoroutine()
    {
        var task = SetSaveData();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("SetSaveData コルーチン失敗: " + task.Exception);
            throw task.Exception;
        }
        Debug.Log("SetSaveData コルーチン成功");
    }

    public IEnumerator ClearSaveDataCoroutine()
    {
        var task = ClearSaveData();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("ClearSaveData コルーチン失敗: " + task.Exception);
            throw task.Exception;
        }
        Debug.Log("ClearSaveData コルーチン成功");
    }
    public async Task SetSaveData()
    {
        try
        {
            var uid = SystemInfo.deviceUniqueIdentifier;

            var mapList = BuildMapList();   // あとで実装
            var stepList = BuildStepList();   // あとで実装

            var data = new Dictionary<string, object>
        {
            { "mapSize", NightSession.Instance.CurrentSize },
            { "walkCount",GameData.Instance.WalkCount},
            { "turnCount",GameData.Instance.TurnCount},
            { "notchCount", GameData.Instance.NotchCount},
            { "startTime", Timestamp.FromDateTime(GameData.Instance.StartTime.ToUniversalTime()) },
            { "map", mapList },
            { "steps", stepList }
        };

            if (GameData.Instance.EndTime != null)
            {
                data["endTime"] = Timestamp.FromDateTime(GameData.Instance.EndTime.Value.ToUniversalTime());
            }

            var doc = db.Collection("SaveData").Document(uid);
            await doc.SetAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError("SaveData保存失敗: " + e);
            throw;
        }
    }
    public async Task ClearSaveData()
    {
        try
        {
            var uid = SystemInfo.deviceUniqueIdentifier;
            var doc = db.Collection("SaveData").Document(uid);

            await doc.DeleteAsync();
            Debug.Log("SaveData ドキュメント削除完了");
        }
        catch (Exception e)
        {
            Debug.LogError("SaveData削除失敗: " + e);
            throw;
        }
    }

    private List<object> BuildStepList()
    {
        var list = new List<object>();

        foreach (var step in GameData.Instance.PathSteps)
        {
            // notchData がある場合だけ辞書を作る
            object notchDict = null;
            if (step.notchData.HasValue)
            {
                var nd = step.notchData.Value;
                notchDict = new Dictionary<string, object>
            {
                { "count", nd.count },
                { "x", nd.pos.x },
                { "y", nd.pos.y },
                { "dir", nd.dir }
            };
            }

            // step 自体の辞書
            var dict = new Dictionary<string, object>
        {
            { "x", step.pos.x },
            { "y", step.pos.y },
            { "dir", step.dir },
            { "notch", notchDict }   // null なら null のまま保存される
        };

            list.Add(dict);
        }
        return list;
    }
    private List<object> BuildMapList()
    {
        var list = new List<object>();

        foreach (var kvp in GameData.Instance.Map)
        {
            Pos pos = kvp.Key;
            Tile tile = kvp.Value;

            var dict = new Dictionary<string, object>
        {
            { "x", pos.x },
            { "y", pos.y },
            { "type", tile.GetType().Name }   // "Tile" / "TreeTile"
        };

            // TreeTile なら marks を追加
            if (tile is TreeTile tree)
            {
                dict["marks"] = new List<int>(tree.marks);
            }

            list.Add(dict);
        }

        return list;
    }
    public IEnumerator LoadSaveDataCoroutine(Action<bool> callback)
    {
        var task = LoadSaveData();

        // 完了を待つ
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError("LoadSaveData 失敗: " + task.Exception);
            callback?.Invoke(false);
        }
        else
        {
            callback?.Invoke(task.Result);
        }
    }
    public async Task<bool> LoadSaveData()
    {
        var uid = SystemInfo.deviceUniqueIdentifier;
        var doc = db.Collection("SaveData").Document(uid);

        var snap = await doc.GetSnapshotAsync();
        if (!snap.Exists)
        {
            return false; // セーブデータ無し
        }

        // map の生データ取得
        var rawMap = snap.GetValue<List<object>>("map");
        var rawStep = snap.GetValue<List<object>>("steps");

        // 復元実行
        var restoredMap = BuildMapFromList(rawMap);
        var restoredSteps = BuildStepsFromList(rawStep);

        GameData.Instance.Map = restoredMap;
        GameData.Instance.PathSteps = restoredSteps;
        GameData.Instance.WalkCount = snap.GetValue<int>("walkCount");
        GameData.Instance.TurnCount = snap.GetValue<int>("turnCount");
        GameData.Instance.NotchCount = snap.GetValue<int>("notchCount");
        GameData.Instance.StartTime = snap.GetValue<Timestamp>("startTime").ToDateTime().ToLocalTime();
        if (snap.ContainsField("endTime"))
        {
            GameData.Instance.EndTime = snap.GetValue<Timestamp>("endTime").ToDateTime();
        }
        else
        {
            GameData.Instance.EndTime = null;
        }
#if UNITY_EDITOR
        DebugMapManager.Instance.Setup();
#endif

        return true;
    }

    private Dictionary<Pos, Tile> BuildMapFromList(List<object> rawList)
    {
        var map = new Dictionary<Pos, Tile>();

        foreach (var raw in rawList)
        {
            // Firestore から来るのは Dictionary<string, object>
            var dict = raw as Dictionary<string, object>;
            if (dict == null) continue;

            int x = Convert.ToInt32(dict["x"]);
            int y = Convert.ToInt32(dict["y"]);
            string type = dict["type"].ToString();

            Tile tile;

            switch (type)
            {
                case "NoneTile":
                    {
                        tile = new NoneTile();
                        break;
                    }
                case "TreeTile":
                    {
                        var tree = new TreeTile();

                        if (dict.TryGetValue("marks", out var marksObj)
                            && marksObj is List<object> marksRaw)
                        {
                            // Firestore の int は long だったりするので変換注意
                            for (int i = 0; i < tree.marks.Length && i < marksRaw.Count; i++)
                            {
                                tree.marks[i] = Convert.ToInt32(marksRaw[i]);
                            }
                        }

                        tile = tree;
                        break;
                    }
                case "HomeTile":
                    {
                        tile = new HomeTile();
                        break;
                    }
                default:
                    tile = new NoneTile(); // 不明なタイプは NoneTile にフォールバック
                    Debug.LogWarning($"不明なタイルタイプ: {type} at ({x}, {y})");
                    break;
            }

            var pos = new Pos { x = x, y = y };
            map[pos] = tile;
        }

        return map;
    }
    public List<PathStep> BuildStepsFromList(List<object> rawList)
    {
        var steps = new List<PathStep>();

        if (rawList == null) return steps;

        foreach (var item in rawList)
        {
            var dict = item as Dictionary<string, object>;
            if (dict == null) continue;

            // pos と dir（存在しないと例外が出るので要チェック）
            int x = dict.ContainsKey("x") ? Convert.ToInt32(dict["x"]) : 0;
            int y = dict.ContainsKey("y") ? Convert.ToInt32(dict["y"]) : 0;
            int dir = dict.ContainsKey("dir") ? Convert.ToInt32(dict["dir"]) : 0;

            var ps = new PathStep
            {
                pos = new Pos { x = x, y = y },
                dir = dir,
                notchData = null
            };

            // notch が存在してかつ null じゃなければ復元
            if (dict.TryGetValue("notch", out var notchObj) && notchObj != null)
            {
                var notchDict = notchObj as Dictionary<string, object>;
                if (notchDict != null)
                {
                    int ncount = notchDict.ContainsKey("count") ? Convert.ToInt32(notchDict["count"]) : 0;
                    int nx = notchDict.ContainsKey("x") ? Convert.ToInt32(notchDict["x"]) : 0;
                    int ny = notchDict.ContainsKey("y") ? Convert.ToInt32(notchDict["y"]) : 0;
                    int ndir = notchDict.ContainsKey("dir") ? Convert.ToInt32(notchDict["dir"]) : 0;

                    var nd = new NotchData
                    {
                        count = ncount,
                        pos = new Pos { x = nx, y = ny },
                        dir = ndir
                    };

                    ps.notchData = nd; // nullable に boxing される
                }
            }

            steps.Add(ps);
        }

        return steps;
    }


}