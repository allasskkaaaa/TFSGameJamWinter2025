using UnityEngine;

public class Breakables : Health
{
    [SerializeField] private float velocityThreshold = 5f;
    [SerializeField] private int damageTaken;
    [SerializeField] private int scoreOnBreak = 10; 
    public override void Start()
    {
        base.Start(); 
    }

    // Object can be thrown, if its velocity exceeds a threshold, it takes damage on collision with other objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Vector3.Magnitude(GetComponent<Rigidbody2D>().linearVelocity) > velocityThreshold)
        {
            TakeDamage(damageTaken);
        }
    }

    public override void Death()
    {
        // Add break animation or effects here if needed 
        GameManager.Instance.Score += scoreOnBreak; 
        Debug.Log($"{gameObject.name} has been broken! Score is now {GameManager.Instance.Score}!");
        GameObject.Destroy(this.gameObject);
    }
}
