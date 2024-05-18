using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareController : MonoBehaviour
{
    public int squareNumber = 0;
    public int pawnNumber = 0;

    private Vector3 startPosition;
    private Color baseColor;
    private Vector3 selectPosition;
    private bool moveMode = false;
    private bool eatMode = false;
    private bool jump = false;
    private int afterEat = -1;

    private Renderer ren;
    private DamaController DC;

    [SerializeField] private GameObject Pawn1;
    [SerializeField] private GameObject Pawn2;

    [SerializeField] private Color selectColor = Color.green; 
    [SerializeField] private Color moveColor = Color.yellow; 
    [SerializeField] private Color eatColor = Color.red;

    void Start()
    {
        ren = GetComponent<Renderer>();
        DC = GetComponentInParent<DamaController>();

        startPosition = transform.position;

        //tekrar tekrar hesaplanmasýn diye ileri çýkacaðý pozisyonu bellkete tutuyorum
        Vector3 selected = startPosition;
        selected.y += .3f;
        selectPosition = selected;

        baseColor = ren.material.color;

        CheckPawn();
    }

    private void OnMouseDown()
    {
        if (eatMode)
        {
            ResetSquare();

            DC.EatPawn(squareNumber);
        }
        else if (moveMode)
        {
            ResetSquare();

            DC.MovePawn(squareNumber);
        }
        else if (jump)
        {

            DC.JumpPawn(squareNumber, afterEat);

            ResetSquare();
        }
        else
        {
            if(pawnNumber % 2 == 1)
            {
                if(DC.tour) DC.setSelectedSquare(squareNumber);
            }
        }
    }

    /// <summary>
    /// cismin seçim moduna geçmesini saðlar
    /// </summary>
    public void Selected()
    {
        transform.position = selectPosition;
        ren.material.color = selectColor;
    }

    /// <summary>
    /// cismin hareket moduna geçmesini saðlar
    /// </summary>
    public void Moveable()
    {
        transform.position = selectPosition;
        ren.material.color = moveColor;

        moveMode = true;
    }
    
    /// <summary>
    /// cismin hareket moduna geçmesini saðlar
    /// </summary>
    public void Jumpable(int eat)
    {
        transform.position = selectPosition;
        ren.material.color = moveColor;

        afterEat = eat;

        jump = true;
    }

    /// <summary>
    /// cismin yeme moduna geçmesini saðlar
    /// </summary>
    public int Eatable()
    {
        transform.position = selectPosition;
        ren.material.color = eatColor;

        eatMode = true;

        afterEat = squareNumber;
        return squareNumber;
    }

    /// <summary>
    /// cismi sýfýrlar
    /// </summary>
    public void ResetSquare()
    {
        CheckPawn();

        transform.position = startPosition;
        ren.material.color = baseColor;

        moveMode = false;
        eatMode = false;
        jump = false;

        afterEat = -1;
    }

    /// <summary>
    /// bölgeye sahip olan pawný gösterir
    /// </summary>
    private void CheckPawn()
    {
        if (pawnNumber == 0)
        {
            Pawn1.SetActive(false);
            Pawn2.SetActive(false);
        }
        else if(pawnNumber%2 == 1)
        {
            Pawn1.SetActive(true);
            Pawn2.SetActive(false);
        }
        else
        {
            Pawn1.SetActive(false);
            Pawn2.SetActive(true);
        }
    }
}
