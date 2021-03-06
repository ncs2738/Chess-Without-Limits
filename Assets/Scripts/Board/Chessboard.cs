using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
           // Debug.Log(hoveredTile + " " + t.tilePlacements[0].GetMappedType());
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
            }
        }

        //The player's raycast hit nothing...
        else
        {
            //If we have a currently dragged tile....
            if (currentHoveredTile != -Vector2Int.one)
            {
                if (!ContainsValidMove(ref availableMoves, currentHoveredTile))
                {
                    //Reset it's hovered-over material back to it's default color
                    tiles[currentHoveredTile.x][currentHoveredTile.y].ResetTileColor();
                }
                //Set our currently hovered tile to nothing.
                currentHoveredTile = -Vector2Int.one;
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

    //TILE FUNCTIONS
    private Tile GenerateSingleTile(int rowLocation, int columnLocation)
    {
        GameObject newTile = Instantiate(tile, new Vector3(rowLocation, 0, columnLocation), Quaternion.identity);
        newTile.gameObject.name = string.Format("Row: " + (rowLocation + 1) + " - Column: " + (columnLocation + 1));
        newTile.transform.parent = transform;
        Tile t = newTile.GetComponent<Tile>();

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

        tiles[8] = new Dictionary<int, Tile>();
        tiles[8][6] = GenerateSingleTile(8, 6);
        tiles[8][5] = GenerateSingleTile(8, 5);
        tiles[8][4] = GenerateSingleTile(8, 4);

        tiles[8][2] = GenerateSingleTile(8, 2);
        tiles[8][1] = GenerateSingleTile(8, 1);
        tiles[8][0] = GenerateSingleTile(8, 0);

        tiles[7][8] = GenerateSingleTile(7, 8);
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
