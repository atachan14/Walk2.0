using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "CommondData", menuName = "CommondData/CommondData")]
public class CommondData : ScriptableObject
{
    public CommondType type;
    public bool btnIsActive;
    public string btnText;

    public bool dontSave;

    public bool curtainIsActive;
    public string curtainOpenText;
    public string curtainCloseText;
}




