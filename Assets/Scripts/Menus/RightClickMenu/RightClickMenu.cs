using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightClickMenu : MonoBehaviour
{
    private Chessboard board;

    private void OnEnable()
    {
        board.SetRCMStatus(true);
    }

    private void OnDisable()
    {
        board.SetRCMStatus(false);
    }

    public void SetBoard(Chessboard _board)
    {
        board = _board;
    }
}
