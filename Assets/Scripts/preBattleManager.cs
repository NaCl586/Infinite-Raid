using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class HeroStatMenu
{
    public GameObject _gameObject;
    public Image _weaponIcon;
    public Text _weaponName;
    public Image _armorIcon;
    public Text _armorName;
    public Text _name;
    public Text _HP;
    public Text _AP;
}

[System.Serializable]
public class CharaStats
{
    public Text _name;
    public Text _HP;
    public Text _MAtk;
    public Text _AP;
    public Text _RAtk;
    public Text _MDef;
    public Text _RDef;
}

[System.Serializable]
public class EquippableStats
{
    public Image _weaponIcon;
    public Text _weaponName;
    public Text[] _weaponStats = new Text[3];
    public Image _armorIcon;
    public Text _armorName;
    public Text[] _armorStats = new Text[2];
}

public class preBattleManager : MonoBehaviour
{
    [Header ("Main References")]
    public GameManager _gameManager;
    public SkillList _skillList;
    public Equippable _equippable;
    public GameObject _cursor;
    public SFX _sfx;
    private menuState _state;

    [Header("Popup UI References")]
    public GameObject _skillSelectMenu;
    public GameObject _sideActionMenu;

    [Header("Action Menu References")]
    public Text[] _actionMenuText;

    [Header("Hero Stats Menu References")]
    public HeroStatMenu[] _heroStatMenu;

    [Header("Side Menu References")]
    public CharaStats _charaStats;
    public EquippableStats _equippableStats;
    public Text[] _skills = new Text[3];

    [Header("Side Action Menu References")]
    public Text _caption;
    public Text[] _choices;

    //selections
    private int _totalAlivePlayers;
    private int _selectedActionMenu;
    private int _selectedHero;
    private int _selectedExitMenu;

    private List<int> _aliveHeroIdx = new List<int>();

    // Start is called before the first frame update

    void Start()
    {
        _skillSelectMenu.SetActive(false);
        _sideActionMenu.SetActive(false);
        _state = menuState.none;

        //Saving and generating stats for the first hero
        
        if (PlayerPrefs.GetInt("GameStarted", 0) == 0)
        {
            //generate first hero stats
            int index = 0; //first hero index
            
            Hero h = Instantiate(_gameManager._heroPool[index]); //first hero selalu sorath
            
            //3 random weapon (sword) stat, 1 or 2 random armor (shield) stat
            //weapon
            h._equippedWeapon = _equippable._weapons[0];
            Weapon w = h._equippedWeapon;


            List<int> rng_weapon = new List<int>();
            for(int i = 0; i < 4; i++) { rng_weapon.Add(i); }

            for (int i = 0; i < 3; i++)
            {
                int rng_idx = Random.Range(0, rng_weapon.Count);
                int rng = rng_weapon[rng_idx];
                rng_weapon.RemoveAt(rng_idx);

                int min = 0, max = 0;
                
                if (rng == 0)
                {
                    min = _equippable._weapons[0].getHP().min;
                    max = _equippable._weapons[0].getHP().max + 1;
                }
                else if (rng == 1)
                {
                    min = _equippable._weapons[0].getMAtk().min;
                    max = _equippable._weapons[0].getMAtk().max + 1;
                }
                else if (rng == 2)
                {
                    min = _equippable._weapons[0].getRAtk().min;
                    max = _equippable._weapons[0].getRAtk().max + 1;
                }
                else if (rng == 3)
                {
                    min = _equippable._weapons[0].getMDef().min;
                    max = _equippable._weapons[0].getMDef().max + 1;
                }
                else if (rng == 4)
                {
                    min = _equippable._weapons[0].getRDef().min;
                    max = _equippable._weapons[0].getRDef().max + 1;
                }
                
                int statRNG = Random.Range(min, max);
                
                w._statsGiven[rng] = statRNG;
                
            }
            
            //armor
            h._equippedArmor = _equippable._armors[0];
            Armor a = h._equippedArmor;
            
            int reps = Random.Range(1, 2 + 1);

            List<int> rng_armor = new List<int>();
            for (int i = 0; i < 3; i++) { rng_armor.Add(i); }

            for (int i = 0; i < reps; i++)
            {
                int rng_idx = Random.Range(0, rng_armor.Count);
                int rng = rng_armor[rng_idx];
                rng_armor.RemoveAt(rng_idx);

                int min = 0, max = 0;

                if (rng == 0)
                {
                    min = _equippable._armors[0].getHP().min;
                    max = _equippable._armors[0].getHP().max + 1;
                }
                else if (rng == 1)
                {
                    min = _equippable._armors[0].getMDef().min;
                    max = _equippable._armors[0].getMDef().max + 1;
                }
                else if (rng == 2)
                {
                    min = _equippable._armors[0].getRDef().min;
                    max = _equippable._armors[0].getRDef().max + 1;
                }
                
                int statRNG = Random.Range(min, max);
                a._statsGiven[rng] = statRNG;
            }

            //karena masih first playthrough, maxHP samakan dengan HP
            h.setHP(h.getNetMaxHP()); 

            //adding to hero list
            _gameManager._hero.Add(h); 

            //and alive hero index
            int idx = 0;
            _aliveHeroIdx.Add(idx); 

            //deactivate krn belon dibutuhkan, bakal di activate pas game start
            h.gameObject.SetActive(false);

            _totalAlivePlayers = 1; 
        }
        
        //if not first playthrough, init all players
        else
        {
            //count what hero index are still alive
            for (int i = 0; i < 4; i++)
            {
                if (PlayerPrefs.GetInt("HeroIsAlive" + i, 0) == 1)
                {
                    _aliveHeroIdx.Add(i);
                }
            }

            _totalAlivePlayers = _aliveHeroIdx.Count;

            //instantiate heroes
            for (int i = 0; i < _totalAlivePlayers; i++)
            { 
                int index = _aliveHeroIdx[i];
                Hero h = Instantiate(_gameManager._heroPool[index]);

                int initHP = PlayerPrefs.GetInt("HeroHP" + index, 100);
                int initMaxHP = PlayerPrefs.GetInt("HeroMaxHP" + index, 100);
                int initAP = PlayerPrefs.GetInt("HeroAP" + index, 20);
                h.setHP(initHP);
                h.setAP(initAP);
                h.setMaxHP(initMaxHP);

                //loading weapon
                int weaponIdx = PlayerPrefs.GetInt("HeroWeaponIdx" + index, 0); //dapetin tipe
                h._equippedWeapon = _equippable._weapons[weaponIdx]; //masukin ke player
                for(int j = 0; j < 5; j++) //copy tiap stats
                {
                    h._equippedWeapon._statsGiven[j] = PlayerPrefs.GetInt("HeroWeaponIdx" + index + "_wsIdx" + j, 0);
                }

                //loading armor
                int armorIdx = PlayerPrefs.GetInt("HeroArmorIdx" + index, 0); //dapetin tipe
                h._equippedArmor = _equippable._armors[armorIdx]; //masukin ke player
                for (int j = 0; j < 3; j++) //copy tiap stats
                {
                    h._equippedArmor._statsGiven[j] = PlayerPrefs.GetInt("HeroArmorIdx" + index + "_asIdx" + j, 0);
                }

                //adding to hero list
                _gameManager._hero.Add(h);

                //deactivate krn belon dibutuhkan, bakal di activate pas game start
                h.gameObject.SetActive(false);
            }
        }
        initPreBattle();
    }

    public void initPreBattle()
    {
        _state = menuState.actionMenu;
        _cursor.SetActive(false);

        _selectedHero = 0;
        updateHeroMenu();
        showHeroMenu();

        _selectedActionMenu = 0;
        resetText(_actionMenuText);
        _actionMenuText[2].text = "Start Battle (Wave " + PlayerPrefs.GetInt("Wave", 1) + ")";
        _actionMenuText[_selectedActionMenu].color = Color.yellow;
    }

    public void updateHeroMenu()
    {
        //blackout everything
        for (int i = 0; i < 4; i++)
        {
            _heroStatMenu[i]._name.text = _heroStatMenu[i]._HP.text = _heroStatMenu[i]._AP.text = _heroStatMenu[i]._armorName.text = _heroStatMenu[i]._weaponName.text = "";

            _heroStatMenu[i]._armorIcon.sprite = _heroStatMenu[i]._weaponIcon.sprite = null;
            _heroStatMenu[i]._armorIcon.color = _heroStatMenu[i]._weaponIcon.color = Color.clear;

            _heroStatMenu[i]._gameObject.GetComponent<Image>().color = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        }

        //show the menu
        for (int i = 0; i < _aliveHeroIdx.Count; i++)
        {           
            _heroStatMenu[i]._gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f);
            int index = _aliveHeroIdx[i];

            Hero _hero = _gameManager._hero[i];

            Weapon w = _hero._equippedWeapon;
            Armor a = _hero._equippedArmor;

            //HP and AP
            int initHP = PlayerPrefs.GetInt("HeroHP" + index, 100);
            int initMaxHP = PlayerPrefs.GetInt("HeroMaxHP" + index, 100);
            int initAP = PlayerPrefs.GetInt("HeroAP" + index, 20);
            int MaxHPBoost = w._statsGiven[0] + a._statsGiven[0];

            _heroStatMenu[i]._name.text = _gameManager._heroPool[index]._name.ToString();
            _heroStatMenu[i]._HP.text = "HP "+initHP+"/"+(initMaxHP + MaxHPBoost);
            _heroStatMenu[i]._AP.text = "AP "+initAP+"/20";

            //Weapon and Armor
            int initWeaponIdx = PlayerPrefs.GetInt("HeroWeaponIdx" + index, 0);
            int initArmorIdx = PlayerPrefs.GetInt("HeroArmorIdx" + index, 0);

            _heroStatMenu[i]._armorIcon.color = _heroStatMenu[i]._weaponIcon.color = Color.white;

            _heroStatMenu[i]._armorIcon.sprite = _equippable._armors[initArmorIdx]._icon;
            _heroStatMenu[i]._armorName.text = _equippable._armors[initArmorIdx]._name;

            _heroStatMenu[i]._weaponIcon.sprite = _equippable._weapons[initWeaponIdx]._icon;
            _heroStatMenu[i]._weaponName.text = _equippable._weapons[initWeaponIdx]._name;
        }
    }

    public void showHeroMenu()
    {
        int index = _aliveHeroIdx[_selectedHero];

        //Chara Stats
        int initHP = PlayerPrefs.GetInt("HeroHP" + index, 100);
        int initMaxHP = PlayerPrefs.GetInt("HeroMaxHP" + index, 100);
        int initAP = PlayerPrefs.GetInt("HeroAP" + index, 20);

        Hero _hero = _gameManager._hero[_selectedHero];

        Weapon w = _hero._equippedWeapon;
        Armor a = _hero._equippedArmor;

        int MaxHPBoost = w._statsGiven[0] + a._statsGiven[0];
        int MAtkBoost = w._statsGiven[1];
        int RAtkBoost = w._statsGiven[2];
        int MDefBoost = w._statsGiven[3] + a._statsGiven[1];
        int RDefBoost = w._statsGiven[4] + a._statsGiven[2];

        _charaStats._name.text = _hero._name.ToString();
        _charaStats._HP.text = "HP        " + initHP + "/" + (initMaxHP + MaxHPBoost);
        _charaStats._AP.text = "AP       " + initAP + "/20";
        _charaStats._MAtk.text = "MAtk.  (" + (_hero.getMAtk().min + MAtkBoost) + "-" + (_hero.getMAtk().max + MAtkBoost) + ")";
        _charaStats._RAtk.text = "Ratk.  (" + (_hero.getRAtk().min + RAtkBoost) + "-" + (_hero.getRAtk().max + RAtkBoost) + ")";
        _charaStats._MDef.text = "MDef:   " + (_hero.getMDef() + MDefBoost);
        _charaStats._RDef.text = "RDef.   " + (_hero.getRDef() + RDefBoost);

        //Equippable Stats
        int initWeaponIdx = PlayerPrefs.GetInt("HeroWeapon" + index, 0);
        int initArmorIdx = PlayerPrefs.GetInt("HeroArmor" + index, 0);

        _equippableStats._armorIcon.sprite = _equippable._armors[initArmorIdx]._icon;
        _equippableStats._armorName.text = _equippable._armors[initArmorIdx]._name;

        _equippableStats._weaponIcon.sprite = _equippable._weapons[initWeaponIdx]._icon;
        _equippableStats._weaponName.text = _equippable._weapons[initWeaponIdx]._name;


        int count = 0;
        for(int i = 0; i < 5; i++)
        {
            if (w._statsGiven[i] == 0) continue;
            if (count >= _equippableStats._weaponStats.Length) break;

            int value = w._statsGiven[i];
            if (i == 0) _equippableStats._weaponStats[count].text = "HP        + " + value;
            else if (i == 1) _equippableStats._weaponStats[count].text = "MAtk.  + " + value;
            else if (i == 2) _equippableStats._weaponStats[count].text = "RAtk.   + " + value;
            else if (i == 3) _equippableStats._weaponStats[count].text = "MDef:   + " + value;
            else if (i == 4) _equippableStats._weaponStats[count].text = "RDef:    + " + value;
            count++;
        }


        for(int i = 0; i < 2; i++)
            _equippableStats._armorStats[i].text = "";

        count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (a._statsGiven[i] == 0) continue;
            if (count >= _equippableStats._armorStats.Length) break;

            int value = a._statsGiven[i];
            if (i == 0) _equippableStats._armorStats[count].text = "HP        + " + value;
            else if (i == 1) _equippableStats._armorStats[count].text = "MDef:   + " + value;
            else if (i == 2) _equippableStats._armorStats[count].text = "RDef:    + " + value;
            count++;
        }
    }

    private float arrowStartTime;
    private bool _isChangingSkill, _isSwappingEquipment;
    // Update is called once per frame
    void Update()
    {
        //Action Menu
        if(_state == menuState.actionMenu)
        {
            //Navigation
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedActionMenu--;
                if (_selectedActionMenu < 0) _selectedActionMenu = _actionMenuText.Length - 1;
                resetText(_actionMenuText);
                setActionMenu();
            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedActionMenu++;
                if (_selectedActionMenu >= _actionMenuText.Length) _selectedActionMenu = 0;
                resetText(_actionMenuText);
                setActionMenu();
            }
            //selection
            if(Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                if(_selectedActionMenu == 0)
                {
                    updateArrowTime();
                    _actionMenuText[_selectedActionMenu].color = Color.white;
                    _isChangingSkill = true;
                    _cursor.SetActive(true);
                    setCursorPosition();
                    _state = menuState.heroPreview;
                }
                if(_selectedActionMenu == 1 && _totalAlivePlayers > 1)
                {
                    updateArrowTime();
                    _actionMenuText[_selectedActionMenu].color = Color.white;
                    _isSwappingEquipment = true;
                    _cursor.SetActive(true);
                    setCursorPosition();
                    _state = menuState.heroPreview;
                }
                if (_selectedActionMenu == 2)
                {
                    updateArrowTime();
                    _actionMenuText[_selectedActionMenu].color = Color.white;
                    _state = menuState.gameStart;
                    _gameManager.initGame();
                }
                if(_selectedActionMenu == 3)
                {
                    updateArrowTime();
                    _actionMenuText[_selectedActionMenu].color = Color.white;
                    _state = menuState.promptExit;
                    _sideActionMenu.SetActive(true);
                    _selectedExitMenu = 1;
                    _choices[_selectedExitMenu].color = Color.yellow;
                    _caption.text = "Are you sure?";
                    _choices[0].text = "Yes";
                    _choices[1].text = "No";
                    _choices[2].text = "";
                }
            }
        }
        if(_state == menuState.promptExit)
        {
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedExitMenu++;
                _selectedExitMenu %= 2;
                resetText(_choices);
                _choices[_selectedExitMenu].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.Escape) || (_selectedExitMenu == 1 && Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f))
            {
                updateArrowTime();
                _sideActionMenu.SetActive(false);
                _state = menuState.actionMenu;
                _actionMenuText[_selectedActionMenu].color = Color.yellow;
            }
            if ((_selectedExitMenu == 0 && Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f))
            {
                updateArrowTime();
                Application.Quit();
            }
        }

        //preview hero sebelum swap/change stat
        if(_state == menuState.heroPreview)
        {
            //Navigation
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedHero--;
                if (_selectedHero < 0) _selectedHero = _totalAlivePlayers - 1;
                showHeroMenu();
                setCursorPosition();

            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedHero++;
                if (_selectedHero >= _totalAlivePlayers) _selectedHero = 0;
                showHeroMenu();
                setCursorPosition();
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                updateArrowTime();
                _cursor.SetActive(false);
                _state = menuState.actionMenu;
                _isChangingSkill = false;
                _isSwappingEquipment = false;
                _actionMenuText[_selectedActionMenu].color = Color.yellow;
            }
        }
    }

    public void setCursorPosition()
    {
        _cursor.transform.position = _heroStatMenu[_selectedHero]._gameObject.transform.position + new Vector3(-4.375f, 1.5f, 0);
    }

    public void updateArrowTime()
    {
        arrowStartTime = Time.time;
        _sfx.cursor();
    }

    public void resetText(Text[] _textArray)
    {
        for(int i = 0; i < _textArray.Length; i ++)
        {
            _textArray[i].color = Color.white;   
        }

        if (_textArray == _actionMenuText && _totalAlivePlayers == 1) _textArray[1].color = Color.gray;
    }

    public void setActionMenu()
    {
        bool red = (_selectedActionMenu == 1 && _totalAlivePlayers == 1);
        _actionMenuText[_selectedActionMenu].color = red ? Color.red : Color.yellow;
    }

    public enum menuState
    {
        none,
        gameStart,
        actionMenu,
        heroPreview,
        skillSelectMenu,
        sideActionMenu,
        promptExit,
    }
}
