using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamasBoard : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public bool ehWhite;
    private bool ehWhiteTurn;
    private bool hasKilled;


    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private Piece pieceSelecionada;
    private List<Piece> forcedPieces;

    private Vector2 mouseOverPiece;
    private Vector2 pontoStart;
    private Vector2 pontoDestino;

    private void Start()
    {
        ehWhite = true;
        ehWhiteTurn = true;
        forcedPieces = new List<Piece>();
        GerarBoard();
    }

    private void Update()
    {
        UpdateMouseOverPiece();

        if((ehWhite)?ehWhiteTurn:!ehWhiteTurn)
        {
            int x = (int)mouseOverPiece.x;
            int y = (int)mouseOverPiece.y;

            if (pieceSelecionada != null)
                UpdatePieceSelecionada(pieceSelecionada);

            if (Input.GetMouseButtonDown(0))
                SelecionarPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                TentarMover((int)pontoStart.x, (int)pontoStart.y, x, y); //pontoDestino??
        }
    }

    private void UpdateMouseOverPiece()
    {
        if (!Camera.main)
        {
            Debug.Log("Main Camera Not Found.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOverPiece.x = (int)(hit.point.x - boardOffset.x);
            mouseOverPiece.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOverPiece.x = -1;
            mouseOverPiece.y = -1;
        }
    }

    //Piece selected to drag to destine
    private void UpdatePieceSelecionada(Piece p)
    {
        if(!Camera.main)
        {
            Debug.Log("Main Camera Not Found.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }

    private void SelecionarPiece(int x, int y)
    {
        if (x < 0 || x >= pieces.GetLength(0) ||
            y < 0 || y >= pieces.GetLength(0))
            return;

        Piece p = pieces[x, y];
        if(p != null && p.isWhite == ehWhite)
        {
            if(forcedPieces.Count == 0) {
                pieceSelecionada = p;
                pontoStart = mouseOverPiece;
            } else {
                //look for the piece under forcedPieces list
                if(forcedPieces.Find(fp => fp == p) == null) {
                    return;
                } else {
                    pieceSelecionada = p;
                    pontoStart = mouseOverPiece;
                    
                }
            }
        }
    }

    private void GerarBoard()
    {
        //white team
        for(int y=0; y<3; y++)
        {
            bool pecasOdd = (y % 2 == 0);
            for(int x=0; x<8; x+=2)
            {
                //pieces
                GerarPiece((pecasOdd) ? x : x+1, y);
            }
        }

        //black team
        for (int y = 7; y > 4; y--)
        {
            bool pecasOdd = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                //pieces
                GerarPiece((pecasOdd) ? x : x + 1, y);
            }
        }
    }
	
    private void GerarPiece(int x, int y)
    {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject gameObject = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        gameObject.transform.SetParent(transform);

        Piece p = gameObject.GetComponent<Piece>();
        pieces[x, y] = p;

        MoverPiece(p, x, y);
    }

    private void MoverPiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }

    //2Players
    private void TentarMover(int x1, int y1, int x2, int y2)
    {

        forcedPieces = ScanForPossibleMove();

        //2Players
        pontoStart = new Vector2(x1, y1);
        pontoDestino = new Vector2(x2, y2);
        pieceSelecionada = pieces[x1, y1];

        if(x2 < 0 || x2 >= pieces.GetLength(0) ||
           y2 < 0 || y2 >= pieces.GetLength(0))
        {
            if (pieceSelecionada != null)
                MoverPiece(pieceSelecionada, x1, y1);

            pontoStart = Vector2.zero;
            pieceSelecionada = null;
            return;
        }

        if(pieceSelecionada != null)
        {
            //it not moved?
            if(pontoDestino == pontoStart)
            {
                MoverPiece(pieceSelecionada, x1, y1);
                pontoStart = Vector2.zero;
                pieceSelecionada = null;
                return;
            }

            //valid move?
            if (pieceSelecionada.ValidMove(pieces, x1, y1, x2, y2))
            {
                //kill a piece than jump
                if(Mathf.Abs(x2-x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if(p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        hasKilled = true;
                        Destroy(p.gameObject);
                    }
                }

                // where we suposed to kill anything
                if(forcedPieces.Count != 0 && !hasKilled) {
                     MoverPiece(pieceSelecionada, x1, y1);
                    pontoStart = Vector2.zero;
                    pieceSelecionada = null;
                    return;
                }

                pieces[x2, y2] = pieceSelecionada;
                pieces[x1, y1] = null;
                MoverPiece(pieceSelecionada, x2, y2);

                EndTurn();
            } else {
                MoverPiece(pieceSelecionada, x1, y1);
                pontoStart = Vector2.zero;
                pieceSelecionada = null;
                return;
            }
        }
    }

    private void EndTurn()
    {
        int x = (int)pontoDestino.x;
        int y = (int)pontoDestino.y;

        //promotions
        if(pieceSelecionada != null) {
            if(pieceSelecionada.isWhite && !pieceSelecionada.isKing && y == (pieces.GetLength(0)-1)) {
                pieceSelecionada.isKing = true;
                pieceSelecionada.transform.Rotate(Vector3.right * 180);
            } else if(!pieceSelecionada.isWhite && !pieceSelecionada.isKing && y == 0) {
                pieceSelecionada.isKing = true;
                pieceSelecionada.transform.Rotate(Vector3.right * 180);
            }
        }

        pieceSelecionada = null;
        pontoStart = Vector2.zero;

        if(ScanForPossibleMove(pieceSelecionada, x, y).Count != 0 && hasKilled) {
            return;
        }

        ehWhiteTurn = !ehWhiteTurn;
        //ehWhite = !ehWhite;
        hasKilled = false;
        ChecarVitoria();
    }

    private List<Piece> ScanForPossibleMove(Piece p, int x, int y) {
        forcedPieces = new List<Piece>();

        if(pieces[x,y].IsForceToMove(pieces, x, y)){
            forcedPieces.Add(pieces[x,y]);
        }
        return forcedPieces;
    }

    private List<Piece> ScanForPossibleMove() {
        forcedPieces = new List<Piece>();

        //check all pieces
        for(int i = 0; i < pieces.GetLength(0); i++) {
            for(int j = 0; j < pieces.GetLength(0); j++) {
                if(pieces[i,j] != null && pieces[i,j].isWhite == ehWhiteTurn) {
                    if(pieces[i,j].IsForceToMove(pieces, i, j)) {
                        forcedPieces.Add(pieces[i,j]);
                    }
                }
            }
        }
        return forcedPieces;
    }

    //END Game
    public void ChecarVitoria()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for(int i = 0; i < ps.Length; i++) {
            if(ps[i].isWhite) {
                hasWhite = true;
            } else {
                hasBlack = true;
            }
        }

        if(!hasWhite) {
            Vitoria(false);
        } 
        if (!hasBlack) {
            Vitoria(true);
        }
    }

    public void Vitoria(bool white) {
        if(white)
            Debug.Log("White team has won!");
        else 
            Debug.Log("Black team has won!");
    }
}
