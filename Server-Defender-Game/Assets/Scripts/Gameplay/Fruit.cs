using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Fruit : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 100;
    public float speed = 3f;
    public int scoreValue = 10;

    [Tooltip("Phải khớp với Tag đã cấu hình trong ObjectPooler")]
    public string poolTag;

    protected int currentHealth;
    protected SpriteRenderer spriteRenderer;
    private Camera cachedCamera;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Ensure trigger collider is ready for the hand to slap it
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    // Called by ObjectPooler when reusing the object
    public virtual void OnSpawn()
    {
        currentHealth = maxHealth;
        CancelInvoke(); // Hủy mọi Invoke đang chờ từ lượt chơi trước
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        // Cache camera mỗi lần spawn (phòng trường hợp camera thay đổi)
        cachedCamera = Camera.main;
    }

    protected virtual void Update()
    {
        Move();
        
        // Auto-despawn if it falls below the screen
        if (cachedCamera != null)
        {
            Vector3 viewportPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewportPos.y < -0.2f)
            {
                // Hoa quả rơi lọt xuống đất → trừ mạng + rung màn hình
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoseLife();
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.serverDamageSFX);
                }
                
                // Rung màn hình khi mất mạng
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake();
                }
                Despawn(); // Rơi đất: không hiệu ứng chém, không SFX chém
            }
        }
    }

    protected virtual void Move()
    {
        // Default movement: falling straight down
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Simple visual feedback when hit
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.fruitHitSFX);
        }

        if (currentHealth <= 0)
        {
            // Bị chém trúng → cộng điểm + khựng hình
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.HitStop();
            }
            Die();
        }
    }

    protected void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Bị chém trúng: Phát hiệu ứng nổ + âm thanh chém, rồi trả về pool.
    /// </summary>
    protected virtual void Die()
    {
        CancelInvoke(); // Hủy ResetColor đang chờ
        SpawnExplosion();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFruitSlice();
        }

        ReturnToPool();
    }

    /// <summary>
    /// Rơi xuống đất: Không phát hiệu ứng chém, chỉ trả về pool.
    /// </summary>
    protected void Despawn()
    {
        CancelInvoke();
        ReturnToPool();
    }

    /// <summary>
    /// Trả hoa quả về Object Pool một cách an toàn.
    /// </summary>
    private void ReturnToPool()
    {
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false); // Fallback nếu không có poolTag
        }
    }

    /// <summary>
    /// Spawn particle hiệu ứng chém tại vị trí hoa quả. Pool tag: "SliceEffect".
    /// </summary>
    private void SpawnExplosion()
    {
        if (ObjectPooler.Instance == null) return;

        // Chỉ cần gọi SpawnFromPool, ObjectPooler đã tự gọi ps.Play() rồi
        ObjectPooler.Instance.SpawnFromPool("SliceEffect", transform.position, Quaternion.identity);
    }
}
