using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamaAI : MonoBehaviour
{
    private DamaController DC;

    private int[][] board;

    void Start()
    {
        DC = GetComponent<DamaController>();
    }

    /// <summary>
    /// s�ras� geldi�inde �a��r�lmas� gereken i�lev
    /// </summary>
    public void AIStart()
    {

    }

    // �ekmesi gereken verileri almay� sa�layan i�levler

    /// <summary>
    /// tahtan�n g�ncel halini �eker
    /// </summary>
    private void CheckBoard()
    {
        board = DC.board;
    }
}
