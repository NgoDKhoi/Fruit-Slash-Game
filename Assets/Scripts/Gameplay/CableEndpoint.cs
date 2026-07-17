using UnityEngine;

/// <summary>
/// Đầu cáp mạng có thể kéo thả bằng cử chỉ Pinch.
/// Khi nhả ra gần đúng ConnectionPort → snap và kết nối.
/// Khi nhả ra sai vị trí → quay về vị trí gốc (spring back).
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class CableEndpoint : MonoBehaviour
{
    [Header("Cable Identity")]
    [Tooltip("ID của cổng đích mà đầu cáp này phải nối vào.")]
    public string targetPortID;

    [Header("Interaction Settings")]
    [Tooltip("Bán kính vùng bắt: tay phải Pinch trong vùng này để nhặt đầu cáp.")]
    public float grabRadius = 1.0f;

    [Tooltip("Tốc độ quay về vị trí gốc khi nhả sai.")]
    public float returnSpeed = 10f;

    [Header("Visual Feedback")]
    public Color idleColor = new Color(1f, 0.6f, 0f, 1f);     // Cam
    public Color grabbedColor = new Color(1f, 1f, 0f, 1f);     // Vàng sáng
    public Color connectedColor = new Color(0f, 1f, 0.4f, 1f); // Xanh lá

    [Header("References")]
    [Tooltip("Transform của điểm neo cố định (đầu kia của dây cáp, gắn vào server/switch).")]
    public Transform anchorPoint;

    /// <summary>Đầu cáp đã nối thành công vào đúng port chưa.</summary>
    public bool IsConnected { get; private set; }

    /// <summary>Đầu cáp đang bị người chơi kéo.</summary>
    public bool IsBeingDragged { get; private set; }

    // Sự kiện khi đầu cáp kết nối thành công (CableManager lắng nghe)
    public System.Action<CableEndpoint> OnConnected;

    private Vector3 originPosition; // Vị trí gốc ban đầu
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private CableRenderer cableRenderer;

    // Tham chiếu tới port gần nhất đang hover
    private ConnectionPort nearestPort;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = grabRadius;

        cableRenderer = GetComponent<CableRenderer>();
    }

    private void Start()
    {
        originPosition = transform.position;
        SetVisualColor(idleColor);

        // Khởi tạo CableRenderer nếu có
        if (cableRenderer != null && anchorPoint != null)
        {
            cableRenderer.Initialize(anchorPoint, transform);
        }
    }

    private void Update()
    {
        if (IsConnected) return; // Đã nối xong, không xử lý gì thêm

        HandCursor2D cursor = FindCursor();
        if (cursor == null) return;

        if (IsBeingDragged)
        {
            HandleDragging(cursor);
        }
        else
        {
            HandleGrabDetection(cursor);
        }
    }

    /// <summary>
    /// Kiểm tra xem tay có đang Pinch trong vùng bắt của đầu cáp không.
    /// </summary>
    private void HandleGrabDetection(HandCursor2D cursor)
    {
        if (!cursor.isPinching) return;

        float distance = Vector2.Distance(transform.position, cursor.transform.position);
        if (distance <= grabRadius)
        {
            // Bắt đầu kéo!
            IsBeingDragged = true;
            SetVisualColor(grabbedColor);
        }
    }

    /// <summary>
    /// Kéo đầu cáp theo vị trí tay. Kiểm tra snap khi nhả.
    /// </summary>
    private void HandleDragging(HandCursor2D cursor)
    {
        if (cursor.isPinching)
        {
            // Đang giữ Pinch → di chuyển đầu cáp theo tay
            transform.position = cursor.transform.position;

            // Tìm port gần nhất để highlight
            UpdateNearestPort();
        }
        else
        {
            // Vừa nhả Pinch → kiểm tra xem có snap được không
            IsBeingDragged = false;
            ClearPortHighlight();

            if (TrySnapToPort())
            {
                // Snap thành công!
                IsConnected = true;
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCableSnap();
                }

                SetVisualColor(connectedColor);
                OnConnected?.Invoke(this);
            }
            else
            {
                // Không khớp port nào → quay về vị trí gốc
                SetVisualColor(idleColor);
            }
        }
    }

    /// <summary>
    /// Khi không đang kéo và chưa nối, từ từ quay về vị trí gốc.
    /// </summary>
    private void LateUpdate()
    {
        if (!IsConnected && !IsBeingDragged)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                originPosition,
                Time.deltaTime * returnSpeed
            );
        }
    }

    /// <summary>
    /// Tìm ConnectionPort gần nhất trong phạm vi snap và highlight nó.
    /// </summary>
    private void UpdateNearestPort()
    {
        // Bỏ highlight port cũ
        ClearPortHighlight();

        ConnectionPort[] allPorts = CableManager.Instance?.GetAllPorts();
        if (allPorts == null) return;

        float closestDist = float.MaxValue;
        ConnectionPort closest = null;

        foreach (ConnectionPort port in allPorts)
        {
            if (port.IsOccupied) continue;
            if (!port.CanAccept(targetPortID)) continue;

            float dist = Vector2.Distance(transform.position, port.transform.position);
            if (dist < port.snapRadius && dist < closestDist)
            {
                closestDist = dist;
                closest = port;
            }
        }

        nearestPort = closest;
        if (nearestPort != null)
        {
            nearestPort.ShowHighlight(true);
        }
    }

    private void ClearPortHighlight()
    {
        if (nearestPort != null)
        {
            nearestPort.ShowHighlight(false);
            nearestPort = null;
        }
    }

    /// <summary>
    /// Thử snap đầu cáp vào port gần nhất.
    /// </summary>
    private bool TrySnapToPort()
    {
        ConnectionPort[] allPorts = CableManager.Instance?.GetAllPorts();
        if (allPorts == null) return false;

        foreach (ConnectionPort port in allPorts)
        {
            if (!port.CanAccept(targetPortID)) continue;

            float dist = Vector2.Distance(transform.position, port.transform.position);
            if (dist <= port.snapRadius)
            {
                // Snap!
                transform.position = port.transform.position;
                port.Occupy();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tìm HandCursor2D trong scene (cache nếu cần tối ưu sau).
    /// </summary>
    private HandCursor2D FindCursor()
    {
        // Dùng FindObjectOfType vì chỉ có 1 HandCursor2D trong scene.
        // TODO: Chuyển sang Singleton hoặc cache reference nếu gặp vấn đề hiệu năng.
        return FindObjectOfType<HandCursor2D>();
    }

    private void SetVisualColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Reset đầu cáp về trạng thái ban đầu (dùng khi restart level).
    /// </summary>
    public void ResetEndpoint()
    {
        IsConnected = false;
        IsBeingDragged = false;
        ClearPortHighlight();
        transform.position = originPosition;
        SetVisualColor(idleColor);
    }
}
