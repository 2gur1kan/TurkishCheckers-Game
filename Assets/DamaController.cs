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

    void Start()
    {
        AI = GetComponent<DamaAI>();
        UIC = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();

        CreateBoard();

        InvokeRepeating("CheckBoard", .1f, .1f);
    }

    public void SkipTour()
    {
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

    // tur sistemleri

    /// <summary>
    /// turu de�i�tirir
    /// </summary>
    public void ChangeTour()
    {
        if (tour) tour = false;
        else tour = true;
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
        selectedSquare = -1;
        ResetBoard();
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

        changeBoard();
    }

    /// <summary>
    /// se�ti�imiz karoyu de�i�tirip de�i�tirmedi�imize bakar
    /// </summary>
    /// <param name="num"></param>
    public void setSelectedSquare(int num)
    {
        if(num != selectedSquare)
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
