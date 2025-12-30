using System.Numerics;
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
        UnityEngine.Vector2 moveVec = new UnityEngine.Vector2( Input.GetAxis("Horizontal") , Input.GetAxis("Vertical") );
        transform.position += (UnityEngine.Vector3)moveVec * speed * Time.deltaTime;
    }
}

