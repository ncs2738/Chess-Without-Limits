using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class Chessboard : MonoBehaviour
{
    [SerializeField]
    private GameObject tile;
    [SerializeField]
    private int rows;
    [SerializeField]
    private int columns;

    public Material HighlightedTileMaterial;
    public Material[] TileMaterials;
    public Material[] TeamMaterials;
    public GameObject[] Prefabs;

    private Dictionary<int, Dictionary<int, Tile>> tiles;

    //Used for the audio componenet - nothing more!
    public Camera SceneCamera;
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

    private bool isGameOver = false;

    [SerializeField]
    private GameObject victoryScreen;
    [SerializeField]
    private Text victoryText;

    public bool IsBoardSquare = true;

    //Player data
    public GameObject PlayerPrefab;
    public int PlayerCount = 2;
    public List<GameObject> PlayerList = new List<GameObject>();
    public List<Player> PlayerTurnQueue = new List<Player>();
    private TeamColor activePlayer;
    private int currentTurnIndex = -1;
    public bool IsCouchCoOp = true;

    private Vector2 boardWidth;
    private Vector2 boardLength;

    private Vector3 LastCameraPosition;

    private bool displayGui = true;

    private Vector2Int noHover = new Vector2Int(-777, -777);

    //Coin-slotters for board dimensions
    private Dictionary<int, int> yDimensions = new Dictionary<int, int>();

    private void Awake()
    {
        SceneCamera.enabled = false;

        //On startup, generate our board - can be a stock-standard square board, or be jagged & rough
        if (IsBoardSquare)
        {
            GenerateSquareBoard(rows, columns);
        }
        else
        {
            GenerateJaggedBoard(rows, columns);
        }

        AddPlayers();

        //Then read in & spawn the pieces based off of the chess-piece layout script given
        CreatePiecesFromLayout(currentPieceLayout);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            OnExitButton();
        }

        if(Input.GetKeyUp(KeyCode.H))
        {
            displayGui = !displayGui;
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            if(IsBoardSquare)
            {
                SceneManager.LoadScene("JaggedBoard", LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene("SquareBoard", LoadSceneMode.Single);
            }
        }

        if (isGameOver)
        {
            return;
        }

        HandleInputs();
    }

    void OnGUI()
    {
       if(displayGui)
        {
            GUI.skin.label.fontSize = 30;
            string instructions1 = "Use WASD to turn & rotate the camera";
            string instructions2 = "You can also use the mouse to rotate the camera";
            string instructions3 = "By moving it to the border of the screen you want to move in.";
            string instructions4 = "Press Shift to speed up your movement speed";
            string instructions5 = "Turn the GUI on & off using the H key.";
            string instructions6 = "Press the B key to change the board type.";

            string instructions7 = "Zoom in & out with the middle-mouse button or R/F keys";
            string instructions8 = "Cycle through camera positions using the Q & E keys or the space bar";

            GUI.Label(new Rect(5, 10, Screen.width, Screen.height), instructions1);
            GUI.Label(new Rect(5, 60, Screen.width, Screen.height), instructions2);
            GUI.Label(new Rect(5, 110, Screen.width, Screen.height), instructions3);
            GUI.Label(new Rect(5, 160, Screen.width, Screen.height), instructions4);
            GUI.Label(new Rect(5, 210, Screen.width, Screen.height), instructions5);
            GUI.Label(new Rect(5, 260, Screen.width, Screen.height), instructions6);
            GUI.Label(new Rect(5, 960, Screen.width, Screen.height), instructions7);
            GUI.Label(new Rect(5, 1010, Screen.width, Screen.height), instructions8);
        }
    }

    //TODO - REWRITE THIS TO BE ACTUALLY CLEAN!
    private void AddPlayers()
    {
        //TODO - REWRITE THIS TO ACTUALLY TAKE INTO ACCOUNT MAX NUMBERS
        boardWidth.x = 0;
        boardLength.x = 0;
        boardWidth.y = rows;
        boardLength.y = columns;

        float midX = ((boardLength.y - boardLength.x) - 1) / 2;
        float midZ = ((boardWidth.y - boardWidth.x) - 1) / 2;

        for (int i = 0; i < PlayerCount; i++)
        {
            PlayerList.Add(Instantiate(PlayerPrefab, new Vector3(midX, 1, midZ), Quaternion.identity));
            PlayerTurnQueue.Add(PlayerList[i].GetComponent<Player>());
        }

        PlayerTurnQueue[0].InitiatePlayer(TeamColor.White, this);
        PlayerTurnQueue[1].InitiatePlayer(TeamColor.Black, this);

        //set up cameras
        PlayerTurnQueue[0].GetCamera().enabled = true;
        PlayerTurnQueue[1].GetCamera().enabled = false;
        currentCamera = PlayerTurnQueue[0].GetCamera();

        //End the player's set-up turn to start the game~!
        EndPlayerTurn();
    }


    //The following code is a modified version of Epitome's chess tutorial's code
    //Note - I will be HEAVILY modifying it in the future, but will be following their logic
    /***************************************************************************************
    *    Title:  Create an Online Chess Game - Placement Grid 
    *    Author: Epitome
    *    Date: Mar 30, 2021
    *    Availability: https://www.youtube.com/watch?v=FtGy7J8XD90&list=PLmcbjnHce7SeAUFouc3X9zqXxiPbCz8Zp&index=3
    ***************************************************************************************/
    //TODO - Break these functions up & move most of this unto the player class; this will allow players to highlight tiles without the other seeing
    private void HandleInputs()
    {
        //Do a raycast check to see if the player's hovering over any tiles (and soon to be pieces)
        RaycastHit raycast;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        //If the player's hovering over a tile...
        if (Physics.Raycast(ray, out raycast, 100, LayerMask.GetMask("Tile")))
        {
            //Grab the raycast's data
            GameObject hoveredTile = raycast.transform.gameObject;
            Tile t = hoveredTile.GetComponent<Tile>();
            //Debug.Log(hoveredTile + " " + t.tilePlacements[0].GetMappedType());

            Debug.Log(t.GetBoardIndex());

            Vector2Int hitPos;

            //SAFE-KEEPING: Make sure we're hovering over a tile.
            if (t)
            {
                hitPos = t.GetBoardIndex();
            }
            else
            {
                //safety-keeping: if this hits, then there's a problem
                hitPos = noHover;
            }

            //If we just started hovering over the board & don't have a "last-hovered tile"...
            if (currentHoveredTile == noHover)
            {
                //Set the current hovered tile to it
                currentHoveredTile = hitPos;
                //Set the tile's material to being highlighted
                tiles[hitPos.x][hitPos.y].gameObject.GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            //If we've been hovering over the board & have a "last-hovered tile"
            if (currentHoveredTile != hitPos)
            {
                if (!ContainsValidMove(ref availableMoves, currentHoveredTile))
                {
                    //Reset it's hovered-over material back to it's default color
                    tiles[currentHoveredTile.x][currentHoveredTile.y].ResetTileColor();
                }
                //Update the current hovered tile
                currentHoveredTile = hitPos;
                //Set it's material to being highlighted
                tiles[hitPos.x][hitPos.y].gameObject.GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            //If there's no pieces moving & attacking another on the board at moment...
            if (!IsPieceAttacking)
            {
                //Check for inputs:

                //If we just left-clicked on a tile...
                if (Input.GetMouseButtonDown(0))
                {
                    //If the currently hovered tile has a chess piece atop of it
                    if (t.tilePlacements[0].Contains(MappedTileType.ChessPiece))
                    {
                        //Get the chess piece...
                        ChessPiece tempPiece = t.tilePlacements[0].GetMappedClass() as ChessPiece;

                        // Is it the players piece?
                        //TODO -REWRITE THIS TO ACTUALLY TAKE PROPER CONSIDERATIONS
                        //if (tempPiece.teamColor == PlayersTeamColor)
                        if (activePlayer.Equals(tempPiece.teamColor))
                        {
                            //Select the player's piece
                            currentlyDragged = tempPiece;
                        }
                        else
                        {
                            //TODO: ADD THIS :0[
                            //Highlight the piece's possible moves?
                            return;
                        }

                        //Get the list of available moves for the piece
                        availableMoves = currentlyDragged.GetAvailableMoves(ref tiles);

                        //WORKING HERE
                        HighlightTiles();
                    }
                }

                //If we were dragging a piece & released the left-mouse button...
                if (currentlyDragged != null && Input.GetMouseButtonUp(0))
                {
                    //Grab the currently dragged pieces' starting tile's coordinates
                    Vector2Int originalPosition = currentlyDragged.pieceCoordinates;

                    //Check to see if the move was valid
                    bool isValidMove = IsValidMove(currentlyDragged, hitPos);

                    //If it wasnt...
                    if (!isValidMove)
                    {
                        //Move the piece back to it's last position
                        currentlyDragged.SetPosition(GetTileCenter(tiles[originalPosition.x][originalPosition.y].gameObject));
                    }
                    //The move was valid!
                    else
                    {
                        //Check if the piece has been inidiated or not...
                        if (!currentlyDragged.IsInitiated())
                        {
                            //It wasn't, so intiate it!
                            currentlyDragged.SetInitiatedStatus();
                        }

                        //End the player's turn
                        EndPlayerTurn();
                    }


                    //set the currently dragged piece to null.
                    currentlyDragged = null;

                    //Clear the highlighted tiles
                    ClearHighlightedTiles();
                }

                //If we just right-clicked on a tile while not dragging any pieces...
                if (currentlyDragged == null && Input.GetKeyDown(KeyCode.Mouse1))
                {
                    //aaaaaaaaaa

                    //isRemoved = DestroyTileColumn(hitPos);
                    //isRemoved = DestroyTile(t, hitPos);

                    // if (DestroyTile(t, hitPos.x, hitPos.y))
                    //if (DestroyTileRow(hitPos))
                    //if (DestroyTileColumn(hitPos))
                    //{
                    //Set our currently hovered tile to nothing.
                    //currentHoveredTile = noHover;
                    //}

                    //CreateTileColumn(hitPos, 1);
                    CreateTileRow(hitPos, 1);
                }
            }
        }

        //The player's raycast hit nothing...
        else
        {
            //If we have a currently dragged tile....
            if (currentHoveredTile != noHover)
            {
                if (!ContainsValidMove(ref availableMoves, currentHoveredTile))
                {
                    //Reset it's hovered-over material back to it's default color
                    tiles[currentHoveredTile.x][currentHoveredTile.y].ResetTileColor();
                }
                //Set our currently hovered tile to nothing.
                currentHoveredTile = noHover;
            }

            //If we were dragging a piece & let go of it off the board...
            if (currentlyDragged && Input.GetMouseButtonUp(0))
            {
                //Grab the currently dragged pieces' starting tile's coordinates
                Vector2Int originalPosition = currentlyDragged.pieceCoordinates;
                //Move the piece back to it's last position
                currentlyDragged.SetPosition(GetTileCenter(tiles[originalPosition.x][originalPosition.y].gameObject));
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

                if (PlayerTurnQueue.Count == 1)
                {
                    isGameOver = true;
                    DisplayVictoryScreen();
                }
            }
        }

        //DRAG ANIMATION
        //If we're currently dragging a chess piece...
        //TODO - PROBABLY WILL WANT TO HAVE THIS JUST FOLLOW THE MOUSE INSTEAD...
        if (currentlyDragged)
        {
            //Cast a plane onto the board to check for it's position
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * 1f);

            //Get it's distance from the board...
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                //Set it's new position to the distance
                currentlyDragged.SetPosition(ray.GetPoint(distance) + Vector3.up * 1.5f);
            }
        }
    }

    //COMENTED UP TILL HERE
    private bool DestroyTileRow(Vector2Int pos)
    {
        bool isInterrupted = false;

        //First, we need to check all the tiles across the row...
        //Start by looping through all x-tile dictionary...
        for (int x = tiles.Keys.Min(); x <= tiles.Keys.Max(); x++)
        {
            //Check if this dictionary has any tiles in the row we're deleting by using the current pos.y...
            if (tiles[x].ContainsKey(pos.y))
            {
                //There is a tile, so check if it has a piece on it...
                if (tiles[x][pos.y].tilePlacements[0].Contains(MappedTileType.ChessPiece))
                {
                    //The tile's not empty; we've been interrupted.
                    isInterrupted = true;
                    //Break out of the search.
                    break;
                }
            }
        }

        //If we can destroy the row...
        if (!isInterrupted)
        {
            //Grab the board's min & max y-values from the y-dimension list
            int minCol = yDimensions.Keys.Min();
            int maxCol = yDimensions.Keys.Max();

            //And grab the middle difference between the two...
            int midBoard = (maxCol - minCol) / 2;

            //Now loop through all the tiles until we get to the row we're deleting...
            //First loop through the x-dictionaries...
            for (int x = tiles.Keys.Min(); x <= tiles.Keys.Max(); x++)
            {
                //check if we're above or below the middle of the board...
                if (pos.y >= midBoard)
                {
                    //We're above! So start pos.y & move up to the top of the board.
                    //Check if we're not deleting a tile past this x-row's farthest y-tile...
                    if (maxCol >= pos.y)
                    {
                        //Start at our pos.y, then move up until we get to the end of the y-dictionaries...
                        for (int y = pos.y; y <= maxCol; y++)
                        {
                            //First check if the key exists in the x-dictionary...
                            if (tiles[x].ContainsKey(y))
                            {
                                //Then check if it's the tile we're deleting...
                                if (y == pos.y)
                                {
                                    //Destroy the tile...
                                    Destroy(tiles[x][y].gameObject);
                                    //And remove it from the dictionary
                                    tiles[x].Remove(y);

                                    //Remove the tally form the y-dimension total
                                    RemoveFromYDimensions(y);
                                }
                                else
                                {
                                    //Update the tile's board-index to be 1 row down.
                                    tiles[x][y].SetBoardIndex(x, y - 1);
                                    //Move the tiles to fill the gaps
                                    tiles[x][y].SetTilePosition(new Vector3Int(x, 0, y - 1));
                                    //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                                    tiles[x][y - 1] = tiles[x][y];
                                    //And remove the current tile from the dictionary
                                    tiles[x].Remove(y);

                                    //Remove the tally form the y-dimension total
                                    RemoveFromYDimensions(y);
                                    //And add it to it's new y-dimension
                                    AddToYDimensions(y - 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //We're below! So start at pos.y & move down to the bottom of the board.
                    //Check if we're not deleting a tile past this x-row's lowest y-tile...
                    if (pos.y >= minCol)
                    {
                        //Start at our pos.y, then move back until we get to the start of the y-dictionaries...
                        for (int y = pos.y; y >= minCol; y--)
                        {
                            //First check if the key exists in the x-dictionary...
                            if (tiles[x].ContainsKey(y))
                            {
                                //Then check if it's the tile we're deleting...
                                if (y == pos.y)
                                {
                                    //Destroy the tile...
                                    Destroy(tiles[x][y].gameObject);
                                    //And remove it from the dictionary
                                    tiles[x].Remove(y);
                                    //Remove the tally form the y-dimension total
                                    RemoveFromYDimensions(y);
                                }
                                else
                                {
                                    //Update the tile's board-index to be 1 row up.
                                    tiles[x][y].SetBoardIndex(x, y + 1);
                                    //Move the tiles to fill the gaps
                                    tiles[x][y].SetTilePosition(new Vector3Int(x, 0, y + 1));
                                    //And update the tile dictionary as well; move the tile ip 1 column in the dictionary...
                                    tiles[x][y + 1] = tiles[x][y];
                                    //And remove the current tile from the dictionary
                                    tiles[x].Remove(y);

                                    //Remove the tally form the y-dimension total
                                    RemoveFromYDimensions(y);
                                    //And add it to it's new y-dimension
                                    AddToYDimensions(y + 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        return !isInterrupted;
    }

    private bool DestroyTileColumn(Vector2Int pos)
    {
        bool isInterrupted = false;

        //First, grab the selected column's min & max y-key values
        int minCol = yDimensions.Keys.Min();
        int maxCol = yDimensions.Keys.Max();

        //Next, loop through all of the tiles in this column of the x-dictionary...
        for (int y = minCol; y <= maxCol; y++)
        {
            //Check if the x-dictionary contains a tile...
            if(tiles[pos.x].ContainsKey(y))
            {
                //There is a tile, so check if it has a piece on it...
                if (tiles[pos.x][y].tilePlacements[0].Contains(MappedTileType.ChessPiece))
                {
                    //The tile's not empty; we've been interrupted.
                    isInterrupted = true;
                    //Break out of the search.
                    break;
                }
            }
        }

        //Then see if we can delete the Column...
        if(!isInterrupted)
        {
            //We can, so loop through all of the tiles in the column again...
            for (int y = minCol; y <= maxCol; y++)
            {
                //Check if the x-dictionary contains a tile...
                if (tiles[pos.x].ContainsKey(y))
                {
                    //It does, so destroy the game object...
                    Destroy(tiles[pos.x][y].gameObject);
                    //And remove it from the dictionary.
                    tiles[pos.x].Remove(y);

                    //Remove the count from the y-dimensions list
                    RemoveFromYDimensions(y);
                }
            }

            //Now that the column's empty, remove it from the dictionary.
            tiles.Remove(pos.x);

            //Grab the board's min & max y-values from the y-dimension list
            int minRow = tiles.Keys.Min();
            int maxRow = tiles.Keys.Max();

            //And grab the middle difference between the two...
            int midBoard = (maxRow - minRow) / 2;

            //Next, check if we removed a column from the left or right of the board...
            if (pos.x < midBoard)
            {
                //We removed from the left!
                //Check if there's still remaining rows to me moved...
                if(pos.x > minRow)
                {
                    //Start at our pos.x - 1, then move left until we get to the start of the x-dictionaries...
                    for (int x = pos.x - 1; x >= minRow; x--)
                    {
                        //Loop through the entire y-dictionary
                        for (int y = minCol; y <= maxCol; y++)
                        {
                            //First check if the key exists in the x-dictionary...
                            if (tiles[x].ContainsKey(y))
                            {
                                //Update the tile's board-index to be 1 row down.
                                tiles[x][y].SetBoardIndex(x + 1, y);
                                //Move the tiles to fill the gaps
                                tiles[x][y].SetTilePosition(new Vector3Int(x + 1, 0, y));
                            }
                        }

                        //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                        tiles[x + 1] = tiles[x];
                        //And remove the current tile from the dictionary
                        tiles.Remove(x);
                    }
                }
            }
            else
            {
                //We removed from the right!
                //Check if there's still remaining rows to me moved...
                if (pos.x < maxRow)
                {
                    //Start at our pos.x + 1, then move right until we get to the end of the x-dictionaries...
                    for (int x = pos.x + 1; x <= maxRow; x++)
                    {
                        //Start at the end of the y-dictionaries, then move back until we get to our pos.y...
                        for (int y = minCol; y <= maxCol; y++)
                        {
                            //First check if the key exists in the x-dictionary...
                            if (tiles[x].ContainsKey(y))
                            {
                                //Update the tile's board-index to be 1 row down.
                                tiles[x][y].SetBoardIndex(x - 1, y);
                                //Move the tiles to fill the gaps
                                tiles[x][y].SetTilePosition(new Vector3Int(x - 1, 0, y));
                            }
                        }

                        //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                        tiles[x - 1] = tiles[x];
                        //And remove the current tile from the dictionary
                        tiles.Remove(x);
                    }
                }
            }
        }

        return !isInterrupted;
    }

    private bool DestroyTile(Tile selectedTile, int posX, int posY)
    {
        //If the currently tile & it's selected face does not have a chess piece atop of it
        if (!selectedTile.tilePlacements[0].Contains(MappedTileType.ChessPiece))
        {
            //Remove the tile from the y-row tile dictionary.
            tiles[posX].Remove(posY);

            //Check to see if that row is now empty...
            if(tiles[posX].Count == 0)
            {
                //it is empty, so destroy the row.
                tiles.Remove(posX);
            }

            //Now, destroy the tile gameobject.
            Destroy(selectedTile.gameObject);
            return true;
        }

        return false;
    }

    private void CreateTileRow(Vector2Int pos, int direction)
    {

        //Grab the board's min & max y-values from the y-dimension list
        int minCol = yDimensions.Keys.Min();
        int maxCol = yDimensions.Keys.Max();

        //And grab the middle difference between the two...
        int midBoard = (maxCol - minCol) / 2;

        //Grab the board's min & max x-values
        int minRow = tiles.Keys.Min();
        int maxRow = tiles.Keys.Max();

        //TODO explain offset - needed for inner-board creations cuz the original column may move
        int offset = 0;

        //First check if we're not creating a new column at either ends of the board...
        if ((pos.y != maxCol && direction == 1) || pos.y != minCol && direction == -1)
        {
            //We're creating a new column somewhere IN the board, so we'll have to move things over...

            //We'll have check if adding on the bottom or top half of the board, so we we'll need to compare against middle of the board...
            //Check if we're creating a new column on the left or right side of the board...
            if (pos.y <= midBoard)
            {
                //We're on the bottom!
                //We'll need to offset the direction; This is needed because the pos.y column is moving if a row is created below it.
                //If we're creating a row above of pos.y, we want the offset to be 0.
                //If it's below it, we'll want it to stay -1
                direction = (direction - 1) / 2;
                //And we'll need to create an off-set to check if we're copying the column next to the pos.x or the pos.x column itself
                //The offset will be the exact opposite of the direction; so right is -1 & left is 0.
                offset = (-1 * direction) - 1;

                //Loop through the entire dictionary...
                for (int x = minRow; x <= maxRow; x++)
                {
                    //Start at the end of the y-dictionaries, then move back until we get to our pos.y...
                    for (int y = minCol; y <= pos.y + direction; y++)
                    {
                        //First check if the key exists in the x-dictionary...
                        if (tiles[x].ContainsKey(y))
                        {
                            //Update the tile's board-index to be 1 row down.
                            tiles[x][y].SetBoardIndex(x, y - 1);
                            //Move the tiles to fill the gaps
                            tiles[x][y].SetTilePosition(new Vector3Int(x, 0, y - 1));
                            //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                            tiles[x][y - 1] = tiles[x][y];
                            //And remove the current tile from the dictionary
                            tiles[x].Remove(y);

                            //Remove the tally form the y-dimension total
                            RemoveFromYDimensions(y);
                            //And add it to it's new y-dimension
                            AddToYDimensions(y - 1);
                        }
                    }
                }
            }
            else
            {
                //We're on the right!
                //We'll need to offset the direction; This is needed because the pos.x column is moving if a column is created on the left of it.
                //If we're creating a column to the right of pos.x, we want the offset to stay 1.
                //If it's to the left, we'll want it to be 0
                direction = (1 + direction) / 2;
                //And we'll need to create an off-set to check if we're copying the column next to the pos.x or the pos.x column itself
                //The offset will be the exact opposite of the direction; so right is 0 & left is 1.
                offset = 1 - (1 * direction);

                //Loop through the entire dictionary...
                for (int x = minRow; x <= maxRow; x++)
                {
                    //Start at the end of the y-dictionaries, then move back until we get to our pos.y...
                    for (int y = maxCol; y >= pos.y + direction; y--)
                    {
                        //First check if the key exists in the x-dictionary...
                        if (tiles[x].ContainsKey(y))
                        {
                            //Update the tile's board-index to be 1 row down.
                            tiles[x][y].SetBoardIndex(x, y + 1);
                            //Move the tiles to fill the gaps
                            tiles[x][y].SetTilePosition(new Vector3Int(x, 0, y + 1));
                            //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                            tiles[x][y + 1] = tiles[x][y];
                            //And remove the current tile from the dictionary
                            tiles[x].Remove(y);

                            //Remove the tally form the y-dimension total
                            RemoveFromYDimensions(y);
                            //And add it to it's new y-dimension
                            AddToYDimensions(y + 1);
                        }
                    }
                }
            }
        }

        //Now we just create the new column.
        int newRow = pos.y + direction;

        //Loop through the x-dictionaries...
        for(int x = minRow; x <= maxCol; x++)
        {
            //SAFETY CHECK: Check if the tiles dictionary contains the current x-pos
            if(tiles.ContainsKey(x))
            {
                //Add & create the new tile
                tiles[x][newRow] = GenerateSingleTile(x, newRow);
            }
        }
    }

    private void CreateTileColumn(Vector2Int pos, int direction)
    {
        //TODO explain offset - needed for inner-board creations cuz the original column may move
        int offset = 0;

        //First check if we're not creating a new column at either ends of the board...
        if ((pos.x != tiles.Keys.Max() && direction == 1) || pos.x != tiles.Keys.Min() && direction == -1)
        {
            //We're creating a new column somewhere IN the board, so we'll have to move things over...

            //We'll have check if adding on the left or right half of the board, so we we'll need to compare against middle of the board...
            //Grab the board's min & max y-values from the y-dimension list
            int minRow = tiles.Keys.Min();
            int maxRow = tiles.Keys.Max();

            //And get the middle difference between the two...
            int midBoard = (maxRow - minRow) / 2;

            //Also grab the board's min & max y-values from the y-dimension list
            int minCol = yDimensions.Keys.Min();
            int maxCol = yDimensions.Keys.Max();

            //Check if we're creating a new column on the left or right side of the board...
            if (pos.x <= midBoard)
            {
                //We're on the left!
                //We'll need to offset the direction; This is needed because the pos.x column is moving if a column is created on the right of it.
                //If we're creating a column to the right of pos.x, we want the offset to be 0.
                //If it's to the left, we'll want it to stay -1
                direction = (direction - 1) / 2;
                //And we'll need to create an off-set to check if we're copying the column next to the pos.x or the pos.x column itself
                //The offset will be the exact opposite of the direction; so right is -1 & left is 0.
                offset = (-1 * direction) - 1;

                //Start at our pos.x + direction, then move right until we get to the end of the x-dictionaries...
                for (int x = minRow; x <= pos.x + direction; x++)
                {
                    //Start at the end of the y-dictionaries, then move back until we get to our pos.y...
                    for (int y = minCol; y <= maxCol; y++)
                    {
                        //First check if the key exists in the x-dictionary...
                        if (tiles[x].ContainsKey(y))
                        {
                            //Update the tile's board-index to be 1 row down.
                            tiles[x][y].SetBoardIndex(x - 1, y);
                            //Move the tiles to fill the gaps
                            tiles[x][y].SetTilePosition(new Vector3Int(x - 1, 0, y));
                        }
                    }

                    //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                    tiles[x - 1] = tiles[x];
                    //And remove the current tile from the dictionary
                    tiles.Remove(x);
                }
            }
            else
            {
                //We're on the right!
                //We'll need to offset the direction; This is needed because the pos.x column is moving if a column is created on the left of it.
                //If we're creating a column to the right of pos.x, we want the offset to stay 1.
                //If it's to the left, we'll want it to be 0
                direction = (1 + direction) / 2;
                //And we'll need to create an off-set to check if we're copying the column next to the pos.x or the pos.x column itself
                //The offset will be the exact opposite of the direction; so right is 0 & left is 1.
                offset = 1 - (1 * direction);

                //Start at our pos.x - direction, then move right until we get to the end of the x-dictionaries...
                for (int x = maxRow; x >= pos.x + direction; x--)
                {
                    //Start at the end of the y-dictionaries, then move back until we get to our pos.y...
                    for (int y = minCol; y <= maxCol; y++)
                    {
                        //First check if the key exists in the x-dictionary...
                        if (tiles[x].ContainsKey(y))
                        {
                            //Update the tile's board-index to be 1 row down.
                            tiles[x][y].SetBoardIndex(x + 1, y);
                            //Move the tiles to fill the gaps
                            tiles[x][y].SetTilePosition(new Vector3Int(x + 1, 0, y));
                        }
                    }

                    //And update the tile dictionary as well; move the tile down 1 column in the dictionary...
                    tiles[x + 1] = tiles[x];
                    //And remove the current tile from the dictionary
                    tiles.Remove(x);
                }
            }
        }

        //Now we just create the new column.
        int newColumn = pos.x + direction;

        //Create a new dictionary entry too.
        tiles[newColumn] = new Dictionary<int, Tile>();

        //Loop through & add the tiles.
        for (int y = tiles[pos.x + offset].Keys.Min(); y <= tiles[pos.x + offset].Keys.Max(); y++)
        {
            tiles[newColumn][y] = GenerateSingleTile(newColumn, y);
        }
    }


    //TILE FUNCTIONS
    private Tile GenerateSingleTile(int rowLocation, int columnLocation)
    {
        GameObject newTile = Instantiate(tile, new Vector3(rowLocation, 0, columnLocation), Quaternion.identity);
        newTile.gameObject.name = string.Format("Row: " + (rowLocation + 1) + " - Column: " + (columnLocation + 1));
        newTile.transform.parent = transform;
        Tile t = newTile.GetComponent<Tile>();

        //Add it's current-y location to the y-dimensions list
        AddToYDimensions(columnLocation);

        if(columnLocation % 2 == rowLocation % 2)
        {
            t.SetTileVars(rowLocation, columnLocation,  TeamMaterials[0]);
        }
        else
        {
            t.SetTileVars(rowLocation, columnLocation, TeamMaterials[1]);
        }

        return t;
    }

    private Vector3 GetTileCenter(GameObject curTile)
    {
        Renderer tileRender = curTile.gameObject.GetComponent<Renderer>();
        float yOffset = tileRender.bounds.center.y != 0 ? tileRender.bounds.center.y + tileRender.bounds.center.y / 2 : 0.5f;

        return new Vector3(tileRender.bounds.center.x, yOffset, tileRender.bounds.center.z);
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x][availableMoves[i].y].gameObject.GetComponent<Renderer>().material = HighlightedTileMaterial;
        }
    }

    private void ClearHighlightedTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x][availableMoves[i].y].ResetTileColor();
        }

        availableMoves.Clear();
    }

    private void GenerateSquareBoard(int rowTiles, int columnTiles)
    {
        tiles = new Dictionary<int, Dictionary<int, Tile>>();

        for (int i = 0; i < rowTiles; i++)
        {
            tiles[i] = new Dictionary<int, Tile>();

            for(int j = 0; j < columnTiles; j++)
            {
                tiles[i][j] = GenerateSingleTile(i, j);
            }
        }
    }

    private void AddToYDimensions(int yDimension)
    {
        if(!yDimensions.ContainsKey(yDimension))
        {
            yDimensions[yDimension] = 1;
        }
        else
        {
            yDimensions[yDimension]++;
        }
    }

    private void RemoveFromYDimensions(int yDimension)
    {
        yDimensions[yDimension]--;

        if (yDimensions[yDimension] == 0)
        {
            yDimensions.Remove(yDimension);
        }
    }

    //Used for testing
    private void GenerateJaggedBoard(int rowTiles, int columnTiles)
    {
        tiles = new Dictionary<int, Dictionary<int, Tile>>();

        for (int i = 0; i < rowTiles; i++)
        {
            tiles[i] = new Dictionary<int, Tile>();

            for (int j = 0; j < columnTiles; j++)
            {
                if (i == 0)
                {
                    if (j != 2 && j != 3 && j != 5)
                    {
                        tiles[i][j] = GenerateSingleTile(i, j);
                    }
                }
                else if (i == 5 || i == 4)
                {
                    if (j != 3 && j != 4)
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

        tiles[-1] = new Dictionary<int, Tile>();
        tiles[-1][0] = GenerateSingleTile(-1, 0);
        tiles[-1][-1] = GenerateSingleTile(-1, -1);
        tiles[-2] = new Dictionary<int, Tile>();
        tiles[-2][0] = GenerateSingleTile(-2, 0);
        tiles[-2][-1] = GenerateSingleTile(-2, -1);


        tiles[8] = new Dictionary<int, Tile>();
        tiles[8][6] = GenerateSingleTile(8, 6);
        tiles[8][5] = GenerateSingleTile(8, 5);
        tiles[8][4] = GenerateSingleTile(8, 4);

        tiles[8][2] = GenerateSingleTile(8, 2);
        tiles[8][1] = GenerateSingleTile(8, 1);
        tiles[8][0] = GenerateSingleTile(8, 0);

        tiles[7][8] = GenerateSingleTile(7, 8);

        tiles[9] = new Dictionary<int, Tile>();
        tiles[9][6] = GenerateSingleTile(9, 6);
        tiles[9][5] = GenerateSingleTile(9, 5);
        tiles[9][4] = GenerateSingleTile(9, 4);

        tiles[9][2] = GenerateSingleTile(9, 2);
        tiles[9][1] = GenerateSingleTile(9, 1);
        tiles[9][0] = GenerateSingleTile(9, 0);
    }

    //CHESS PIECE FUNCTIONS
    private ChessPiece SpawnChessPiece(ChessPieceType pieceType, Vector2Int newPos, TeamColor teamColor)
    {
        if(pieceType == 0 || teamColor == 0)
        {
            string errorMSG = pieceType == 0 ? "The piece type was Null" : "The piece's team color was Null";
            Debug.LogError("A piece read-in wrong from the piece layout object - " + errorMSG);
            return null;
        }

        ChessPiece newPiece = Instantiate(Prefabs[(int)pieceType - 1], transform).GetComponent<ChessPiece>();

        newPiece.pieceType = pieceType;
        newPiece.teamColor = teamColor;
        newPiece.GetComponent<MeshRenderer>().material = TeamMaterials[(int)teamColor - 1];

        //activeChessPieces[chessPiece.teamColor].Find(obj => obj == chessPiece).pieceCoordinates = newPos;
        Tile selectedTile = tiles[newPos.x][newPos.y];
        if (selectedTile.tilePlacements[0].Contains(MappedTileType.Empty))
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
            selectedTile.tilePlacements[0].SetMappedValues(MappedTileType.ChessPiece, chessPiece);
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


    //The following code in the IsValidMove function is a modified version of Epitome's chess tutorial's code
    /***************************************************************************************
    *    Title:  Create an Online Chess Game - Moving Pieces 
    *    Author: Epitome
    *    Date: Apr 6, 2021
    *    Availability: https://www.youtube.com/watch?v=3kW54hU98os&list=PLmcbjnHce7SeAUFouc3X9zqXxiPbCz8Zp&index=4
    ***************************************************************************************/
    private bool IsValidMove(ChessPiece currentPiece, Vector2Int newPos)
    {
        if (!ContainsValidMove(ref availableMoves, newPos))
        {
            return false;
        }

        Tile newTile = tiles[newPos.x][newPos.y];

        //Check if there's no other piece at that tile
        if (newTile.tilePlacements[0].Contains(MappedTileType.Empty))
        {
            Vector2Int oldPos = currentPiece.pieceCoordinates;
            tiles[oldPos.x][oldPos.y].tilePlacements[0].SetMappedValues(MappedTileType.Empty, null);
            PositionChessPiece(currentPiece, newTile, newPos);
            return true;
        }
        else if(newTile.tilePlacements[0].Contains(MappedTileType.ChessPiece))
        {
            //TODO - CLEAN UP & SAFETY NET THIS
            ChessPiece otherPiece = newTile.tilePlacements[0].GetMappedClass() as ChessPiece;

            if (otherPiece.teamColor == currentPiece.teamColor)
            {
                return false;
            }

            Vector2Int oldPos = currentPiece.pieceCoordinates;
            tiles[oldPos.x][oldPos.y].tilePlacements[0].SetMappedValues(MappedTileType.Empty, null);
            tiles[newPos.x][newPos.y].tilePlacements[0].SetMappedValues(MappedTileType.Empty, null);

            activeChessPieces[otherPiece.teamColor].Remove(otherPiece);
            //unactiveChessPieces[otherPiece.teamColor].Add(otherPiece);

            PositionChessPiece(currentPiece, newTile, newPos);

            attackingPiece = currentPiece;
            pieceToBeRemoved = otherPiece;
            IsPieceAttacking = true;

            IsGameOver(pieceToBeRemoved.teamColor);

            return true;
        }
        else
        {
            return false;
        }
    }

    private void EndPlayerTurn()
    {
        //TODO - find a better way to get the last player's camera cooridanates; this is gross as hell
        int curPlayer = (int) activePlayer - 1;
        int lastPlayerIndex = curPlayer >= 0 && curPlayer < PlayerTurnQueue.Count ? curPlayer : 0;
        LastCameraPosition = PlayerTurnQueue[lastPlayerIndex].GetCameraRotation();
        currentTurnIndex++;

        if(PlayerTurnQueue.Count > 1)
        {
            if (currentTurnIndex == PlayerTurnQueue.Count)
            {
                currentTurnIndex = 0;
            }

            activePlayer = PlayerTurnQueue[currentTurnIndex].GetPlayerTeamColor();
            UpdatePlayerCameras();
        }
    }

    private void IsGameOver(TeamColor attackedTeam)
    {
        if(!activeChessPieces[attackedTeam].Find(pieceType => pieceType.pieceType == ChessPieceType.King))
        {
            //todo - rewrite this to be not gross.
            PlayerTurnQueue.Remove(PlayerTurnQueue.Find(player => player.GetPlayerTeamColor() == attackedTeam));
        }
    }

    private void DisplayVictoryScreen()
    {
        victoryText.text = activePlayer.ToString() + " Wins!";
        victoryScreen.SetActive(true);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnResetButton()
    {
        victoryScreen.SetActive(false);

        //TODO - most likely way better to manually reset things, but for now, this shall suffice.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public TeamColor GetActivePlayer()
    {
        return activePlayer;
    }

    private void UpdatePlayerCameras()
    {
        int curPlayerIndex = (int)activePlayer - 1;

        for (int i = 0; i < PlayerTurnQueue.Count; i++)
        {
            if (i != currentTurnIndex)
            {
                PlayerTurnQueue[i].GetCamera().enabled = false;
            }

            PlayerTurnQueue[i].SetCameraPosition(LastCameraPosition);
        }

        PlayerTurnQueue[curPlayerIndex].GetCamera().enabled = true;
        PlayerTurnQueue[curPlayerIndex].SetPlayerCameraIndex(curPlayerIndex, true);
        currentCamera = PlayerTurnQueue[curPlayerIndex].GetCamera();
    }
}
