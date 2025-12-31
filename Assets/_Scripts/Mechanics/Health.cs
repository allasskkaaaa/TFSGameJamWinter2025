using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class Health : MonoBehaviour, IDamageable
{
    // Enemies and Breakable Objects have health and can take damage
    protected SpriteRenderer sr;
    protected Animator anim;
    protected int _health;
    public int maxHealth;


    public int health
    {
        get => _health; set
        {
            _health = value;
            if (_health > maxHealth)
                _health = maxHealth;

            if (_health <= 0)
            {
                Death();
            }
        }
    }


    public virtual void Death()
    {
        //anim.SetTrigger("Death");
    }

    public virtual void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (maxHealth <= 0)
        {
            maxHealth = 5;
        }

        health = maxHealth;

    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
