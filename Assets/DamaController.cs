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

    // yeme sonrasý yapýlacak iþlemler
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
    /// tahtanýn durmunun kontrol edilmesini saðlayan iþlev
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

    // zorunlu yeme kuralý 

    private void CompulsiveEating()
    {
        int Move = eatFinder();
        Debug.Log("eat num : " + Move);

        if(Move >= 0)
        {
            selectedSquare = Move;
            SetAfterEatBoard();
            again = true;
        }
    }

    private int eatFinder()
    {
        int move;
        int type = CheckTourPawnType();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i][j] > 0 && board[i][j] % 2 == type)
                {
                    move = AddPossibleMoves(i, j);

                    if (move > -1) return move;
                }
            }
        }

        return -1;
    }

    private int AddPossibleMoves(int row, int col)
    {
        int pawn = board[row][col];

        if (pawn > 2)
        {
            // Dama taþýnýn hareketleri
            return AddDamaMoves(row, col);
        }
        else if (pawn > 0)
        {
            // Normal taþýn hareketleri
            return AddNormalMoves(row, col);
        }

        return -1;
    }

    /// <summary>
    /// dama piyonun yiyebileceði bir yer varmý ona bakar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private int AddDamaMoves(int row, int col)
    {
        int type = board[row][col] % 2;
        int reverseType = (type + 1) % 2;

        for (int i = col - 1; i >= 0; i--)
        {
            if (IsValidMoveCol(row, i, type) && !IsValidMoveCol(row, i)) break;

            if (IsValidMoveCol(row, i, reverseType) && IsValidMoveCol(row, i - 1) && !IsValidMoveCol(row, i))
            {
                return row * 8 + col;
            }
        }
        for (int i = col + 1; i < 8; i++)
        {
            if (IsValidMoveCol(row, i, type) && !IsValidMoveCol(row, i)) break;

            if (IsValidMoveCol(row, i, reverseType) && IsValidMoveCol(row, i + 1) && !IsValidMoveCol(row, i))
            {
                return row * 8 + col;
            }
        }
        for (int i = row + 1; i < 8; i++)
        {
            if (IsValidMoveRow(col, i, type) && !IsValidMoveCol(row, i)) break;

            if (IsValidMoveRow(col, i, reverseType) && IsValidMoveRow(col, i + 1) && !IsValidMoveRow(col, i))
            {
                return row * 8 + col;
            }
        }
        for (int i = row - 1; i >= 0; i--)
        {
            if (IsValidMoveRow(col, i, type) && !IsValidMoveCol(row, i)) break;

            if (IsValidMoveRow(col, i, reverseType) && IsValidMoveRow(col, i - 1) && !IsValidMoveRow(col, i))
            {
                return row * 8 + col;
            }
        }

        return -1;
    }

    /// <summary>
    /// normal piyonun yeme iþlevi için etrafýna bakar
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private int AddNormalMoves(int row, int col)
    {
        int reverseType = (board[row][col] + 1) % 2;

        if (IsValidMoveCol(row, col - 1, reverseType) && IsValidMoveCol(row, col - 2) && !IsValidMoveCol(row, col - 1))
        {
            return row * 8 + col;
        }
        else if (IsValidMoveCol(row, col + 1, reverseType) && IsValidMoveCol(row, col + 2) && !IsValidMoveCol(row, col + 1))
        {
            return row * 8 + col;
        }
        else if (IsValidMoveRow(col, row + 1, reverseType) && IsValidMoveRow(col, row + 2) && !IsValidMoveRow(col, row + 1))
        {
            return row * 8 + col;
        }

        return -1;
    }

    /// <summary>
    /// boþ nokta bulmak için stunu arar
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="cal"></param>
    /// <returns></returns>
    private bool IsValidMoveCol(int startRow, int col)
    {
        if (col < 0 || col > 7)
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
        if (raw < 0 || raw > 7)
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
        if (board[raw][startCol] % 2 != type)
            return false;
        return true;
    }

    /// <summary>
    /// rakibin numarasýný bulur
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int findOrherType(int type)
    {
        if (type > 0) return 0;
        else return 1;
    }

    /// <summary>
    /// sýrasý gelen pawný kontrol eder
    /// </summary>
    /// <returns></returns>
    private int CheckTourPawnType()
    {
        if (tour) return 1;
        return 0;
    }

    /// <summary>
    /// yeme hareketini zoraki oynatýr
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
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
    /// turu deðiþtirir
    /// </summary>
    public void ChangeTour()
    {
        if (tour) tour = false;
        else tour = true;

        again = false;
        AI.again = false;
    }

    // tahtayý kontrol sistemleri

    /// <summary>
    /// bloklarý kontrol eder
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
    /// pawn ýn hangi pawn olduðunu bulur
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
    /// pawný dama yapar
    /// </summary>
    /// <param name="square"></param>
    private void PawntoDama(int square)
    {
        int x = square / 8;
        int z = square % 8;

        board[x][z] += 2;
    }

    /// <summary>
    /// yeme iþlemi gerçekleþtikten sonra düþebileceði noktalarý ayarlar
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
                for (int i = eat + z; i > -1 && i < boardSCList.Count; i += z)
                {
                    if (CalculatePawnType(i)  == 0) boardSCList[i].Jumpable(eat);
                    else break; //þimdilik bir engel gelince duruyor
                }
            }
            else
            {
                for (int i = eat + (8 * x); i > -1 && i < boardSCList.Count; i += 8 * x) 
                {
                    if (CalculatePawnType(i) == 0) boardSCList[i].Jumpable(eat);
                    else break; //þimdilik bir engel gelince duruyor
                }
            }
        } 
    }

    /// <summary>
    /// dama olduðu zaman ileriye doðru hareket etmesini saðlayacak kareleri kontrol eder
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
    /// pawnlarýn hareket edeceði yerleri sahnede ayarlar
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    private void checkNormalPawn(int x, int z)
    {
        int pawnType = board[x][z];

        //eðer sýfýr ise iþlem yapýlmamasýný saðlar
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
        else// üst tarafýn seçmesi
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
    /// seçildikten sonra iþlemleri gerçekleþtirir ve tahtayý düzenler
    /// (yeme modu true, move modu false)
    /// </summary>
    /// <param name="selected"></param>
    private void changeBoard()
    {
        if (!again) selectedSquare = -1;

        ResetBoard();

        if(again) SetAfterEatBoard();
        else if (tour) CompulsiveEating();
    }

    /// <summary>
    /// yeme iþlemi gerçekleþtikten sonra olacaklarý ayarlar
    /// </summary>
    private void SetAfterEatBoard()
    {
        boardSCList[selectedSquare].Selected();
    }

    /// <summary>
    /// seçilen yerdeki piyonu yer ve arkasýna geçer
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

        if (x == 0 || x == 7) i += 2;//pawný dama yapar

        boardSCList[jump].pawnNumber = i;
        board[x][z] = i;
        selectedSquare = jump;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// seçilen yerdeki piyonu deðiþtirir
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

        if (x == 0 || x == 7) i += 2;//pawný dama yapar

        boardSCList[selected].pawnNumber = i;
        board[x][z] = i;

        ChangeTour();// boþ hareket ettiðinde turu deðiþtirir
        changeBoard();
    }

    /// <summary>
    /// yeme iþlemi yapýldýktan sonra hareket edilecek yerin seçilme ile olur
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

        if (x == 0 || x == 7) i += 2;//pawný dama yapar

        boardSCList[selected].pawnNumber = i;
        board[x][z] = i;

        selectedSquare = selected;

        if (tour) again = true;
        else AI.again = true;

        changeBoard();
    }

    /// <summary>
    /// seçtiðimiz karoyu deðiþtirip deðiþtirmediðimize bakar
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
    /// tahtayý hiç seçim yapýlmamýþ haline dönderir
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
    /// tahtayý oluþturur
    /// </summary>
    private void CreateBoard()
    {
        CreateEmptyBoard();

        CreatePawn();

        CreateSquare();
    }

    /// <summary>
    /// piyonlarý baþlangýç noktasýna çeker
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
    /// bana tahtada satýrlar oluþturur
    /// </summary>
    private void CreateSquare()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject go = Instantiate(square, new Vector3(i,0,j), Quaternion.identity);
                go.transform.parent = transform;

                //oluþturulduðunda rengini ayarlamak için
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
    /// verilen deðere göre rengini döndürür
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private Color FindStartColor(int x)
    {
        if (x % 2 == 1) return Color.black;

        return Color.white;
    }

    /// <summary>
    /// dama tahtasýný sýfýrlar
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
