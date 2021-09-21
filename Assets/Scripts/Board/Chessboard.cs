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

    private bool IsPieceMoving = false;


    private void Awake()
    {
        GenerateSquareBoard(rows, columns);
        CreatePiecesFromLayout(currentPieceLayout);
    }

    private void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }

        RaycastHit raycast;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out raycast, 100, LayerMask.GetMask("Tile")))
        {
            GameObject hoveredTile = raycast.transform.gameObject;
            Debug.Log(hoveredTile);
            Tile t = hoveredTile.GetComponent<Tile>();
            Vector2Int hitPos;


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
                currentHoveredTile = hitPos;
                //tilles[hitPos.x][hitPos.y].layer = LayerMask.NameToLayer("HoveredTile");
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            //If we've been hovering over the board & have a "last-hovered tile"
            if (currentHoveredTile != hitPos)
            {
                tiles[currentHoveredTile.x][currentHoveredTile.y].GetComponent<Tile>().ResetTileColor();
                //tilles[currentHoveredTile.x][currentHoveredTile.y].layer = LayerMask.NameToLayer("Tile");
                currentHoveredTile = hitPos;
                //tilles[hitPos.x][hitPos.y].layer = LayerMask.NameToLayer("HoveredTile");
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            if(!IsPieceMoving)
            {
                //If we just left-clicked on a tile...
                if (Input.GetMouseButtonDown(0))
                {
                    //TODO: CHECK IF ITS A CHECK PIECE ATOP OF IT
                    //And the tile has a chess piece on it
                    if (t != null && t.tilePlacements[0] != null)
                    {
                        if (t.tilePlacements[0].GetComponent<ChessPiece>())
                        {
                            ChessPiece tempPiece = t.tilePlacements[0].GetComponent<ChessPiece>();

                            //TODO -REWRITE THIS TO ACTUALLY TAKE PROPER CONSIDERATIONS
                            // Is it the players piece?
                            if (tempPiece.teamColor == TeamColor.Black)
                            {
                                currentlyDragged = tempPiece;
                            }
                            else
                            {
                                currentlyDragged = tempPiece;
                            }
                        }
                    }
                }

                //If we were dragging a piece & released the left-mouse button...
                if (currentlyDragged != null && Input.GetMouseButtonUp(0))
                {
                    Vector2Int prevPosition = currentlyDragged.pieceCoordinates;

                    bool isMoveValid = IsMoveValid(currentlyDragged, hitPos);
                    if (!isMoveValid)
                    {
                        currentlyDragged.SetPosition(GetTileCenter(tiles[prevPosition.x][prevPosition.y]));
                        currentlyDragged = null;
                    }
                    else
                    {
                        currentlyDragged = null;
                    }
                }
            }
        }
        else
        {
            if (currentHoveredTile != -Vector2Int.one)
            {
                tiles[currentHoveredTile.x][currentHoveredTile.y].GetComponent<Tile>().ResetTileColor();
                //tilles[currentHoveredTile.x][currentHoveredTile.y].layer = LayerMask.NameToLayer("Tile");
                currentHoveredTile = -Vector2Int.one;
            }

            if(currentlyDragged && Input.GetMouseButtonUp(0))
            {
                Vector2Int prevPosition = currentlyDragged.pieceCoordinates;
                currentlyDragged.SetPosition(GetTileCenter(tiles[prevPosition.x][prevPosition.y]));
                currentlyDragged = null;
            }
        }

        //REMOVING PIECE ANIMATION
        if(IsPieceMoving)
        {
            if (Vector3.Distance(attackingPiece.transform.position, pieceToBeRemoved.transform.position) < .35f)
            {
                GameObject.Destroy(pieceToBeRemoved.gameObject);
                IsPieceMoving = false;
            }
        }

        //DRAG ANIMATION
        if(currentlyDragged)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * 1f);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragged.SetPosition(ray.GetPoint(distance) + Vector3.up * 1.5f);
            }
        }
    }

    private GameObject GenerateSingleTile(int rowLocation, int columnLocation)
    {
        GameObject newTile = Instantiate(tile, new Vector3(rowLocation, -0.5f, columnLocation), Quaternion.identity);
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

    private void GenerateSquareBoard(int rowTiles, int columnTiles)
    {
        tiles = new Dictionary<int, Dictionary<int, GameObject>>();
        //tiles = new GameObject[rowTiles, columnTiles];
        
        for(int i = 0; i < rowTiles; i++)
        {
            //tilles.Add(rowTiles, new Dictionary<int, GameObject>());
            tiles[i] = new Dictionary<int, GameObject>();

            for(int j = 0; j < columnTiles; j++)
            {
                tiles[i][j] = GenerateSingleTile(i, j);
                //    tilles[rowTiles].Add(columnTiles, GenerateSingleTile(i, j));
                // tiles[i, j] = GenerateSingleTile(i, j);
            }
        }
    }

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

        PositionChessPiece(newPiece, newPos, true);

        return newPiece;
    }

    private void PositionChessPiece(ChessPiece chessPiece, Vector2Int newPos, bool instantMovement = false)
    {
        //activeChessPieces[chessPiece.teamColor].Find(obj => obj == chessPiece).pieceCoordinates = newPos;
        Tile selectedTile = tiles[newPos.x][newPos.y].GetComponent<Tile>();

        if (selectedTile.tilePlacements[0] == null)
        {
            chessPiece.pieceCoordinates = newPos;
            //chessPiece.transform.position = new Vector3(newPos.x, 1, newPos.y);
            //chessPiece.transform.position = selectedTile.GetComponent<Renderer>().bounds.center;
            //chessPiece.transform.position = GetTileCenter(selectedTile.gameObject);
            chessPiece.SetPosition(GetTileCenter(selectedTile.gameObject), instantMovement);
            selectedTile.tilePlacements[0] = chessPiece.gameObject;
        }
        else
        {
            Debug.LogError("There are two pieces on the sime tile - Position is: " + newPos.x + "-" + newPos.y);
        }
    }

    private Vector3 GetTileCenter(GameObject curTile)
    {
        Renderer tileRender = curTile.GetComponent<Renderer>();
        return new Vector3(tileRender.bounds.center.x, Mathf.Abs(tileRender.bounds.center.y + tileRender.bounds.center.y/2), tileRender.bounds.center.z);
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

    private bool IsMoveValid(ChessPiece currentPiece, Vector2Int newPos)
    {
        Tile newTile = tiles[newPos.x][newPos.y].GetComponent<Tile>();
        //Check if there's no other piece at that tile
        if (newTile.tilePlacements[0] == null)
        {
            Vector2Int oldPos = currentPiece.pieceCoordinates;
            tiles[oldPos.x][oldPos.y].GetComponent<Tile>().tilePlacements[0] = null;
            PositionChessPiece(currentPiece, newPos);
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

            PositionChessPiece(currentPiece, newPos);

            attackingPiece = currentPiece;
            pieceToBeRemoved = otherPiece;
            IsPieceMoving = true;

            return true;
        }
    }
}