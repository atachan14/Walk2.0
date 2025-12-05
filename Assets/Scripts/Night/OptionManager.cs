using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
   
    public static OptionManager Instance;
    public GameObject optionPanel;
    public Slider bgmSlider;
    public Slider seSlider;
    private void Awake()
    {
        Instance = this;
    }
    public void Open()
    {
        optionPanel.SetActive(true);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        seSlider.value = PlayerPrefs.GetFloat("SEVolume", 1f);
    }
    public void Close()
    {
        optionPanel.SetActive(false);
    }
    public void SetBGMVolume(float v)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        BGMManager.Instance.SetVolume();
    }
    public void SetSEVolume(float v)
    {
        PlayerPrefs.SetFloat("SEVolume", seSlider.value);
        SEManager.Instance.SetVolume();
        SEManager.Instance.PlayClick();
    }
}
