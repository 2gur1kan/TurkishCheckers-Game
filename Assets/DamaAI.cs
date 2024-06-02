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
        float elapsedTimeMilliseconds = (endTime - startTime) * 1000; // Saniyeden milisaniyeye d�n��t�rme

        Debug.Log("Fonksiyonun �al��ma s�resi: " + elapsedTimeMilliseconds.ToString("F2") + " milisaniye");
        
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
        else if (move.State == state.eat) DC.JumpPawn(move.To,move.Eat);

        if (move.State == state.move || !again) DC.tour = true;

        DC.startAI = false;
    }

    private Move FindBestAgainMove()
    {
        List<Move> possibleMoves = GetPossibleAgainMove(0);// �iftleri ai kontrole diyor
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

        Debug.Log("eat num : " + bestMove.Eat + "\nselect num : " + bestMove.From + "\ndirection num : " + bestMove.To);
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
    /// board�n durumunu al�r
    /// </summary>
    private void CheckBoard()
    {
        board = DC.board;
    }

    // tahtadaki olas� hamleleri bulma a�amas� ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// b�t�n hamleleri hesaplad�ktan sorna en iyi 11 hamleyi al�r ve onalar�da tekrar hesaplar tekrarlar ve 15. defan�n ard�ndan en iyi sonucu d�nd�r�r  
    /// </summary>
    /// <returns></returns>
    private Move FindBestMove()
    {
        List<Move> possibleMoves;
        List<Move> BestMoves = new List<Move>();

        if (again) possibleMoves = GetPossibleMoves(0, jumper);
        else possibleMoves = GetPossibleMoves(0);// �iftleri ai kontrole diyor

        Move bestMove = null;
        int bestScore = int.MinValue;

        foreach (Move move in possibleMoves)
        {
            int[][] newBoard = MakeMove(move);
            int score = EvaluateBoard(newBoard, 0);
            if (score > bestScore)
            {
                bestScore = score;
                BestMoves.Add(move);
            }
        }

        bestMove = FindBestMove(BestMoves, 11, 1);// sonraki 11 ad�ma bakarak en iyi hamleyi bulur. Not: tek olmas� laz�m ��nk� bizim yapaca��m�z hamleyide tahmin edecek

        Debug.Log("eat num : " + bestMove.Eat + "\nselect num : " + bestMove.From + "\ndirection num : " + bestMove.To);

        return bestMove.RootMove == null ? bestMove : bestMove.RootMove;
    } 
    
    /// <summary>
    /// rekrsif olarak daha iyi hamle bulmaya u�ra��r
    /// </summary>
    private Move FindBestMove(List<Move> moves, int step, int type)
    {
        List <Move> possibleMoves;
        findBests(moves);

        possibleMoves = GetPossibleMoves(type, moves);// �iftleri ai kontrole diyor

        int bestScore = int.MinValue;

        foreach (Move move in possibleMoves)
        {
            int[][] newBoard = MakeMove(move);
            int score = EvaluateBoard(newBoard, 0);
            if (score > bestScore)
            {
                bestScore = score;
                moves.Add(move);
            }
        }

        if (step > 0) return FindBestMove(moves, step - 1, findOrherType(type));

        return moves[0].RootMove;
    }

    private List<Move> GetPossibleMoves(int type, List<Move> moves)
    {
        List<Move> posibleMove = new List<Move>();

        int count = moves.Count - 1;

        while (count > -1)
        {
            int[][] board = MakeMove(moves[count]);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i][j] > 0 && board[i][j] % 2 == type)
                    {
                        AddPossibleMoves(i, j, posibleMove, findRoot(moves[count]));
                    }
                }
            }

            count--;
        }

        return posibleMove;
    }

    /// <summary>
    /// ana k�k� bulur ve o k�k� d�nd�r�r
    /// </summary>
    private Move findRoot(Move move)
    {
        return move.RootMove == null ? move : move.RootMove;
    }

    /// <summary>
    /// verilen ta��n muhtemel hamlelerinin hepsini hesaplar
    /// </summary>
    private void AddPossibleMoves(int row, int col, List<Move> moves, Move root)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama ta��n�n hareketleri
            AddDamaMoves(row, col, moves, root);
        }
        else if (pawn > 0)
        {
            // Normal ta��n hareketleri
            AddNormalMoves(row, col, moves, root);
        }
    }

    /// <summary>
    /// verilen listedeki en sonda bulunan en iyi hamlelerden belirlenen miktar�n� ay�r�r
    /// </summary>
    private void findBests(List<Move> moves, int count = 3)
    {
        if (moves.Count <= count) return;

        while (moves.Count > count)
        {
            moves.RemoveAt(0);
        }
    }

    // step 1 // ******************************************************************

    /// <summary>
    /// verilen tipteki ta�lar�n b�t�n hareketlerini bulur
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
    /// verilen tipteki ta�lar�n b�t�n hareketlerini bulur
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
    /// verilen ta��n muhtemel hamlelerinin hepsini hesaplar
    /// </summary>
    private void AddPossibleMoves(int row, int col, List<Move> moves)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama ta��n�n hareketleri
            AddDamaMoves(row, col, moves);
        }
        else if(pawn > 0)
        {
            // Normal ta��n hareketleri
            AddNormalMoves(row, col, moves);
        }
    }

    /// <summary>
    /// dama pawnlar�n�n hareket edebilece�i yerleri hesaplar
    /// </summary>
    private void AddDamaMoves(int row, int col, List<Move> moves, Move root = null)
    {
        // yeme

        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        for (int i = col - 1; i > -1; i--)
        {
            if (IsValidMoveCol(row, i, reverseType) && !IsValidMoveCol(row, i)) 
            {
                for(int j = i - 1; j > -1; j--)
                {
                    if (IsValidMoveCol(row, j)) moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i, root));
                    else break;
                }
                break;
            }
        }
        for (int i = col + 1; i < 8; i++)
        {
            if (IsValidMoveCol(row, i, reverseType) && !IsValidMoveCol(row, i))
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (IsValidMoveCol(row, j)) moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i, root));
                    else break;
                }
                break;
            }
        }
        for (int i = row + 1; i < 8; i++)
        {
            if (IsValidMoveRow(col, i, reverseType)&& !IsValidMoveRow(col, i))
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (IsValidMoveRow(col, j)) moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col, root));
                    else break;
                }
                break;
            }
        }
        for (int i = row - 1; i > -1; i--)
        {
            if (IsValidMoveRow(col, i, reverseType) && !IsValidMoveRow(col, i))
            {
                for (int j = i - 1; j > -1; j--)
                {
                    if (IsValidMoveRow(col, j)) moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col, root));
                    else break;
                }
                break;
            }
        }

        // hareket

        if (!again)
        {
            for (int i = col - 1; IsValidMoveCol(row, i); i--)
            {
                moves.Add(new Move(row * 8 + col, row * 8 + i, root));
            }
            for (int i = col + 1; IsValidMoveCol(row, i); i++)
            {
                moves.Add(new Move(row * 8 + col, row * 8 + i, root));
            }
            for (int i = row + 1; IsValidMoveRow(col, i); i++)
            {
                moves.Add(new Move(row * 8 + col, i * 8 + col, root));
            }
            for (int i = row - 1; IsValidMoveRow(col, i); i--)
            {
                moves.Add(new Move(row * 8 + col, i * 8 + col, root));
            }
        }
    }

    /// <summary>
    /// normal pawnlar�n hareket edebilece�i yerleri hesaplar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="moves"></param>
    private void AddNormalMoves(int row, int col, List<Move> moves, Move root = null)
    {
        // yeme i�in

        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        if (IsValidMoveCol(row, col - 1, reverseType) && IsValidMoveCol(row, col - 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col - 2, row * 8 + col - 1, root));
            again = true;
        }
        else if (IsValidMoveCol(row, col + 1, reverseType) && IsValidMoveCol(row, col + 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col + 2, row * 8 + col + 1, root));
            again = true;
        }
        else if (IsValidMoveRow(col, row - 1, reverseType) && IsValidMoveRow(col, row - 2))
        {
            moves.Add(new Move(row * 8 + col, (row - 2) * 8 + col, (row - 1) * 8 + col, root));
            again = true;
        }

        //hareket etmek i�in

        if (!again)
        {
            if (IsValidMoveRow(col, row - 1))// �nceli�imiz ileri gitmek oldu�u i�in �nce ileri gitme �anslar�m�za bak�yoruz
            {
                moves.Add(new Move(row * 8 + col, (row - 1) * 8 + col, root));
            }
            if (IsValidMoveCol(row, col - 1))
            {
                moves.Add(new Move(row * 8 + col, row * 8 + col - 1, root));
            }
            if (IsValidMoveCol(row, col + 1))
            {
                moves.Add(new Move(row * 8 + col, row * 8 + col + 1, root));
            }
        }
    }

    /// <summary>
    /// bo� nokta bulmak i�in stunu arar
    /// </summary>
    private bool IsValidMoveCol(int startRow, int col)
    {
        return (col < 0 || col > 7 || board[startRow][col] != 0) ? false : true;
    }

    /// <summary>
    /// bo� nokta aramak i�in sat�r� tarar
    /// </summary>
    private bool IsValidMoveRow(int startCol, int row)
    {
        return (row < 0 || row > 7 || board[row][startCol] != 0) ? false : true;
    }

    /// <summary>
    /// tipine uygun mu diye stunu arar
    /// </summary>
    private bool IsValidMoveCol(int startRow, int col, int type)
    {
        return (col < 0 || col > 7 || board[startRow][col] % 2 != type) ? false : true;
    }

    /// <summary>
    /// tipine uygun mu diye sat�r� arar
    /// </summary>
    private bool IsValidMoveRow(int startCol, int row, int type)
    {
        return (row < 0 || row > 7 || board[row][startCol] % 2 != type) ? false : true;
    }

    /// <summary>
    /// board� istenilen hareketi yaparak yeniden d�zenle
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
    /// �zerinde oynama yapmak i�in tahtan�n e�leni�ini al�r
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

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // AI �n de�erlendirmesi i�in tahtan�n puanland��� k�s�m

    /// <summary>
    /// tahtay� puanlar
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
    /// kenara yak�n olmas�na g�re puan vermesini sa�lar
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private int EdgeProximityScore(int y)
    {
        return (int)Mathf.Abs(3.5f - (float)y);
    }

    /// <summary>
    /// etraf�nda bulunan dostlara g�re puan verir
    /// 
    /// not tipini ters vermek yak�n�ndaki d��manlar� buldurur
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
    /// rakibin numaras�n� bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int findOrherType(int type)
    {
        return (type + 1) % 2;
    }
}

/// <summary>
/// yap�lacak hareketi tutan de�i�ken
/// </summary>
public class Move
{
    public int From { get; set; } // Hangi ta�� oynataca��m�z� belirten de�i�ken
    public int To { get; set; }   // Nereye oynataca��m�z� belirten de�i�ken
    public int Eat { get; set; } // yemesi gereken ta��n de�erini tutar
    public state State { get; set; } //hangi i�lemi ger�ekle�tirece�ini g�sterir
    public Move RootMove { get; set; } // birka� hamle sonras�n� g�r�rken hangi hamleden t�redi�ini g�rmek i�in

    public Move(int from, int to, Move rootMove = null)
    {
        From = from;
        To = to;
        State = state.move;
        RootMove = rootMove;
    }

    public Move(int from, int to, int eat, Move rootMove = null)
    {
        From = from;
        To = to;
        Eat = eat;
        State = state.eat;
        RootMove = rootMove;
    }
}

/// <summary>
/// hareket s�ras�nda al�nabilecek durumlar
/// </summary>
public enum state
{
    move,
    eat
}