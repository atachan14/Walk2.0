using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nightSelect;
    public static TitleMenuManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // “ñd¢Š«‚ğ‘¦ˆŒY
            return;
        }
        Instance = this;
    }
    void Start()
    {
    }
    
    public void ClickStart()
    {
        SceneManager.LoadScene("Night");
    }

    public void ClickNightSelect()
    {

    }

    public void ClickRanking()
    {

    }

    public void ClickOption()
    {

    }
}
