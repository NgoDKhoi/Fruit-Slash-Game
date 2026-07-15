using UnityEngine;

/// <summary>
/// Vẽ đường cáp mạng giữa 2 điểm bằng LineRenderer (đường cong Bézier bậc 2).
/// Gắn vào cùng GameObject chứa CableEndpoint.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CableRenderer : MonoBehaviour
{
    [Header("Curve Settings")]
    [Tooltip("Số điểm trên đường cong (càng nhiều càng mượt nhưng tốn FPS hơn).")]
    [Range(8, 32)]
    public int curveResolution = 16;

    [Tooltip("Độ võng của dây cáp theo trục Y (gravity sag). Giá trị dương = võng xuống.")]
    public float sagAmount = 1.5f;

    [Header("Width")]
    public float cableWidth = 0.08f;

    private LineRenderer lineRenderer;

    // Hai đầu mút của dây cáp
    private Transform anchorPoint;  // Đầu cố định (gắn vào server/switch)
    private Transform dragPoint;    // Đầu di chuyển (người chơi kéo)

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = curveResolution;
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
        lineRenderer.useWorldSpace = true;
    }

    /// <summary>
    /// Thiết lập 2 điểm neo cho đường cáp.
    /// </summary>
    public void Initialize(Transform anchor, Transform drag)
    {
        anchorPoint = anchor;
        dragPoint = drag;
    }

    private void LateUpdate()
    {
        if (anchorPoint == null || dragPoint == null) return;
        DrawCurve(anchorPoint.position, dragPoint.position);
    }

    /// <summary>
    /// Vẽ đường cong Bézier bậc 2 giữa startPos và endPos,
    /// với control point ở giữa bị kéo xuống để tạo hiệu ứng võng dây.
    /// </summary>
    private void DrawCurve(Vector3 startPos, Vector3 endPos)
    {
        // Control point nằm giữa 2 đầu, dịch xuống dưới theo sagAmount
        Vector3 midPoint = (startPos + endPos) * 0.5f;
        midPoint.y -= sagAmount;

        for (int i = 0; i < curveResolution; i++)
        {
            float t = (float)i / (curveResolution - 1);
            Vector3 point = QuadraticBezier(startPos, midPoint, endPos, t);
            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// Tính điểm trên đường cong Bézier bậc 2: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
    /// </summary>
    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }

    /// <summary>
    /// Ẩn/hiện đường cáp.
    /// </summary>
    public void SetVisible(bool visible)
    {
        lineRenderer.enabled = visible;
    }
}
