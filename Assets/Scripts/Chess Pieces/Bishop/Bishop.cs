using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //Check the top-right diagonal for moves
        int tempY = pieceCoordinates.y;
        for (int x = pieceCoordinates.x + 1; x < tiles.Count; x++)
        {
            tempY++;

            //check if the tiles exists
            if (tiles.ContainsKey(x) && tiles[x].ContainsKey(tempY))
            {
                if (tiles[x][tempY].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(x, tempY));
                }
                else
                {
                    if (tiles[x][tempY].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(x, tempY));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        //Check the top-left diagonal for moves
        tempY = pieceCoordinates.y;
        for (int x = pieceCoordinates.x - 1; x >= 0 ; x--)
        {
            tempY++;

            //check if the tiles exists
            if (tiles.ContainsKey(x) && tiles[x].ContainsKey(tempY))
            {
                if (tiles[x][tempY].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(x, tempY));
                }
                else
                {
                    if (tiles[x][tempY].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(x, tempY));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }


        //Check the bottom-right diagonal for moves
        tempY = pieceCoordinates.y;
        for (int x = pieceCoordinates.x + 1; x < tiles.Count; x++)
        {
            tempY--;

            //check if the tiles exists
            if (tiles.ContainsKey(x) && tiles[x].ContainsKey(tempY))
            {
                if (tiles[x][tempY].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(x, tempY));
                }
                else
                {
                    if (tiles[x][tempY].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(x, tempY));
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        //Check the bottom-left diagonal for moves
        tempY = pieceCoordinates.y;
        for (int x = pieceCoordinates.x - 1; x >= 0; x--)
        {
            tempY--;

            //check if the tiles exists
            if (tiles.ContainsKey(x) && tiles[x].ContainsKey(tempY))
            {
                if (tiles[x][tempY].tilePlacements[0].Contains(MappedTileType.Empty))
                {
                    moves.Add(new Vector2Int(x, tempY));
                }
                else
                {
                    if (tiles[x][tempY].CanPieceAttack(0, teamColor))
                    {
                        moves.Add(new Vector2Int(x, tempY));
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
