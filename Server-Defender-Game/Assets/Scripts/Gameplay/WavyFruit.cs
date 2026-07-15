using UnityEngine;

public class WavyFruit : Fruit
{
    private float phaseOffset; // Offset ngẫu nhiên để mỗi quả lắc khác nhau

    protected override void Awake()
    {
        base.Awake();
        // Hoa quả bay nhanh, dễ chém (1 nhát)
        maxHealth = 100;
        speed = 5f;
        scoreValue = 10;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        // Mỗi lần spawn lại sẽ lắc theo pha khác nhau
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void Move()
    {
        // Bay dích dắc (Sóng Sin) thay vì rơi thẳng xuống
        float xOffset = Mathf.Sin((Time.time + phaseOffset) * 5f) * 2f * Time.deltaTime;
        transform.Translate(new Vector3(xOffset, -speed * Time.deltaTime, 0), Space.World);
    }
}
