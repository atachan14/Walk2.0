using Firebase.Firestore;
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

    // 名前が存在するかチェック
    public async Task<bool> CheckName()
    {
        var uid = SystemInfo.deviceUniqueIdentifier;
        var doc = db.Collection("ParsonalData").Document(uid);
        var snap = await doc.GetSnapshotAsync();

        return snap.Exists && snap.ContainsField("name");
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

    // ★ これが追加する SetName() 完成形
    public async Task SetName(string name)
    {
        var uid = SystemInfo.deviceUniqueIdentifier;

        var doc = db.Collection("ParsonalData").Document(uid);

        var data = new
        {
            name = name,
            created = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        await doc.SetAsync(data, SetOptions.MergeAll);
    }

    // クリア記録追加
    public async Task AddClearRecord()
    {
        var uid = SystemInfo.deviceUniqueIdentifier;

        var clearRecord = new
        {
            uid,
            name = NameManager.Instance.Name,
            mapSize = NightSession.Instance.CurrentSize,
            walk = GameData.Instance.WalkCount,
            turn = GameData.Instance.TurnCount,
            time = GameData.Instance.StartTime,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        var doc = db.Collection("ClearRecords").Document();
        await doc.SetAsync(clearRecord);
    }
}
