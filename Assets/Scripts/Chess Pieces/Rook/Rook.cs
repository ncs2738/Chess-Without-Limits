using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();    

        //VERTICAL MOVEMENTS

        //check for moves going  down the board
        for(int i = pieceCoordinates.y - 1; i >= tiles[pieceCoordinates.x].Keys.Min(); i--)
        {
            //check if exists
            if (tiles[pieceCoordinates.x].ContainsKey(i))
            {
                if (tiles[pieceCoordinates.x][i].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x, i));
                }
                else
                {
                    if (tiles[pieceCoordinates.x][i].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(pieceCoordinates.x, i));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        //check for moves going up the board
        for (int i = pieceCoordinates.y + 1; i <= tiles[pieceCoordinates.x].Keys.Max(); i++)
        {
            if (tiles[pieceCoordinates.x].ContainsKey(i))
            {
                if (tiles[pieceCoordinates.x][i].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x, i));
                }

                else
                {
                    if (tiles[pieceCoordinates.x][i].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(pieceCoordinates.x, i));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }


        //HORIZONTAL MOVEMENTS

        //Check for moves moving left through the board
        for (int i = pieceCoordinates.x - 1; i >= tiles.Keys.Min(); i--)
        {
            //check if exists
            if (tiles[i].ContainsKey(pieceCoordinates.y))
            {
                if (tiles[i][pieceCoordinates.y].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(i, pieceCoordinates.y));
                }
                else
                {
                    if (tiles[i][pieceCoordinates.y].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(i, pieceCoordinates.y));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        //Check for moves moving rigt through the board
        for (int i = pieceCoordinates.x + 1; i <= tiles.Keys.Max(); i++)
        {
            //check if exists
            if (tiles[i].ContainsKey(pieceCoordinates.y))
            {
                if (tiles[i][pieceCoordinates.y].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(i, pieceCoordinates.y));
                }
                else
                {
                    if (tiles[i][pieceCoordinates.y].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(i, pieceCoordinates.y));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        return moves;
    }
}
