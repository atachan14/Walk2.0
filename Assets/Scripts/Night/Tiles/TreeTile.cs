using System.Collections.Generic;
using UnityEngine;

public class TreeTile : Tile
{
    public int[] marks = new int[4];

    public override Sprite GetMainVisual()
    {
        return AssetManager.Instance.TileData.Tree.mainVisual[0];
    }
    public override string GetMainText()
    {
        int mark = marks[GetTargetDir()];
        return mark == 0 ? "–Ø‚¾" : $"{mark}‚¾";
    }
    public override CommondData GetCommond()
    {
        int mark = marks[GetTargetDir()];
         return mark == 0 ? AssetManager.Instance.Commond.Notch : AssetManager.Instance.Commond.None;
    }
    public override GameObject GetResultPrefab()
    {
        return AssetManager.Instance.TileData.Tree.resultPrefab;
    }
    public override TileData GetData()
    {
        return AssetManager.Instance.TileData.Tree;
    }
    public int GetFrontMark()
    {
        return marks[GetTargetDir()];
    }
}
