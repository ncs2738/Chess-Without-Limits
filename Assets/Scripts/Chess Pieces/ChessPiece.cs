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
    private Vector3 desiredScale = Vector3.one;

    private void Update()
    {
        if(transform.position != desiredPosition)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        }

       // if (transform.localScale != desiredScale)
        {
            //transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
        }
    }

    public virtual List<Vector2Int> GetAvailableMoves (ref Dictionary<int, Dictionary<int, GameObject>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        moves.Add(new Vector2Int(4, 4));
        moves.Add(new Vector2Int(4, 3));
        return moves;
    }

    public virtual void SetPosition(Vector3 newPosition, bool instantUpdate = false)
    {
        desiredPosition = newPosition;

        if(instantUpdate)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 newScale, bool instantUpdate = false)
    {
        desiredPosition = newScale;

        if (instantUpdate)
        {
            transform.position = desiredPosition;
        }
    }
}
