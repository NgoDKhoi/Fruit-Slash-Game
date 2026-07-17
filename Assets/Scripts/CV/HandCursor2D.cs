using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class HandCursor2D : MonoBehaviour
{
    [Header("Cursor Settings")]
    [Tooltip("Khoảng cách từ Camera đến mặt phẳng Game 2D (Z depth).")]
    public float zDepth = 10f;

    [Header("Adaptive Smoothing")]
    [Tooltip("Tốc độ bám dính khi tay đứng yên hoặc rê chậm (giúp chống rung).")]
    public float minLerpSpeed = 5f;
    [Tooltip("Tốc độ bám dính khi vung tay chém (giúp bám sát tay, không bị độ trễ).")]
    public float maxLerpSpeed = 30f;
    [Tooltip("Độ nhạy chuyển đổi giữa chống rung và bám sát.")]
    public float velocitySensitivity = 2f;

    [Header("Gesture Settings")]
    [Tooltip("Ngưỡng vận tốc để nhận diện hành động chém. Đã giảm xuống để nhạy hơn.")]
    public float swipeVelocityThreshold = 12f;

    // Các biến Color đã bị loại bỏ để không làm hỏng màu ảnh gốc
    
    // Public states
    public bool isPinching { get; private set; }
    public bool isSwiping { get; private set; }
    private bool wasSwiping = false; // Để theo dõi state change và play SFX

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;

    private Vector3 previousPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>(); // Có thể null nếu user quên gắn
        
        // Setup Physics components
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // We move it manually via Transform
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Prevent passing through fruits when swiping fast
    }

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        // 0. Ẩn con trỏ nếu không ở trạng thái Playing (ở Menu hoặc GameOver)
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            spriteRenderer.enabled = false;
            circleCollider.enabled = false;
            if (trailRenderer != null) trailRenderer.enabled = false;
            return;
        }

        // 1. Đọc dữ liệu từ Receiver
        if (WebGLHandReceiver.Instance == null || !WebGLHandReceiver.Instance.CurrentHandData.IsTracking)
        {
            spriteRenderer.enabled = false;
            circleCollider.enabled = false; // Disable collisions if not tracking
            if (trailRenderer != null) trailRenderer.enabled = false;
            return;
        }

        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        if (trailRenderer != null) trailRenderer.enabled = true;
        HandData data = WebGLHandReceiver.Instance.CurrentHandData;

        // 2. Ánh xạ tọa độ (X axis mirroring chỉ áp dụng cho WebGL build)
#if UNITY_WEBGL && !UNITY_EDITOR
        float viewportX = 1f - data.X;
#else
        float viewportX = data.X;
#endif
        Vector3 viewportPos = new Vector3(viewportX, 1f - data.Y, zDepth);
        
        if (Camera.main != null)
        {
            Vector3 targetWorldPos = Camera.main.ViewportToWorldPoint(viewportPos);
            
            // 3. Làm mượt chuyển động bằng Bộ lọc Thích ứng (Adaptive Smoothing)
            // Tính khoảng cách giữa vị trí hiện tại và tọa độ đích do AI trả về
            float distance = Vector3.Distance(transform.position, targetWorldPos);
            
            // Tự động điều chỉnh tốc độ mượt: Rê chậm thì nội suy ít (chống rung), Chém nhanh thì nội suy nhiều (chống lag)
            float dynamicLerp = Mathf.Lerp(minLerpSpeed, maxLerpSpeed, Mathf.Clamp01(distance * velocitySensitivity));
            
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * dynamicLerp);
        }

        // 4. Tính toán vận tốc (Velocity) để nhận diện Swipe
        float currentDeltaTime = Time.deltaTime;
        if (currentDeltaTime > 0)
        {
            Vector3 velocity = (transform.position - previousPosition) / currentDeltaTime;
            isSwiping = velocity.magnitude >= swipeVelocityThreshold;
        }
        else
        {
            isSwiping = false;
        }
        previousPosition = transform.position;

        // 5. Cập nhật state Pinch
        isPinching = data.IsPinching;

        // 6. Phản hồi (Âm thanh vung kiếm/tay)
        if (isSwiping)
        {
            // Phát âm thanh khi vung tay (chỉ phát 1 lần lúc bắt đầu vung)
            if (!wasSwiping && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlap();
            }
        }

        wasSwiping = isSwiping;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chạm vào là chém hoa quả luôn, không cần vung tay mạnh
        Fruit fruit = other.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.TakeDamage(100); // Gây 100 sát thương
        }
    }
}
