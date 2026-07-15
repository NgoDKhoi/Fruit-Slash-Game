using UnityEngine;

/// <summary>
/// Quản lý toàn bộ hệ thống âm thanh trong game.
/// Chơi nhạc nền (BGM) và các hiệu ứng âm thanh (SFX).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Source dùng để phát nhạc nền lặp lại (BGM)")]
    public AudioSource bgmSource;
    [Tooltip("Source dùng để phát các hiệu ứng âm thanh ngắn (SFX)")]
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip gameplayBGM;
    public AudioClip gameOverBGM;

    [Header("SFX Clips - Chém Hoa Quả")]
    public AudioClip slapSFX;       // Tiếng tát/quẹt tay
    public AudioClip fruitHitSFX;     // Tiếng hoa quả bị đánh trúng
    public AudioClip fruitSliceSFX; // Tiếng chém hoa quả đứt đôi

    [Header("SFX Clips - Cable Reconnect")]
    public AudioClip cableSnapSFX;  // Tiếng cáp cắm đúng lỗ
    public AudioClip cableAllConnectedSFX; // Tiếng hoàn thành nối cáp

    [Header("SFX Clips - System")]
    public AudioClip serverDamageSFX; // Tiếng server mất máu
    public AudioClip alarmSFX;        // Còi báo động khi máu thấp
    public AudioClip buttonClickSFX;  // Tiếng click UI

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Giữ AudioManager tồn tại xuyên suốt các scene (tùy chọn)
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayBGM(mainMenuBGM);
    }

    // ===== BGM (Nhạc nền) =====

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // Nếu nhạc đang chơi giống hệt thì không đổi để tránh bị ngắt quãng
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // ===== SFX (Hiệu ứng) =====

    /// <summary>
    /// Phát một hiệu ứng âm thanh 1 lần. Có thể phát đè lên nhau (PlayOneShot).
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    // Các hàm wrapper tiện lợi (tùy chọn)
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlaySlap() => PlaySFX(slapSFX);
    public void PlayFruitSlice() => PlaySFX(fruitSliceSFX);
    public void PlayCableSnap() => PlaySFX(cableSnapSFX);
}
