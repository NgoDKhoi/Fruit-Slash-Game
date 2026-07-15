using UnityEngine;

/// <summary>
/// Script tự động phóng to/thu nhỏ tấm ảnh nền (SpriteRenderer) 
/// để vừa khít với kích thước của Camera bất kể tỷ lệ màn hình là bao nhiêu.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private float lastAspect;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        FitToScreen();
    }

    void Update()
    {
        // Nếu người chơi kéo nhỏ/phóng to cửa sổ trình duyệt làm thay đổi tỷ lệ
        if (Camera.main != null && Camera.main.aspect != lastAspect)
        {
            FitToScreen();
        }
    }

    void FitToScreen()
    {
        if (Camera.main == null) return;
        if (sr == null || sr.sprite == null) return;

        // Ép vị trí của ảnh vào chính giữa Camera
        Vector3 camPos = Camera.main.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);

        // Reset scale về 1 để lấy kích thước gốc chính xác
        transform.localScale = Vector3.one;

        // Lấy kích thước thực tế của Camera
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        // Lấy kích thước gốc của tấm ảnh (Sprite.bounds luôn là local space)
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // Tính toán tỷ lệ cần kéo giãn (Stretch)
        float scaleY = cameraHeight / spriteHeight;
        float scaleX = cameraWidth / spriteWidth;

        // Áp dụng tỷ lệ mới để lấp đầy 100% màn hình
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
        
        lastAspect = Camera.main.aspect;
    }
}
