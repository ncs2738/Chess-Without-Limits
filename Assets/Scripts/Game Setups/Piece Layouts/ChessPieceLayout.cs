using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Scriptable Objects/Game Setups/Chess Piece Layout")]
public class ChessPieceLayout : ScriptableObject
{
    [Serializable]
    private class ChessPieceSetup
    {
        public Vector2Int piecePosition;
        public ChessPieceType chessPieceType;
        public TeamColor teamColor;
    }

    [SerializeField]
    private ChessPieceSetup[] chessPieceSetup;

    public int GetTotalPieceCount()
    {
        return chessPieceSetup.Length;
    }

    public Vector2Int GetPieceCoordinates(int pieceIndex)
    {
        if (chessPieceSetup.Length <= pieceIndex)
        {
            Debug.LogError("The index of piece is out of range in this scriptable object. The index is: " + pieceIndex);
            return -Vector2Int.one;
        }
        return chessPieceSetup[pieceIndex].piecePosition;
    }

    public ChessPieceType GetPieceType(int pieceIndex)
    {
        if (chessPieceSetup.Length <= pieceIndex)
        {
            Debug.LogError("The index of piece is out of range in this scriptable object. The index is: " + pieceIndex);
            return ChessPieceType.None;
        }
        return chessPieceSetup[pieceIndex].chessPieceType;
    }

    public TeamColor GetTeamColor(int pieceIndex)
    {
        if (chessPieceSetup.Length <= pieceIndex)
        {
            Debug.LogError("The index of piece is out of range in this scriptable object. The index is: " + pieceIndex);
            return TeamColor.None;
        }
        return chessPieceSetup[pieceIndex].teamColor;
    }
}
