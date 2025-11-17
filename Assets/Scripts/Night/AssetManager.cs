using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance;

    public Commonds Commond;
    public TileDatas TileData;
    public VisualDatas Visual;

    private void Awake()
    {
        Instance=this;
    }
}
[System.Serializable]
public class Commonds
{
    public CommondData None;
    public CommondData LeftTurn;
    public CommondData RightTurn;
    public CommondData Walk;
    public CommondData Notch;
    public CommondData GoHome;

    public CommondData Diary;
    public CommondData Sleep;
}
[System.Serializable]
public class TileDatas
{
    public TileData None;
    public TileData Tree;
    public TileData Home;
}
[System.Serializable]
public class VisualDatas
{
    public Sprite InHome;
}