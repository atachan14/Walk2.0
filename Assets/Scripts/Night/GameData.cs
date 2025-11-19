using System;
using System.Collections.Generic;
using UnityEngine;


public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    public Dictionary<Pos, Tile> Map { get; private set; } = new();
    public List<PathStep> PathSteps { get; } = new();
    public int WalkCount { get; set; }
    public int TurnCount { get; set; }
    public int NotchCount { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan? Time { get; set; } = null;
    public Pos PlayerPos => PathSteps[^1].pos;
    public int PlayerDir => PathSteps[^1].dir;
    public Pos NextPos => GetNextPos();

    private void Awake()
    {
        Instance = this;
    }

    int MapSize => NightSession.Instance.CurrentSize;


    public Pos GetNextPos()
    {
        Pos next = PlayerPos + Pos.FromDir(PlayerDir);
        return WrapPosition(next);
    }
    Pos WrapPosition(Pos pos)
    {
        int x = (pos.x + MapSize) % MapSize;
        int y = (pos.y + MapSize) % MapSize;
        return new Pos(x, y);
    }

    public void AddPlayer(Pos pos, int dir)
    {
        PathSteps.Add(new PathStep
        {
            pos = pos,
            dir = dir,
            notchData = null
        });
    }
    public void AddWalk()
    {
        WalkCount++;
        PathSteps.Add(new PathStep
        {
            pos = GetNextPos(),  // 移動後の座標
            dir = PlayerDir,
            notchData = null     // ノッチしてないので null
        });
    }

    public void AddLeftTurn()
    {
        TurnCount++;
        int newDir = (PlayerDir + 3) % 4; // 左に90度回転
        PathSteps.Add(new PathStep
        {
            pos = PlayerPos,
            dir = newDir,
            notchData = null
        });
    }

    public void AddRightTurn()
    {
        TurnCount++;
        int newDir = (PlayerDir + 1) % 4; // 右に90度回転
        PathSteps.Add(new PathStep
        {
            pos = PlayerPos,
            dir = newDir,
            notchData = null
        });
    }

    public void AddNotch()
    {
        NotchCount++;
        PathSteps.Add(new PathStep
        {
            pos = PlayerPos,
            dir = PlayerDir,
            notchData = new NotchData
            {
                count = NotchCount,
                pos = NextPos,
                dir = (PlayerDir + 2) % 4
            }
        });
        AddNotchToTree();
    }
    void AddNotchToTree()
    {
        // ① ノッチ番号を増やす（Exe側で増やしてるのでここでは不要）

        // ④ ターゲットのTileがTreeTileか確認
        if (Map.TryGetValue(NextPos, out var tile) && tile is TreeTile tree)
        {
            // ⑤ プレイヤー視点から見た「反対側」の方向を求める
            int oppositeDir = PlayerDir switch
            {
                0 => 2,    // Down
                1 => 3, // Left
                2 => 0,  // Up
                3 => 1,  // Right
                _ => 0
            };

            // ⑥ TreeTileのmarksにノッチ番号を刻む
            tree.marks[oppositeDir] = NotchCount;

            Debug.Log($"🌳 Notch {NotchCount} marked on Tree at {NextPos} (dir {oppositeDir})");
        }
        else
        {
            Debug.LogWarning("❌ 正面にTreeTileがないためNotch失敗");
        }
    }


    [System.Serializable]
    public struct PathStep
    {
        public Pos pos;
        public int dir;
        public NotchData? notchData;
    }
    [System.Serializable]
    public struct NotchData
    {
        public int count;
        public Pos pos;
        public int dir;

        public Vector3 PosAsVector()
        {
            return new Vector3(pos.x, pos.y, 0);
        }
    }


    [System.Serializable]
    public struct Pos
    {
        public int x;
        public int y;


        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        // 演算子オーバーロード
        public static Pos operator +(Pos a, Pos b) => new Pos(a.x + b.x, a.y + b.y);
        public static Pos operator *(Pos a, int scalar) => new Pos(a.x * scalar, a.y * scalar);

        // 新規：Direction(int) → Pos
        public static Pos FromDir(int dir)
        {
            return dir switch
            {
                0 => new Pos(0, 1),   // Up
                1 => new Pos(1, 0),   // Right
                2 => new Pos(0, -1),  // Down
                3 => new Pos(-1, 0),  // Left
                _ => new Pos(0, 0),
            };
        }
    }
}
