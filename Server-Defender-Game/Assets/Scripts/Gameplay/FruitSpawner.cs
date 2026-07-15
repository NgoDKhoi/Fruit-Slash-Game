using System.Collections;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public float initialSpawnInterval = 1.5f;
    public float minimumSpawnInterval = 0.3f;
    
    [Tooltip("Matches tags configured in ObjectPooler")]
    public string[] fruitTags = { "Banana", "Blackberry", "Cherry", "Peach", "Orange" };

    [Header("Bomb Settings")]
    [Tooltip("Tỉ lệ xuất hiện bom (0.0 đến 1.0)")]
    [Range(0f, 1f)]
    public float bombSpawnChance = 0.15f; 
    
    [Tooltip("Matches tags configured in ObjectPooler for bombs")]
    public string[] bombTags = { "Bomb" };

    private bool isSpawning = false;
    private float currentSpawnInterval;

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        // Không tự động spawn — chờ GameManager gọi StartSpawning()
    }

    /// <summary>
    /// Reset interval về mặc định và bắt đầu spawn lại. Gọi bởi GameManager.StartGame().
    /// </summary>
    public void ResetAndStart()
    {
        currentSpawnInterval = initialSpawnInterval;
        StopSpawning(); // Dừng coroutine cũ nếu đang chạy
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        // Wait for ObjectPooler to finish Awake/Start initialization
        yield return new WaitForSeconds(1f);

        while (isSpawning)
        {
            SpawnRandomFruit();
            yield return new WaitForSeconds(currentSpawnInterval);
            
            // Simple difficulty scaling over time (spawn faster)
            currentSpawnInterval = Mathf.Max(minimumSpawnInterval, currentSpawnInterval - 0.02f);
        }
    }

    private void SpawnRandomFruit()
    {
        if (ObjectPooler.Instance == null) return;

        string selectedTag = "";

        // Tung xúc xắc xem lần này ra bom hay ra quả
        if (bombTags.Length > 0 && Random.value <= bombSpawnChance)
        {
            // Ra Bom
            selectedTag = bombTags[Random.Range(0, bombTags.Length)];
        }
        else
        {
            // Ra Quả
            if (fruitTags.Length > 0)
            {
                selectedTag = fruitTags[Random.Range(0, fruitTags.Length)];
            }
        }

        if (string.IsNullOrEmpty(selectedTag)) return;
        
        // Spawn at top of screen with random X position
        float spawnY = 6f; 
        float randomX = Random.Range(-5f, 5f);
        
        // Dynamically calculate camera bounds to spawn within view
        if (Camera.main != null)
        {
            Vector3 topEdge = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10f));
            spawnY = topEdge.y;
            
            Vector3 leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0.1f, 1.1f, 10f));
            Vector3 rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(0.9f, 1.1f, 10f));
            randomX = Random.Range(leftEdge.x, rightEdge.x);
        }

        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);
        ObjectPooler.Instance.SpawnFromPool(selectedTag, spawnPosition, Quaternion.identity);
    }
}
