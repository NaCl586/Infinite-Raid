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
    public Text[] _armorStats = new Text[3];
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
    public Text[] _skillsAP = new Text[3];

    [Header("Side Action Menu References")]
    public Text _caption;
    public Text[] _choices;

    [Header("Skill Select Menu References")]
    public Text[] _skillNames;
    public Image[] _skillImages;
    public Text _skillCaption;
    public Text[] _skillMenuAP;

    [Header("Post Battle Menu References")]
    public GameObject _postBattleMenu;
    public Text _itemName;
    public Image _itemIcon;
    public Text[] _itemStats;
    public Text[] _pbmTexts;
    public Text _pbmCaption;

    //selections
    private int _totalAlivePlayers;
    private int _selectedActionMenu;
    private int _selectedHero;
    private int _selectedExitMenu;
    private int _selectedSkill;
    private int _selectedSkillToReplace;
    private int _selectedWhatToSwapMenu;
    private int _selectedWhoToSwapWith;
    private int _selectedPostBattleMenu;
    private swapType _swapType;

    private int _postBattleItems;
    private int _postBattleIdx;
    private Armor _postBattleArmor;
    private Weapon _postBattleWeapon;

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
            instantiateFirstHero();
        }
        
        //if not first playthrough, init all players
        else
        {
            instantiateAllHeroes();
        }
        initPreBattle();
    }

    public void instantiateFirstHero()
    {
        //delete previous data
        PlayerPrefs.DeleteAll();

        //generate first hero stats
        int index = 0; //first hero index

        Hero h = Instantiate(_gameManager._heroPool[index]); //first hero selalu sorath

        //3 random weapon (sword) stat, 1 or 2 random armor (shield) stat
        //weapon
        h._equippedWeapon = _gameManager.generateWeapon(0);

        //armor
        h._equippedArmor = _gameManager.generateArmor(0);

        //skill
        h._equippedArmor._skills[0] = 0; //skill healing pasti ada
        h._equippedArmor._skills[2] = -1; //sword only have 1 skill
        h._equippedArmor._skills[1] = Random.Range(4, 6); //diantara skill idx 4 or 5

        //karena masih first playthrough, maxHP samakan dengan HP
        h.setHP(h.getNetMaxHP());
        PlayerPrefs.SetInt("HeroHP" + index, h.getNetMaxHP());

        //adding to hero list
        _gameManager._hero.Add(h);

        //and alive hero index
        int idx = 0;
        _aliveHeroIdx.Add(idx);

        //deactivate krn belon dibutuhkan, bakal di activate pas game start
        h.gameObject.SetActive(false);

        _totalAlivePlayers = 1;
    }

    public void instantiateAllHeroes()
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
            h._equippedWeapon = Instantiate(_equippable._weapons[weaponIdx]); //masukin ke player
            for (int j = 0; j < 5; j++) //copy tiap stats
            {
                h._equippedWeapon._statsGiven[j] = PlayerPrefs.GetInt("HeroWeaponIdx" + index + "_wsIdx" + j, 0);
            }

            //loading armor
            int armorIdx = PlayerPrefs.GetInt("HeroArmorIdx" + index, 0); //dapetin tipe
            h._equippedArmor = Instantiate(_equippable._armors[armorIdx]); //masukin ke player
            for (int j = 0; j < 3; j++) //copy tiap stats
            {
                h._equippedArmor._statsGiven[j] = PlayerPrefs.GetInt("HeroArmorIdx" + index + "_asIdx" + j, 0);
            }

            //loading skills
            for (int j = 0; j < 3; j++)
            {
                h._equippedArmor._skills[j] = PlayerPrefs.GetInt("HeroSkills" + index + "_asIdx" + j, -1);
            }

            //adding to hero list
            _gameManager._hero.Add(h);

            //deactivate krn belon dibutuhkan, bakal di activate pas game start
            h.gameObject.SetActive(false);
        }
    }

    public void initPostBattle()
    {
        _aliveHeroIdx.Clear();
        instantiateAllHeroes();
        updateHeroMenu();

        _postBattleMenu.SetActive(true);
        _cursor.SetActive(false);

        _postBattleItems = _gameManager._weaponDrops.Count + _gameManager._armorDrops.Count;
        _postBattleIdx = 0;
        _selectedPostBattleMenu = 0;

        setPostBattleCaption(_postBattleIdx);

        resetText(_pbmTexts);
        _pbmTexts[_selectedPostBattleMenu].color = Color.yellow;

        _state = menuState.postBattle;
        
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

        _selectedSkill = 0;
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
            int initWeaponIdx = 0;
            for(int j=0;j<_gameManager._equippable._weapons.Length;j++)
                if (w._name == _gameManager._equippable._weapons[j]._name) initWeaponIdx = j;

            int initArmorIdx = 0;
            for (int j = 0; j < _gameManager._equippable._armors.Length; j++)
                if (a._name == _gameManager._equippable._armors[j]._name) initArmorIdx = j;

            _heroStatMenu[i]._armorIcon.color = _heroStatMenu[i]._weaponIcon.color = Color.white;

            _heroStatMenu[i]._armorIcon.sprite = _equippable._armors[initArmorIdx]._icon;
            _heroStatMenu[i]._armorName.text = _equippable._armors[initArmorIdx]._name;

            _heroStatMenu[i]._weaponIcon.sprite = _equippable._weapons[initWeaponIdx]._icon;
            _heroStatMenu[i]._weaponName.text = _equippable._weapons[initWeaponIdx]._name;
        }
    }

    public void showHeroMenu()
    {
        int index;

        if (_state != menuState.swapWithWho) index = _aliveHeroIdx[_selectedHero];
        else index = _aliveHeroIdx[_selectedWhoToSwapWith];

        //Chara Stats
        int initHP = PlayerPrefs.GetInt("HeroHP" + index, 100);
        int initMaxHP = PlayerPrefs.GetInt("HeroMaxHP" + index, 100);
        int initAP = PlayerPrefs.GetInt("HeroAP" + index, 20);

        Hero _hero;
        if (_state != menuState.swapWithWho) _hero = _gameManager._hero[_selectedHero];
        else _hero = _gameManager._hero[_selectedWhoToSwapWith];

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
        int initWeaponIdx = 0;
        for (int j = 0; j < _gameManager._equippable._weapons.Length; j++)
            if (w._name == _gameManager._equippable._weapons[j]._name) initWeaponIdx = j;

        int initArmorIdx = 0;
        for (int j = 0; j < _gameManager._equippable._armors.Length; j++)
            if (a._name == _gameManager._equippable._armors[j]._name) initArmorIdx = j;

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
            bool negative = value < 0;
            string negSign = value < 0 ? "-" : "+";
            value = Mathf.Abs(value);

            if (negative)
            {
                if (i == 0) _equippableStats._weaponStats[count].text = "HP        <color=red>" + negSign + " " + value + "</color>";
                else if (i == 1) _equippableStats._weaponStats[count].text = "MAtk.  <color=red>" + negSign + " " + value + "</color>";
                else if (i == 2) _equippableStats._weaponStats[count].text = "RAtk.   <color=red>" + negSign + " " + value + "</color>";
                else if (i == 3) _equippableStats._weaponStats[count].text = "MDef:   <color=red>" + negSign + " " + value + "</color>";
                else if (i == 4) _equippableStats._weaponStats[count].text = "RDef:    <color=red>" + negSign + " " + value + "</color>";
            }
            else
            {
                if (i == 0) _equippableStats._weaponStats[count].text = "HP        <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 1) _equippableStats._weaponStats[count].text = "MAtk.  <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 2) _equippableStats._weaponStats[count].text = "RAtk.   <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 3) _equippableStats._weaponStats[count].text = "MDef:   <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 4) _equippableStats._weaponStats[count].text = "RDef:    <color=lime>" + negSign + " " + value + "</color>";
            }
            count++;
        }


        for(int i = 0; i < 3; i++)
            _equippableStats._armorStats[i].text = "";

        count = 0;
        for (int i = 0; i < 2; i++)
        {
            if (a._statsGiven[i] == 0) continue;
            if (count >= _equippableStats._armorStats.Length) break;

            int value = a._statsGiven[i];
            bool negative = value < 0;
            string negSign = value < 0 ? "-" : "+";
            value = Mathf.Abs(value);

            if (negative)
            {
                if (i == 0) _equippableStats._armorStats[count].text = "HP        <color=red>" + negSign + " " + value + "</color>";
                else if (i == 1) _equippableStats._armorStats[count].text = "MDef:   <color=red>" + negSign + " " + value + "</color>";
                else if (i == 2) _equippableStats._armorStats[count].text = "RDef:    <color=red>" + negSign + " " + value + "</color>";
            }
            else
            {
                if (i == 0) _equippableStats._armorStats[count].text = "HP        <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 1) _equippableStats._armorStats[count].text = "MDef:   <color=lime>" + negSign + " " + value + "</color>";
                else if (i == 2) _equippableStats._armorStats[count].text = "RDef:    <color=lime>" + negSign + " " + value + "</color>";
            }
                
            count++;
        }
        _equippableStats._armorStats[2].text = "Skill slots:   " + a._slots;


        //Skills
        foreach (Text t in _skills) t.text = "";
        foreach (Text t in _skillsAP) t.text = "";

        for (int i = 0; i < a._slots; i++)
        {
            if (a._skills[i] != -1)
            {
                _skills[i].text = _skillList._skillList[a._skills[i]]._skillName;
                _skillsAP[i].text = _skillList._skillList[a._skills[i]]._APNeeded.ToString();
            }
            else
            {
                _skills[i].text = "[Empty]";
                _skillsAP[i].text = "-";
            }
        }
    }

    private float arrowStartTime;
    public void setArrowStartTime(float _arrowStartTime) { arrowStartTime = _arrowStartTime;}

    private bool _isChangingSkill, _isSwappingEquipment;
    // Update is called once per frame
    void Update()
    {
        //Action Menu
        if (_state == menuState.actionMenu)
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

        //Prompt exit
        if (_state == menuState.promptExit)
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
        if (_state == menuState.heroPreview)
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
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _cursor.SetActive(false);
                _state = menuState.actionMenu;
                _isChangingSkill = false;
                _isSwappingEquipment = false;
                _actionMenuText[_selectedActionMenu].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                if (_isChangingSkill)
                {
                    _state = menuState.skillSelectMenu;
                    _skillSelectMenu.SetActive(true);
                    selectSkill(_selectedSkill);
                }
                else if (_isSwappingEquipment)
                {
                    _state = menuState.whatToSwapMenu;
                    _sideActionMenu.SetActive(true);
                    resetText(_choices);
                    _selectedWhatToSwapMenu = 0;
                    _choices[_selectedWhatToSwapMenu].color = Color.yellow;
                    _caption.text = "What to swap?";
                    _choices[0].text = "Weapons";
                    _choices[1].text = "Armors";
                    _choices[2].text = "Both";
                }
            }
        }

        //what to swap menu
        if (_state == menuState.whatToSwapMenu)
        {
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedWhatToSwapMenu--;
                if (_selectedWhatToSwapMenu < 0) _selectedWhatToSwapMenu = 2;
                resetText(_choices);
                _choices[_selectedWhatToSwapMenu].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedWhatToSwapMenu++;
                if (_selectedWhatToSwapMenu >= 3) _selectedWhatToSwapMenu = 0;
                resetText(_choices);
                _choices[_selectedWhatToSwapMenu].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                resetText(_choices);
                _sideActionMenu.SetActive(false);
                _state = menuState.heroPreview;
                _cursor.SetActive(true);
                setCursorPosition();
            }
            if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();

                if (_selectedWhatToSwapMenu == 0) _swapType = swapType.weapons;
                else if (_selectedWhatToSwapMenu == 1) _swapType = swapType.armors;
                else if (_selectedWhatToSwapMenu == 2) _swapType = swapType.both;

                _heroStatMenu[_selectedHero]._gameObject.GetComponent<Image>().color = new Color(0.45f, 0.45f, 0.45f, 0.75f);
                _heroStatMenu[_selectedHero]._name.color = _heroStatMenu[_selectedHero]._HP.color = _heroStatMenu[_selectedHero]._AP.color = _heroStatMenu[_selectedHero]._armorName.color = _heroStatMenu[_selectedHero]._weaponName.color = Color.gray;

                _state = menuState.swapWithWho;
                _selectedWhoToSwapWith = 0;
                if (_selectedWhoToSwapWith == _selectedHero) _selectedWhoToSwapWith++;
                showHeroMenu();
                setCursorPosition();
            }
        }

        //swap with who menu
        if (_state == menuState.swapWithWho)
        {
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedWhoToSwapWith--;
                if (_selectedWhoToSwapWith >= _totalAlivePlayers) _selectedWhoToSwapWith = 0;
                if (_selectedWhoToSwapWith < 0) _selectedWhoToSwapWith = _totalAlivePlayers - 1;

                while (_selectedWhoToSwapWith == _selectedHero)
                {
                    _selectedWhoToSwapWith--;
                    if (_selectedWhoToSwapWith >= _totalAlivePlayers) _selectedWhoToSwapWith = 0;
                    if (_selectedWhoToSwapWith < 0) _selectedWhoToSwapWith = _totalAlivePlayers - 1;
                }
                showHeroMenu();
                setCursorPosition();

            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedWhoToSwapWith++;
                if (_selectedWhoToSwapWith >= _totalAlivePlayers) _selectedWhoToSwapWith = 0;
                if (_selectedWhoToSwapWith < 0) _selectedWhoToSwapWith = _totalAlivePlayers - 1;

                while (_selectedWhoToSwapWith == _selectedHero)
                {
                    _selectedWhoToSwapWith++;
                    if (_selectedWhoToSwapWith >= _totalAlivePlayers) _selectedWhoToSwapWith = 0;
                    if (_selectedWhoToSwapWith < 0) _selectedWhoToSwapWith = _totalAlivePlayers - 1;
                }
                showHeroMenu();
                setCursorPosition();
            }
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _state = menuState.whatToSwapMenu;
                showHeroMenu();
                setCursorPosition();
                _heroStatMenu[_selectedHero]._gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f);
                _heroStatMenu[_selectedHero]._name.color = _heroStatMenu[_selectedHero]._HP.color = _heroStatMenu[_selectedHero]._AP.color = _heroStatMenu[_selectedHero]._armorName.color = _heroStatMenu[_selectedHero]._weaponName.color = Color.white;
            }
            if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                arrowStartTime = Time.time;
                _sfx.confirm();

                _heroStatMenu[_selectedHero]._gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f);
                _heroStatMenu[_selectedHero]._name.color = _heroStatMenu[_selectedHero]._HP.color = _heroStatMenu[_selectedHero]._AP.color = _heroStatMenu[_selectedHero]._armorName.color = _heroStatMenu[_selectedHero]._weaponName.color = Color.white;
                _sideActionMenu.SetActive(false);

                //swap process
                if(_swapType == swapType.weapons || _swapType == swapType.both)
                {
                    Weapon _tempWeapon = _gameManager._hero[_selectedHero]._equippedWeapon;
                    _gameManager._hero[_selectedHero]._equippedWeapon = _gameManager._hero[_selectedWhoToSwapWith]._equippedWeapon;
                    _gameManager._hero[_selectedWhoToSwapWith]._equippedWeapon = _tempWeapon;
                }
                if (_swapType == swapType.armors || _swapType == swapType.both)
                {
                    Armor _tempArmor = _gameManager._hero[_selectedHero]._equippedArmor;
                    _gameManager._hero[_selectedHero]._equippedArmor = _gameManager._hero[_selectedWhoToSwapWith]._equippedArmor;
                    _gameManager._hero[_selectedWhoToSwapWith]._equippedArmor = _tempArmor;
                }

                //reset skills
                _gameManager._hero[_selectedHero]._equippedArmor._skills[0] = 0;
                _gameManager._hero[_selectedHero]._equippedArmor._skills[1] = _gameManager._hero[_selectedHero]._equippedArmor._skills[2] = -1;

                _gameManager._hero[_selectedWhoToSwapWith]._equippedArmor._skills[0] = 0;
                _gameManager._hero[_selectedWhoToSwapWith]._equippedArmor._skills[1] = _gameManager._hero[_selectedWhoToSwapWith]._equippedArmor._skills[2] = -1;

                _cursor.SetActive(false);
                _state = menuState.actionMenu;

                updateHeroMenu();
                showHeroMenu();

                _isSwappingEquipment = false;
                _actionMenuText[_selectedActionMenu].color = Color.yellow;
            }
        }

        //skill select
        if (_state == menuState.skillSelectMenu)
        {
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _state = menuState.heroPreview;
                _skillSelectMenu.SetActive(false);
                _cursor.SetActive(true);
            }
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedSkill--;
                if (_selectedSkill < 0) _selectedSkill = 7;
                selectSkill(_selectedSkill);
            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedSkill++;
                if (_selectedSkill >= 8) _selectedSkill = 0;
                selectSkill(_selectedSkill);
            }
            if(Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                if (selectSkill(_selectedSkill))
                {
                    updateArrowTime();
                    _selectedSkillToReplace = 1;
                    _state = menuState.sideActionMenu;
                    resetText(_skills);
                    _skills[_selectedSkillToReplace].color = Color.yellow;
                }
            }
        }

        //selecting what skill to replace
        if (_state == menuState.sideActionMenu)
        {
            if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedSkillToReplace--;
                if (_selectedSkillToReplace < 1) _selectedSkillToReplace = _gameManager._hero[_selectedHero]._equippedArmor._slots - 1;
                resetText(_skills);
                _skills[_selectedSkillToReplace].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedSkillToReplace++;
                if (_selectedSkillToReplace >= _gameManager._hero[_selectedHero]._equippedArmor._slots) _selectedSkillToReplace = 1;
                resetText(_skills);
                _skills[_selectedSkillToReplace].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                resetText(_skills);
                _state = menuState.skillSelectMenu;
            }
            //replace
            if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                arrowStartTime = Time.time;
                _gameManager._hero[_selectedHero]._equippedArmor._skills[_selectedSkillToReplace] = _selectedSkill + 1;
                
                _sfx.confirm();
                selectSkill(_selectedSkill);

                showHeroMenu();
                _skills[_selectedSkillToReplace].color = Color.white;

                _state = menuState.skillSelectMenu;
            }
        }

        //post battle
        if (_state == menuState.postBattle)
        {
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _selectedPostBattleMenu++;
                _selectedPostBattleMenu %= 2;
                resetText(_pbmTexts);
                _pbmTexts[_selectedPostBattleMenu].color = Color.yellow;
            }
            //skip
            if ((_selectedPostBattleMenu == 1 && Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f))
            {
                updateArrowTime();
                postBattleNext();
            }
            //select hero
            if ((_selectedPostBattleMenu == 0 && Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f))
            {
                updateArrowTime();
                _cursor.SetActive(true);
                setCursorPosition();
                _state = menuState.postBattleHero;
            }
        }

        //selecting who to use the item drop
        if(_state == menuState.postBattleHero)
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
            if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
            {
                updateArrowTime();
                _cursor.SetActive(false);
                _state = menuState.postBattle;
                resetText(_pbmTexts);
                _pbmTexts[_selectedPostBattleMenu].color = Color.yellow;
            }
            if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
            {
                //use item
                arrowStartTime = Time.time;
                _sfx.confirm();

                if (_postBattleWeapon == null)
                {
                    _gameManager._hero[_selectedHero]._equippedArmor = _postBattleArmor;
                }
                else if(_postBattleArmor == null)
                {
                    _gameManager._hero[_selectedHero]._equippedWeapon = _postBattleWeapon;
                    _gameManager._hero[_selectedHero]._equippedArmor._skills[0] = 0;
                    _gameManager._hero[_selectedHero]._equippedArmor._skills[1] = -1;
                    _gameManager._hero[_selectedHero]._equippedArmor._skills[2] = -1;
                }
                    
                updateHeroMenu();
                showHeroMenu();
                
                postBattleNext();
            }
        }
    }

    void postBattleNext()
    {
        _postBattleIdx++;

        if (_postBattleIdx >= _postBattleItems)
        {
            _postBattleIdx--;
            _gameManager.saveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
            
        setPostBattleCaption(_postBattleIdx);

        _selectedPostBattleMenu = 0;
        resetText(_pbmTexts);
        _pbmTexts[_selectedPostBattleMenu].color = Color.yellow;
    }

    void setPostBattleCaption(int idx)
    {
        _pbmCaption.text = "Item (" + (idx+1) + "/" + _postBattleItems + "):";

        foreach (Text t in _itemStats) t.text = "";

        if (_gameManager._armorDrops.Count != 0)
        {
            Armor a = Instantiate(_gameManager._armorDrops[0]);

            _itemIcon.sprite = a._icon;
            _itemName.text = a._name;

            int count = 0;
            for (int i = 0; i < 2; i++)
            {
                if (a._statsGiven[i] == 0) continue;
                if (count >= _equippableStats._armorStats.Length) break;

                int value = a._statsGiven[i];
                bool negative = value < 0;
                string negSign = value < 0 ? "-" : "+";
                value = Mathf.Abs(value);

                if (negative)
                {
                    if (i == 0) _itemStats[count].text = "HP        <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 1) _itemStats[count].text = "MDef:   <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 2) _itemStats[count].text = "RDef:    <color=red>" + negSign + " " + value + "</color>";
                }
                else
                {
                    if (i == 0) _itemStats[count].text = "HP        <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 1) _itemStats[count].text = "MDef:   <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 2) _itemStats[count].text = "RDef:    <color=lime>" + negSign + " " + value + "</color>";
                }

                count++;
            }

            _itemStats[2].text = "Skill slots:   " + a._slots;

            _postBattleArmor = a;
            _postBattleWeapon = null;
            _gameManager._armorDrops.RemoveAt(0);
        }
        else if(_gameManager._weaponDrops.Count != 0)
        {
           Weapon w = _gameManager._weaponDrops[0];

            _itemIcon.sprite = w._icon;
            _itemName.text = w._name;

            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (w._statsGiven[i] == 0) continue;
                if (count >= _equippableStats._weaponStats.Length) break;

                int value = w._statsGiven[i];
                bool negative = value < 0;
                string negSign = value < 0 ? "-" : "+";
                value = Mathf.Abs(value);

                if (negative)
                {
                    if (i == 0) _itemStats[count].text = "HP        <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 1) _itemStats[count].text = "MAtk.  <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 2) _itemStats[count].text = "RAtk.   <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 3) _itemStats[count].text = "MDef:   <color=red>" + negSign + " " + value + "</color>";
                    else if (i == 4) _itemStats[count].text = "RDef:    <color=red>" + negSign + " " + value + "</color>";
                }
                else
                {
                    if (i == 0) _itemStats[count].text = "HP        <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 1) _itemStats[count].text = "MAtk.  <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 2) _itemStats[count].text = "RAtk.   <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 3) _itemStats[count].text = "MDef:   <color=lime>" + negSign + " " + value + "</color>";
                    else if (i == 4) _itemStats[count].text = "RDef:    <color=lime>" + negSign + " " + value + "</color>";
                }
                count++;
            }

            _postBattleArmor = null;
            _postBattleWeapon = w;
            _gameManager._weaponDrops.RemoveAt(0);
        }
    }

    public bool selectSkill(int index)
    {
        _skillCaption.text = _skillList._skillList[index + 1]._skillDesc;

        bool _onlyOneSkill = _gameManager._hero[_selectedHero]._equippedArmor._slots <= 1;

        bool[] valid = new bool[8];
        for (int i = 0; i < _skillNames.Length; i++)
        {
            valid[i] = false;
            _skillNames[i].color = Color.gray;
            _skillNames[i].text = _skillList._skillList[i + 1]._skillName;

            _skillMenuAP[i].color = Color.gray;
            _skillMenuAP[i].text = _skillList._skillList[i + 1]._APNeeded.ToString();

            if (_onlyOneSkill) continue;

            bool flag = false;
            for (int j = 1; j < 3; j++)
            {
                if (i+1 == _gameManager._hero[_selectedHero]._equippedArmor._skills[j])
                {
                    flag = true;
                    break;
                }

            }
            if (flag) continue;

            int initWeaponIdx = 0;
            for (int j = 0; j < _gameManager._equippable._weapons.Length; j++)
                if (_gameManager._hero[_selectedHero]._equippedWeapon._name == _gameManager._equippable._weapons[j]._name) initWeaponIdx = j;

            foreach (int j in _skillList._skillList[i + 1]._usableWeapons)
            {
                if (j == initWeaponIdx)
                {
                    valid[i] = true;
                    _skillNames[i].color = _skillMenuAP[i].color = Color.white;
                    break;
                }
            }
        }
        _skillNames[index].color = _skillMenuAP[index].color = valid[index] ? Color.yellow : Color.red;

        if (_onlyOneSkill) return false;
        return valid[index];
    }

    public void setCursorPosition()
    {
        if(_state != menuState.swapWithWho) _cursor.transform.position = _heroStatMenu[_selectedHero]._gameObject.transform.position + new Vector3(-4.375f, 1.5f, 0);
        else _cursor.transform.position = _heroStatMenu[_selectedWhoToSwapWith]._gameObject.transform.position + new Vector3(-4.375f, 1.5f, 0);
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

    public enum swapType
    {
        weapons,
        armors,
        both,
    }

    public enum menuState
    {
        none,
        gameStart,
        actionMenu,
        heroPreview,
        skillSelectMenu,
        sideActionMenu,
        whatToSwapMenu,
        swapWithWho,
        promptExit,
        postBattle,
        postBattleHero,
    }
}
