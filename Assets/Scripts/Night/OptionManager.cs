using UnityEngine;

public class OptionManager : MonoBehaviour
{
   
    public static OptionManager Instance;
    public GameObject optionPanel;
    private void Awake()
    {
        Instance = this;
    }
    public void Open()
    {
        optionPanel.SetActive(true);
    }
    public void Close()
    {
        optionPanel.SetActive(false);
    }
}
