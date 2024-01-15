using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    public GameplayState gameplayState;


    public Faction faction;

    void Awake()
    {
        Debug.Log("Gameplay Manager init");
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Gameplay Manager start");
        // UpdateGameplayState(GameplayState.BoardSetup);
    }

    public void UpdateGameplayState(GameplayState gameplayState)
    {
        Debug.Log($"Gameplay State {gameplayState}");
        this.gameplayState = gameplayState;
        switch (gameplayState)
        {
            case GameplayState.BoardSetup:
                TileManager.Instance.SetupBoard();
                break;
            case GameplayState.TurnBlack:
                faction = Faction.Black;
                break;
            case GameplayState.TurnWhite:
                faction = Faction.White;
                break;
            case GameplayState.DropPiece:
                break;
            case GameplayState.BoardCleanup:
                TileManager.Instance.CleanupBoard();
                break;
        }
    }

    public void ToggleTurn(Faction faction)
    {
        if (faction == Faction.Black)
        {
            UpdateGameplayState(GameplayState.TurnWhite);
            return;
        }
        UpdateGameplayState(GameplayState.TurnBlack);
    }
}

public enum GameplayState
{
    BoardSetup,
    TurnBlack,
    TurnWhite,
    DropPiece,
    BoardCleanup
}