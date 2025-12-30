using System.Numerics;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [Header("Player Parameters")]
    // speed variable for player
    [SerializeField] private float speed = 5f;
    public Rigidbody2D playerRigidbody;

    [Header ("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private List<SpriteRenderer> spriteComponents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created (as soon as the game is started)
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame (constantly running)
    void Update()
    {
        UnityEngine.Vector2 moveVec = new UnityEngine.Vector2( Input.GetAxis("Horizontal") , Input.GetAxis("Vertical") );
        transform.position += (UnityEngine.Vector3)moveVec * speed * Time.deltaTime;

        HandleAnimations();
        
    }

    void HandleAnimations()
    {
        //Connects the player movement to the animator parameter "HorizontalSpeed"
        if (animator != null)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                animator.SetBool("isMoving", true);
            } else
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
            } else
            {
                    spriteComponent.flipX = false;
            }
        }
    }
}

