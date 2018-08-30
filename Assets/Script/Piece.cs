using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;

    public bool IsForceToMove(Piece[,] board, int x, int y) {
        if(isWhite || isKing) {
            //top left
            if(x >= 2 && y <= 5) {
                Piece p = board[x - 1, y + 1];
                //if there is a piece, and is not the same color as ours
                if(p != null && p.isWhite != isWhite) {
                    //check if is possible to land after jump
                    if(board[x - 2, y + 2] == null) {
                        return true;
                    }
                }
            }

            //top right
            if(x <= 5 && y <= 5) {
                Piece p = board[x + 1, y + 1];
                //if there is a piece, and is not the same color as ours
                if(p != null && p.isWhite != isWhite) {
                    //check if is possible to land after jump
                    if(board[x + 2, y + 2] == null) {
                        return true;
                    }
                }
            }
        } 

        if(!isWhite || isKing) {
            //botton left
            if(x >= 2 && y >= 2) {
                Piece p = board[x - 1, y - 1];
                //if there is a piece, and is not the same color as ours
                if(p != null && p.isWhite != isWhite) {
                    //check if is possible to land after jump
                    if(board[x - 2, y - 2] == null) {
                        return true;
                    }
                }
            }

            //botton right
            if(x <= 5 && y >= 2) {
                Piece p = board[x + 1, y - 1];
                //if there is a piece, and is not the same color as ours
                if(p != null && p.isWhite != isWhite) {
                    //check if is possible to land after jump
                    if(board[x + 2, y - 2] == null) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2) {
        //on top of another pieces?
        if (board[x2, y2] != null)
            return false;

        int movimentoAbsolutoX = Mathf.Abs(x1 - x2);
        int movimentoAbsolutoY = y2 - y1; // cannot be absolute because need a -1 for the black piece

        if (isWhite || isKing)
        {
            if (movimentoAbsolutoX == 1) // move just one up
            {
                if (movimentoAbsolutoY == 1)
                    return true;
            }
            else if (movimentoAbsolutoX == 2) // killer(eater) move
            {
                if (movimentoAbsolutoY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }

        //black pieces
        if (!isWhite || isKing)
        {
            if (movimentoAbsolutoX == 1) // move just one up
            {
                if (movimentoAbsolutoY == -1)
                    return true;
            }
            else if (movimentoAbsolutoX == 2) // killer(eater) move
            {
                if (movimentoAbsolutoY == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }
        return false;
    }
}