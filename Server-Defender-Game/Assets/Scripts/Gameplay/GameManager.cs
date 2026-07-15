using UnityEngine;

/// <summary>
/// Singleton điều phối toàn bộ game flow.
/// Quản lý State Machine (MainMenu → Playing → GameOver),
/// timer đếm ngược, điểm số, và số mạng.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ===== GAME STATES =====
    public enum GameState { MainMenu, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public enum GameMode { FruitSlash, CableReconnect }
    public GameMode CurrentMode { get; private set; }

    // ===== SETTINGS =====
    [Header("Timer Settings")]
    [Tooltip("Thời gian mỗi ván chơi (tính bằng giây). Mặc định: 60 giây = 1 phút.")]
    public float gameDuration = 60f;

    [Header("Lives System")]
    [Tooltip("Số mạng tối đa của người chơi. Mất 1 mạng khi hoa quả rơi xuống đất.")]
    public int maxLives = 3;

    [Header("Scoring")]
    [Tooltip("Điểm thưởng khi nối đúng tất cả cáp mạng.")]
    public int allCablesBonus = 500;

    // ===== RUNTIME STATE =====
    /// <summary>Thời gian còn lại (giây).</summary>
    public float TimeRemaining { get; private set; }

    /// <summary>Điểm số hiện tại.</summary>
    public int Score { get; private set; }

    /// <summary>Số mạng hiện tại.</summary>
    public int CurrentLives { get; private set; }

    // ===== EVENTS (UI và các hệ thống khác lắng nghe) =====
    public System.Action<GameState> OnStateChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<float> OnTimerUpdated;

    // ===== REFERENCES =====
    [Header("References (Kéo thả từ Hierarchy)")]
    public FruitSpawner fruitSpawner;
    public CableManager cableManager;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Đăng ký lắng nghe sự kiện từ CableManager
        if (cableManager != null)
        {
            cableManager.OnAllCablesConnected += HandleAllCablesConnected;
        }

        // Bắt đầu ở trạng thái MainMenu
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Đếm ngược timer
        TimeRemaining -= Time.deltaTime;
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            ChangeState(GameState.GameOver);
        }
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Bắt đầu ván chơi mới theo chế độ được chọn từ UI.
    /// </summary>
    public void StartGame(GameMode mode)
    {
        CurrentMode = mode;

        // Reset tất cả giá trị
        Score = 0;
        CurrentLives = maxLives;
        TimeRemaining = gameDuration;

        OnScoreChanged?.Invoke(Score);
        OnLivesChanged?.Invoke(CurrentLives);
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (mode == GameMode.FruitSlash)
        {
            // BẬT chém hoa quả, TẮT cáp
            if (fruitSpawner != null) fruitSpawner.ResetAndStart();
            if (cableManager != null) cableManager.SetSystemActive(false);
        }
        else if (mode == GameMode.CableReconnect)
        {
            // TẮT chém hoa quả, BẬT cáp
            if (fruitSpawner != null) fruitSpawner.StopSpawning();
            if (cableManager != null)
            {
                cableManager.SetSystemActive(true);
                cableManager.ResetAllCables();
            }
        }

        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// Cộng điểm khi chém trúng hoa quả. Gọi bởi Fruit.Die().
    /// </summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>
    /// Trừ mạng khi hoa quả lọt qua. Gọi bởi Fruit khi rơi khỏi màn hình.
    /// </summary>
    public void LoseLife()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        OnLivesChanged?.Invoke(CurrentLives);

        // Phát tiếng báo động nếu còn 1 mạng cuối cùng
        if (CurrentLives == 1)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.alarmSFX != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.alarmSFX);
            }
        }

        if (CurrentLives <= 0)
        {
            ChangeState(GameState.GameOver);
        }
    }

    /// <summary>
    /// Quay về MainMenu (từ GameOver hoặc Pause).
    /// </summary>
    public void ReturnToMainMenu()
    {
        ChangeState(GameState.MainMenu);
    }

    // ===== PRIVATE =====

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(AudioManager.Instance.mainMenuBGM);
                // Dừng mọi hoạt động gameplay
                if (fruitSpawner != null) fruitSpawner.StopSpawning();
                Time.timeScale = 1f;

                // Tắt khung Camera khi ở Menu
                if (WebGLHandReceiver.Instance != null) WebGLHandReceiver.Instance.SetCameraVisible(false);
                break;

            case GameState.Playing:
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(AudioManager.Instance.gameplayBGM);
                Time.timeScale = 1f;

                // Bật khung Camera khi chơi game
                if (WebGLHandReceiver.Instance != null) WebGLHandReceiver.Instance.SetCameraVisible(true);
                break;

            case GameState.GameOver:
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(AudioManager.Instance.gameOverBGM);
                // Dừng spawn hoa quả
                if (fruitSpawner != null) fruitSpawner.StopSpawning();
                
                // Tắt khung Camera khi GameOver
                if (WebGLHandReceiver.Instance != null) WebGLHandReceiver.Instance.SetCameraVisible(false);
                // TODO: Phase 4 — Push score lên Firebase Leaderboard
                break;
        }

        OnStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Thưởng điểm khi nối đúng tất cả cáp mạng.
    /// </summary>
    private void HandleAllCablesConnected()
    {
        AddScore(allCablesBonus);
    }

    private void OnDestroy()
    {
        if (cableManager != null)
        {
            cableManager.OnAllCablesConnected -= HandleAllCablesConnected;
        }
    }
}
