using System.Collections;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    public static OpeningManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    
}
