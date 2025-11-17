using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public TileType type;
   
    public Sprite[] mainVisual;
    public string[] mainText;
    public CommondData[] commond;

    public GameObject resultPrefab;
}
