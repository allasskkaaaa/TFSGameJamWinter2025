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
    }


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

    public int Health
    {
        get => health;
        set
        {
            health = value;

            if (OnHealthValueChanged != null)
                OnHealthValueChanged.Invoke(health);
        }
    }

    public UnityEvent<int> OnHealthValueChanged;
    private int health;


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
