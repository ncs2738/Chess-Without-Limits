using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, GameObject>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        return moves;
    }
}
