using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : BoardObject
{
    [SerializeField]
    private int rowIndex;
    [SerializeField]
    private int columnIndex;

    [SerializeField]
    public Material tileColor;

    //will hold 4 objects - null, chesspiece, tile, boarder 
    [SerializeField]
    public List<MappedTileValue> tilePlacements = new List<MappedTileValue>();

    public void Awake()
    {
        //TODO: CHANGE THIS TO 5 ADDITIONS
        tilePlacements.Add(new MappedTileValue(MappedTileType.Empty, null));
    }


    public void SetTileVars(int _rowIndex, int _columnIndex, Material _tileColor)
    {
        rowIndex = _rowIndex;
        columnIndex = _columnIndex;
        tileColor = _tileColor;
        GetComponent<Renderer>().material = tileColor;
    }

    public void SetBoardIndex(int _rowIndex, int _columnIndex)
    {
        rowIndex = _rowIndex;
        columnIndex = _columnIndex;
    }

    public Vector2Int GetBoardIndex()
    {
        Vector2Int boardIndex = new Vector2Int(rowIndex, columnIndex);
        return boardIndex;
    }

    public void ResetTileColor()
    {
        GetComponent<Renderer>().material = tileColor;
    }

    public Material GetTileColor()
    {
        return tileColor;
    }

    public bool CanPieceAttack(int placementIndex, TeamColor teamColor)
    {
        if (tilePlacements[placementIndex].Contains(MappedTileType.ChessPiece) && GetPieceType(placementIndex).teamColor != teamColor)
        {
            return true;
        }

        return false;
    }

    private ChessPiece GetPieceType(int placementIndex)
    {
        return tilePlacements[placementIndex].GetMappedClass() as ChessPiece;
    }
}
