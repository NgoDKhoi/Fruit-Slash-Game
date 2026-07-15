using UnityEngine;

/// <summary>
/// Bom kế thừa từ Fruit, nhưng có luật chơi đảo ngược:
/// Chém trúng = Mất mạng. Rơi ra khỏi màn hình = An toàn (không mất mạng).
/// </summary>
public class Bomb : Fruit
{
    protected override void Awake()
    {
        base.Awake();
        // Bom bay chậm hơn hoặc bằng quả thường để người chơi dễ nhìn thấy mà né
        speed = 2.5f; 
        maxHealth = 100;
        scoreValue = 0; // Chém bom không được điểm
    }

    protected override void Update()
    {
        Move();
        
        // Auto-despawn if it falls below the screen (Bom rớt khỏi màn hình)
        if (Camera.main != null)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            if (viewportPos.y < -0.2f)
            {
                // KHÁC VỚI FRUIT: Bom rơi đáy màn hình là an toàn, không mất mạng, không rung camera
                Despawn(); 
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            // BỊ CHÉM TRÚNG (NGUY HIỂM)
            
            // 1. Trừ mạng
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }

            // 2. Rung màn hình cực mạnh
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(); // Dùng rung màn thay vì HitStop
            }

            // 3. Phát tiếng nổ lớn (tạm dùng tiếng server damage hoặc bạn có thể thêm clip tiếng nổ riêng vào AudioManager)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.serverDamageSFX);
            }

            // 4. Phát hiệu ứng nổ bom và biến mất
            Die();
        }
    }

    protected override void Die()
    {
        CancelInvoke();
        SpawnBombExplosion();
        
        // Trả bom về pool
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Phát hiệu ứng nổ bom. Cần thêm pool tag "BombExplosion" trong ObjectPooler.
    /// </summary>
    private void SpawnBombExplosion()
    {
        if (ObjectPooler.Instance == null) return;
        ObjectPooler.Instance.SpawnFromPool("BombExplosion", transform.position, Quaternion.identity);
    }
}
