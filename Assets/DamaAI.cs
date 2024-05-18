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
    /// sýrasý geldiðinde çaðýrýlmasý gereken iþlev
    /// </summary>
    public void AIStart()
    {

    }

    // çekmesi gereken verileri almayý saðlayan iþlevler

    /// <summary>
    /// tahtanýn güncel halini çeker
    /// </summary>
    private void CheckBoard()
    {
        board = DC.board;
    }
}
