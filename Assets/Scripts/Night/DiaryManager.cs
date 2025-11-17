using System.Collections;
using UnityEngine;

public class DiaryManager : MonoBehaviour
{
    public static DiaryManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Open()
    {
        yield return null;
    }
}
