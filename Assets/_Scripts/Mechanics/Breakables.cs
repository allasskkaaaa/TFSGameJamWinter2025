using System.Collections;
using UnityEngine;

public class Breakables : Health
{
    [Header("VFX")]
    [SerializeField] private GameObject scorePopupPrefab;

    [Header("Break Settings")]
    [SerializeField] private float velocityThreshold = 5f;
    [SerializeField] private int damageTaken;
    [SerializeField] private int scoreOnBreak = 10;

    [Header("Power-Up Drops")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpDropChance = 0.25f;
    [SerializeField] private GameObject[] powerUpPrefabs;

    private Animator animator;
    private Rigidbody2D rb;

    private bool hasDied = false; // FLAG FOR WHEN IT DIES MULTIPLE TIMES

    public override void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        base.Start();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null || hasDied) return;

        if (rb.linearVelocity.magnitude > velocityThreshold)
        {
            TakeDamage(damageTaken);
        }
    }

    public override void Death()
    {
        if (hasDied) return;   // STOP DUPLICATES
        hasDied = true;

        if (CompareTag("Gift") && animator != null)
        {
            animator.Play("Gift_Destroy");
        }

        if (CompareTag("Elf") && animator != null)
        {
            animator.Play("Elf_Death");
        }

        TrySpawnPowerUp();
        SpawnScorePopup();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Score += scoreOnBreak;
        }

        Destroy(gameObject, 1f);
    }

    public void ArmDestroyAfterThrow(float delay = 1f)
    {
        StartCoroutine(DestroyAfterThrowRoutine(delay));
    }

    IEnumerator DestroyAfterThrowRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hasDied) yield break;

        if (CompareTag("Gift") || CompareTag("Elf"))
        {
            Death();
        }
    }


    private void SpawnScorePopup()
    {
        if (scorePopupPrefab == null) return;

        GameObject popup = Instantiate(
            scorePopupPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        if (popup == null) return;

        ScorePopup sp = popup.GetComponent<ScorePopup>();
        if (sp != null)
        {
            sp.Setup(scoreOnBreak);
        }
    }

    private void TrySpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            return;

        if (Random.value > powerUpDropChance)
            return;

        int index = Random.Range(0, powerUpPrefabs.Length);
        GameObject prefab = powerUpPrefabs[index];

        if (prefab != null)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
