using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class music : MonoBehaviour
{
    public AudioSource _audioSource;
    public AudioClip _battleMusic;
    public AudioClip _fanfareMusic;
    public AudioClip _battleLostMusic;
    public AudioClip _preBattle;

    // Start is called before the first frame update
    void Start() { _audioSource = this.GetComponent<AudioSource>(); }

    public void battle() { playMusic(_battleMusic, true); }
    public void fanfare() { playMusic(_fanfareMusic, true); }
    public void gameOver() { playMusic(_battleLostMusic, false); }
    public void preBattle() { playMusic(_preBattle, true); }

    public void playMusic(AudioClip _audioClip, bool loop)
    {
        _audioSource.Stop();
        _audioSource.loop = loop;
        _audioSource.clip = _audioClip;
        _audioSource.Play();
    }
}
