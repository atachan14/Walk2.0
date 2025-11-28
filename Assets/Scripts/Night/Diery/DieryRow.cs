using System;
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
        time.text = TransformDateTime(c.Time);
        date.text = c.Date.ToString("yyyy/MM/dd HH:mm:ss");
        rowGroup.alpha = 1f;
    }
    string TransformDateTime(long t)
    {
        long totalSeconds = t;

        long h = totalSeconds / 3600;
        long m = (totalSeconds % 3600) / 60;
        long s = totalSeconds % 60;

        // 2åÖå≈íËÇæÇØÇ«ÅA3åÖà»è„Ç»ÇÁèüéËÇ…êLÇ—ÇÈ
        string hStr = h < 100 ? h.ToString("00") : h.ToString();

        string formatted = $"{hStr}:{m:00}:{s:00}";
        return formatted;
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
