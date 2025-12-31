
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    [SerializeField] private GameState currentState; 

    public event Action OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // ensure sane defaults
        if (maxHealth <= 0f) maxHealth = 100f;
        if (health <= 0f) health = maxHealth;
    }

//score
    public int Score
    {
        get => score; 
        set
        {
            score = value;
            if (OnScoreValueChanged != null)
            {
                OnScoreValueChanged.Invoke(score);
            }
        }
    }
    private int score; 
    public UnityEvent<int> OnScoreValueChanged; 

    //health
    public float Health
    {
        get => health;
        set
        {
            health = value;

            if (OnHealthValueChanged != null)
            {
                OnHealthValueChanged.Invoke(health);
                Debug.Log("Player Health changed to: " + health);
            }

            if (health <= 0 && currentState != GameState.GameOver)
            {
                SwitchState(GameState.GameOver);
            }
        }
    }

    public UnityEvent<float> OnHealthValueChanged;
    [SerializeField] private float health = 100f;

    // maxHealth property + event; clamps Health when maxHealth changes
    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = Mathf.Max(1f, value);

            // Clamp current health to the new max
            if (Health > maxHealth)
            {
                Health = maxHealth; // uses setter to fire events
            }

            OnMaxHealthValueChanged?.Invoke(maxHealth);
        }
    }

    [SerializeField] private float maxHealth = 100f;

    // Optional event for UI max health updates
    public UnityEvent<float> OnMaxHealthValueChanged;


// helpers for health mb 
    /// Heal but do not exceed MaxHealth.
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        Health = Mathf.Min(Health + amount, MaxHealth);
    }

    /// apply damage (no negatives). GameOver handled by Health setter
    public void ApplyDamage(float damage)
    {
        if (damage <= 0f) return;
        Health = Mathf.Max(Health - damage, 0f);
    }

    /// temporary increase MaxHealth. Optionally grant the bonus to current health
    public void AddTemporaryMaxHealth(float bonus, bool alsoHealToNewMax = true)
    {
        if (bonus <= 0f) return;

        // increase max first (fires clamp + event)
        MaxHealth += bonus;

        // optionally give player up to the new max
        if (alsoHealToNewMax)
        {
            Health = Mathf.Min(Health + bonus, MaxHealth);
        }
    }


    /// remove previously added temporary MaxHealth bonus and clamp Health.
    /// this is for  TempMaxHealthBuff Deactivate().

    public void RemoveTemporaryMaxHealth(float bonus)
    {
        if (bonus <= 0f) return;

        MaxHealth = Mathf.Max(MaxHealth - bonus, 1f); // will clamp Health via setter
        Health = Mathf.Min(Health, MaxHealth);
    }

    // game state methods
    public GameState GetGameState()
    {
        return currentState;
    }

    public void SwitchState(GameState newState)
    {
        Debug.Log("New Game State has been changed from " + GetGameState() + " to " + newState);
        currentState = newState;
        OnGameStateChanged?.Invoke();
    }
}
