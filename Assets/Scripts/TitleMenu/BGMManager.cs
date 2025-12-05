using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip mainBGM;
    [SerializeField] AudioClip clearBGM;
    [SerializeField] AudioClip retireBGM;

    bool isPlayingSpecial = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetVolume();
        PlayMain();
    }
    public void SetVolume()
    {
        audioSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
    }
    public void PlayMain()
    {
        if (isPlayingSpecial) return; // スペシャル再生中はMainに戻さない
        audioSource.loop = true;
        audioSource.clip = mainBGM;
        audioSource.Play();
    }

    public void PlayClear()
    {
        PlaySpecial(clearBGM);
    }

    public void PlayRetire()
    {
        PlaySpecial(retireBGM);
    }

    void PlaySpecial(AudioClip clip)
    {
        if (isPlayingSpecial) return;

        isPlayingSpecial = true;
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();

        // 終わったらMainに戻す
        StartCoroutine(BackToMainAfter(clip.length));
    }

    System.Collections.IEnumerator BackToMainAfter(float t)
    {
        yield return new WaitForSeconds(t);
        isPlayingSpecial = false;
        PlayMain();
    }


}
