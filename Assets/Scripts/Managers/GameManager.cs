using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState gameState;

    

    void Awake()
    {
        Debug.Log("Game Manager init");
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Game Manager start");
        UpdateGameState(GameState.BeforeGame);
    }

    public void UpdateGameState(GameState gameState)
    {
        Debug.Log($"Game state is now: {gameState}");
        this.gameState = gameState;
        switch (gameState)
        {
            case GameState.BeforeGame:
                UpdateGameState(GameState.Gameplay);
                break;
            case GameState.Gameplay:
                GameplayManager.Instance.UpdateGameplayState(GameplayState.BoardSetup);
                break;
            case GameState.AfterGame:
                break;
        }

       
    }
}


public enum GameState
{
    BeforeGame,
    Gameplay,
    AfterGame
}