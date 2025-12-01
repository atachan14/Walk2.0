using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameData;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    public int MapSize { get; private set; }
    public int CenterY { get; private set; }
    public Dictionary<Pos, Tile> Map { get; private set; } = new();

    [Header("Tile Data")]
    [SerializeField] private TileData noneData;
    [SerializeField] private TileData homeData;
    [SerializeField] private TreeData treeData;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Generate()
    {
        Map = GameData.Instance.Map;
        SetupMapSize();

        bool ok = false;
        // ここでチェック
        while (!ok)
        {
            // 生成し直す
            SetupNone();
            SetupTree();
            SetupHome();
            SetupPlayer();

            ok = IsReachable();
            Debug.Log($"Map Generation Retry: {!ok}, TreeCount: {CountTree(Map)}");
        }

        GameData.Instance.StartTime = DateTime.Now;
        yield return FirebaseManager.Instance.SetSaveDataCoroutine();

#if UNITY_EDITOR
        DebugMapManager.Instance.Setup();
#endif

    }

    void SetupMapSize()
    {
        MapSize = NightSession.Instance.CurrentSize;
        CenterY = MapSize / 2;
    }

    void SetupNone()
    {
        for (int x = 0; x < MapSize; x++)
        {
            for (int y = 0; y < MapSize; y++)
            {
                var pos = new Pos(x, y);
                Map[pos] = new NoneTile();
            }
        }
    }

    void SetupTree()
    {
        var map = GameData.Instance.Map;
        int mapSize = MapSize;

        HashSet<Pos> candidates = new();
        foreach (var kvp in map)
        {
            if (kvp.Value.GetData().type == TileType.none)
                candidates.Add(kvp.Key);
        }

        Pos WrapPos(int x, int y) => new Pos((x + mapSize) % mapSize, (y + mapSize) % mapSize);

        void ExcludeAround(Pos pos, int rangeX, int rangeY)
        {
            for (int dx = -rangeX; dx <= rangeX; dx++)
                for (int dy = -rangeY; dy <= rangeY; dy++)
                    candidates.Remove(WrapPos(pos.x + dx, pos.y + dy));
        }

        bool IsTreeAt(Pos pos)
        {
            if (map.TryGetValue(pos, out var tile))
                return tile.GetData().type == TileType.tree;
            return false;
        }

        bool IsValidTreePos(Pos pos)
        {
            Pos[] dirs = { new Pos(0, 1), new Pos(0, -1), new Pos(-1, 0), new Pos(1, 0) };
            bool hasDirect = false;
            bool hasDiagonal = false;

            foreach (var d in dirs)
                if (IsTreeAt(WrapPos(pos.x + d.x, pos.y + d.y)))
                    hasDirect = true;

            Pos[] diag = { new Pos(1, 1), new Pos(1, -1), new Pos(-1, 1), new Pos(-1, -1) };
            foreach (var d in diag)
                if (IsTreeAt(WrapPos(pos.x + d.x, pos.y + d.y)))
                    hasDiagonal = true;

            return hasDirect || !hasDiagonal;
        }

        List<Pos> GetAdjacentCandidates(Pos pos)
        {
            List<Pos> list = new();
            Pos[] dirs = { new Pos(0, 1), new Pos(0, -1), new Pos(-1, 0), new Pos(1, 0) };
            foreach (var d in dirs)
            {
                var n = WrapPos(pos.x + d.x, pos.y + d.y);
                if (candidates.Contains(n))
                    list.Add(n);
            }
            return list;
        }

        Pos GetRandomCandidate()
        {
            int index = Random.Range(0, candidates.Count);
            int i = 0;
            foreach (var c in candidates)
            {
                if (i == index) return c;
                i++;
            }
            return default;
        }

        int maxConnection = mapSize / 2;
        int minCandidates = 0;

        while (candidates.Count > minCandidates)
        {
            int linkCount = Random.Range(0, maxConnection + 1);
            Pos start = GetRandomCandidate();
            if (!IsValidTreePos(start)) { candidates.Remove(start); continue; }

            List<Pos> newTrees = new() { start };
            candidates.Remove(start);

            for (int i = 0; i < linkCount; i++)
            {
                var basePos = newTrees[Random.Range(0, newTrees.Count)];
                var adj = GetAdjacentCandidates(basePos);
                if (adj.Count == 0) break;

                var next = adj[Random.Range(0, adj.Count)];
                if (!IsValidTreePos(next)) continue;

                newTrees.Add(next);
                candidates.Remove(next);
            }

            foreach (var t in newTrees)
            {
                if (map.ContainsKey(t))
                {
                    map[t] = new TreeTile();
                    ExcludeAround(t, 1, 1);
                }
            }
        }
    }

    int CountTree(Dictionary<Pos, Tile> map)
    {
        int count = 0;
        foreach (var kvp in map)
            if (kvp.Value.GetData().type == TileType.tree)
                count++;
        return count;
    }

    void SetupHome()
    {
        var map = GameData.Instance.Map;
        int mapSize = MapSize;

        List<Pos> candidates = new();

        foreach (var kvp in map)
        {
            var pos = kvp.Key;
            var tile = kvp.Value;

            if (tile == null || tile.GetData().type != TileType.none)
                continue;

            Pos under = new Pos(pos.x, (pos.y - 1 + mapSize) % mapSize);

            if (map.TryGetValue(under, out var underTile) && underTile.GetData().type == TileType.tree)
                continue;

            candidates.Add(pos);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SetupHome: 有効な候補マスが見つかりませんでした！");
            return;
        }

        Pos chosen = candidates[Random.Range(0, candidates.Count)];
        Map[chosen] = new HomeTile();

    }

    void SetupPlayer()
    {
        var map = GameData.Instance.Map;
        int mapSize = MapSize;

        Pos homePos = new(0, 0);
        foreach (var kvp in map)
        {
            if (kvp.Value is HomeTile)
            {
                homePos = kvp.Key;
                break;
            }
        }

        List<Pos> candidates = new();
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                Pos p = new Pos(x, y);
                if (map.TryGetValue(p, out var tile) && tile.GetData().type == TileType.none)
                {
                    if (p.x == homePos.x || p.y == homePos.y)
                        continue;

                    candidates.Add(p);
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SetupPlayer: 有効な候補マスが見つかりませんでした！");
            return;
        }

        Pos pos = candidates[Random.Range(0, candidates.Count)];
        GameData.Instance.AddPlayer(pos, Random.Range(0, 4));
    }
    public bool IsReachable()
    {
        var map = GameData.Instance.Map;
        int w = NightSession.Instance.CurrentSize;
        int h = NightSession.Instance.CurrentSize;

        // ▼ Home の座標を探す
        Pos homePos = new Pos(-1, -1);
        foreach (var kv in map)
        {
            if (kv.Value.GetData().type == TileType.home)
            {
                homePos = kv.Key;
                break;
            }
        }
        if (homePos.x == -1)
        {
            Debug.LogError("Homeが見つからん。お前の生成壊れてんぞ。");
            return false;
        }

        // ▼ Home の下（y - 1） ※トーラス対応
        Pos start = new Pos(homePos.x, Mod(homePos.y - 1, h));

        // ▼ Player
        Pos goal = GameData.Instance.PlayerPos;

        // ▼ BFS 開始
        Queue<Pos> q = new();
        HashSet<(int, int)> visited = new();

        q.Enqueue(start);
        visited.Add((start.x, start.y));

        // 4方向
        Pos[] dirs =
        {
        new Pos(1, 0),
        new Pos(-1, 0),
        new Pos(0, 1),
        new Pos(0, -1),
    };

        while (q.Count > 0)
        {
            Pos p = q.Dequeue();

            // ゴール到達
            if (p.x == goal.x && p.y == goal.y)
                return true;

            foreach (var d in dirs)
            {
                Pos np = new Pos(
                    Mod(p.x + d.x, w),
                    Mod(p.y + d.y, h)
                );

                if (visited.Contains((np.x, np.y)))
                    continue;

                // 通れるタイル？
                Tile t = map[np];
                var type = t.GetData().type;

                if (type != TileType.none && type != TileType.home)
                    continue;

                visited.Add((np.x, np.y));
                q.Enqueue(np);
            }
        }

        return false;
    }

    // ▼ C# は負数 % がクソなので自作
    int Mod(int a, int m)
    {
        return (a % m + m) % m;
    }

}
