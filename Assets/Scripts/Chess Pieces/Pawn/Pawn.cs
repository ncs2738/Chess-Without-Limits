using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        //TODO - REWORK THIS TO WORK WITH 3 PLAYERS
        int direction = teamColor == TeamColor.White ? 1 : -1;

        List<Vector2Int> moves = new List<Vector2Int>();
        int pos = pieceCoordinates.y + direction;
        Debug.Log("x:" + pieceCoordinates.x + "- y:" + pieceCoordinates.y + " - newY :" + pos);

        //MOVING PAWN 1 TILE AHEAD

        //First check if the tile directly in front of the pawn EXISTS
        if (tiles[pieceCoordinates.x].ContainsKey(pieceCoordinates.y + direction))
        {
            //next check if the tile directly in front of the pawn is open
            if (tiles[pieceCoordinates.x][pieceCoordinates.y + direction].tilePlacements[0].Contains(MappedTileType.Empty))
            {
                moves.Add(new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + direction));

                if(!isInitiated)
                {
                    InitiatingMove(ref tiles, ref direction, moves);
                }
            }
            else
            {
                Debug.LogError("Piece in front of the pawn.");
            }
        }
        //the block in front of it didn't exist so...
        else
        {
            //Check if the tiles to the right of it are empty...
            if (tiles.ContainsKey(pieceCoordinates.x + 1))
            {
                if (tiles[pieceCoordinates.x + 1].ContainsKey(pieceCoordinates.y) && tiles[pieceCoordinates.x + 1][pieceCoordinates.y].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y));
                }

                if (tiles[pieceCoordinates.x + 1].ContainsKey(pieceCoordinates.y + direction) && tiles[pieceCoordinates.x + 1][pieceCoordinates.y + direction].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y + direction));
                }
            }

            //Check if the tiles to the left of it are empty...
            if (tiles.ContainsKey(pieceCoordinates.x - 1))
            {
                if (tiles[pieceCoordinates.x - 1].ContainsKey(pieceCoordinates.y) && tiles[pieceCoordinates.x - 1][pieceCoordinates.y].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y));
                }

                if (tiles[pieceCoordinates.x - 1].ContainsKey(pieceCoordinates.y + direction) && tiles[pieceCoordinates.x - 1][pieceCoordinates.y + direction].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y + direction));
                }
            }

            //Debug.LogError("PROBLEM WITH PAWN MOVEMENT - TILE IN FRONT OF IT DID NOT EXIST");
        }

        //KILL MOVE
        //TODO - REWRITE TO WORK WITH PILLARS

        //Check if there are still tiles to the right of the piece
        if (tiles.ContainsKey(pieceCoordinates.x + 1))
        {
            if (tiles[pieceCoordinates.x + 1].ContainsKey(pieceCoordinates.y + direction))
            {
                //OH GOD OH GOD REWRITE OH GOD
                if (tiles[pieceCoordinates.x + 1][pieceCoordinates.y + direction].CanPieceAttack(0, teamColor))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x + 1, pieceCoordinates.y + direction));
                }
            }
        }

        //check if we're not at the left end of the board
        if (tiles.ContainsKey(pieceCoordinates.x - 1))
        {
            if (tiles[pieceCoordinates.x - 1].ContainsKey(pieceCoordinates.y + direction))
            {
                if (tiles[pieceCoordinates.x - 1][pieceCoordinates.y + direction].CanPieceAttack(0, teamColor))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y + direction));
                }
            }
        }

        return moves;
    }

    //INITIATING MOVE - LETS PAWN MOVE 2 TILES
    private void InitiatingMove(ref Dictionary<int, Dictionary<int, Tile>> tiles, ref int direction, List<Vector2Int> moves)
    {
        if (tiles[pieceCoordinates.x].ContainsKey(pieceCoordinates.y + (direction * 2)))
        {
            //check if the tile directly in front of the pawn is open
            if (tiles[pieceCoordinates.x][pieceCoordinates.y + (direction * 2)].tilePlacements[0].Contains(MappedTileType.Empty))
            {
                moves.Add(new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + (direction * 2)));
            }
        }
        else
        {
            Debug.LogError("Piece in front of the pawn when initiating.");
        }
    }
}
