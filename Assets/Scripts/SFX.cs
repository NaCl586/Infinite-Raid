using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public AudioSource _audioSource;
    public AudioClip _cursor;
    public AudioClip _attack;
    public AudioClip _enemyDie;
    public AudioClip _battleStart;
    public AudioClip _select;
    public AudioClip _heal;
    public AudioClip _confirm;

    // Start is called before the first frame update
    void Start() { _audioSource = this.GetComponent<AudioSource>();  }

    public void cursor() { _audioSource.PlayOneShot(_cursor); }
    public void attack() { _audioSource.PlayOneShot(_attack); }
    public void enemyDie() { _audioSource.PlayOneShot(_enemyDie); }
    public void select() { _audioSource.PlayOneShot(_select); }
    public void battleStart() { _audioSource.PlayOneShot(_battleStart); }
    public void heal() { _audioSource.PlayOneShot(_heal); }
    public void confirm() { _audioSource.PlayOneShot(_confirm); }
}
