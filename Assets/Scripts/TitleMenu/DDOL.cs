using UnityEngine;

public class DDOL : MonoBehaviour
{
    private static DDOL _instance;

    private void Awake()
    {
        // Šù‚É¶‚«c‚è‚ª‚¢‚é‚È‚ç‚±‚¢‚Â‚ÍˆŒY
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ‰‘ã‚¾‚¯¶‘¶
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}