using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class Elf_NPC : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private List<SpriteRenderer> spriteComponents;

    [SerializeField] private bool playerInRange;
    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            LookAtPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            player = collision.gameObject;
            playerInRange = true;
        }
        Debug.Log("Player is in range");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            player = null;
            playerInRange = false;
        }
        Debug.Log("Player is no longer in range");
    }

    void LookAtPlayer()
    {
        if (player.transform.position.x < this.gameObject.transform.position.x)
        {
            foreach (SpriteRenderer spriteComponent in spriteComponents)
            {
                spriteComponent.flipX = true;
            }
        } else
        {
            foreach (SpriteRenderer spriteComponent in spriteComponents)
            {
                spriteComponent.flipX = false;
            }
        }

    }
}
