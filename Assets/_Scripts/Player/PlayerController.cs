using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;



public class PlayerController : MonoBehaviour
{
    [Header("Player Parameters")]
    // speed variable for player
    [SerializeField] public float speed = 5f;
    public Rigidbody2D playerRigidbody;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private List<SpriteRenderer> spriteComponents;


    [Header("Dash Parameters")]
    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 30f;
    private float dashingTime = 0.2f;
    private float dashCoolDown = 1f; //optional maybe
    [SerializeField] private TrailRenderer tr; //trail effect when dashing


    // Track input and last facing direction (cardinal: up/down/left/right)
    private UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
    private UnityEngine.Vector2 facingDir = UnityEngine.Vector2.right; // default facing right



    // Start is called once before the first execution of Update after the MonoBehaviour is created (as soon as the game is started)
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame (constantly running)
    void Update()
    {
        // prevent player movement while dashing
        if (isDashing)
        {
            return;
        }

        //move input
        moveInput = new UnityEngine.Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //player regular movement
        UnityEngine.Vector2 moveVec = new UnityEngine.Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.position += (UnityEngine.Vector3)moveVec * speed * Time.deltaTime;



        // safety line to prevent stupid tiny drift if Rigidbody regained velocity
        if (!isDashing) playerRigidbody.linearVelocity = UnityEngine.Vector2.zero;


        //animation handling
        HandleAnimations();

        //trigger dash

        if (moveInput.sqrMagnitude > 0.01f)
        {
            facingDir = SnapToCardinal(moveInput);
        }

        //trigger dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }


    }

    void HandleAnimations()
    {
        //Connects the player movement to the animator parameter "HorizontalSpeed"
        if (animator != null)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }

        }
        else
        {
            Debug.Log("Animator is not set");
        }

        //Flips the sprite depending on if the player is moving left or right
        //Because the sprites are multiple components, the components are added to a list and then each are flipped
        if (spriteComponents.Count != 0)
        {
            foreach (SpriteRenderer spriteComponent in spriteComponents)
                if (Input.GetAxis("Horizontal") < 0)
                {
                    spriteComponent.flipX = true;
                }
                else
                {
                    spriteComponent.flipX = false;
            }
        }
    }


    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;

        // Ensure we have a direction (defaults to right)
        if (facingDir.sqrMagnitude < 0.001f)
        {
            facingDir = UnityEngine.Vector2.right;
        }

        // Apply dash velocity
        playerRigidbody.linearVelocity = facingDir * dashPower;

        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;

        // --- Important: STOP residual physics so transform movement works again ---
        playerRigidbody.linearVelocity = UnityEngine.Vector2.zero;
        playerRigidbody.angularVelocity = 0f;

        playerRigidbody.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }



    //helper method for dash
    private UnityEngine.Vector2 SnapToCardinal(UnityEngine.Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new UnityEngine.Vector2(Mathf.Sign(v.x), 0f);
        else
            return new UnityEngine.Vector2(0f, Mathf.Sign(v.y));
    }

    // powerup activation method
    public void ApplyPowerupModifier(PowerupModifier powerupModifier)
    {
        powerupModifier.Activate(gameObject);

        //check if the powerup passed is timed class
        var TimedPowerupModifier = powerupModifier as TimedPowerupModifier;
        if (TimedPowerupModifier != null)
        {
            //start the countdown coroutine for timed powerups
            StartCoroutine(TimedPowerupModifier.StartPowerupCountdown(gameObject));
        }
    }

}


