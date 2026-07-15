using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý toàn bộ hệ thống cáp mạng trong màn chơi.
/// Theo dõi số cáp đã nối đúng và kích hoạt sự kiện khi hoàn thành.
/// Áp dụng Singleton pattern.
/// </summary>
public class CableManager : MonoBehaviour
{
    public static CableManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Kéo thả GameObject cha chứa toàn bộ dây cáp vào đây để bật/tắt hiển thị.")]
    public GameObject cableSystemContainer;

    [Tooltip("Danh sách tất cả các đầu cáp có thể kéo thả trong scene.")]
    public CableEndpoint[] cableEndpoints;

    [Tooltip("Danh sách tất cả các cổng kết nối (snap targets) trong scene.")]
    public ConnectionPort[] connectionPorts;

    /// <summary>Số cáp đã nối thành công.</summary>
    public int ConnectedCount { get; private set; }

    /// <summary>Tổng số cáp cần nối.</summary>
    public int TotalCables => cableEndpoints != null ? cableEndpoints.Length : 0;

    /// <summary>Tất cả các cáp đã nối xong chưa.</summary>
    public bool AllCablesConnected => ConnectedCount >= TotalCables && TotalCables > 0;

    // Sự kiện khi tất cả cáp đã nối xong (GameManager lắng nghe)
    public System.Action OnAllCablesConnected;

    // Sự kiện khi bất kỳ cáp nào được nối (UI update)
    public System.Action<int, int> OnCableProgressChanged; // (connected, total)

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ConnectedCount = 0;

        // Đăng ký lắng nghe sự kiện kết nối từ mỗi đầu cáp
        if (cableEndpoints != null)
        {
            foreach (CableEndpoint endpoint in cableEndpoints)
            {
                endpoint.OnConnected += HandleCableConnected;
            }
        }
    }

    /// <summary>
    /// Callback khi một đầu cáp nối thành công vào port.
    /// </summary>
    private void HandleCableConnected(CableEndpoint endpoint)
    {
        ConnectedCount++;
        OnCableProgressChanged?.Invoke(ConnectedCount, TotalCables);

        if (AllCablesConnected)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.cableAllConnectedSFX != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cableAllConnectedSFX);
            }
            OnAllCablesConnected?.Invoke();
        }
    }

    /// <summary>
    /// Trả về danh sách tất cả ConnectionPort (CableEndpoint gọi để tìm snap target).
    /// </summary>
    public ConnectionPort[] GetAllPorts()
    {
        return connectionPorts;
    }

    /// <summary>
    /// Reset toàn bộ hệ thống cáp (dùng khi restart level).
    /// </summary>
    public void ResetAllCables()
    {
        ConnectedCount = 0;

        if (cableEndpoints != null)
        {
            foreach (CableEndpoint endpoint in cableEndpoints)
            {
                endpoint.ResetEndpoint();
            }
        }

        if (connectionPorts != null)
        {
            foreach (ConnectionPort port in connectionPorts)
            {
                port.Release();
            }
        }

        OnCableProgressChanged?.Invoke(0, TotalCables);
    }

    /// <summary>
    /// Bật hoặc tắt hiển thị toàn bộ hệ thống cáp (ẩn đi khi đang chơi mode Bắt Bọ).
    /// </summary>
    public void SetSystemActive(bool isActive)
    {
        if (cableSystemContainer != null)
        {
            cableSystemContainer.SetActive(isActive);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký để tránh memory leak
        if (cableEndpoints != null)
        {
            foreach (CableEndpoint endpoint in cableEndpoints)
            {
                endpoint.OnConnected -= HandleCableConnected;
            }
        }
    }
}
