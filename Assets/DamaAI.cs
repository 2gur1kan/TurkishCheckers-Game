using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamaAI : MonoBehaviour
{
    private DamaController DC;
    private int[][] board;

    public bool again = false;
    public int jumper = -1;//yiyen eleman

    void Start()
    {
        DC = GetComponent<DamaController>();
    }

    public void AIStart()
    {
        float startTime = Time.realtimeSinceStartup;

        AI();

        float endTime = Time.realtimeSinceStartup;
        float elapsedTimeMilliseconds = (endTime - startTime) * 1000; // Saniyeden milisaniyeye dönüþtürme

        Debug.Log("Fonksiyonun çalýþma süresi: " + elapsedTimeMilliseconds.ToString("F2") + " milisaniye");
        
    }

    private void AI()
    {
        DC.startAI = false;
        CheckBoard();

        if (again)
        {
            Move againMove = FindBestAgainMove();
            StartCoroutine(PlayTheMove(againMove));
            return;
        }

        Move bestMove = FindBestMove();
        if (bestMove != null)
        {
            StartCoroutine(PlayTheMove(bestMove));
        }
    }

    private IEnumerator PlayTheMove(Move move)
    {
        jumper = move.To;

        yield return new WaitForSeconds(.5f);

        DC.setSelectedSquare(move.From);

        yield return new WaitForSeconds(.5f);

        if (move.State == state.move) DC.MovePawn(move.To);
        else if (move.State == state.eat) DC.EatPawn(move.Eat);

        if (move.State == state.move || !again) DC.tour = true;

        DC.startAI = false;
    }

    private Move FindBestAgainMove()
    {
        List<Move> possibleMoves = GetPossibleAgainMove(0);// çiftleri ai kontrole diyor
        Move bestMove = null;
        int bestScore = int.MinValue;

        foreach (Move move in possibleMoves)
        {
            int[][] newBoard = MakeMove(move);
            int score = EvaluateBoard(newBoard, 0);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private List<Move> GetPossibleAgainMove(int type)
    {
        int x = jumper / 8;
        int z = jumper % 8;

        List<Move> moves = new List<Move>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0 && board[i][j] % 2 == type)
                {
                    AddPossibleMoves(i, j, moves);
                }
            }
        }

        if (moves.Count < 1) 
        {
            again = false;//yeme hamlesi yoksa durur
            moves.Add(new Move(jumper, jumper));
        }

        return moves;
    }

    /// <summary>
    /// boardýn durumunu alýr
    /// </summary>
    private void CheckBoard()
    {
        board = DC.board;
    }

    /// <summary>
    /// bütün hamleleri hesapladýktan sorna en iyi 11 hamleyi alýr ve onalarýda tekrar hesaplar tekrarlar ve 15. defanýn ardýndan en iyi sonucu döndürür  
    /// </summary>
    /// <returns></returns>
    private Move FindBestMove()
    {
        List<Move> possibleMoves;

        if (again) possibleMoves = GetPossibleMoves(0, jumper);
        else possibleMoves = GetPossibleMoves(0);// çiftleri ai kontrole diyor

        Move bestMove = null;
        int bestScore = int.MinValue;

        foreach (Move move in possibleMoves)
        {
            int[][] newBoard = MakeMove(move);
            int score = EvaluateBoard(newBoard, 0);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    /// <summary>
    /// verilen tipteki taþlarýn bütün hareketlerini bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private List<Move> GetPossibleMoves(int type)
    {
        List<Move> moves = new List<Move>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0 && board[i][j] % 2 == type)
                {
                    AddPossibleMoves(i, j, moves);
                }
            }
        }
        return moves;
    }

    /// <summary>
    /// verilen tipteki taþlarýn bütün hareketlerini bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private List<Move> GetPossibleMoves(int type, int square)
    {
        int x = square / 8;
        int z = square % 8;

        List<Move> moves = new List<Move>();

        AddPossibleMoves(x, z, moves);

        return moves;
    }

    /// <summary>
    /// verilen taþýn muhtemel hamlelerinin hepsini hesaplar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="moves"></param>
    private void AddPossibleMoves(int row, int col, List<Move> moves)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama taþýnýn hareketleri
            AddDamaMoves(row, col, moves);
        }
        else if(pawn > 0)
        {
            // Normal taþýn hareketleri
            AddNormalMoves(row, col, moves);
        }
    }

    /// <summary>
    /// dama pawnlarýnýn hareket edebileceði yerleri hesaplar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="moves"></param>
    private void AddDamaMoves(int row, int col, List<Move> moves)
    {
        // yeme

        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        for (int i = col - 1; i > -1; i--)
        {
            if (IsValidMoveCol(row, i, reverseType) && IsValidMoveCol(row, i - 1)) 
            {
                for(int j = i - 1; j > -1; j--)
                {
                    if (!IsValidMoveCol(row, j)) break;
                    moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i));
                }
                break;
            }
        }
        for (int i = col + 1; i < 8; i++)
        {
            if (IsValidMoveCol(row, i, reverseType) && IsValidMoveCol(row, i + 1))
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (!IsValidMoveCol(row, j)) break;
                    moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i));
                }
                break;
            }
        }
        for (int i = row + 1; i < 8; i++)
        {
            if (IsValidMoveRow(col, i, reverseType) && IsValidMoveRow(col, i + 1))
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (!IsValidMoveRow(col, j)) break;
                    moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col));
                }
                break;
            }
        }
        for (int i = row - 1; i > -1; i--)
        {
            if (IsValidMoveRow(col, i, reverseType) && IsValidMoveRow(col, i - 1))
            {
                for (int j = i - 1; j > -1; j--)
                {
                    if (!IsValidMoveRow(col, j)) break;
                    moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col));
                }
                break;
            }
        }

        // hareket

        if (!again)
        {
            for (int i = col - 1; IsValidMoveCol(row, i); i--)
            {
                moves.Add(new Move(row * 8 + col, row * 8 + i));
            }
            for (int i = col + 1; IsValidMoveCol(row, i); i++)
            {
                moves.Add(new Move(row * 8 + col, row * 8 + i));
            }
            for (int i = row + 1; IsValidMoveRow(col, i); i++)
            {
                moves.Add(new Move(row * 8 + col, i * 8 + col));
            }
            for (int i = row - 1; IsValidMoveRow(col, i); i--)
            {
                moves.Add(new Move(row * 8 + col, i * 8 + col));
            }
        }
    }

    /// <summary>
    /// normal pawnlarýn hareket edebileceði yerleri hesaplar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="moves"></param>
    private void AddNormalMoves(int row, int col, List<Move> moves)
    {
        // yeme için

        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        if (IsValidMoveCol(row, col - 1, reverseType) && IsValidMoveCol(row, col - 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col - 2, row * 8 + col - 1));
            again = true;
        }
        else if (IsValidMoveCol(row, col + 1, reverseType) && IsValidMoveCol(row, col + 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col + 2, row * 8 + col + 1));
            again = true;
        }
        else if (IsValidMoveRow(col, row - 1, reverseType) && IsValidMoveRow(col, row - 2))
        {
            moves.Add(new Move(row * 8 + col, (row - 2) * 8 + col, (row - 1) * 8 + col));
            again = true;
        }

        //hareket etmek için

        if (!again)
        {
            if (IsValidMoveRow(col, row - 1))// önceliðimiz ileri gitmek olduðu için önce ileri gitme þanslarýmýza bakýyoruz
            {
                moves.Add(new Move(row * 8 + col, (row - 1) * 8 + col));
            }
            if (IsValidMoveCol(row, col - 1))
            {
                moves.Add(new Move(row * 8 + col, row * 8 + col - 1));
            }
            if (IsValidMoveCol(row, col + 1))
            {
                moves.Add(new Move(row * 8 + col, row * 8 + col + 1));
            }
        }
    }

    /// <summary>
    /// boþ nokta bulmak için stunu arar
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="cal"></param>
    /// <returns></returns>
    private bool IsValidMoveCol(int startRow, int col)
    {
        if ( col < 0 || col > 7)
            return false;
        if (board[startRow][col] != 0) // Hedef konum boþ olmalý
            return false;
        return true;
    }

    /// <summary>
    /// boþ nokta aramak için satýrý tarar
    /// </summary>
    /// <param name="startCol"></param>
    /// <param name="raw"></param>
    /// <returns></returns>
    private bool IsValidMoveRow(int startCol, int raw)
    {
        if ( raw < 0 || raw > 7)
            return false;
        if (board[raw][startCol] != 0) // Hedef konum boþ olmalý
            return false;
        return true;
    }

    /// <summary>
    /// tipine uygun ise hareket ettirir
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="cal"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool IsValidMoveCol(int startRow, int col, int type)
    {
        if (col < 0 || col > 7)
            return false;
        if (board[startRow][col] % 2 != type) // Hedef konum boþ olmalý
            return false;
        return true;
    }

    /// <summary>
    /// tipine uygun ise hareket ettirir
    /// </summary>
    /// <param name="startCol"></param>
    /// <param name="raw"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool IsValidMoveRow(int startCol, int raw, int type)
    {
        if (raw < 0 || raw > 7)
            return false;
        if (board[raw][startCol] % 2 != type) // Hedef konum boþ olmalý
            return false;
        return true;
    }

    /// <summary>
    /// boardý istenilen hareketi yaparak yeniden düzenle
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    private int[][] MakeMove(Move move)
    {
        int[][] newBoard = CloneBoard();
        int startRow = move.From / 8;
        int startCol = move.From % 8;
        int endRow = move.To / 8;
        int endCol = move.To % 8;
        
        if(move.State == state.eat)
        {
            int eatRow = move.Eat / 8;
            int eatCol = move.Eat % 8;

            newBoard[eatRow][eatCol] = 0;
        }

        newBoard[endRow][endCol] = newBoard[startRow][startCol];
        newBoard[startRow][startCol] = 0;

        if (endRow == 0 || endRow == 7)
        {
            newBoard[endRow][endCol] += 2; // Dama yap
        }

        return newBoard;
    }

    /// <summary>
    /// üzerinde oynama yapmak için tahtanýn eþleniðini alýr
    /// </summary>
    /// <returns></returns>
    private int[][] CloneBoard()
    {
        int[][] newBoard = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            newBoard[i] = new int[8];

            for (int j = 0; j < 8; j++)
            {
                newBoard[i][j] = board[i][j];
            }
        }
        return newBoard;
    }

    // AI ýn deðerlendirmesi için tahtanýn puanlandýðý kýsým

    /// <summary>
    /// tahtayý puanlar
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private int EvaluateBoard(int[][] board, int type)
    {
        int reverseType = findOrherType(type);
        int score = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0)
                {
                    if (board[i][j] % 2 == type)
                    {
                        if (board[i][j] > 2) score += 20 + 2 * EdgeProximityScore(i) + 2 * EdgeProximityScore(j) - FindAlly(board, i, j, reverseType) + FindAlly(board, i, j, type);
                        else score += 10 + (7 - i) + EdgeProximityScore(j) + FindAlly(board, i, j, type) - FindAlly(board, i, j, reverseType);
                    }
                    else
                    {
                        if (board[i][j] > 2) score -= 20 + 2 * EdgeProximityScore(i) + 2 * EdgeProximityScore(j) + FindAlly(board, i, j, reverseType) - FindAlly(board, i, j, type);
                        else score -= 10 + i + EdgeProximityScore(j) + FindAlly(board, i,j,reverseType) - FindAlly(board, i, j, type);
                    }
                }
            }
        }
        return score;
    }

    /// <summary>
    /// kenara yakýn olmasýna göre puan vermesini saðlar
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private int EdgeProximityScore(int y)
    {
        return (int)Mathf.Abs(3.5f - (float)y);
    }

    /// <summary>
    /// etrafýnda bulunan dostlara göre puan verir
    /// 
    /// not tipini ters vermek yakýnýndaki düþmanlarý buldurur
    /// </summary>
    /// <param name="board"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private int FindAlly(int[][] board, int x, int z, int type)
    {
        int result = 0;

        if (x < 7 && board[x + 1][z] % 2 == type) result++;
        if (x > 0 && board[x - 1][z] % 2 == type) result++;
        if (z < 7 && board[x][z + 1] % 2 == type) result++;
        if (z > 0 && board[x][z - 1] % 2 == type) result++;

        return result;
    }

    /// <summary>
    /// rakibin numarasýný bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int findOrherType(int type)
    {
        return (type + 1) % 2;
    }
}

/// <summary>
/// yapýlacak hareketi tutan deðiþken
/// </summary>
public class Move
{
    public int From { get; set; } // Hangi taþý oynatacaðýmýzý belirten deðiþken
    public int To { get; set; }   // Nereye oynatacaðýmýzý belirten deðiþken
    public int Eat { get; set; } // yemesi gereken taþýn deðerini tutar
    public state State { get; set; } //hangi iþlemi gerçekleþtireceðini gösterir

    public Move(int from, int to)
    {
        From = from;
        To = to;
        State = state.move;
    }

    public Move(int from, int to, int eat)
    {
        From = from;
        To = to;
        Eat = eat;
        State = state.eat;
    }
}

/// <summary>
/// hareket sýrasýnda alýnabilecek durumlar
/// </summary>
public enum state
{
    move,
    eat
}