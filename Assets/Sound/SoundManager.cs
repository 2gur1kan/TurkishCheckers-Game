using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource AS;
    [SerializeField] private AudioClip EatSound;
    [SerializeField] private AudioClip MoveSound;
    [SerializeField] private AudioClip SelectSound;

    private void Awake()
    {
        // Singleton �rne�ini ayarla
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu GameObject'in sahne de�i�tirdi�inde yok olmamas�n� sa�lar
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AS = GetComponent<AudioSource>();
    }

    public void PlayEatSound()
    {
        AS.PlayOneShot(EatSound);
    }

    public void PlayMoveSound()
    {
        AS.PlayOneShot(MoveSound);
    }

    public void PlaySelectSound()
    {
        AS.PlayOneShot(SelectSound);
    }
}
