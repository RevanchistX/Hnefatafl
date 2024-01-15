using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private PieceType pieceType = PieceType.Rook;

    public void SetPieceType(PieceType pieceType)
    {
        this.pieceType = pieceType;
    }
    
    public PieceType GetPieceType()
    {
        return this.pieceType;
    }
    public void SetFaction(Faction faction)
    {
        this.faction = faction;
    }

    public Faction GetFaction()
    {
        return this.faction;
    }
}

public enum Faction
{
    Black,
    White
}

public enum PieceType
{
    Rook,
    King
}