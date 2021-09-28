using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //NOTES: 
        //0 = Starting Position
        //o = Momvent trail
        //X = Position moving to

        /*
            o X
            o
            0
        */
        int x = pieceCoordinates.x + 1;
        int y = pieceCoordinates.y + 2;
        if(tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if(tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
          X o 
            o
            0
        */
        x = pieceCoordinates.x - 1;
        y = pieceCoordinates.y + 2;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
            o 0
            o
            X
        */
        x = pieceCoordinates.x - 1;
        y = pieceCoordinates.y - 2;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
          0 o 
            o
            X
        */
        x = pieceCoordinates.x + 1;
        y = pieceCoordinates.y - 2;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }


        /*
                X
            0 o o
        */
        x = pieceCoordinates.x + 2;
        y = pieceCoordinates.y + 1;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
            X
            o o 0
        */
        x = pieceCoordinates.x - 2;
        y = pieceCoordinates.y + 1;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
                0
            X o o
        */
        x = pieceCoordinates.x - 2;
        y = pieceCoordinates.y - 1;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        /*
            0
            o o X
        */
        x = pieceCoordinates.x + 2;
        y = pieceCoordinates.y - 1;
        if (tiles.ContainsKey(x) && tiles[x].ContainsKey(y))
        {
            if (tiles[x][y].tilePlacements[0].Contains(MappedTileType.Empty) || tiles[x][y].CanPieceAttack(0, teamColor))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        return moves;
    }
}
