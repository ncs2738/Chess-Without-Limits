using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Dictionary<int, Dictionary<int, GameObject>> tiles)
    {
        //TODO - REWORK THIS TO WORK WITH 3 PLAYERS
        int direction = teamColor == TeamColor.White ? 1 : -1;

        List<Vector2Int> moves = new List<Vector2Int>();
        int pos = pieceCoordinates.y + direction;
        Debug.Log("x:" + pieceCoordinates.x + "- y:" + pieceCoordinates.y + " - newY :" + pos);

        //MOVING PAWN 1 TILE AHEAD

        //First check if the tile directly in front of the pawn EXISTS
        if (tiles[pieceCoordinates.x][pieceCoordinates.y + direction])
        {
            //next check if the tile directly in front of the pawn is open
            if (tiles[pieceCoordinates.x][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0] == null)
            {

                moves.Add(new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + direction));
            }
            else
            {
                Debug.LogError("Piece in front of the pawn.");
            }
        }
        else
        {
            Debug.LogError("PROBLEM WITH PAWN MOVEMENT - TILE IN FRONT OF IT DID NOT EXIST");
        }

        //INITIATING MOVE - LETS PAWN MOVE 2 TILES
        //TODO - ADD INITIATED VARIABLE
        //TODO - MERGE WITH ONE IN FRONT?
        //First check if the tile directly in front of the pawn EXISTS
        if (tiles[pieceCoordinates.x][pieceCoordinates.y + direction])
        {
            //next check if the tile directly in front of the pawn is open
            if (tiles[pieceCoordinates.x][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0] == null)
            {
                //OH MY GOD REWRITE THESE HOLY HEL

                //white team
                if(direction == 1 && pieceCoordinates.y == 1 && tiles[pieceCoordinates.x][pieceCoordinates.y + (direction * 2)].GetComponent<Tile>().tilePlacements[0] == null)
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + (direction * 2)));
                }

                //black team
                if (direction == -1 && pieceCoordinates.y == 6 && tiles[pieceCoordinates.x][pieceCoordinates.y + (direction * 2)].GetComponent<Tile>().tilePlacements[0] == null)
                {
                    moves.Add(new Vector2Int(pieceCoordinates.x, pieceCoordinates.y + (direction * 2)));
                }

            }
            else
            {
                Debug.LogError("Piece in front of the pawn when initiating.");
            }
        }
        else
        {
            Debug.LogError("PROBLEM WITH PAWN MOVEMENT - TILE IN FRONT OF IT DID NOT EXIST - INITIATING MOVE");
        }

        //KILL MOVE
        //TODO - REWRITE TO WORK WITH PILLARS

        //check if we're not at the right end of the board
        if(pieceCoordinates.x != tiles.Count - 1)
        {
            //OH GOD OH GOD REWRITE OH GOD
            if(tiles[pieceCoordinates.x + 1][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0] != null && tiles[pieceCoordinates.x + 1][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0].GetComponent<ChessPiece>().teamColor != teamColor)
            {
                moves.Add(new Vector2Int(pieceCoordinates.x + 1,pieceCoordinates.y + direction));
            }
        }

        //check if we're not at the left end of the board
        if(pieceCoordinates.x != 0)
        {
            if (tiles[pieceCoordinates.x - 1][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0] != null && tiles[pieceCoordinates.x - 1][pieceCoordinates.y + direction].GetComponent<Tile>().tilePlacements[0].GetComponent<ChessPiece>().teamColor != teamColor)
            {
                moves.Add(new Vector2Int(pieceCoordinates.x - 1, pieceCoordinates.y + direction));
            }
        }


        return moves;
    }
}
