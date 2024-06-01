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

    /// <summary>
    /// skip butonuna bas�nca yap�lmas� gereken de�i�iklikleri ayarlar
    /// </summary>
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
        Move Move = eatFinder();
        Debug.Log("eat num : " + Move);

        if(Move != null)
        {
            selectedSquare = Move.From;
            again = true;
            SetAfterEatBoard();
            SetForEat(Move);
        }
    }

    private void SetForEat(Move move)
    {
        int direction =  move.To - move.From;

        if(Mathf.Abs(direction) > 7)
        {
            direction /= 8;

            SetAfterEat(move.Eat, direction, 0, false);
        }
        else SetAfterEat(move.Eat, 0, direction, false);
    }

    private Move eatFinder()
    {
        Move move;
        int type = CheckTourPawnType();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0 && board[i][j] % 2 == type)
                {
                    move = AddPossibleMoves(i, j);

                    if (move  != null) return move;
                }
            }
        }

        return null;
    }

    private Move AddPossibleMoves(int row, int col)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama ta��n�n hareketleri
            return AddDamaMoves(row, col);
        }
        else if (pawn > 0)
        {
            // Normal ta��n hareketleri
            return AddNormalMoves(row, col);
        }

        return null;
    }

    /// <summary>
    /// dama piyonun yiyebilece�i bir yer varm� ona bakar
    /// </summary>
    private Move AddDamaMoves(int row, int col)
    {
        int type = board[row][col] % 2;
        int reverseType = (type + 1) % 2;

        for (int i = col - 1; i >= 0; i--)
        {
            if (IsValidMoveCol(row, i, type)) break;

            if (IsValidMoveCol(row, i, reverseType))
            {
                if(IsValidMoveCol(row, i - 1) && !IsValidMoveCol(row, i))  return new Move(row * 8 + col, row * 8 + col - 1, row * 8 + i);
                else break;
            }
        }
        for (int i = col + 1; i < 8; i++)
        {
            if (IsValidMoveCol(row, i, type)) break;

            if (IsValidMoveCol(row, i, reverseType))
            {
                if(IsValidMoveCol(row, i + 1) && !IsValidMoveCol(row, i)) return new Move(row * 8 + col, row * 8 + col + 1, row * 8 + i);
                else break;
            }
        }
        for (int i = row + 1; i < 8; i++)
        {
            if (IsValidMoveRow(col, i, type)) break;

            if (IsValidMoveRow(col, i, reverseType))
            {
                if (IsValidMoveRow(col, i + 1) && !IsValidMoveRow(col, i)) return new Move(row * 8 + col, (row + 1) * 8 + col, i * 8 + col);
                else break;
            }
        }
        for (int i = row - 1; i >= 0; i--)
        {
            if (IsValidMoveRow(col, i, type)) break;

            if (IsValidMoveRow(col, i, reverseType))
            {
                if (IsValidMoveRow(col, i - 1) && !IsValidMoveRow(col, i)) return new Move(row * 8 + col, (row - 1) * 8 + col, i * 8 + col);
                else break;
            }
        }

        return null;
    }

    /// <summary>
    /// normal piyonun yeme i�levi i�in etraf�na bakar
    /// </summary>
    private Move AddNormalMoves(int row, int col)
    {
        int reverseType = (board[row][col] + 1) % 2;

        if (IsValidMoveCol(row, col - 1, reverseType) && IsValidMoveCol(row, col - 2) && !IsValidMoveCol(row, col - 1))
        {
            return new Move(row * 8 + col, row * 8 + col - 2, row * 8 + col -1);
        }
        else if (IsValidMoveCol(row, col + 1, reverseType) && IsValidMoveCol(row, col + 2) && !IsValidMoveCol(row, col + 1))
        {
            return new Move(row * 8 + col, row * 8 + col + 2, row * 8 + col + 1);
        }
        else if (IsValidMoveRow(col, row + 1, reverseType) && IsValidMoveRow(col, row + 2) && !IsValidMoveRow(col, row + 1))
        {
            return new Move(row * 8 + col, (row + 2) * 8 + col, (row + 1) * 8 + col);
        }

        return null;
    }

    /// <summary>
    /// bo� nokta bulmak i�in stunu arar
    /// </summary>
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
    private bool IsValidMoveRow(int startCol, int raw, int type)
    {
        if (raw < 0 || raw > 7)
            return false;
        if (board[raw][startCol] % 2 != type)
            return false;
        return true;
    }

    /// <summary>
    /// rakibin numaras�n� bulur
    /// </summary>
    private int findOrherType(int type)
    {
        return (type > 0) ? 0 : 1;
    }

    /// <summary>
    /// s�ras� gelen pawn� kontrol eder
    /// </summary>
    private int CheckTourPawnType()
    {
        return tour ? 1 : 0;
    }

    /// <summary>
    /// yeme hareketini zoraki oynat�r
    /// </summary>
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
        tour = (tour) ? false : true;

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
    private int CalculatePawnType(int square)
    {
        int x = square / 8;
        int z = square % 8;

        return board[x][z];
    }

    /// <summary>
    /// pawn� dama yapar
    /// </summary>
    private void PawntoDama(int square)
    {
        int x = square / 8;
        int z = square % 8;

        board[x][z] += 2;
    }

    /// <summary>
    /// yeme i�lemi ger�ekle�tikten sonra d��ebilece�i noktalar� ayarlar
    /// </summary>
    private void SetAfterEat(int eat, int x, int z, bool isPawn)
    {
        int jumppoint = eat + ((8 * x) + z);

        if (isPawn) boardSCList[jumppoint].Jumpable(eat);
        else
        {
            if (x == 0)
            {
                for (int i = eat + z; i > -1 && i < boardSCList.Count; i += z)
                {
                    if (CalculatePawnType(i)  == 0) boardSCList[i].Jumpable(eat);
                    else break; //�imdilik bir engel gelince duruyor
                }
            }
            else
            {
                for (int i = eat + (8 * x); i > -1 && i < boardSCList.Count; i += 8 * x) 
                {
                    if (CalculatePawnType(i) == 0) boardSCList[i].Jumpable(eat);
                    else break; //�imdilik bir engel gelince duruyor
                }
            }
        } 
    }

    /// <summary>
    /// dama oldu�u zaman ileriye do�ru hareket etmesini sa�layacak kareleri kontrol eder
    /// </summary>
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

                if(!again) boardSCList[i].Moveable();
            }
        }
        else
        {
            for (int i = squareNum + (x * 8); i > -1 && i < boardSCList.Count; i += x * 8)
            {
                if (CalculatePawnType(i) > 0)
                {
                    if (CalculatePawnType(squareNum) % 2 != CalculatePawnType(i) % 2)
                    {
                        if (CalculatePawnType(i + (x * 8)) == 0) SetAfterEat(boardSCList[i].Eatable(), x, z, false);
                    }
                    break;
                }

                if(!again) boardSCList[i].Moveable();
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
                if (board[x + 1][z] == 0 && !again)
                {
                    SetMoveDama((x * 8) + z, 1, 0);
                }
            }
        }

        if (x > 0)
        {
            if (pawnType != board[x - 1][z])
            {
                if (board[x - 1][z] == 0 && !again)
                {
                    SetMoveDama((x * 8) + z, -1, 0);
                }
            }
        }

        if (z > 0)
        {
            if (pawnType != board[x][z - 1])
            {
                if (board[x][z - 1] == 0 && !again)
                {
                    SetMoveDama((x * 8) + z, 0, -1);
                }
            }
        }

        if (z < 7)
        {
            if (board[x][z] != board[x][z + 1])
            {
                if (board[x][z + 1] == 0 && !again)
                {
                    SetMoveDama((x * 8) + z, 0, 1);
                }
            }
        }

    }

    /// <summary>
    /// pawnlar�n hareket edece�i yerleri sahnede ayarlar
    /// </summary>
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
                        if(!again) boardSCList[((x + 1) * 8) + z].Moveable();
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
                        if (!again) boardSCList[((x - 1) * 8) + z].Moveable();
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
                    if (!again) boardSCList[(x * 8) + (z - 1)].Moveable();
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
                    if (!again) boardSCList[(x * 8) + (z + 1)].Moveable();
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
    private void changeBoard()
    {
        if (!again) selectedSquare = -1;

        ResetBoard();

        if(again) SetAfterEatBoard();
        else if (tour) CompulsiveEating();
    }

    /// <summary>
    /// yeme i�lemi ger�ekle�tikten sonra olacaklar� ayarlar
    /// </summary>
    private void SetAfterEatBoard()
    {
        boardSCList[selectedSquare].Selected();
    }

    /// <summary>
    /// se�ilen yerdeki piyonu yer ve arkas�na ge�er
    /// </summary>
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
        selectedSquare = jump;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// se�ilen yerdeki piyonu de�i�tirir
    /// </summary>
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

        selectedSquare = selected;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// se�ti�imiz karoyu de�i�tirip de�i�tirmedi�imize bakar
    /// </summary>
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
                board[i][j] = 3;
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
