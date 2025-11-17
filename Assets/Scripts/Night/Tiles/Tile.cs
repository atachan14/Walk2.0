
using UnityEngine;
using UnityEngine.UI;

public enum TileType
{
    none,
    river,
    bridge,
    tree,
    home
}

public abstract class Tile 
{

    public abstract Sprite GetMainVisual();
    public abstract string GetMainText();
    public abstract CommondData GetCommond();
    public abstract GameObject GetResultPrefab();
    public abstract TileData GetData();
    protected int GetTargetDir()
    {
        return ((int)GameData.Instance.PlayerDir + 2) % 4;
    }

}
