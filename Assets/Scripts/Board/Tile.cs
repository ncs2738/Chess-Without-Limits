using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private int rowIndex;
    [SerializeField]
    private int columnIndex;

    [SerializeField]
    public Material tileColor;

    //will hold 4 objects - null, chesspiece, tile, boarder 
    [SerializeField]
    public List<GameObject> tilePlacements = new List<GameObject>();

    public void Awake()
    {
        //TODO: CHANGE THIS TO 5 ADDITIONS
        tilePlacements.Add(null);
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
}
