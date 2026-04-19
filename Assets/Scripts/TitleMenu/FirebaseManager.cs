using Firebase;
using Firebase.Auth;
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

    const string ParsonalDataCollection = "ParsonalData";
    const string ClearRecordsCollection = "ClearRecords";
    const string SaveDataCollection = "SaveData";

    FirebaseFirestore db;
    FirebaseAuth auth;
    Task initializationTask;

    public string CurrentUid => auth?.CurrentUser?.UserId;
    string LegacyDeviceId => SystemInfo.deviceUniqueIdentifier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        initializationTask = InitializeAsync();
    }

    public IEnumerator EnsureReadyCoroutine()
    {
        var task = EnsureReadyAsync();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            throw task.Exception;
    }

    public async Task EnsureReadyAsync()
    {
        initializationTask ??= InitializeAsync();
        await initializationTask;
        await SignInAnonymouslyIfNeededAsync();
    }

    public bool IsCurrentUserId(string uid)
    {
        if (string.IsNullOrEmpty(uid))
            return false;

        return uid == CurrentUid || uid == LegacyDeviceId;
    }

    async Task InitializeAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
            throw new Exception($"Firebase dependencies are not available: {dependencyStatus}");

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        await SignInAnonymouslyIfNeededAsync();
    }

    async Task SignInAnonymouslyIfNeededAsync()
    {
        if (auth == null)
            auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
            return;

        AuthResult result = await auth.SignInAnonymouslyAsync();
        if (result?.User == null)
            throw new Exception("Firebase anonymous auth returned no user.");
    }

    string RequireCurrentUid()
    {
        string uid = CurrentUid;
        if (string.IsNullOrEmpty(uid))
            throw new InvalidOperationException("Firebase anonymous auth is not ready.");

        return uid;
    }

    DocumentReference ParsonalDataDoc(string uid) =>
        db.Collection(ParsonalDataCollection).Document(uid);

    DocumentReference SaveDataDoc(string uid) =>
        db.Collection(SaveDataCollection).Document(uid);

    async Task<DocumentSnapshot> GetCurrentOrLegacyDocAsync(DocumentReference currentDoc, DocumentReference legacyDoc)
    {
        var currentSnap = await currentDoc.GetSnapshotAsync();
        if (currentSnap.Exists || legacyDoc == null)
            return currentSnap;

        return await legacyDoc.GetSnapshotAsync();
    }

    ParsonalDataResult ConvertToParsonalDataResult(DocumentSnapshot snap)
    {
        string name = snap.ContainsField("name") ? snap.GetValue<string>("name") : null;
        int maxSize = snap.ContainsField("maxSize") ? snap.GetValue<int>("maxSize") : 0;
        return new ParsonalDataResult(name, maxSize);
    }

    async Task MigrateParsonalDataIfNeededAsync(string currentUid, string legacyUid, DocumentSnapshot legacySnap)
    {
        if (legacySnap == null || !legacySnap.Exists || currentUid == legacyUid)
            return;

        var data = new Dictionary<string, object>
        {
            { "maxSize", legacySnap.ContainsField("maxSize") ? legacySnap.GetValue<int>("maxSize") : 0 },
            { "migratedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
        };

        if (legacySnap.ContainsField("name"))
            data["name"] = legacySnap.GetValue<string>("name");

        if (legacySnap.ContainsField("created"))
            data["created"] = legacySnap.GetValue<Timestamp>("created");

        await ParsonalDataDoc(currentUid).SetAsync(data, SetOptions.MergeAll);
    }

    Dictionary<string, object> BuildSaveDataFromSnapshot(DocumentSnapshot snap)
    {
        var data = new Dictionary<string, object>
        {
            { "mapSize", snap.GetValue<int>("mapSize") },
            { "walkCount", snap.GetValue<int>("walkCount") },
            { "turnCount", snap.GetValue<int>("turnCount") },
            { "notchCount", snap.GetValue<int>("notchCount") },
            { "startTime", snap.GetValue<Timestamp>("startTime") },
            { "map", snap.GetValue<List<object>>("map") },
            { "steps", snap.GetValue<List<object>>("steps") },
            { "migratedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
        };

        if (snap.ContainsField("endTime"))
            data["endTime"] = snap.GetValue<Timestamp>("endTime");

        return data;
    }

    async Task<DocumentSnapshot> GetMigratedSaveDataSnapshotAsync(string currentUid, string legacyUid)
    {
        var currentDoc = SaveDataDoc(currentUid);
        var currentSnap = await currentDoc.GetSnapshotAsync();
        if (currentSnap.Exists || currentUid == legacyUid)
            return currentSnap;

        var legacyDoc = SaveDataDoc(legacyUid);
        var legacySnap = await legacyDoc.GetSnapshotAsync();
        if (!legacySnap.Exists)
            return legacySnap;

        await currentDoc.SetAsync(BuildSaveDataFromSnapshot(legacySnap));
        return legacySnap;
    }

    public async Task<ParsonalDataResult> GetParsonalData()
    {
        await EnsureReadyAsync();

        string currentUid = RequireCurrentUid();
        string legacyUid = LegacyDeviceId;

        var currentDoc = ParsonalDataDoc(currentUid);
        var legacyDoc = currentUid == legacyUid ? null : ParsonalDataDoc(legacyUid);
        var snap = await GetCurrentOrLegacyDocAsync(currentDoc, legacyDoc);

        if (!snap.Exists)
            return new ParsonalDataResult(null, 0);

        if (snap.Id == legacyUid && currentUid != legacyUid)
            await MigrateParsonalDataIfNeededAsync(currentUid, legacyUid, snap);

        return ConvertToParsonalDataResult(snap);
    }

    public async Task SetName(string name)
    {
        await EnsureReadyAsync();

        string currentUid = RequireCurrentUid();
        var doc = ParsonalDataDoc(currentUid);
        var existingSnap = await doc.GetSnapshotAsync();

        var data = new Dictionary<string, object>
        {
            { "name", name }
        };

        if (!existingSnap.Exists || !existingSnap.ContainsField("created"))
            data["created"] = Timestamp.FromDateTime(DateTime.UtcNow);

        await doc.SetAsync(data, SetOptions.MergeAll);
    }

    public async Task<bool> IsNameAlreadyUsed(string name)
    {
        await EnsureReadyAsync();

        var query = db.Collection(ParsonalDataCollection)
            .WhereEqualTo("name", name);

        var snapshot = await query.GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            if (!IsCurrentUserId(doc.Id))
                return true;
        }

        return false;
    }

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
        await EnsureReadyAsync();

        if (!GameData.Instance.EndTime.HasValue)
            throw new Exception("EndTime was null when ClearRecord was requested.");

        try
        {
            var elapsed = GameData.Instance.EndTime.Value - GameData.Instance.StartTime;
            long timeSec = (long)elapsed.TotalSeconds;

            var data = new Dictionary<string, object>
            {
                { "uid", RequireCurrentUid() },
                { "name", ParsonalManager.Instance.Name },
                { "mapSize", NightSession.Instance.CurrentSize },
                { "walkCount", GameData.Instance.WalkCount },
                { "turnCount", GameData.Instance.TurnCount },
                { "notchCount", GameData.Instance.NotchCount },
                { "timeSec", timeSec },
                { "endTime", Timestamp.FromDateTime(GameData.Instance.EndTime.Value.ToUniversalTime()) },
                { "startTime", Timestamp.FromDateTime(GameData.Instance.StartTime.ToUniversalTime()) }
            };

            await db.Collection(ClearRecordsCollection).AddAsync(data);
            Debug.Log("ClearRecord saved.");
        }
        catch (Exception e)
        {
            Debug.LogError("ClearRecord save failed: " + e);
            throw;
        }

        if (NightSession.Instance.CurrentSize > ParsonalManager.Instance.MaxSize)
        {
            ParsonalManager.Instance.MaxSize = NightSession.Instance.CurrentSize;

            await ParsonalDataDoc(RequireCurrentUid())
                .SetAsync(new Dictionary<string, object>
                {
                    { "maxSize", NightSession.Instance.CurrentSize }
                }, SetOptions.MergeAll);
        }
    }

    public IEnumerator LoadClearRecordsCoroutine()
    {
        var task = LoadAllClearRecords();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
    }

    public async Task LoadAllClearRecords()
    {
        await EnsureReadyAsync();

        try
        {
            QuerySnapshot snap = await db.Collection(ClearRecordsCollection).GetSnapshotAsync();
            List<ClearRecord> list = new();

            foreach (var doc in snap.Documents)
                list.Add(ConvertToClearRecord(doc));

            DieryManager.Instance.AllClearRecords = list;
            Debug.Log($"ClearRecord load success: {list.Count} rows.");
        }
        catch (Exception e)
        {
            Debug.LogError("ClearRecord load failed: " + e);
            throw;
        }
    }

    ClearRecord ConvertToClearRecord(DocumentSnapshot snap)
    {
        string uid = snap.ContainsField("uid") ? snap.GetValue<string>("uid") : "";
        int size = snap.ContainsField("mapSize") ? snap.GetValue<int>("mapSize") : 0;
        string name = snap.ContainsField("name") ? snap.GetValue<string>("name") : "";
        int walk = snap.ContainsField("walkCount") ? snap.GetValue<int>("walkCount") : 0;
        int turn = snap.ContainsField("turnCount") ? snap.GetValue<int>("turnCount") : 0;
        long time = snap.ContainsField("timeSec") ? snap.GetValue<long>("timeSec") : 0;

        DateTime date = DateTime.MinValue;
        if (snap.ContainsField("endTime"))
            date = snap.GetValue<Timestamp>("endTime").ToDateTime().ToLocalTime();

        return new ClearRecord(uid, size, name, walk, turn, time, date);
    }

    public IEnumerator SetSaveDataCoroutine()
    {
        var task = SetSaveData();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("SetSaveData coroutine failed: " + task.Exception);
            throw task.Exception;
        }

        Debug.Log("SetSaveData coroutine success.");
    }

    public IEnumerator ClearSaveDataCoroutine()
    {
        var task = ClearSaveData();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("ClearSaveData coroutine failed: " + task.Exception);
            throw task.Exception;
        }

        Debug.Log("ClearSaveData coroutine success.");
    }

    public async Task SetSaveData()
    {
        await EnsureReadyAsync();

        try
        {
            var data = new Dictionary<string, object>
            {
                { "mapSize", NightSession.Instance.CurrentSize },
                { "walkCount", GameData.Instance.WalkCount },
                { "turnCount", GameData.Instance.TurnCount },
                { "notchCount", GameData.Instance.NotchCount },
                { "startTime", Timestamp.FromDateTime(GameData.Instance.StartTime.ToUniversalTime()) },
                { "map", BuildMapList() },
                { "steps", BuildStepList() }
            };

            if (GameData.Instance.EndTime != null)
                data["endTime"] = Timestamp.FromDateTime(GameData.Instance.EndTime.Value.ToUniversalTime());

            await SaveDataDoc(RequireCurrentUid()).SetAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError("SaveData save failed: " + e);
            throw;
        }
    }

    public async Task ClearSaveData()
    {
        await EnsureReadyAsync();

        try
        {
            await SaveDataDoc(RequireCurrentUid()).DeleteAsync();
            Debug.Log("SaveData deleted.");
        }
        catch (Exception e)
        {
            Debug.LogError("SaveData delete failed: " + e);
            throw;
        }
    }

    List<object> BuildStepList()
    {
        var list = new List<object>();

        foreach (var step in GameData.Instance.PathSteps)
        {
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

            list.Add(new Dictionary<string, object>
            {
                { "x", step.pos.x },
                { "y", step.pos.y },
                { "dir", step.dir },
                { "notch", notchDict }
            });
        }

        return list;
    }

    List<object> BuildMapList()
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
                { "type", tile.GetType().Name }
            };

            if (tile is TreeTile tree)
                dict["marks"] = new List<int>(tree.marks);

            list.Add(dict);
        }

        return list;
    }

    public IEnumerator LoadSaveDataCoroutine(Action<bool> callback)
    {
        var task = LoadSaveData();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError("LoadSaveData failed: " + task.Exception);
            callback?.Invoke(false);
            yield break;
        }

        callback?.Invoke(task.Result);
    }

    public async Task<bool> LoadSaveData()
    {
        await EnsureReadyAsync();

        string currentUid = RequireCurrentUid();
        string legacyUid = LegacyDeviceId;
        var snap = await GetMigratedSaveDataSnapshotAsync(currentUid, legacyUid);
        if (!snap.Exists)
            return false;

        var rawMap = snap.GetValue<List<object>>("map");
        var rawStep = snap.GetValue<List<object>>("steps");

        GameData.Instance.Map = BuildMapFromList(rawMap);
        GameData.Instance.PathSteps = BuildStepsFromList(rawStep);
        NightSession.Instance.CurrentSize = snap.GetValue<int>("mapSize");
        GameData.Instance.WalkCount = snap.GetValue<int>("walkCount");
        GameData.Instance.TurnCount = snap.GetValue<int>("turnCount");
        GameData.Instance.NotchCount = snap.GetValue<int>("notchCount");
        GameData.Instance.StartTime = snap.GetValue<Timestamp>("startTime").ToDateTime().ToLocalTime();
        GameData.Instance.EndTime = snap.ContainsField("endTime")
            ? snap.GetValue<Timestamp>("endTime").ToDateTime().ToLocalTime()
            : null;

#if UNITY_EDITOR
        DebugMapManager.Instance.Setup();
#endif

        return true;
    }

    Dictionary<Pos, Tile> BuildMapFromList(List<object> rawList)
    {
        var map = new Dictionary<Pos, Tile>();

        foreach (var raw in rawList)
        {
            var dict = raw as Dictionary<string, object>;
            if (dict == null)
                continue;

            int x = Convert.ToInt32(dict["x"]);
            int y = Convert.ToInt32(dict["y"]);
            string type = dict["type"].ToString();

            Tile tile = type switch
            {
                "NoneTile" => new NoneTile(),
                "HomeTile" => new HomeTile(),
                "TreeTile" => BuildTreeTile(dict),
                _ => new NoneTile()
            };

            if (type != "NoneTile" && type != "HomeTile" && type != "TreeTile")
                Debug.LogWarning($"Unknown tile type: {type} at ({x}, {y})");

            map[new Pos { x = x, y = y }] = tile;
        }

        return map;
    }

    Tile BuildTreeTile(Dictionary<string, object> dict)
    {
        var tree = new TreeTile();
        if (dict.TryGetValue("marks", out var marksObj) && marksObj is List<object> marksRaw)
        {
            for (int i = 0; i < tree.marks.Length && i < marksRaw.Count; i++)
                tree.marks[i] = Convert.ToInt32(marksRaw[i]);
        }

        return tree;
    }

    public List<PathStep> BuildStepsFromList(List<object> rawList)
    {
        var steps = new List<PathStep>();
        if (rawList == null)
            return steps;

        foreach (var item in rawList)
        {
            var dict = item as Dictionary<string, object>;
            if (dict == null)
                continue;

            var step = new PathStep
            {
                pos = new Pos
                {
                    x = dict.ContainsKey("x") ? Convert.ToInt32(dict["x"]) : 0,
                    y = dict.ContainsKey("y") ? Convert.ToInt32(dict["y"]) : 0
                },
                dir = dict.ContainsKey("dir") ? Convert.ToInt32(dict["dir"]) : 0,
                notchData = null
            };

            if (dict.TryGetValue("notch", out var notchObj) && notchObj is Dictionary<string, object> notchDict)
            {
                step.notchData = new NotchData
                {
                    count = notchDict.ContainsKey("count") ? Convert.ToInt32(notchDict["count"]) : 0,
                    pos = new Pos
                    {
                        x = notchDict.ContainsKey("x") ? Convert.ToInt32(notchDict["x"]) : 0,
                        y = notchDict.ContainsKey("y") ? Convert.ToInt32(notchDict["y"]) : 0
                    },
                    dir = notchDict.ContainsKey("dir") ? Convert.ToInt32(notchDict["dir"]) : 0
                };
            }

            steps.Add(step);
        }

        return steps;
    }
}
