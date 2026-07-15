using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace

/// <summary>
/// Quản lý giao diện người dùng (UI) cho toàn bộ game.
/// Lắng nghe các sự kiện từ GameManager để cập nhật hiển thị.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;
    
    [Tooltip("Hình nền của riêng màn chơi (SpriteRenderer nằm ngoài Canvas)")]
    public GameObject gameplayBackground;
    [Tooltip("Hình nền của Main Menu")]
    public GameObject menuBackground;
    [Tooltip("Hình nền của màn hình Game Over")]
    public GameObject gameOverBackground;

    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    
    [Header("Lives System")]
    public Image[] heartIcons;
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("GameOver Elements")]
    public TextMeshProUGUI finalScoreText;

    private void Start()
    {
        // Kiểm tra xem GameManager có tồn tại không
        if (GameManager.Instance == null)
        {
            Debug.LogError("UIManager: Không tìm thấy GameManager trong Scene!");
            return;
        }

        // Đăng ký lắng nghe các sự kiện từ GameManager
        GameManager.Instance.OnStateChanged += HandleStateChanged;
        GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        GameManager.Instance.OnTimerUpdated += UpdateTimerDisplay;
        GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;

        // Bật panel MainMenu ban đầu
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký lắng nghe khi object bị hủy để tránh lỗi memory leak
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            GameManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
    }

    // ===== EVENT HANDLERS =====

    private void HandleStateChanged(GameManager.GameState newState)
    {
        // Tắt tất cả các panel
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameplayBackground != null) gameplayBackground.SetActive(false);
        if (menuBackground != null) menuBackground.SetActive(false);
        if (gameOverBackground != null) gameOverBackground.SetActive(false);

        // Bật panel tương ứng với trạng thái
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                if (menuBackground != null) menuBackground.SetActive(true);
                break;

            case GameManager.GameState.Playing:
                if (hudPanel != null) hudPanel.SetActive(true);
                if (gameplayBackground != null) gameplayBackground.SetActive(true);
                break;

            case GameManager.GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                // Vẫn giữ lại ảnh nền của màn chơi (gameplay) thay vì bật ảnh nền GameOver
                if (gameplayBackground != null) gameplayBackground.SetActive(true);
                
                // (Tùy chọn) Vẫn hiện luôn HUD nếu bạn muốn thấy số mạng lúc thua
                // if (hudPanel != null) hudPanel.SetActive(true);

                // Hiển thị điểm số cuối cùng
                if (finalScoreText != null && GameManager.Instance != null)
                {
                    finalScoreText.text = GameManager.Instance.Score.ToString();
                }
                break;
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = newScore.ToString("D5"); // Chỉ in ra số: 00010
        }
    }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            // Format sang dạng MM:SS
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateLivesDisplay(int currentLives)
    {
        if (heartIcons == null || heartIcons.Length == 0) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                if (i < currentLives)
                {
                    heartIcons[i].sprite = heartFull;
                }
                else
                {
                    heartIcons[i].sprite = heartEmpty;
                }
            }
        }
    }

    // ===== UI BUTTON BINDINGS =====

    /// <summary>
    /// Gắn hàm này vào nút PLAY FRUIT SLASH
    /// </summary>
    public void OnStartFruitSlashClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(GameManager.GameMode.FruitSlash);
        }
    }

    /// <summary>
    /// Gắn hàm này vào nút PLAY CABLE RECONNECT
    /// </summary>
    public void OnStartCableReconnectClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(GameManager.GameMode.CableReconnect);
        }
    }

    /// <summary>
    /// Gắn hàm này vào nút PLAY AGAIN trên màn hình Game Over
    /// </summary>
    public void OnPlayAgainButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
        }
    }

    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick() của nút MAIN MENU
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
