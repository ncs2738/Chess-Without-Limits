using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MappedTileType
{
    Empty = 0,
    ChessPiece = 1,
    Tile = 2,
    Boarder = 3,
}


public class MappedTileValue
{
    private MappedTileType mappedType;
    private BoardObject mappedClass;

    public MappedTileValue()
    {
        mappedType = MappedTileType.Empty;
        mappedClass = null;
    }

    public MappedTileValue(MappedTileType _mappedType, BoardObject _mappedClass)
    {
        mappedType = _mappedType;
        mappedClass = _mappedClass;
    }

    public void SetMappedValues(MappedTileType associatedType, BoardObject associatedClass)
    {
        mappedType = associatedType;
        mappedClass = associatedClass;
    }

    public MappedTileType GetMappedType()
    {
        return mappedType;
    }

    public BoardObject GetMappedClass()
    {
        return mappedClass;
    }

    public bool Contains(MappedTileType typeCheck)
    {
        if(mappedType == typeCheck)
        {
            return true;
        }
        return false;
    }
}
