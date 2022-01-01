using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    [Header ("General Sounds")]
    public AudioSource _audioSource;
    public AudioClip _cursor;
    public AudioClip _attack;
    public AudioClip _enemyDie;
    public AudioClip _battleStart;
    public AudioClip _select;
    public AudioClip _heal;
    public AudioClip _confirm;
    public AudioClip _enemySpecialAttack1;
    public AudioClip _enemySpecialAttack2;
    public AudioClip _gainAP;

    [Header("Skill Sound")]
    public AudioClip _preserverance;
    public void preserverance() { _audioSource.PlayOneShot(_preserverance); }

    public AudioClip _sunlanceStrike;
    public void sunlanceStrike() { _audioSource.PlayOneShot(_sunlanceStrike); }

    public AudioClip _midnightElegance;
    public void midnightElegance() { _audioSource.PlayOneShot(_midnightElegance); }

    public AudioClip _birdOfPrey;
    public void birdOfPrey() { _audioSource.PlayOneShot(_birdOfPrey); }

    public AudioClip _chronoSlash;
    public void chronoSlash() { _audioSource.PlayOneShot(_chronoSlash); }

    public AudioClip _lunarBlessing;
    public void lunarBlessing() { _audioSource.PlayOneShot(_lunarBlessing); }

    public AudioClip _firestorm;
    public void firestorm() { _audioSource.PlayOneShot(_firestorm); }

    public AudioClip _daisycutter;
    public void daisycutter() { _audioSource.PlayOneShot(_daisycutter); }

    // Start is called before the first frame update
    void Start() { _audioSource = this.GetComponent<AudioSource>();  }

    public void cursor() { _audioSource.PlayOneShot(_cursor); }
    public void attack() { _audioSource.PlayOneShot(_attack); }
    public void enemyDie() { _audioSource.PlayOneShot(_enemyDie); }
    public void select() { _audioSource.PlayOneShot(_select); }
    public void battleStart() { _audioSource.PlayOneShot(_battleStart); }
    public void heal() { _audioSource.PlayOneShot(_heal); }
    public void confirm() { _audioSource.PlayOneShot(_confirm); }

    public void enemySpecialAttackOne() { _audioSource.PlayOneShot(_enemySpecialAttack1); }
    public void enemySpecialAttackTwo() { _audioSource.PlayOneShot(_enemySpecialAttack2); }

    public void gainAP() { _audioSource.PlayOneShot(_gainAP); }
}
