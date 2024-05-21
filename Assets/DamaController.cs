using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamaController : MonoBehaviour
{
    public int[][] board;
    private List<SquareController> boardSCList = new List<SquareController>();

    [SerializeField] private GameObject square;

    private DamaAI AI;
    private UIController UIC;

    private int selectedSquare = -1;
    private int oldSquare = -1;
    public bool tour = true;
    public bool startAI = false;

    // yeme sonras� yap�lacak i�lemler
    private bool again = false;
    private int jumper = -1;

    void Start()
    {
        AI = GetComponent<DamaAI>();
        UIC = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();

        CreateBoard();

        InvokeRepeating("CheckBoard", .1f, .1f);
    }

    public void SkipTour()
    {
        again = false;
        tour = false;
    }

    /// <summary>
    /// tahtan�n durmunun kontrol edilmesini sa�layan i�lev
    /// </summary>
    private void CheckBoard()
    {
        UIC.UpdateTourText(tour);

        if (tour)
        {
            if (selectedSquare >= 0 && selectedSquare != oldSquare)
            {
                oldSquare = selectedSquare;

                CheckSquare();
            }
        }
        else
        {
            if (!startAI)
            {
                AI.AIStart();
                startAI = true;
            }
        }

    }

    // zorunlu yeme kural� 

    private void CompulsiveEating()
    {
        List<Move> Moves = eatFinder();

        if(Moves.Count > 0)
        {
            if (Moves.Count > 1)//gidilebilecek yerler �ok oldu�unda
            {


                return;
            }

            StartCoroutine(PlayTheMove(Moves[0]));
        }
    }

    private List<Move> eatFinder()
    {
        List<Move> moves = new List<Move>();
        int type = CheckTourPawnType();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0 && board[i][j] % 2 == type)
                {
                    AddPossibleMoves(i, j, moves);

                    if (moves.Count < 1) return moves;
                }
            }
        }

        return moves;
    }

    private void AddPossibleMoves(int row, int col, List<Move> moves)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama ta��n�n hareketleri
            AddDamaMoves(row, col, moves);
        }
        else if (pawn > 0)
        {
            // Normal ta��n hareketleri
            AddNormalMoves(row, col, moves);
        }
    }

    private void AddDamaMoves(int row, int col, List<Move> moves)
    {
        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        for (int i = col - 1; i >= 0; i--)
        {
            if (IsValidMoveCol(row, i, reverseType))
            {
                if (IsValidMoveCol(row, i - 1)) break;

                for (int j = i - 1; j >= 0; j--)
                {
                    if (IsValidMoveCol(row, j)) moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i));
                }
                return;
            }

        }
        for (int i = col + 1; i < 8; i++)
        {
            if (IsValidMoveCol(row, i, reverseType))
            {
                if (IsValidMoveCol(row, i + 1)) break;

                for (int j = i + 1; j < 8; j++)
                {
                    if (IsValidMoveCol(row, j)) moves.Add(new Move(row * 8 + col, row * 8 + j, row * 8 + i));
                }
                return;
            }
        }
        for (int i = row + 1; i < 8; i++)
        {
            if (IsValidMoveRow(col, i, reverseType))
            {
                if (IsValidMoveRow(col, i + 1)) break;

                for (int j = i + 1; j >= 0; j++)
                {
                    if (IsValidMoveRow(col, j)) moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col));
                }
                return;
            }
        }
        for (int i = row - 1; i >= 0; i--)
        {
            if (IsValidMoveRow(col, i, reverseType))
            {
                if (IsValidMoveRow(col, i - 1)) break;

                for (int j = i - 1; j >= 0; j--)
                {
                    if (IsValidMoveRow(col, j)) moves.Add(new Move(row * 8 + col, j * 8 + col, i * 8 + col));
                }
                return;
            }
        }
    }

    private void AddNormalMoves(int row, int col, List<Move> moves)
    {
        // yeme i�in

        int pawnType = board[row][col] % 2;
        int reverseType = findOrherType(pawnType);

        if (IsValidMoveCol(row, col - 1, reverseType) && IsValidMoveCol(row, col - 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col - 2, row * 8 + col - 1));
            return;
        }
        else if (IsValidMoveCol(row, col + 1, reverseType) && IsValidMoveCol(row, col + 2))
        {
            moves.Add(new Move(row * 8 + col, row * 8 + col + 2, row * 8 + col + 1));
            return;
        }
        else if (IsValidMoveRow(col, row - 1, reverseType) && IsValidMoveRow(col, row - 2))
        {
            moves.Add(new Move(row * 8 + col, (row - 2) * 8 + col, (row - 1) * 8 + col));
            return;
        }
    }

    /// <summary>
    /// bo� nokta bulmak i�in stunu arar
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="cal"></param>
    /// <returns></returns>
    private bool IsValidMoveCol(int startRow, int col)
    {
        if (col < 0 || col > 7)
            return false;
        if (board[startRow][col] != 0) // Hedef konum bo� olmal�
            return false;
        return true;
    }

    /// <summary>
    /// bo� nokta aramak i�in sat�r� tarar
    /// </summary>
    /// <param name="startCol"></param>
    /// <param name="raw"></param>
    /// <returns></returns>
    private bool IsValidMoveRow(int startCol, int raw)
    {
        if (raw < 0 || raw > 7)
            return false;
        if (board[raw][startCol] != 0) // Hedef konum bo� olmal�
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
        if (board[startRow][col] % 2 != type) // Hedef konum bo� olmal�
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
        if (board[raw][startCol] % 2 != type) // Hedef konum bo� olmal�
            return false;
        return true;
    }

    /// <summary>
    /// rakibin numaras�n� bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int findOrherType(int type)
    {
        if (type > 0) return 0;
        else return 1;
    }

    /// <summary>
    /// s�ras� gelen pawn� kontrol eder
    /// </summary>
    /// <returns></returns>
    private int CheckTourPawnType()
    {
        if (tour) return 1;
        return 0;
    }

    private IEnumerator PlayTheMove(Move move)
    {
        yield return new WaitForSeconds(.5f);

        setSelectedSquare(move.From);

        yield return new WaitForSeconds(.5f);

        if (move.State == state.move) MovePawn(move.To);
        else if (move.State == state.eat) EatPawn(move.Eat);

        if (tour)
        {
            UIC.SetButton();
            setSelectedSquare(move.To);
            again = true;
        }      
    }

    // tur sistemleri

    /// <summary>
    /// turu de�i�tirir
    /// </summary>
    public void ChangeTour()
    {
        if (tour) tour = false;
        else tour = true;

        again = false;
        AI.again = false;
    }

    // tahtay� kontrol sistemleri

    /// <summary>
    /// bloklar� kontrol eder
    /// </summary>
    private void CheckSquare()
    {
        int x = selectedSquare / 8;
        int z = selectedSquare % 8;

        int pawnType = board[x][z];

        if (pawnType < 3) checkNormalPawn(x, z);
        else checkDamaPawn(x, z);
    }

    /// <summary>
    /// pawn �n hangi pawn oldu�unu bulur
    /// </summary>
    /// <param name="square"></param>
    /// <returns></returns>
    private int CalculatePawnType(int square)
    {
        int x = square / 8;
        int z = square % 8;

        return board[x][z];
    }

    /// <summary>
    /// pawn� dama yapar
    /// </summary>
    /// <param name="square"></param>
    private void PawntoDama(int square)
    {
        int x = square / 8;
        int z = square % 8;

        board[x][z] += 2;
    }

    /// <summary>
    /// yeme i�lemi ger�ekle�tikten sonra d��ebilece�i noktalar� ayarlar
    /// </summary>
    /// <param name="jump"></param>
    private void SetAfterEat(int eat, int x, int z, bool isPawn)
    {
        int jumppoint = eat + ((8 * x) + z);

        if (isPawn) boardSCList[jumppoint].Jumpable(eat);
        else
        {
            if (x == 0)
            {
                int row = eat / 8;
                row *= 8;

                for (int i = eat + z; i > row - 1 && i < row + 8; i += z)
                {
                    if (CalculatePawnType(i) < 1) boardSCList[i].Jumpable(eat);
                    else break; //�imdilik bir engel gelince duruyor
                }
            }
            else
            {
                for (int i = jumppoint; i > -1 && i < boardSCList.Count; i += 8 * x)
                {
                    if (CalculatePawnType(i) < 1) boardSCList[i].Jumpable(eat);
                    else break; //�imdilik bir engel gelince duruyor
                }
            }
        } 
    }

    /// <summary>
    /// dama oldu�u zaman ileriye do�ru hareket etmesini sa�layacak kareleri kontrol eder
    /// </summary>
    /// <param name="squareNum"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    private void SetMoveDama(int squareNum, int x, int z)
    {

        if (x == 0)
        {
            int row = squareNum / 8;
            row *= 8;

            for (int i = squareNum + z; i > row-1 && i < row + 8; i += z)
            {
                if (CalculatePawnType(i) > 0)
                {
                    if (CalculatePawnType(squareNum) % 2 != CalculatePawnType(i) % 2)
                    {
                        if(CalculatePawnType(i + z) == 0) SetAfterEat(boardSCList[i].Eatable(), x, z, false);
                    }
                    break;
                }

                boardSCList[i].Moveable();
            }
        }
        else
        {
            for (int i = squareNum + (x*8); i > -1 && i < boardSCList.Count; i += x * 8)
            {
                if (CalculatePawnType(i) > 0)
                {
                    if (CalculatePawnType(squareNum) % 2 != CalculatePawnType(i) % 2)
                    {
                        if (CalculatePawnType(i + (x * 8)) == 0) SetAfterEat(boardSCList[i].Eatable(), x, z, false);
                    }
                    break;
                }

                boardSCList[i].Moveable();
            }
        }

    }

    private void checkDamaPawn(int x, int z)
    {
        int pawnType = board[x][z];

        if (x < 7)
        {
            if (pawnType != board[x + 1][z])
            {
                if (board[x + 1][z] == 0)
                {
                    SetMoveDama((x * 8) + z, 1, 0);
                }
                else if (x < 6 && board[x + 1][z] % 2 != pawnType % 2 && board[x + 2][z] == 0)
                {
                    SetAfterEat(boardSCList[((x + 1) * 8) + z].Eatable(), 1, 0, false);
                }
            }
        }

        if (x > 0)
        {
            if (pawnType != board[x - 1][z])
            {
                if (board[x - 1][z] == 0)
                {
                    SetMoveDama((x * 8) + z, -1, 0);
                }
                else if (x > 1 && board[x - 1][z] % 2 != pawnType % 2 && board[x - 2][z] == 0)
                {
                    SetAfterEat(boardSCList[((x - 1) * 8) + z].Eatable(), -1, 0, false);
                }
            }
        }

        if (z > 0)
        {
            if (pawnType != board[x][z - 1])
            {
                if (board[x][z - 1] == 0)
                {
                    SetMoveDama((x * 8) + z, 0, -1);
                }
                else if (z > 1 && board[x][z - 1] % 2 != pawnType % 2 && board[x][z - 2] == 0)
                {
                    SetAfterEat(boardSCList[(x * 8) + (z - 1)].Eatable(), 0, -1, false);
                }
            }
        }

        if (z < 7)
        {
            if (board[x][z] != board[x][z + 1])
            {
                if (board[x][z + 1] == 0)
                {
                    SetMoveDama((x * 8) + z, 0, 1);
                }
                else if (z < 6 && board[x][z + 1] % 2 != pawnType % 2 && board[x][z + 2] == 0)
                {
                    SetAfterEat(boardSCList[(x * 8) + (z + 1)].Eatable(), 0, 1, false);
                }
            }
        }

    }

    /// <summary>
    /// pawnlar�n hareket edece�i yerleri sahnede ayarlar
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    private void checkNormalPawn(int x, int z)
    {
        int pawnType = board[x][z];

        //e�er s�f�r ise i�lem yap�lmamas�n� sa�lar
        if (pawnType == 0) return;

        if (pawnType % 2 == 1)
        {
            if (x < 7)
            {
                if (pawnType != board[x + 1][z])
                {
                    if (board[x + 1][z] == 0)
                    {
                        boardSCList[((x + 1) * 8) + z].Moveable();
                    }
                    else if (x < 6 && board[x + 1][z] % 2 != pawnType % 2 && board[x + 2][z] == 0)
                    {
                        SetAfterEat(boardSCList[((x + 1) * 8) + z].Eatable(), 1, 0, true);
                    }
                }
            }

        }
        else// �st taraf�n se�mesi
        {
            if (x > 0)
            {
                if (pawnType != board[x - 1][z])
                {
                    if (board[x - 1][z] == 0)
                    {
                        boardSCList[((x - 1) * 8) + z].Moveable();
                    }
                    else if (x > 1 && board[x - 1][z] % 2 != pawnType % 2 && board[x - 2][z] == 0)
                    {
                        SetAfterEat(boardSCList[((x - 1) * 8) + z].Eatable(), -1, 0, true);
                    }
                }
            }
        }

        if (z > 0)
        {
            if (pawnType != board[x][z - 1])
            {
                if (board[x][z - 1] == 0)
                {
                    boardSCList[(x * 8) + (z - 1)].Moveable();
                }
                else if (z > 1 && board[x][z - 1] % 2 != pawnType % 2 && board[x][z - 2] == 0)
                {
                    SetAfterEat(boardSCList[(x * 8) + (z - 1)].Eatable(), 0, -1, true);
                }
            }
        }

        if (z < 7)
        {
            if (board[x][z] != board[x][z + 1])
            {
                if (board[x][z + 1] == 0)
                {
                    boardSCList[(x * 8) + (z + 1)].Moveable();
                }
                else if (z < 6 && board[x][z + 1] % 2 != pawnType % 2 && board[x][z + 2] == 0)
                {
                    SetAfterEat(boardSCList[(x * 8) + (z + 1)].Eatable(), 0, 1, true);
                }
            }
        }
    }

    /// <summary>
    /// se�ildikten sonra i�lemleri ger�ekle�tirir ve tahtay� d�zenler
    /// (yeme modu true, move modu false)
    /// </summary>
    /// <param name="selected"></param>
    private void changeBoard()
    {
        if(!again) selectedSquare = -1;
        ResetBoard();

        if (!again && tour) CompulsiveEating();
    }

    /// <summary>
    /// se�ilen yerdeki piyonu yer ve arkas�na ge�er
    /// </summary>
    /// <param name="selected"></param>
    public void EatPawn(int selected)
    {
        if(tour) UIC.SetButton();

        int x = selectedSquare / 8;
        int z = selectedSquare % 8;
        int i = board[x][z];

        boardSCList[selectedSquare].pawnNumber = 0;
        board[x][z] = 0;

        x = selected / 8;
        z = selected % 8;

        boardSCList[selected].pawnNumber = 0;
        board[x][z] = 0;

        int jump = selected + (selected - selectedSquare);

        x = jump / 8;
        z = jump % 8;

        if (x == 0 || x == 7) i += 2;//pawn� dama yapar

        boardSCList[jump].pawnNumber = i;
        board[x][z] = i;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// se�ilen yerdeki piyonu de�i�tirir
    /// </summary>
    /// <param name="selected"></param>
    public void MovePawn(int selected)
    {
        int x = selectedSquare / 8;
        int z = selectedSquare % 8;
        int i = board[x][z];

        boardSCList[selectedSquare].pawnNumber = 0;
        board[x][z] = 0;

        x = selected / 8;
        z = selected % 8;

        if (x == 0 || x == 7) i += 2;//pawn� dama yapar

        boardSCList[selected].pawnNumber = i;
        board[x][z] = i;

        ChangeTour();// bo� hareket etti�inde turu de�i�tirir
        changeBoard();
    }

    /// <summary>
    /// yeme i�lemi yap�ld�ktan sonra hareket edilecek yerin se�ilme ile olur
    /// </summary>
    /// <param name="selected"></param>
    /// <param name="eat"></param>
    public void JumpPawn(int selected, int eat)
    {
        if(tour) UIC.SetButton();

        int x = selectedSquare / 8;
        int z = selectedSquare % 8;
        int i = board[x][z];

        boardSCList[selectedSquare].pawnNumber = 0;
        board[x][z] = 0;

        x = eat / 8;
        z = eat % 8;

        boardSCList[eat].pawnNumber = 0;
        board[x][z] = 0;

        x = selected / 8;
        z = selected % 8;

        if (x == 0 || x == 7) i += 2;//pawn� dama yapar

        boardSCList[selected].pawnNumber = i;
        board[x][z] = i;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// se�ti�imiz karoyu de�i�tirip de�i�tirmedi�imize bakar
    /// </summary>
    /// <param name="num"></param>
    public void setSelectedSquare(int num)
    {
        if (num != selectedSquare && !again)
        {
            ResetBoard();
            selectedSquare = num;
            boardSCList[num].Selected();
        }
    }

    /// <summary>
    /// tahtay� hi� se�im yap�lmam�� haline d�nderir
    /// </summary>
    private void ResetBoard()
    {
        foreach(SquareController square in boardSCList)
        {
            square.ResetSquare();
        }
    }

    ///////////////////////////////////////////////////////////////////
    /// create 
    ///////////////////////////////////////////////////////////////////

    /// <summary>
    /// tahtay� olu�turur
    /// </summary>
    private void CreateBoard()
    {
        CreateEmptyBoard();

        CreatePawn();

        CreateSquare();
    }

    /// <summary>
    /// piyonlar� ba�lang�� noktas�na �eker
    /// </summary>
    private void CreatePawn()
    {
        for (int i = 1; i < 3; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                board[i][j] = 1;
            }
        }

        for (int i = 5; i < 7; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                board[i][j] = 2;
            }
        }
    }

    /// <summary>
    /// bana tahtada sat�rlar olu�turur
    /// </summary>
    private void CreateSquare()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject go = Instantiate(square, new Vector3(i,0,j), Quaternion.identity);
                go.transform.parent = transform;

                //olu�turuldu�unda rengini ayarlamak i�in
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = FindStartColor(i + j);
                go.GetComponent<Renderer>().material = mat;

                boardSCList.Add(go.GetComponent<SquareController>());
                go.GetComponent<SquareController>().squareNumber = (i * 8) + j;
                go.GetComponent<SquareController>().pawnNumber = board[i][j];
            }
        }
    }

    /// <summary>
    /// verilen de�ere g�re rengini d�nd�r�r
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private Color FindStartColor(int x)
    {
        if (x % 2 == 1) return Color.black;

        return Color.white;
    }

    /// <summary>
    /// dama tahtas�n� s�f�rlar
    /// </summary>
    private void CreateEmptyBoard()
    {
        board = new int[8][];

        for (int i = 0; i < 8; i++)
        {
            board[i] = new int[8];

            for (int j = 0; j < 8; j++)
            {
                board[i][j] = 0;
            }
        }
    }
}
