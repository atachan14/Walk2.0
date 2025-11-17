using UnityEngine;

public class HomeManager : MonoBehaviour
{
    public static HomeManager instance;
    public CommondData leftBtn;
    public CommondData centerBtn;
    public CommondData rightBtn;
    public Sprite mainVisual;
    private void Awake()
    {
        instance = this;
    }


}
