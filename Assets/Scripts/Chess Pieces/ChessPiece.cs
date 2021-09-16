using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Bishop = 3,
    Knight = 4,
    Queen = 5,
    King = 6,
}

public enum TeamColor
{
    None = 0,
    White = 1,
    Black = 2,
    Gray = 3,
}


public class ChessPiece : MonoBehaviour
{
    public TeamColor teamColor;
    //TODO - REWRITE THIS TO A DICTIONARY REF
    public Vector2Int pieceCoordinates;
    public ChessPieceType pieceType;

    private Vector3 desiredPosition;
    private Vector3 desiredVelocity;
}
