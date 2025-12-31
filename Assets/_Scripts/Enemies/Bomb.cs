using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float travelTime = 1f; 
    [SerializeField] private float explodeDelay = 0.5f; 
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackRadius = 2.5f;
    [SerializeField] private int damage = 20;

    private Vector3 targetPos;

    public void LaunchTowards(Vector3 target)
    {
        targetPos = target;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < travelTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        yield return new WaitForSeconds(explodeDelay);

        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, knockbackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Player hit by bomb!");
                GameManager.Instance.Health -= damage;

                // knockback effect
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                    float knockDuration = 0.2f;
                    StartCoroutine(KnockbackPlayer(hit.transform, knockDir, knockDuration));
                }
            }
            if (hit.CompareTag("Boss")) 
            {
                BossController boss = hit.GetComponent<BossController>();
                if (boss != null)
                {
                    Debug.Log("HIT BOSS!"); 
                    boss.TakeDamage(damage);
                    boss.OnBossHit();
                }
            }



        }

        Destroy(gameObject); 
    }

    private IEnumerator KnockbackPlayer(Transform target, Vector2 direction, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            target.position += (Vector3)(direction * knockbackForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }
}
