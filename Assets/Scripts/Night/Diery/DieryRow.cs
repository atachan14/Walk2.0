using TMPro;
using UnityEngine;

public class DieryRow : MonoBehaviour
{
    public CanvasGroup rowGroup;
    public TextMeshProUGUI size;
    public TextMeshProUGUI userName;
    public TextMeshProUGUI walk;
    public TextMeshProUGUI turn;
    public TextMeshProUGUI time;
    public TextMeshProUGUI date;

    public void Set(ClearRecord c)
    {
        size.text = c.Size.ToString();
        userName.text = c.UserName;
        walk.text = c.Walk.ToString();
        turn.text = c.Turn.ToString();
        time.text = c.Time.ToString();
        date.text = c.Date.ToString("yyyy/MM/dd HH:mm:ss");
        rowGroup.alpha = 1f;
    }

    public void Hide()
    {
        rowGroup.alpha = 0f;
    }
    public void ClickSize()
    {
        Debug.Log("Click Size: " + size.text);
        DieryManager.Instance.OnSizeSelect(int.Parse(size.text));
    }
}
