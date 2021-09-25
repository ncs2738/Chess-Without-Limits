using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [SerializeField]
    private GameObject tile;

    [SerializeField]
    private int rows;
    [SerializeField]
    private int columns;

    public Material Material1;
    public Material Material2;

    public Material HighlightedTileMaterial;

    public Material[] TeamMaterials;
    public GameObject[] Prefabs;

    private Dictionary<int, Dictionary<int, GameObject>> tiles;

    private Camera currentCamera;

    private Vector2Int currentHoveredTile;

    [SerializeField]
    private ChessPieceLayout currentPieceLayout;

    [SerializeField]
    private Dictionary<TeamColor, List<ChessPiece>> activeChessPieces;
    private Dictionary<TeamColor, List<ChessPiece>> unactiveChessPieces;

    ChessPiece currentlyDragged;

    ChessPiece attackingPiece;
    ChessPiece pieceToBeRemoved;

    private bool IsPieceAttacking = false;

    private List<Vector2Int> availableMoves = new List<Vector2Int>();


    private void Awake()
    {
        //On startup, generate our board - for now, it's just a simple 8x8 board
        GenerateSquareBoard(rows, columns);
        //Then read in & spawn the pieces based off of the chess-piece layout script given
        CreatePiecesFromLayout(currentPieceLayout);
    }

    private void Update()
    {
        //If we don't have a camera selected...
        if(!currentCamera)
        {
            //grab the current camera
            currentCamera = Camera.current;
            return;
        }

        //Do a raycast check to see if the player's hovering over any tiles (and soon to be pieces)
        RaycastHit raycast;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        //If the player's hovering over a tile...
        if (Physics.Raycast(ray, out raycast, 100, LayerMask.GetMask("Tile")))
        {
            //Grab the raycast's data
            GameObject hoveredTile = raycast.transform.gameObject;
            Debug.Log(hoveredTile);
            Tile t = hoveredTile.GetComponent<Tile>();
            Vector2Int hitPos;

            //SAFE-KEEPING: Make sure we're hovering over a tile.
            if (t)
            {
                hitPos = t.GetBoardIndex();
            }
            else
            {
                //safety-keeping: if this hits, then there's a problem
                hitPos = -Vector2Int.one;
            }

            //If we just started hovering over the board & don't have a "last-hovered tile"...
            if (currentHoveredTile == -Vector2Int.one)
            {
                //Set the current hovered tile to it
                currentHoveredTile = hitPos;
                //Set the tile's material to being highlighted
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            //If we've been hovering over the board & have a "last-hovered tile"
            if (currentHoveredTile != hitPos)
            {
                if (!ContainsValidMove(ref availableMoves, currentHoveredTile))
                {
                    //Reset it's hovered-over material back to it's default color
                    tiles[currentHoveredTile.x][currentHoveredTile.y].GetComponent<Tile>().ResetTileColor();
                }
                //Update the current hovered tile
                currentHoveredTile = hitPos;
                //Set it's material to being highlighted
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            //If there's no pieces moving & attacking another on the board at moment...
            if(!IsPieceAttacking)
            {
                //Check for inputs:

                //If we just left-clicked on a tile...
                if (Input.GetMouseButtonDown(0))
                {
                    //If the currently hovered tile has something atop of it...
                    if (t.tilePlacements[0] != null)
                    {
                        //AND the item on the tile is a chess piece...
                        if (t.tilePlacements[0].GetComponent<ChessPiece>())
                        {
                            //Get the chess piece...
                            ChessPiece tempPiece = t.tilePlacements[0].GetComponent<ChessPiece>();

                            // Is it the players piece?
                            //TODO -REWRITE THIS TO ACTUALLY TAKE PROPER CONSIDERATIONS
                            //if (tempPiece.teamColor == PlayersTeamColor)
                            if (tempPiece.teamColor == TeamColor.Black)
                            {
                                //Select the player's piece
                                currentlyDragged = tempPiece;
                            }
                            else
                            {
                                //TODO: ADD THIS :0[
                                //Highlight the piece's possible moves?
                                currentlyDragged = tempPiece;
                            }

                            //Get the list of available moves for the piece
                            availableMoves = currentlyDragged.GetAvailableMoves(ref tiles);

                            //WORKING HERE
                            HighlightTiles();
                        }
                    }
                }

                //If we were dragging a piece & released the left-mouse button...
                if (currentlyDragged != null && Input.GetMouseButtonUp(0))
                {
                    //Grab the currently dragged pieces' starting tile's coordinates
                    Vector2Int originalPosition = currentlyDragged.pieceCoordinates;

                    //Check to see if the move was valid
                    bool isMoveValid = IsMoveValid(currentlyDragged, hitPos);

                    //If it wasnt...
                    if (!isMoveValid)
                    {
                        //Move the piece back to it's last position
                        currentlyDragged.SetPosition(GetTileCenter(tiles[originalPosition.x][originalPosition.y]));
                    }
                    //The move was valid!
                    else
                    {
                        //Check if the piece has been inidiated or not...
                        //if(!currentlyDragged.IsInitiated())
                        {
                            //It wasn't, so intiate it!
                        //    currentlyDragged.SetInitiatedStatus();
                        }

                        //set the currently dragged piece to null.
                        currentlyDragged = null;

                        //Clear the highlighted tiles
                        ClearHighlightedTiles();
                    }
                }
            }
        }

        //The player's raycast hit nothing...
        else
        {
            //If we have a currently dragged tile....
            if (currentHoveredTile != -Vector2Int.one)
            {
                if(!ContainsValidMove(ref availableMoves, currentHoveredTile))
                {
                    //Reset it's hovered-over material back to it's default color
                    tiles[currentHoveredTile.x][currentHoveredTile.y].GetComponent<Tile>().ResetTileColor();
                }
                //Set our currently hovered tile to nothing.
                currentHoveredTile = -Vector2Int.one;
            }

            //If we were dragging a piece & let go of it off the board...
            if(currentlyDragged && Input.GetMouseButtonUp(0))
            {
                //Grab the currently dragged pieces' starting tile's coordinates
                Vector2Int originalPosition = currentlyDragged.pieceCoordinates;
                //Move the piece back to it's last position
                currentlyDragged.SetPosition(GetTileCenter(tiles[originalPosition.x][originalPosition.y]));
                //set the currently dragged piece to null.
                currentlyDragged = null;

                //Clear the highlighted tiles
                ClearHighlightedTiles();
            }
        }

        //REMOVING PIECE ANIMATION
        //If a piece is currently moving across the board & attacking another piece...
        if (IsPieceAttacking)
        {
            //Wait until it's close enough to the attacked piece...
            if (Vector3.Distance(attackingPiece.transform.position, pieceToBeRemoved.transform.position) < .35f)
            {
                //Then remove the attacked from piece from play...
                GameObject.Destroy(pieceToBeRemoved.gameObject);
                //and finish the call.
                IsPieceAttacking = false;
            }
        }

        //DRAG ANIMATION
        //If we're currently dragging a chess piece...
        //TODO - PROBABLY WILL WANT TO HAVE THIS JUST FOLLOW THE MOUSE INSTEAD...
        if(currentlyDragged)
        {
            //Cast a plane onto the board to check for it's position
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * 1f);

            //Get it's distance from the board...
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                //Set it's new position to the distance
                currentlyDragged.SetPosition(ray.GetPoint(distance) + Vector3.up * 1.5f);
            }
        }
    }

    //COMENTED UP TILL HERE

    //TILE FUNCTIONS
    private GameObject GenerateSingleTile(int rowLocation, int columnLocation)
    {
        GameObject newTile = Instantiate(tile, new Vector3(rowLocation, 0, columnLocation), Quaternion.identity);
        newTile.gameObject.name = string.Format("Row: " + (rowLocation + 1) + " - Column: " + (columnLocation + 1));
        newTile.transform.parent = transform;
        Tile t = newTile.GetComponent<Tile>();

        int flipflopper;
        if(rowLocation % 2 == 0)
        {
            flipflopper = 0;
        }
        else
        {
            flipflopper = 1;
        }

        if(columnLocation % 2 == flipflopper)
        {
            t.SetTileVars(rowLocation, columnLocation, Material1);
        }
        else
        {
            t.SetTileVars(rowLocation, columnLocation, Material2);
        }

        return newTile;
    }

    private Vector3 GetTileCenter(GameObject curTile)
    {
        Renderer tileRender = curTile.GetComponent<Renderer>();
        float yOffset = tileRender.bounds.center.y != 0 ? tileRender.bounds.center.y + tileRender.bounds.center.y / 2 : 0.5f;

        return new Vector3(tileRender.bounds.center.x, yOffset, tileRender.bounds.center.z);
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x][availableMoves[i].y].GetComponent<Renderer>().material = HighlightedTileMaterial;
        }
    }

    private void ClearHighlightedTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x][availableMoves[i].y].GetComponent<Tile>().ResetTileColor();
        }

        availableMoves.Clear();
    }

    private void GenerateSquareBoard(int rowTiles, int columnTiles)
    {
        tiles = new Dictionary<int, Dictionary<int, GameObject>>();
        
        for(int i = 0; i < rowTiles; i++)
        {
            tiles[i] = new Dictionary<int, GameObject>();

            for(int j = 0; j < columnTiles; j++)
            {
                if (i == 0)
                {
                    if (j != 2 && j != 3 && j != 5)
                    //if (j >= 2 || j <= 5)
                    {
                        tiles[i][j] = GenerateSingleTile(i, j);
                    }
                }
                else if(i == 5 || i == 4)
                {
                    if(j != 3 && j != 4)
                    {
                        tiles[i][j] = GenerateSingleTile(i, j);
                    }
                }
                else
                {
                    tiles[i][j] = GenerateSingleTile(i, j);
                }
            }
        }

        
        //tiles[8] = new Dictionary<int, GameObject>();
       // tiles[8][6] = GenerateSingleTile(8, 6);
        //tiles[8][5] = GenerateSingleTile(8, 5);
        //tiles[8][4] = GenerateSingleTile(8, 4);

  //      tiles[8][2] = GenerateSingleTile(8, 2);
//        tiles[8][1] = GenerateSingleTile(8, 1);
       // tiles[8][0] = GenerateSingleTile(8, 0);
        
       // tiles[7][8] = GenerateSingleTile(7, 8);
    }

    //CHESS PIECE FUNCTIONS
    private ChessPiece SpawnChessPiece(ChessPieceType pieceType, Vector2Int newPos, TeamColor teamColor)
    {
        if(pieceType == 0 || teamColor == 0)
        {
            Debug.LogError("A piece read-in wrong from the piece layout object.");
            return null;
        }

        ChessPiece newPiece = Instantiate(Prefabs[(int)pieceType - 1], transform).GetComponent<ChessPiece>();

        newPiece.pieceType = pieceType;
        newPiece.teamColor = teamColor;
        newPiece.GetComponent<MeshRenderer>().material = TeamMaterials[(int)teamColor - 1];

        //activeChessPieces[chessPiece.teamColor].Find(obj => obj == chessPiece).pieceCoordinates = newPos;
        Tile selectedTile = tiles[newPos.x][newPos.y].GetComponent<Tile>();
        if (selectedTile.tilePlacements[0] == null)
        {
            PositionChessPiece(newPiece, selectedTile, newPos, true);
            return newPiece;
        }
        else
        {
            Debug.LogError("There are two pieces on the sime tile - Position is: " + newPos.x + "-" + newPos.y);
            return null;
        }
    }

    private void PositionChessPiece(ChessPiece chessPiece, Tile selectedTile, Vector2Int newPos, bool instantMovement = false)
    {
            chessPiece.pieceCoordinates = newPos;
            chessPiece.SetPosition(GetTileCenter(selectedTile.gameObject), instantMovement);
            selectedTile.tilePlacements[0] = chessPiece.gameObject;
    }

    private void CreatePiecesFromLayout(ChessPieceLayout pieceLayout)
    {
        activeChessPieces = new Dictionary<TeamColor, List<ChessPiece>>();

        for (int i = 0; i < pieceLayout.GetTotalPieceCount(); i++)
        {
            Vector2Int pieceCoords = pieceLayout.GetPieceCoordinates(i);
            TeamColor teamColor = pieceLayout.GetTeamColor(i);
            ChessPieceType pieceType = pieceLayout.GetPieceType(i);

            if(!activeChessPieces.ContainsKey(teamColor))
            {
                activeChessPieces[teamColor] = new List<ChessPiece>();
            }

            activeChessPieces[teamColor].Add(SpawnChessPiece(pieceType, pieceCoords, teamColor));
        }
    }

    //OPERATION FUNCTIONS
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0;  i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }

    //TODO - REWRITE THIS BECAUSE OH YM FUCKING GOD HOLY SHIT GOD NO
    private bool IsMoveValid(ChessPiece currentPiece, Vector2Int newPos)
    {
        if (!ContainsValidMove(ref availableMoves, newPos))
        {
            return false;
        }

        Tile newTile = tiles[newPos.x][newPos.y].GetComponent<Tile>();

        //Check if there's no other piece at that tile
        if (newTile.tilePlacements[0] == null)
        {
            Vector2Int oldPos = currentPiece.pieceCoordinates;
            tiles[oldPos.x][oldPos.y].GetComponent<Tile>().tilePlacements[0] = null;
            PositionChessPiece(currentPiece, newTile, newPos);
            return true;
        }
        else
        {
            //TODO - CLEAN UP & SAFETY NET THIS
            ChessPiece otherPiece = newTile.tilePlacements[0].GetComponent<ChessPiece>();

            if (otherPiece.teamColor == currentPiece.teamColor)
            {
                return false;
            }

            Vector2Int oldPos = currentPiece.pieceCoordinates;
            tiles[oldPos.x][oldPos.y].GetComponent<Tile>().tilePlacements[0] = null;
            tiles[newPos.x][newPos.y].GetComponent<Tile>().tilePlacements[0] = null;

            activeChessPieces[otherPiece.teamColor].Remove(otherPiece);
            //unactiveChessPieces[otherPiece.teamColor].Add(otherPiece);

            PositionChessPiece(currentPiece, newTile, newPos);

            attackingPiece = currentPiece;
            pieceToBeRemoved = otherPiece;
            IsPieceAttacking = true;

            return true;
        }
    }
}