using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Enemy))]
public class BossController : Health
{
    [Header("References")]
    public Enemy enemy;
    public Transform firePoint;
    public GameObject bombPrefab;
    public LineRenderer laser;
    public LayerMask playerMask;

    [Header("Boss Settings")]
    public float bombInterval = 2f;
    public float idleAfterHit = 1f;
    public float laserDuration = 10f;
    public int laserDamage = 20;
    public float laserFollowSpeed = 6f;
    public float shootRange = 10f;
    public float moveDistanceBeforeBomb = 3f; 

    private Transform player;
    private bool isHit;
    private bool laserActive;
    private NavMeshAgent agent;

    public override void Start()
    {
        if (enemy == null) enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;

        if (laser != null) laser.enabled = false;

        // Start bomb loop after initial idle
        StartCoroutine(BombLoop(initialIdle: true));
    }

    private IEnumerator BombLoop(bool initialIdle = false)
    {
        // Initial idle at the very start
        if (initialIdle)
        {
            enemy.CurrentState = EnemyStates.Idle;
            yield return new WaitForSeconds(5f); // 5 seconds idle before throwing bombs
        }

        while (!isHit && enemy.CurrentState != EnemyStates.Dead)
        {
            // Move before throwing
            Vector3 moveTarget = GetRandomNearbyPosition();
            agent.isStopped = false;
            agent.SetDestination(moveTarget);

            // Wait until boss reaches target or 0.5s max
            float timer = 0f;
            while (Vector3.Distance(transform.position, moveTarget) > 0.1f && timer < 0.5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            agent.isStopped = true;

            // Throw bomb
            ThrowBombAtPlayer();

            yield return new WaitForSeconds(bombInterval);
        }
    }


    public void OnBossHit()
    {
        if (!isHit)
        {
            isHit = true;
            StopAllCoroutines(); // END OF BOMB SEQUENCE 
            StartCoroutine(HitAndLaserSequence());
        }
    }

    private Vector3 GetRandomNearbyPosition()
    {
        if (player == null) return transform.position;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 target = transform.position + new Vector3(randomDir.x, randomDir.y, 0f) * moveDistanceBeforeBomb;

        return target;
    }

    private void ThrowBombAtPlayer()
    {
        if (bombPrefab == null || firePoint == null || player == null) return;

        GameObject bomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
            bombScript.LaunchTowards(player.position);
    }

    private IEnumerator HitAndLaserSequence()
    {
        // 1 second idle
        enemy.CurrentState = EnemyStates.Idle;
        yield return new WaitForSeconds(idleAfterHit);

        // Laser attack for 10 seconds
        float timer = 0f;
        laserActive = true;

        while (timer < laserDuration && enemy.CurrentState != EnemyStates.Dead)
        {
            if (player != null && firePoint != null)
            {
                Vector3 start = firePoint.position;
                Vector3 desiredEnd = start + (player.position - start).normalized * shootRange;

                Vector3 currentEnd = laser != null && laser.positionCount == 2 ? laser.GetPosition(1) : start;
                currentEnd = Vector3.Lerp(currentEnd, desiredEnd, Time.deltaTime * laserFollowSpeed);

                // Laser hit detection
                RaycastHit2D hit = Physics2D.Raycast(start, (desiredEnd - start).normalized, shootRange, playerMask);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    GameManager.Instance.Health -= laserDamage * Time.deltaTime;
                }

                DrawLaser(start, currentEnd);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (laser != null) laser.enabled = false;
        laserActive = false;
        isHit = false;

        // Resume bomb loop
        StartCoroutine(BombLoop());
    }

    private void DrawLaser(Vector3 start, Vector3 end)
    {
        if (laser == null) return;
        laser.enabled = true;
        laser.positionCount = 2;
        laser.useWorldSpace = true;
        laser.SetPosition(0, start);
        laser.SetPosition(1, end);
    }
}
