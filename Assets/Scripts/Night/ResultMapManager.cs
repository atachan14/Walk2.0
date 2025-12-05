using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameData;

public class ResultMapManager : DebugMapManager
{
    public new static ResultMapManager Instance;

    [SerializeField] Camera cam;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] Material lineMaterial;
    [SerializeField] GameObject resultNotchPrefab;
    [SerializeField] ReplayDataSerializable replayData;
    protected override void Awake() => Instance = this;

    public override void CreateBaseGrid()
    {
        base.CreateBaseGrid();
        CamSetup();
    }

    void CamSetup()
    {
        int mapSize = NightSession.Instance.CurrentSize;
        Vector3 mapCenter = new((mapSize * cellSize) / 2f - (cellSize / 2f),
                                (mapSize * cellSize) / 2f - (cellSize / 2f),
                                -10f);
        cam.transform.localPosition = mapCenter;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        if (cam.orthographic)
            cam.orthographicSize = (mapSize * cellSize) / 2f;
    }

    // =======================================
    // 🎬 再生本体
    // =======================================
    public IEnumerator PlayReplay()
    {
        replayData = FormatReplayPath(GameData.Instance.PathSteps);

        float totalTime = 10f; // ← お前が好きに設定する尺
        float totalDistance = CalcTotalDistance(replayData);

        foreach (var seg in replayData.segments)
        {
            var lr = CreateLine();
            float segDistance = 0f;

            // セグメントの距離
            for (int i = 1; i < seg.points.Count; i++)
                segDistance += Vector3.Distance(seg.points[i - 1].pos, seg.points[i].pos);

            // セグメントの持ち時間（ここが最重要）
            float segDuration = (segDistance / totalDistance) * totalTime;
            float speed = segDistance / segDuration; // = 距離 / 時間（秒速）

            float t = 0f;

            for (int i = 1; i < seg.points.Count; i++)
            {
                Vector3 a = seg.points[i - 1].pos;
                Vector3 b = seg.points[i].pos;
                float d = Vector3.Distance(a, b);
                float localTime = d / speed; // その区間の時間

                float elapsed = 0f;

                while (elapsed < localTime)
                {
                    elapsed += Time.deltaTime;

                    float lerpT = Mathf.Clamp01(elapsed / localTime);
                    Vector3 p = Vector3.Lerp(a, b, lerpT);

                    AddPointToLine(lr, p);

                    yield return null;
                }

                SpawnNotch(seg.points[i]);
            }
        }
    }

    float CalcTotalDistance(ReplayDataSerializable data)
    {
        float dist = 0f;

        foreach (var seg in data.segments)
        {
            for (int i = 1; i < seg.points.Count; i++)
                dist += Vector3.Distance(seg.points[i - 1].pos, seg.points[i].pos);
        }

        return dist;
    }

    LineRenderer CreateLine()
    {
        GameObject lineObj = Instantiate(lineRendererPrefab, overlayLayer);
        var lr = lineObj.GetComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = cellSize * 0.1f;
        lr.material = lineMaterial;
        lr.positionCount = 0;    // ← ここを明示
        lr.useWorldSpace = false;
        return lr;
    }

    // これをクラス内に追加して使って
    void AddPointToLine(LineRenderer lr, Vector3 p)
    {
        int idx = lr.positionCount;    // 現在の最後の index を取得
        lr.positionCount = idx + 1;    // 末尾に1つ分スペースを作る
        lr.SetPosition(idx, p);        // その位置に座標を入れる
    }


    // =======================================
    // 🧩 整形処理
    // =======================================
    ReplayDataSerializable FormatReplayPath(List<GameData.PathStep> replay)
    {
        ReplayDataSerializable result = new();
        ReplaySegment current = new();
        result.segments.Add(current);

        if (replay == null || replay.Count == 0) return result;

        for (int i = 0; i < replay.Count; i++)
        {
            var step = replay[i];
            Vector3 cellCenter = new(step.pos.x * cellSize, step.pos.y * cellSize, -0.1f);
            Vector3 pos = cellCenter + DirToOffset(step.dir);

            if (i == 0)
            {
                current.points.Add(new ReplayPoint(pos, step.notchData));
                continue;
            }

            var prev = replay[i - 1];
            int dx = step.pos.x - prev.pos.x;
            int dy = step.pos.y - prev.pos.y;

            bool warpedX = IsWarp(dx);
            bool warpedY = IsWarp(dy);

            if (warpedX)
                current = HandleWarpX(prev.pos, step.pos, current, result, NightSession.Instance.CurrentSize, cellCenter);

            if (warpedY)
                current = HandleWarpY(prev.pos, step.pos, current, result, NightSession.Instance.CurrentSize, cellCenter);

            current.points.Add(new ReplayPoint(pos, step.notchData)); // notchDataを渡す
        }

        return result;
    }

    // =======================================
    // ✨ 線描画
    // =======================================
    IEnumerator DrawLineSmooth(LineRenderer line, List<ReplayPoint> points, float stepDuration)
    {
        line.positionCount = 0;
        line.useWorldSpace = false;

        List<Vector3> path = new();
        float subStepTime = stepDuration;   // subStep 1個の長さ

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 a = points[i - 1].pos;
            Vector3 b = points[i].pos;

            int subSteps = 8;

            for (int j = 1; j <= subSteps; j++)
            {
                float t = j / (float)subSteps;

                float elapsed = 0f;
                while (elapsed < subStepTime)
                {
                    elapsed += Time.deltaTime;

                    float lerpT = Mathf.Clamp01(elapsed / subStepTime);
                    Vector3 p = Vector3.Lerp(a, b, (j - 1 + lerpT) / subSteps);

                    path.Add(p);
                    line.positionCount = path.Count;
                    line.SetPositions(path.ToArray());

                    yield return null; // フレーム毎に更新
                }

                // Notchは区間の最後でだけ
                if (j == subSteps)
                    SpawnNotch(points[i]);
            }
        }
    }


    // =======================================
    // 🔧 ヘルパー系（再利用OK）
    // =======================================
    //LineRenderer CreateLine()
    //{
    //    GameObject lineObj = Instantiate(lineRendererPrefab, overlayLayer);
    //    var lr = lineObj.GetComponent<LineRenderer>();
    //    lr.startWidth = lr.endWidth = cellSize * 0.1f;
    //    lr.material = lineMaterial;
    //    // 色は後で設定するのでここでは省略！
    //    return lr;
    //}


    Vector3 DirToOffset(int dir)
    {
        float offset = 0.4f;
        return dir switch
        {
            0 => new Vector3(0, offset, 0),
            2 => new Vector3(0, -offset, 0),
            3 => new Vector3(-offset, 0, 0),
            1 => new Vector3(offset, 0, 0),
            _ => Vector3.zero
        };
    }

    bool IsWarp(int d)
    {
        int mapSize = NightSession.Instance.CurrentSize;
        return Mathf.Abs(d) >= 4;
    }


    ReplaySegment HandleWarpX(Pos prevPos, Pos currPos, ReplaySegment current,
        ReplayDataSerializable result, int mapSize, Vector3 cellCenter)
    {
        if (prevPos.x > currPos.x)
        {
            // 右端 → 左端
            float edgeX = mapSize * cellSize;
            Vector3 exit = new(edgeX, cellCenter.y, -0.1f);
            Vector3 entry = new(-1f, cellCenter.y, -0.1f);
            return AddEdgeAndStartNew(current, result, exit, entry);
        }
        else
        {
            // 左端 → 右端
            float edgeX = -cellSize;
            Vector3 exit = new(edgeX, cellCenter.y, -0.1f);
            Vector3 entry = new(mapSize * cellSize, cellCenter.y, -0.1f);
            return AddEdgeAndStartNew(current, result, exit, entry);
        }
    }

    ReplaySegment HandleWarpY(Pos prevPos, Pos currPos, ReplaySegment current,
        ReplayDataSerializable result, int mapSize, Vector3 cellCenter)
    {
        if (prevPos.y > currPos.y)
        {
            // 上端 → 下端
            float edgeY = mapSize * cellSize;
            Vector3 exit = new(cellCenter.x, edgeY, -0.1f);
            Vector3 entry = new(cellCenter.x, -1f, -0.1f);
            return AddEdgeAndStartNew(current, result, exit, entry);
        }
        else
        {
            // 下端 → 上端
            float edgeY = -cellSize;
            Vector3 exit = new(cellCenter.x, edgeY, -0.1f);
            Vector3 entry = new(cellCenter.x, mapSize * cellSize, -0.1f);
            return AddEdgeAndStartNew(current, result, exit, entry);
        }
    }

    ReplaySegment AddEdgeAndStartNew(ReplaySegment current, ReplayDataSerializable result, Vector3 exit, Vector3 entry)
    {
        current.points.Add(new ReplayPoint(exit));
        var newSeg = new ReplaySegment();
        newSeg.points.Add(new ReplayPoint(entry));
        result.segments.Add(newSeg);
        return newSeg;
    }
    void SpawnNotch(ReplayPoint point)
    {
        if (point.notch == null) return; // Notchなしなら何もしない

        // NotchData の位置と方向から配置
        Vector3 pos = point.notch.Value.PosAsVector() * cellSize; // Pos → Vector3に変換
       
        var go = Instantiate(resultNotchPrefab, overlayLayer);
        var t = go.transform;
        t.localPosition = pos;
        t.localRotation = Quaternion.Euler(0, 0, point.notch.Value.dir * -90f);
        go.GetComponentInChildren<TextMeshPro>().text = point.notch.Value.count.ToString();

    }
}

[System.Serializable]
public class ReplayPoint
{
    public Vector3 pos;
    public GameData.NotchData? notch;

    public ReplayPoint() { }
    public ReplayPoint(Vector3 p, GameData.NotchData? n = null) { pos = p; notch = n; }
}

[System.Serializable]
public class ReplaySegment
{
    public List<ReplayPoint> points = new();
}

[System.Serializable]
public class ReplayDataSerializable
{
    public List<ReplaySegment> segments = new();
}
