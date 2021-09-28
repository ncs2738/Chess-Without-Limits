using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] KingMoves = {
            //Up 
            new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + 1),
            //Top-Right
            new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y + 1),
            //Right
            new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y),
            //Bottom-Right
            new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y - 1),
            //Down
            new Vector2Int(pieceCoordinates.x, pieceCoordinates.y - 1),
            //Bottom-Left
            new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y - 1),
            //Left
            new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y),
            //Top-Left
            new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y + 1)
        };

        for (int i = 0; i < KingMoves.Length; i++)
        {
            //check if the tiles exists
            if (tiles.ContainsKey(KingMoves[i].x) && tiles[KingMoves[i].x].ContainsKey(KingMoves[i].y))
            {
                if (tiles[KingMoves[i].x][KingMoves[i].y].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(KingMoves[i].x, KingMoves[i].y));
                }
                else
                {
                    if (tiles[KingMoves[i].x][KingMoves[i].y].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(KingMoves[i].x, KingMoves[i].y));
                    }
                }
            }
        }

        //Check
        return moves;
    }
}
