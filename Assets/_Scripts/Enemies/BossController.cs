using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossController : Health
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bombPrefab;
    public LineRenderer laser;
    public LayerMask playerMask;

    [Header("Boss Settings")]
    public float bombInterval = 2f;
    public float idleAfterHit = 1f;
    public float laserDuration = 5f; // laser spin duration
    public int laserDamage = 20;
    public float shootRange = 10f;
    public float moveDistanceBeforeBomb = 3f;

    private Transform player;
    private Animator animator; 
    private NavMeshAgent agent;

    private enum EnemyStates { Idle, Moving, Laser, Dead }
    private EnemyStates currentState = EnemyStates.Idle;

    private bool laserActive;

    public override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.updateUpAxis = false;
            agent.updateRotation = false;
        }

        if (laser != null)
        {
            laser.enabled = false;
            var lrRenderer = laser.GetComponent<Renderer>();
            if (lrRenderer != null)
            {
                lrRenderer.sortingLayerName = "Foreground";
                lrRenderer.sortingOrder = 100;
            }
        }

        maxHealth = 100;
        health = 100;

        PlayAnim("Santa_Idle");

        currentState = EnemyStates.Moving;
        StartCoroutine(BombLoop(initialIdle: true));
    }


    private IEnumerator BombLoop(bool initialIdle = false)
    {
        if (initialIdle)
        {
            currentState = EnemyStates.Idle;
            yield return new WaitForSeconds(5f);
            currentState = EnemyStates.Moving;
        }

        while (currentState != EnemyStates.Dead)
        {
            if (currentState != EnemyStates.Moving)
            {
                yield return null; // pause if not in Moving state
                continue;
            }

            // Move boss
            Vector3 moveTarget = GetRandomNearbyPosition();

            if (agent != null)
            {
                animator.SetBool("isRunning", true); 
                PlayAnim("Santa_Run");

                agent.isStopped = false;
                agent.SetDestination(moveTarget);

                float timer = 0f;
                while (Vector3.Distance(transform.position, moveTarget) > 0.1f && timer < 0.5f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                agent.isStopped = true;
                animator.SetBool("isRunning", false);
                PlayAnim("Santa_Attack");
            }


            // Throw bomb
            ThrowBombAtPlayer();
            yield return new WaitForSeconds(bombInterval);
        }
    }

    public void OnBossHit()
    {
        if (currentState == EnemyStates.Dead) return;

        if (health <= 0)
        {
            currentState = EnemyStates.Dead;
            StartCoroutine(DeathSequence());
            return;
        }

        StartCoroutine(FallThenIdle());

        if (currentState != EnemyStates.Laser)
            StartCoroutine(HitAndLaserSequence());
    }


    IEnumerator FallThenIdle()
    {
        PlayAnim("Santa_Die_Fall");

        // wait for the fall animation to finish
        float fallLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(fallLength);

        // only return to idle if still alive
        if (currentState != EnemyStates.Dead)
        {
            PlayAnim("Santa_Idle");
        }
    }


    private IEnumerator DeathSequence()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            animator.SetBool("isRunning", false);
        }
            

        PlayAnim("Santa_Die_Fall");
        yield return new WaitForSeconds(1.2f);

        PlayAnim("Santa_Die_Explode");
        yield return new WaitForSeconds(1f);

        GameManager.Instance.SwitchState(GameManager.GameState.GameWin);
        Destroy(gameObject);
    }



    private Vector3 GetRandomNearbyPosition()
    {
        if (player == null) return transform.position;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return transform.position + new Vector3(randomDir.x, randomDir.y, 0f) * moveDistanceBeforeBomb;
    }

    private void ThrowBombAtPlayer()
    {
        if (bombPrefab == null || firePoint == null || player == null) return;

        PlayAnim("Santa_Attack");

        GameObject bomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
            bombScript.LaunchTowards(player.position);
    }


    private IEnumerator HitAndLaserSequence()
    {
        currentState = EnemyStates.Laser;

        PlayAnim("Santa_Idle");
        yield return new WaitForSeconds(idleAfterHit);

        PlayAnim("Santa_Attack");

        float timer = 0f;
        laserActive = true;

        int numRays = 20;
        float angleSpread = 20f;

        if (laser != null)
        {
            laser.positionCount = numRays * 2;
            laser.enabled = true;
            laser.useWorldSpace = true;
        }

        while (timer < laserDuration && currentState != EnemyStates.Dead)
        {
            Vector3 start = firePoint.position;
            float elapsedRotation = (timer / laserDuration) * 360f;

            for (int i = 0; i < numRays; i++)
            {
                float angle = Mathf.Lerp(-angleSpread / 2f, angleSpread / 2f, (float)i / (numRays - 1));
                angle += elapsedRotation;
                Vector3 rayDir = Quaternion.Euler(0, 0, angle) * Vector3.right;

                RaycastHit2D hit = Physics2D.Raycast(start, rayDir, shootRange, playerMask);
                Vector3 endPoint = hit.collider ? hit.point : start + rayDir * shootRange;

                if (hit.collider && hit.collider.CompareTag("Player"))
                    GameManager.Instance.Health -= laserDamage * Time.deltaTime;

                DrawLaser(start, endPoint, i);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (laser != null) laser.enabled = false;
        laserActive = false;
        animator.SetBool("isRunning", true); 
        PlayAnim("Santa_Run");
        currentState = EnemyStates.Moving;
    }


    private void DrawLaser(Vector3 start, Vector3 end, int rayIndex)
    {
        if (laser == null) return;

        start.z = -5f;
        end.z = -5f;

        laser.SetPosition(rayIndex * 2, start);
        laser.SetPosition(rayIndex * 2 + 1, end);
    }

    private void PlayAnim(string animName)
    {
        if (animator == null) 
        { 
            Debug.Log("No Animator!"); 
            return; 
        }
        animator.Play(animName);
    }

}
