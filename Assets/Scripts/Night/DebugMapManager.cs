using UnityEngine;
using System.Collections.Generic;
using static GameData;
#if UNITY_EDITOR
public class DebugMapManager : MonoBehaviour
{
    public static DebugMapManager Instance;

    [SerializeField] protected GameObject playerPrefab;
    [SerializeField] protected float cellSize = 1f;
    [SerializeField] protected Transform baseLayer;
    [SerializeField] protected Transform overlayLayer;

    protected Dictionary<Pos, GameObject> baseTiles = new();
    protected GameObject playerObj;

    protected virtual void Awake()
    {
        Instance = this;
    }

    public void Setup()
    {
        CreateBaseGrid();
        UpdateOverlay();
    }
    public void UpdateOverlay()
    {
        ClearOverlay();
        CreateOverlay();
    }

    private void ClearOverlay()
    {
        if (playerObj != null)
            Destroy(playerObj);
    }

    public virtual void CreateBaseGrid()
    {
        var map = GameData.Instance.Map;

        foreach (var kvp in map)
        {
            var pos = kvp.Key;
            var tile = kvp.Value;
            if (tile.GetResultPrefab() == null) continue;

            var obj = Instantiate(tile.GetResultPrefab(), baseLayer);
            obj.name = $"{tile.GetData().type}_{pos.x}_{pos.y}";

            // Spriteサイズ補正
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // スプライトの1タイルあたりのサイズを取得
                Vector2 size = sr.bounds.size;
                float scaleX = cellSize / size.x;
                float scaleY = cellSize / size.y;
                obj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            // 配置（左下原点想定）
            obj.transform.localPosition = new Vector3(pos.x * cellSize, pos.y * cellSize, 0);
            baseTiles.Add(pos, obj);
        }
    }

    protected virtual void CreateOverlay()
    {
        var mapSize = NightSession.Instance.CurrentSize;
        var playerPos = GameData.Instance.PlayerPos;
        var playerDir = GameData.Instance.PlayerDir;

        playerObj = Instantiate(playerPrefab, overlayLayer);
        playerObj.name = "Player";

        Vector3 basePos = new Vector3(playerPos.x * cellSize, playerPos.y * cellSize, -0.1f);

        // Directionに応じたオフセット（中央から少しずらす）
        float offset = cellSize * 0.25f;
        Vector3 dirOffset = Vector3.zero;

        switch (playerDir)
        {
            case 0:
                dirOffset = new Vector3(0, offset, 0);
                break;
            case 1:
                dirOffset = new Vector3(offset, 0, 0);
                break;
            case 2:
                dirOffset = new Vector3(0, -offset, 0);
                break;
            case 3:
                dirOffset = new Vector3(-offset, 0, 0);
                break;
        }

        playerObj.transform.localPosition = basePos + dirOffset;
    }
}
#endif
