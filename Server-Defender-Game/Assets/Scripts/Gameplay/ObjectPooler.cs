using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                // Init with fallback to prevent crash if prefab is missing (needs setup in Editor)
                if (pool.prefab != null)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform);
                    objectPool.Enqueue(obj);
                }
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            // Optional: expand pool or just return null
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Call OnSpawn to reset fruit state (health, color, etc)
        Fruit fruit = objectToSpawn.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.OnSpawn();
        }

        // Nếu là Particle System → phát và tự trả về pool sau khi xong
        ParticleSystem ps = objectToSpawn.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(DeactivateAfterParticle(objectToSpawn, ps, tag));
        }

        return objectToSpawn;
    }

    /// <summary>
    /// Trả object về lại pool. Gọi khi object bị tắt (SetActive(false)).
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag)) return;
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    private System.Collections.IEnumerator DeactivateAfterParticle(GameObject obj, ParticleSystem ps, string tag)
    {
        // Chờ cho particle phát xong hoàn toàn
        yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
        ReturnToPool(tag, obj);
    }
}
