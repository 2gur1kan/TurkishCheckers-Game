using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : MonoBehaviour
{
    [SerializeField] private GameObject Pawn1;
    [SerializeField] private GameObject Pawn2;

    [SerializeField] private float rotationSpeed = 50f; // Dönme hýzý

    void Start()
    {
        if (!Pawn1.activeSelf) Pawn1.SetActive(true);
        if (Pawn2.activeSelf) Pawn2.SetActive(false);

        InvokeRepeating("ChangePawn", 5f, 5f);
    }

    private void Update()
    {
        turn();
    }

    private void ChangePawn()
    {
        if (Pawn1.activeSelf)
        {
            Pawn1.SetActive(false);
            Pawn2.SetActive(true);
        }
        else
        {
            Pawn1.SetActive(true);
            Pawn2.SetActive(false);
        }
    }

    private void turn()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
