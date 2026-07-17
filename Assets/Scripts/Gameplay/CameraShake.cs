using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn vào Main Camera. Cung cấp 2 hiệu ứng Game Feel:
/// 1. Screen Shake — rung lắc camera khi mất mạng.
/// 2. Hit-Stop — khựng hình thoáng qua khi chém trúng hoa quả.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Screen Shake Settings")]
    [Tooltip("Biên độ rung tối đa (đơn vị world units).")]
    public float shakeMagnitude = 0.15f;

    [Tooltip("Thời gian rung (giây).")]
    public float shakeDuration = 0.2f;

    [Header("Hit-Stop Settings")]
    [Tooltip("TimeScale khi khựng hình (0.05 = gần như đứng hình).")]
    public float hitStopTimeScale = 0.05f;

    [Tooltip("Thời gian thực (real-time seconds) của hiệu ứng khựng hình.")]
    public float hitStopDuration = 0.07f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private Coroutine hitStopCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Rung màn hình. Gọi khi mất mạng (hoa quả lọt qua).
    /// </summary>
    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition;
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    /// <summary>
    /// Khựng hình thoáng qua. Gọi khi người chơi chém trúng hoa quả.
    /// Dùng unscaledTime để không bị ảnh hưởng bởi chính Time.timeScale.
    /// </summary>
    public void HitStop()
    {
        if (hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
            Time.timeScale = 1f;
        }
        hitStopCoroutine = StartCoroutine(HitStopRoutine());
    }

    // ===== PRIVATE =====

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Rung ngẫu nhiên trong vùng tròn quanh vị trí gốc
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalPosition + new Vector3(offset.x, offset.y, 0f);

            // Dùng unscaledDeltaTime để rung vẫn hoạt động khi Hit-Stop
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = hitStopTimeScale;

        // Chờ bằng thời gian thực (không bị ảnh hưởng bởi timeScale)
        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = 1f;
        hitStopCoroutine = null;
    }
}
