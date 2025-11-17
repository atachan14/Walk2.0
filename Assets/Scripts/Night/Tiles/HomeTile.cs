using UnityEngine;

public class HomeTile :Tile
{
    public int[] dirs = new int[4];
    public HomeTile()
    {
        int[] order = { 2, 3, 0, 1 };
        for (int i = 0; i < dirs.Length; i++)
            dirs[i] = order[i];
    }
    public override Sprite GetMainVisual()
    {
        int dir = dirs[GetTargetDir()];
        Debug.Log(dir);
        return AssetManager.Instance.TileData.Home.mainVisual[dir];
    }
    public override string GetMainText()
    {
        int dir = dirs[GetTargetDir()];
        return AssetManager.Instance.TileData.Home.mainText[dir];
    }
    public override CommondData GetCommond()
    {
        int dir = dirs[GetTargetDir()];
        return AssetManager.Instance.TileData.Home.commond[dir];
    }
    public override GameObject GetResultPrefab()
    {
        return AssetManager.Instance.TileData.Home.resultPrefab;
    }
    public override TileData GetData()
    {
        return AssetManager.Instance.TileData.Home;
    }
}
