using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, Tile>> tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //ROOK MOVEMENTS

        //check for moves going  down the board
        for (int i = pieceCoordinates.y - 1; i >= tiles[pieceCoordinates.x].Keys.Min(); i--)
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

        //BISHOP MOVEMENT

        //Check the top-right diagonal for moves
        int tempY = pieceCoordinates.y;
        for (int x = pieceCoordinates.x + 1; x < tiles.Keys.Max(); x++)
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
        for (int x = pieceCoordinates.x - 1; x >= tiles.Keys.Min(); x--)
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
        for (int x = pieceCoordinates.x + 1; x < tiles.Keys.Max(); x++)
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
        for (int x = pieceCoordinates.x - 1; x >= tiles.Keys.Min(); x--)
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
