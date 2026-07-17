using UnityEngine;

/// <summary>
/// Cổng kết nối mạng (snap target). Khi đầu cáp được nhả gần cổng đúng ID,
/// nó sẽ snap vào vị trí và kích hoạt kết nối.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class ConnectionPort : MonoBehaviour
{
    [Header("Port Identity")]
    [Tooltip("ID duy nhất của cổng này (dùng để match với CableEndpoint.targetPortID).")]
    public string portID;

    [Header("Snap Settings")]
    [Tooltip("Bán kính snap — đầu cáp nhả trong phạm vi này sẽ tự động gắn vào.")]
    public float snapRadius = 1.5f;

    [Header("Visual Feedback")]
    public Color idleColor = new Color(0.3f, 0.3f, 0.3f, 1f);       // Xám tối
    public Color highlightColor = new Color(0f, 0.8f, 1f, 1f);       // Xanh sáng khi hover
    public Color connectedColor = new Color(0f, 1f, 0.4f, 1f);       // Xanh lá khi nối thành công

    /// <summary>
    /// Cổng này đã có cáp nối vào hay chưa.
    /// </summary>
    public bool IsOccupied { get; private set; }

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = snapRadius;
    }

    private void Start()
    {
        SetVisualState(PortVisualState.Idle);
    }

    /// <summary>
    /// Kiểm tra xem cổng có chấp nhận đầu cáp với cableTargetID đã cho không.
    /// </summary>
    public bool CanAccept(string cableTargetID)
    {
        return !IsOccupied && cableTargetID == portID;
    }

    /// <summary>
    /// Gắn đầu cáp vào cổng này. Gọi bởi CableEndpoint khi snap thành công.
    /// </summary>
    public void Occupy()
    {
        IsOccupied = true;
        SetVisualState(PortVisualState.Connected);
    }

    /// <summary>
    /// Giải phóng cổng (nếu cần reset level).
    /// </summary>
    public void Release()
    {
        IsOccupied = false;
        SetVisualState(PortVisualState.Idle);
    }

    /// <summary>
    /// Hiển thị highlight khi đầu cáp đang hover gần (gọi mỗi frame bởi CableEndpoint).
    /// </summary>
    public void ShowHighlight(bool show)
    {
        if (IsOccupied) return; // Không đổi màu nếu đã nối
        SetVisualState(show ? PortVisualState.Highlight : PortVisualState.Idle);
    }

    private enum PortVisualState { Idle, Highlight, Connected }

    private void SetVisualState(PortVisualState state)
    {
        if (spriteRenderer == null) return;
        switch (state)
        {
            case PortVisualState.Idle:
                spriteRenderer.color = idleColor;
                break;
            case PortVisualState.Highlight:
                spriteRenderer.color = highlightColor;
                break;
            case PortVisualState.Connected:
                spriteRenderer.color = connectedColor;
                break;
        }
    }
}
