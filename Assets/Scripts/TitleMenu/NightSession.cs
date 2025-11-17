using UnityEngine;

public class NightSession : MonoBehaviour
{
    public static NightSession Instance { get; private set; }

    public int CurrentSize = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // “ñd¢Š«‚ğ‘¦ˆŒY
            return;
        }
        Instance = this;
    }

}
