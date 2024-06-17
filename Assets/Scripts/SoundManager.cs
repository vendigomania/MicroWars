using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource blupSound;
    [SerializeField] private AudioSource loseSound;
    [SerializeField] private AudioSource winSound;

    public static SoundManager Instance;

    private void Start()
    {
        Instance = this;
    }

    public void PlayBlup()
    {
        if(enabled) blupSound.Play();
    }

    public void PlayLose()
    {
        if(enabled) loseSound.Play();
    }

    public void PlayWin()
    {
        if(enabled) winSound.Play();
    }
}
