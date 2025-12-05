using UnityEngine;
using UnityEngine.Audio;

public class SEManager : MonoBehaviour
{
    public static SEManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clickSE;
    public void PlayClick()
    {

        audioSource.loop = false;
        audioSource.clip = clickSE;
        audioSource.Play();

    }
    private void Start()
    {
        SetVolume();
    }
    public void SetVolume()
    {
        audioSource.volume = PlayerPrefs.GetFloat("SEVolume", 1f);
    }
}
