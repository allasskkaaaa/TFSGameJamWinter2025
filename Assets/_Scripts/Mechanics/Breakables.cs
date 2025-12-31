using UnityEngine;

public class Breakables : Health
{
    [Header("Break Settings")]
    [SerializeField] private float velocityThreshold = 5f;
    [SerializeField] private int damageTaken;
    [SerializeField] private int scoreOnBreak = 10;

    [Header("Power-Up Drops")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpDropChance = 0.25f; // 25% chance
    [SerializeField] private GameObject[] powerUpPrefabs;

    private Animator animator;
    private Rigidbody2D rb;

    public override void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        base.Start();
    }

    // Object can be thrown, if its velocity exceeds a threshold, it takes damage on collision with other objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null && rb.linearVelocity.magnitude > velocityThreshold)
        {
            TakeDamage(damageTaken);
        }
    }

    public override void Death()
    {
        // Play break animation if gift breaks
        if (CompareTag("Gift") && animator != null)
        {
            animator.Play("Gift_Destroy");
        }
        

        TrySpawnPowerUp();

        GameManager.Instance.Score += scoreOnBreak;
        Debug.Log($"{gameObject.name} broken! Score: {GameManager.Instance.Score}");

        Destroy(gameObject,1f);
    }

    private void TrySpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            return;

        float roll = Random.value;

        if (roll <= powerUpDropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
        }
    }
}
