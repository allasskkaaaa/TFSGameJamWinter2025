using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // speed variable for player
    [SerializeField] private float speed = 5f;
    public Rigidbody2D playerRigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created (as soon as the game is started)
    void Start()
    {

    }

    // Update is called once per frame (constantly running)
    void Update()
    {
        // player goes up
        if (Input.GetKey(KeyCode.UpArrow))
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, speed);
        }
        // player goes down
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, -speed);
        }
        // player goes right
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            playerRigidbody.linearVelocity = new Vector2(speed, playerRigidbody.linearVelocity.y);
        }
        // player goes left
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerRigidbody.linearVelocity = new Vector2(-speed, playerRigidbody.linearVelocity.y);
        }
        else
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }
    }
}

