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

        if(Physics.Raycast(ray, out raycast, 100, LayerMask.GetMask("Tile")))
        {
            GameObject hoveredTile = raycast.transform.gameObject;
            Debug.Log(hoveredTile);
            Tile t = hoveredTile.GetComponent<Tile>();
            Vector2Int hitPos;

            if(t)
            {
                hitPos = t.GetBoardIndex();
            }
            else
            {
                //safety-keeping: if this hits, then there's a problem
                hitPos = -Vector2Int.one;
            }

            if(currentHoveredTile == -Vector2Int.one)
            {
                currentHoveredTile = hitPos;
                //tilles[hitPos.x][hitPos.y].layer = LayerMask.NameToLayer("HoveredTile");
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
            }

            else if (currentHoveredTile != hitPos)
            {
                tiles[currentHoveredTile.x][currentHoveredTile.y].GetComponent<Tile>().ResetTileColor();
                //tilles[currentHoveredTile.x][currentHoveredTile.y].layer = LayerMask.NameToLayer("Tile");
                currentHoveredTile = hitPos;
                //tilles[hitPos.x][hitPos.y].layer = LayerMask.NameToLayer("HoveredTile");
                tiles[hitPos.x][hitPos.y].GetComponent<Renderer>().material = HighlightedTileMaterial;
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
        }
    }

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
        chessPiece.pieceCoordinates = newPos;
        chessPiece.transform.position = new Vector3(newPos.x, 1, newPos.y);
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
}
