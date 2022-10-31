using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class MainMenuManager : MonoBehaviour
{
    public Image blackout;
    public Text[] menuTexts;
    public AudioClip cursor;
    public AudioSource soundAS;

    private bool isQuitting = false;
    private int _selectedMenu;
    private float arrowStartTime;

    // Start is called before the first frame update
    void Start()
    {
        blackout.DOColor(Color.clear,1f);
        _selectedMenu = 0;
        selectMenu(_selectedMenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (isQuitting) return;

        if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
        {
            updateArrowTime();
            _selectedMenu--;
            if (_selectedMenu < 0) _selectedMenu = 1;
            selectMenu(_selectedMenu);
        }
        if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
        {
            updateArrowTime();
            _selectedMenu++;
            if (_selectedMenu >= 2) _selectedMenu = 0;
            selectMenu(_selectedMenu);
        }
        if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
        {
            soundAS.PlayOneShot(cursor);
            if (_selectedMenu == 1)
            {
                isQuitting = true;
                Quit();
            }
            else if(_selectedMenu == 0)
            {
                Play();
            }
        }
    }

    private void selectMenu(int selectedMenu)
    {
        foreach (Text t in menuTexts) t.color = Color.white;
        menuTexts[selectedMenu].color = Color.yellow;
    }

    public void updateArrowTime()
    {
        arrowStartTime = Time.time;
        soundAS.PlayOneShot(cursor);
    }

    public void Quit()
    {
        blackout.DOColor(Color.black, 1f).OnComplete(() => {
            Application.Quit();
        });
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}
