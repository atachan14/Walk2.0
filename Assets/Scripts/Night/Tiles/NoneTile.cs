using UnityEngine;

public class NoneTile:Tile
{
    public override Sprite GetMainVisual()
    {
        return AssetManager.Instance.TileData.None.mainVisual[0];
    }
    public override string GetMainText()
    {
        return AssetManager.Instance.TileData.None.mainText[0];
    }
    public override CommondData GetCommond()
    {
        return AssetManager.Instance.TileData.None.commond[0];
    }
    public override GameObject GetResultPrefab()
    {
        return AssetManager.Instance.TileData.None.resultPrefab;
    }
    public override TileData GetData()
    {
        return AssetManager.Instance.TileData.None;
    }
}
