using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource AS;

    [Header(" ")]
    [SerializeField] private AudioClip FonSound;

    [Header("Pawn Sound")]
    [SerializeField] private AudioClip EatSound;
    [SerializeField] private AudioClip MoveSound;
    [SerializeField] private AudioClip SelectSound;

    private void Awake()
    {
        // Singleton örneğini ayarla
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu GameObject'in sahne değiştirdiğinde yok olmamasını sağlar
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AS = GetComponent<AudioSource>();
        AS.clip = FonSound;
        AS.loop = true;
        AS.Play();
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
