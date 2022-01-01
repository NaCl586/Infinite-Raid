using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class HeroStats
{
    public GameObject gameObject;
    public Image _weapon;
    public Text name;
    public Text HP;
    public Text AP;
}

[System.Serializable]
public class LevelUpStats
{
    public Text Name;
    public Text HP;
}

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    public bool enableDebug = false;
    private int _initSkillCount;
    [SerializeField] private Vector3 _enemiesDebug;
    private int _enemyCount;
    private float _scalingMultiplier = 1;

    [Header("GUI")]
    public GameObject preBattle;
    public GameObject game;
    public preBattleManager _preBattleManager;

    //public references
    [Header("Character Placements")]
    public GameObject[] Positions;
    //0-3 = player; 4-6 enemy

    [Header("Action")]
    public Text[] action;

    [Header("Hero")]
    public HeroStats[] heroStats;
    public Hero[] _heroPool;
    [HideInInspector] public List<Hero> _hero = new List<Hero>();
    public Text[] _heroNames;

    [Header("Enemy")]
    public Text[] _enemyNames;
    public Enemy[] _enemyPool;
    private List<Enemy> _enemy = new List<Enemy>();
    
    [Header("Popups")]
    public GameObject Damage;
    public GameObject BigMessage;
    public GameObject arrow;

    [Header("Skill")]
    public SkillList _skillList;
    public GameObject _skillBar;
    public GameObject _upperBar;
    public Text _skillDesc;
    public Text _APNeeded;
    public Text[] _skillNames;
    public GameObject _skillUsedBar;
    public Text _skillUsedName;

    [Header("Equippable List")]
    public Equippable _equippable;

    [Header("Level Up")]
    public GameObject _levelUpBar;
    public LevelUpStats[] _levelUpText;
    public Text _pressAnyKeyMessage;
    public Text _waveText;
    private int _messagesRemaining;
    [HideInInspector] public List<Weapon> _weaponDrops = new List<Weapon>();
    [HideInInspector] public List<Armor> _armorDrops = new List<Armor>();

    [Header("SFX")]
    public SFX _sfx;
    public music _music;

    //game related stuff
    private int _wave;
    private GameState _gameState;
    private bool _newHeroSpawned = false;

    //UI related stuff
    private menuState _selectedMenu;
    private int _selectedAction = 0;
    private int _selectedEnemy = 0;
    private int _selectedSkill = -1; //-1 artinya normal attack
    private int _selectedSkillMenu = 0;
    private int _selectedHero = 0; //for skill, krn pasti player cuma bs control siapapun di index 0
    private bool _enemyTawuran = false;

    //skill effects
    public GameObject _defenseUp;
    public Text _defenseUpText;
    private int _prerseveranceTurnsRemaining;
    private Enemy _birdOfPreyEnemyTarget;
    public GameObject _sleepyGameObject;
    private GameObject _sleepyGameObjectRef;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        //GameManager Init
        _gameState = GameState.none;

        //Wave init
        _wave = PlayerPrefs.GetInt("Wave", 1);
        _waveText.text = "Wave " + _wave;

        _scalingMultiplier = Utils.scalingMultiplier(_wave);

        game.SetActive(false);
        preBattle.SetActive(true);
        _music.preBattle();
        _gameState = GameState.preBattle;
    }

    public Weapon generateWeapon(int _weaponIndexRNG)
    {
        Weapon w = Instantiate(_equippable._weapons[_weaponIndexRNG]);

        List<int> rng_weapon = new List<int>();
        for (int i = 0; i < 4; i++) { rng_weapon.Add(i); }

        for (int i = 0; i < 3; i++)
        {
            int rng_idx = Random.Range(0, rng_weapon.Count);
            int rng = rng_weapon[rng_idx];
            rng_weapon.RemoveAt(rng_idx);

            int min = 0, max = 0;

            if (rng == 0)
            {
                min = _equippable._weapons[_weaponIndexRNG].getHP().min;
                max = _equippable._weapons[_weaponIndexRNG].getHP().max + 1;
            }
            else if (rng == 1)
            {
                min = _equippable._weapons[_weaponIndexRNG].getMAtk().min;
                max = _equippable._weapons[_weaponIndexRNG].getMAtk().max + 1;
            }
            else if (rng == 2)
            {
                min = _equippable._weapons[_weaponIndexRNG].getRAtk().min;
                max = _equippable._weapons[_weaponIndexRNG].getRAtk().max + 1;
            }
            else if (rng == 3)
            {
                min = _equippable._weapons[_weaponIndexRNG].getMDef().min;
                max = _equippable._weapons[_weaponIndexRNG].getMDef().max + 1;
            }
            else if (rng == 4)
            {
                min = _equippable._weapons[_weaponIndexRNG].getRDef().min;
                max = _equippable._weapons[_weaponIndexRNG].getRDef().max + 1;
            }

            int statRNG = Random.Range(min, max);

            w._statsGiven[rng] = statRNG;
        }

        return w;
    }

    public Armor generateArmor(int _armorIndexRNG)
    {
        Armor a = Instantiate(_equippable._armors[_armorIndexRNG]);

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
                min = _equippable._armors[_armorIndexRNG].getHP().min;
                max = _equippable._armors[_armorIndexRNG].getHP().max + 1;
            }
            else if (rng == 1)
            {
                min = _equippable._armors[_armorIndexRNG].getMDef().min;
                max = _equippable._armors[_armorIndexRNG].getMDef().max + 1;
            }
            else if (rng == 2)
            {
                min = _equippable._armors[_armorIndexRNG].getRDef().min;
                max = _equippable._armors[_armorIndexRNG].getRDef().max + 1;
            }

            int statRNG = Random.Range(min, max);
            a._statsGiven[rng] = statRNG;
        }

        return a;
    }

    public void instantiateNewHero()
    {
        //preventive measure supaya gak lebih
        if (_hero.Count == _heroPool.Length) return;

        int _index = Random.Range(0, 3+1);

        //kalo ada yg sama, maju satu nomor, terus cek ulang
        for (int i = 0; i < _hero.Count; i++)
        {
            while (_hero[i]._name == _heroPool[_index]._name)
            {
                _index++;
                _index %= _heroPool.Length;
            }
        }

        Hero h = Instantiate(_heroPool[_index]); //first hero selalu sorath

        //3 random weapon (sword) stat, 1 or 2 random armor (shield) stat
        //weapon
        int _weaponIndexRNG = Random.Range(0, _equippable._weapons.Length);
        h._equippedWeapon = generateWeapon(_weaponIndexRNG);

        //armor
        int _armorIndexRNG = Random.Range(0, _equippable._armors.Length);
        h._equippedArmor = generateArmor(_armorIndexRNG);

        //skill
        h._equippedArmor._skills = Utils.skillRandomizer(_weaponIndexRNG, _equippable._armors[_armorIndexRNG]._slots);

        //set HP krn first appearance, max HP adalah 95% dari max hp terendah dari party excluding skills
        int _minHP = 999999999;
        foreach(Hero _h in _hero)
        {
            if (_h.getMaxHP() < _minHP)
                _minHP = _h.getMaxHP();
        }

        h.setMaxHP(_minHP * 95 / 100);
        h.setHP(h.getNetMaxHP());

        //adding to hero list
        _hero.Add(h);

        //deactivate krn belon dibutuhkan, bakal di activate pas game start
        h.gameObject.SetActive(false);
    }

    public Enemy generateEnemy(int index)
    {
        Enemy e = Instantiate(_enemyPool[index]);
        float initHP = Random.Range(e.getMaxHP().min, e.getMaxHP().max + 1);

        initHP = initHP * _scalingMultiplier;
        e.setHP((int)initHP);
        e.setAP(0);

        return e;
    }

    public void initGame()
    { 
        _music.battle();

        game.SetActive(true);
        preBattle.SetActive(false);

        //this is to instantiate additional hero
        int chance = PlayerPrefs.GetInt("NewHeroSpawnChance", 0);
        Debug.Log(chance);
        int partyCount = _hero.Count;
        if (_wave == 5 && partyCount == 1)
        {
            instantiateNewHero();
        }
        else if(partyCount != 4)
        {
            float rng = Random.Range(0f, 1f);
            float gacha = chance / 100; //rng harus masuk kedalam range 0 - chance/100 kalo mau yes, which means rng <= chance
            if(rng <= chance)
            {
                instantiateNewHero();
                _newHeroSpawned = true;
            }
        }
        
        //hero instantiation moved to pre battle manager, disini cuma handle animation aja
        //Player Stat Init
        foreach (HeroStats h in heroStats)
            h.gameObject.SetActive(false);

        //set active + animation
        for (int i = 0; i < _hero.Count; i++)
        {
            heroStats[i].gameObject.SetActive(true);
            _hero[i].gameObject.SetActive(true);
            Hero h = _hero[i];
            Transform hpos = h.gameObject.transform;
            hpos.position = new Vector2(12, 0);
            hpos.transform.DOMove(Positions[i].transform.position, 0.5f);
        }

        //Enemy Init
        foreach (Text t in _enemyNames)
            t.gameObject.SetActive(false);

        _enemyCount = 0;
        int[] _initEnemies = new int[3];
        if(enableDebug)
        { 
            _initEnemies[0] = (int)_enemiesDebug.x; 
            _initEnemies[1] = (int)_enemiesDebug.y; 
            _initEnemies[2] = (int)_enemiesDebug.z; 
        }
        else
        {
            _initEnemies = Utils._enemySpawn(_wave, _hero.Count);
        }

        for (int i = 0; i < 3; i++)
        {
            int idx = _initEnemies[i];
            if (idx == -1) continue;

            _enemyCount++;

            _enemyNames[i].gameObject.SetActive(true);
            Enemy e = generateEnemy(idx);
            _enemy.Add(e);

            _enemyNames[i].text = e._name;
            Transform epos = e.gameObject.transform;
            epos.position = new Vector2(-7, 0);
            epos.transform.DOMove(Positions[i + 4].transform.position, 0.5f);
        }

        _selectedSkill = -1;
        setUsedSkillBarCaption(_selectedSkill);

        _prerseveranceTurnsRemaining = 0;
        _defenseUp.SetActive(false);

        _birdOfPreyEnemyTarget = null;

        //UI Init
        _levelUpBar.SetActive(false);
        _levelUpBar.GetComponent<Image>().color = Color.clear;
        displaySkillMenuAnim(false, false);
        arrow.SetActive(false);
        _messagesRemaining = 2;

        _selectedMenu = menuState.none;
        updateGameCondition();
        StartCoroutine(gameStart());
    }

    IEnumerator gameStart()
    {
        yield return new WaitForSeconds(1f);
        _gameState = GameState.heroTurn;
        _selectedMenu = menuState.actionSelect;
        setActionMenu();
        _sfx.select();
    }

    void InstantiateDamage(int amount, GameObject character, bool heal)
    {
        GameObject dmgText = Instantiate(Damage);
        dmgText.transform.position = character.transform.position;
        dmgText.transform.GetChild(0).GetComponent<Text>().color = heal ? Color.green : Color.white;
        if (heal && _selectedSkill != 6) _sfx.heal();
        dmgText.transform.GetChild(0).GetComponent<Text>().text = amount.ToString();
        dmgText.transform.DOMoveY(dmgText.transform.position.y + 0.5f, 0.3f).OnComplete(() => {
            dmgText.transform.DOMoveY(dmgText.transform.position.y - 0.5f, 0.3f).OnComplete(() => {
                Destroy(dmgText,1f);
            });
        });
    }

    void displaySkillMenuAnim(bool show, bool animate)
    {
        float dur = animate ? 0.5f : 0.0001f;
        //move to pos
        if (show)
        {
            _skillBar.SetActive(true);
            _upperBar.SetActive(true);

            _skillBar.transform.localPosition = new Vector2(-900f, 67.5f);
            _upperBar.transform.localPosition = new Vector2(-200f, 500f);

            _skillBar.transform.DOMoveX(_skillBar.transform.position.x + 11.5f, dur);
            _upperBar.transform.DOMoveY(_upperBar.transform.position.y - 2.25f, dur);

            foreach (Text t in _skillNames)
                t.gameObject.SetActive(false);

            for (int i = 0; i < _hero[_selectedHero]._equippedArmor._slots; i++)
            {
                if (_hero[_selectedHero]._equippedArmor._skills[i] == -1) continue;

                _skillNames[i].gameObject.SetActive(true);
                _skillNames[i].text = _skillList._skillList[_hero[_selectedHero]._equippedArmor._skills[i]]._skillName;
            }

        }
        else
        {
            _skillBar.transform.DOMoveX(_skillBar.transform.position.x - 11.5f, dur).OnComplete(() => {_skillBar.SetActive(false); });
            _upperBar.transform.DOMoveY(_upperBar.transform.position.y + 2.25f, dur).OnComplete(() => { _upperBar.SetActive(false); });
        }
    }

    IEnumerator clearSkillUsedText(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        setUsedSkillBarCaption(-1);
    }

    private float arrowStartTime;
    private bool enemyAttackInProgress, enemyAttackStarted;
    private bool npcAttackInProgress, npcAttackStarted;
    void Update()
    {
        //Player's Turn
        if(_gameState == GameState.heroTurn)
        {
            //Select Action Menu
            if(_selectedMenu == menuState.actionSelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeActionMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeActionMenu(1);
                if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
                {
                    if (_selectedAction == 0)
                    {
                        arrowStartTime = Time.time;
                        _sfx.cursor();
                        _selectedSkill = -1;
                        _selectedEnemy = 0;
                        _selectedMenu = menuState.enemySelect;
                        arrow.SetActive(true);
                        clearActionMenu();
                        setEnemyMenu();
                    }
                    if (_selectedAction == 1)
                    {
                        arrowStartTime = Time.time;
                        _sfx.cursor();
                        _selectedMenu = menuState.skillSelect;
                        if (_selectedSkill == -1) _selectedSkill = 0;
                        _selectedSkillMenu = 0;
                        clearActionMenu();
                        displaySkillMenuAnim(true, true);
                        setSkillMenu();
                    }
                    if( _selectedAction == 2)
                    {
                        arrowStartTime = Time.time;
                        _sfx.cursor();

                        enemyGeludCaption("All party members' AP restored!");
                        StartCoroutine(clearSkillUsedText(1f));
                        _sfx.gainAP();

                        _selectedSkill = -1;
                        regenerateAP(false, -2);
                        clearActionMenu();
                        _gameState = GameState.enemyTurn;
                    }
                }
            }

            //Skill Select Menu
            if (_selectedMenu == menuState.skillSelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeSkillMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeSkillMenu(1);

                if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.actionSelect;
                    displaySkillMenuAnim(false, true);
                    clearSkillMenu();
                    setActionMenu();
                }
                if (Input.GetKey(KeyCode.Return) && _hero[0].getAP() >= _skillList._skillList[_selectedSkill]._APNeeded && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    //skill 0 - prayer of healing perlu select player yg mau di heal
                    if (_selectedSkill == 0)
                    {
                        _selectedMenu = menuState.heroSelect;
                        arrow.SetActive(true);
                        setHeroMenu();
                    }
                    //skill 1 - preserverance: langsung set turn = 3
                    else if (_selectedSkill == 1)
                    {
                        _selectedMenu = menuState.skillInProgress;
                        arrow.SetActive(false);
                        displaySkillMenuAnim(false, false);
                        clearHeroMenu(true);
                        clearSkillMenu();
                        useSkill(_selectedSkill, 0, -1);
                    }
                    //skill yg perlu cari subject: indeks 2,3,4,5
                    else if (_selectedSkill >= 2 && _selectedSkill <= 5)
                    {
                        _selectedEnemy = 0;
                        _selectedMenu = menuState.enemySelect;
                        arrow.SetActive(true);
                        setEnemyMenu();
                    }
                    //skill semua subject: 6,7,8
                    else if(_selectedSkill >= 6 && _selectedSkill <= 8)
                    {
                        _selectedMenu = menuState.skillInProgress;
                        arrow.SetActive(false);
                        displaySkillMenuAnim(false, false);
                        clearHeroMenu(true);
                        clearSkillMenu();
                        useSkill(_selectedSkill, 0, -1);
                    }
                }
            }

            //Select Hero Menu
            if(_selectedMenu == menuState.heroSelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeHeroMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeHeroMenu(1);
                if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.skillSelect;
                    arrow.SetActive(false);
                    _selectedSkillMenu = 0;
                    clearHeroMenu(true);
                    setSkillMenu();
                }
                if (Input.GetKey(KeyCode.Return) && _hero[_selectedHero].getHP() < _hero[_selectedHero].getMaxHP() && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.skillInProgress;
                    arrow.SetActive(false);
                    displaySkillMenuAnim(false, false);
                    clearHeroMenu(true);
                    clearSkillMenu();
                    useSkill(_selectedSkill, 0, _selectedHero);
                }
            }

            //Select Enemy Menu
            if(_selectedMenu == menuState.enemySelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeEnemyMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeEnemyMenu(1);
                if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.actionSelect;
                    arrow.SetActive(false);
                    clearEnemyMenu();
                    setActionMenu();
                }
                if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.attackInProgress;
                    arrow.SetActive(false);
                    clearEnemyMenu();
                    //regular attack
                    if (_selectedSkill == -1)
                    {
                        PlayerGelud(0, _selectedEnemy);
                    }
                    //skill serang satu enemy: 2, 4, 5
                    else if(_selectedSkill == 2 || _selectedSkill == 4 || _selectedSkill == 5)
                    {
                        arrow.SetActive(false);
                        displaySkillMenuAnim(false, false);
                        clearHeroMenu(true);
                        clearSkillMenu();
                        useSkill(_selectedSkill, 0, -1);
                        PlayerGelud(0, _selectedEnemy);
                    }
                    //apply efek birdofprey
                    else if(_selectedSkill == 3)
                    {
                        _selectedMenu = menuState.skillInProgress;
                        arrow.SetActive(false);
                        displaySkillMenuAnim(false, false);
                        clearHeroMenu(true);
                        clearSkillMenu();
                        useSkill(_selectedSkill, 0, _selectedEnemy);
                    }
                }
            }
        }

        //Enemy's Turn
        if (_gameState == GameState.enemyTurn && !enemyAttackStarted)
        {
            StartCoroutine(SemuaEnemyGelud());
        }

        if(_gameState == GameState.npcTurn && !npcAttackStarted)
        {
            StartCoroutine(NPCGelud());
        }

        //Battle End
        if (_gameState == GameState.battleEnd)
        {
            clearActionMenu();
            clearEnemyMenu();
            clearHeroMenu(true);
        }

        //Wave End
        if (_gameState == GameState.waitingForInput)
        {
            if (Input.anyKey && (Time.time - arrowStartTime) > 0.175f)
            {
                arrowStartTime = Time.time;
                _messagesRemaining--;
                _gameState = GameState.none;
                if (_messagesRemaining == 0)
                {
                    //post game
                    _endGameHeader.SetActive(false);

                    foreach (Hero h in _hero)
                        h.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;

                    game.SetActive(false);
                    preBattle.SetActive(true);
                    _preBattleManager.setArrowStartTime(Time.time);
                    _preBattleManager.initPostBattle();
                }
                else if (_messagesRemaining == 1)
                {
                    _gameState = GameState.battleEnd;
                    secondMessage();
                }
            }
        }
    }

    void endPlayerTurn(int user)
    {
        regenerateAP(true, user);
        _selectedSkill = -1; //reset selected skill
        setUsedSkillBarCaption(_selectedSkill);
        if (_hero.Count > 1) _gameState = GameState.npcTurn; //AI turn
        else _gameState = GameState.enemyTurn;
    }

    void regenerateAP(bool partial, int user)
    {
        if (partial)
        {
            for (int i = 0; i < _hero.Count; i++)
            {
                if (_selectedSkill != -1 && i == user)
                {
                    continue;
                }
                int newAP = _hero[i].getAP() + Random.Range(1, 6 + 1);
                _hero[i].setAP(newAP > 20 ? 20 : newAP);
            }
        }
        else
        {
            for (int i = 0; i < _hero.Count; i++)
                _hero[i].setAP(20);
        }
        UpdateHPAP();
    }

    #region gelud satu2
    //NPC satu2 gelud
    //AI mechanics dimulai
    IEnumerator NPCGelud()
    {
        npcAttackStarted = true;
        yield return new WaitForSecondsRealtime(1.5f);
        for(int i = 1; i < _hero.Count; i++)
        {
            //kalo udah battle end, jgn diterusin
            if (_gameState == GameState.battleEnd) break;

            //get weapon dlu
            Weapon w = _hero[i]._equippedWeapon;
            Armor a = _hero[i]._equippedArmor;
            int rng = Random.Range(0, _enemy.Count);
            int target = -1;
            bool healFlag = false;
            int hpLowCount = 0;

            if (w._name == "Wand")
            {
                //cari yang bisa di heal
                for (int j = 0; j < _hero.Count; j++)
                {
                    if ((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30)
                    {
                        healFlag = true;
                        target = j;
                        //dan itung ada brp
                        hpLowCount++;
                    }
                }
                if(_hero[i].getAP() >= 13)
                {
                    if (healFlag)
                    {
                        if (hpLowCount == 1) //kalo cuma satu, heal that person aja
                        {
                            _selectedSkill = 0;
                            useSkill(0, i, target);
                        }
                        else //kalo lebih
                        {
                            if (_hero[i].getAP() >= 15) //kalo lebih, cek AP cukup buat lunar blessing or not
                            {
                                _selectedSkill = 6;
                                useSkill(6, i, -1);
                            }
                            if (_hero[i].getAP() >= 13) //kalo ga cukup, cek AP cukup buat prayer of healing or not
                            {
                                _selectedSkill = 0;
                                useSkill(0, i, target);
                            }
                            else //no: basic attack
                            {
                                _selectedSkill = -1;
                                PlayerGelud(i, rng);
                            }
                        }
                    }
                    else //kalo hp semua member msh banyak
                    {
                        if (_hero[i].getAP() >= 20) //sunlance strike
                        {
                            useSkill(2, i, rng);
                            _selectedSkill = 2;
                            PlayerGelud(i, rng);
                        }
                        else //no: basic attack
                        {
                            _selectedSkill = -1;
                            PlayerGelud(i, rng);
                        }
                    }
                }
                else //no: basic attack
                {
                    _selectedSkill = -1;
                    PlayerGelud(i, rng);
                }
            }
            else if (w._name == "Gauntlet")
            {
                //cari yg bisa diheal
                for (int j = 0; j < _hero.Count; j++)
                {
                    if ((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30)
                    {
                        healFlag = true;
                        target = j;
                        break;
                    }
                }
                if (healFlag && _hero[i].getAP() >= 13) //yes
                {
                    _selectedSkill = 0;
                    useSkill(0, i, target);
                }
                else
                {
                    if(_hero[i].getAP() >= 10)
                    {
                        //cari buffer
                        bool buffer = false;
                        for (int j = 0; j < _hero.Count; j++)
                        {
                            if (_hero[j]._equippedWeapon.isBuffer())
                            {
                                buffer = true;
                                break;
                            }
                        }
                        if (buffer) //yes there is buffer weapon
                        {
                            if (_hero[i].getAP() >= 18)
                            {
                                _selectedSkill = 5;
                                useSkill(5, i, -1);
                                PlayerGelud(i, rng);
                            }
                            else //no: basic attack
                            {
                                _selectedSkill = 5;
                                PlayerGelud(i, rng);
                            }
                        }
                        else //no: use preserverance
                        {
                            _selectedSkill = 1;
                            useSkill(1, 0, -1);
                        }
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
            }
            else if (w._name == "Bow")
            {
                //cari yg bisa diheal
                for (int j = 0; j < _hero.Count; j++)
                {
                    if ((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30)
                    {
                        healFlag = true;
                        target = j;
                        break;
                    }
                }
                //hp low?
                if (healFlag) //yes
                {
                    if (_hero[i].getAP() >= 13) //yes: use prayer of healing
                    {
                        _selectedSkill = 0;
                        useSkill(0, i, target);
                    }
                    if (_hero[i].getAP() >= 8) //yes: use bird of prey
                    {
                        _selectedSkill = 3;
                        useSkill(3, i, rng);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
                else //no
                {
                    if (_hero[i].getAP() >= 20) //yes: use firestorm
                    {
                        _selectedSkill = 7;
                        useSkill(7, i, -1);
                    }
                    else if (_hero[i].getAP() >= 8) //yes: use bird of prey
                    {
                        _selectedSkill = 3;
                        useSkill(3, i, rng);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
            }
            else if (w._name == "Sword")
            {
                //cari yg bisa diheal
                for (int j = 0; j < _hero.Count; j++)
                {
                    if ((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30)
                    {
                        healFlag = true;
                        target = j;
                        break;
                    }
                }
                //hp low?
                if (healFlag) //yes
                {
                    if (_hero[i].getAP() >= 13) //yes: use prayer of healing
                    {
                        _selectedSkill = 0;
                        useSkill(0, i, target);
                    }
                    if (_hero[i].getAP() >= 10) //yes: use chrono slash
                    {
                        _selectedSkill = 4;
                        useSkill(4, i, -1);
                        PlayerGelud(i, rng);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
                else //no
                {
                    if (_hero[i].getAP() >= 18) //yes: use daisycutter
                    {
                        _selectedSkill = 5;
                        useSkill(5, i, -1);
                        PlayerGelud(i, rng);
                    }
                    else if (_hero[i].getAP() >= 10) //yes: use chrono slash
                    {
                        _selectedSkill = 4;
                        useSkill(4, i, -1);
                        PlayerGelud(i, rng);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
            }
            else if (w._name == "Morning Star")
            {
                //cari yg bisa diheal
                for (int j = 0; j < _hero.Count; j++)
                {
                    if ((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30)
                    {
                        healFlag = true;
                        target = j;
                        break;
                    }
                }
                //hp low?
                if (healFlag) //yes
                {
                    if (_hero[i].getAP() >= 13) //yes: use prayer of healing
                    {
                        _selectedSkill = 0;
                        useSkill(0, i, target);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
                else //no
                {
                    if (_hero[i].getAP() >= 20) //yes: use daisycutter
                    {
                        _selectedSkill = 8;
                        useSkill(8, i, -1);
                    }
                    else if (_hero[i].getAP() >= 18) //yes: use midnight elegance
                    {
                        _selectedSkill = 5;
                        useSkill(5, i, -1);
                        PlayerGelud(i, rng);
                    }
                    else //no: basic attack
                    {
                        _selectedSkill = -1;
                        PlayerGelud(i, rng);
                    }
                }
            }
            else
            {
                //preventive measure = basic atack
                _selectedSkill = -1;
                PlayerGelud(i, rng);
            }

            if (healFlag)
            {
                yield return new WaitForSecondsRealtime(3.8f);
                continue;
            }

            while (npcAttackInProgress)
            {
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1f);
        }
        if (_gameState != GameState.battleEnd)
        {
            _gameState = GameState.enemyTurn;
            _selectedSkill = -1;
        }
        npcAttackStarted = false;
    }

    //Enemy satu2 gelud
    IEnumerator SemuaEnemyGelud()
    {
        enemyAttackStarted = true;
        yield return new WaitForSecondsRealtime(1.5f);
        for (int i = 0; i < _enemy.Count; i++)
        {
            if (_enemy[i] == _birdOfPreyEnemyTarget) continue;

            //kalo udah battle end, jgn diterusin
            if (_gameState == GameState.battleEnd) break;

            int rng = Random.Range(1, 100) % _hero.Count;

            if (_wave > 5 && _enemy[i].getAP() >= 20)
            {
                //50:50 chance special attack
                int fate = Random.Range(0, 2);
                if(fate == 1) //special attack
                {
                    _enemy[i].setAP(0);
                    _enemyTawuran = true;
                    EnemyGelud(i, rng);
                    continue;
                }
                else
                {
                    _enemy[i].setAP(Random.Range(10,16));
                }
            }
            //keluar loop = gak tawuran
            _enemyTawuran = false;

            //bakal beda2 tergantung enemy apa
            if(_enemy[i]._name == "Rogue")
            {
                bool healer = false;
                int target = 0;
                for(int j = 0; j < _hero.Count; j++)
                {
                    if (_hero[j]._equippedWeapon.isHealer())
                    {
                        healer = true;
                        target = j;
                    }
                }
                //kalo ada healer, serang healer
                if (healer)
                    EnemyGelud(i, target);
                else//kalo ga ada, equal chance
                    EnemyGelud(i, rng);
            }
            else if(_enemy[i]._name == "Wraith")
            {
                //selalu serang player
                EnemyGelud(i, 0);
            }
            else //golem and yeti
            {
                //equal chance
                EnemyGelud(i, rng);
            }

            while (enemyAttackInProgress)
            {
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1f);
        }
        if(_gameState != GameState.battleEnd)
        {
            _gameState = GameState.heroTurn;
            _selectedMenu = menuState.actionSelect;
            _sfx.select();
            _selectedAction = 0;
            setActionMenu();
            _selectedSkill = -1;
            regenerateAP(true, -1);

            if (_prerseveranceTurnsRemaining != 0) _prerseveranceTurnsRemaining--;
            _defenseUpText.text = _prerseveranceTurnsRemaining.ToString();

            if (_prerseveranceTurnsRemaining == 0)
            {
                _defenseUpText.DOColor(Color.clear, 0.5f);
                _defenseUp.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.5f);
            }

            if(_birdOfPreyEnemyTarget != null) {
                _sleepyGameObjectRef.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.5f).OnComplete(() => {
                    Destroy(_sleepyGameObjectRef);
                });
            }

            _birdOfPreyEnemyTarget = null;
        }

        for(int i = 0; i < _enemyCount; i++)
        {
            int APGainRNG = Random.Range(1, 6);
            int APGain;

            if (APGainRNG == 1 || APGainRNG == 2) APGain = 1;
            else if (APGainRNG == 3 || APGainRNG == 4) APGain = 2;
            else APGain = 3;

            _enemy[i].setAP(_enemy[i].getAP() + APGain);
        }
        enemyAttackStarted = false;
    }

    //Use Skill
    void useSkill(int skillIndex, int userIndex, int targetIndex)
    {
        setUsedSkillBarCaption(skillIndex);

        //Prayer of Healing
        if (skillIndex == 0)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);

            //healing effect
            int addedHP = Random.Range(25, 61) * _hero[targetIndex].getNetMaxHP() / 100;
            int maxHP = _hero[targetIndex].getNetMaxHP();
            int newHP = addedHP + _hero[targetIndex].getHP();
            if (newHP > maxHP) newHP = maxHP;
            _hero[targetIndex].setHP(newHP);
            StartCoroutine(healAnim(addedHP, userIndex, targetIndex));
        }
        //Preserverance
        else if(skillIndex == 1)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);

            //preserverance effect
            _sfx.preserverance();
            _prerseveranceTurnsRemaining = 3;
            _defenseUp.SetActive(true);
            _defenseUpText.text = _prerseveranceTurnsRemaining.ToString();

            _defenseUpText.color = _defenseUp.GetComponent<SpriteRenderer>().color = Color.clear;
            _defenseUpText.DOColor(Color.white, 0.5f);
            _defenseUp.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.5f);

            StartCoroutine(delayBeforeNextTurn(1f, userIndex));
        }

        //bird of prey
        else if(skillIndex == 3)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);

            //bird of prey effect
            _sfx.birdOfPrey();
            _birdOfPreyEnemyTarget = _enemy[targetIndex];

            _sleepyGameObjectRef = Instantiate(_sleepyGameObject);
            _sleepyGameObjectRef.transform.position = _enemy[targetIndex].gameObject.transform.position + new Vector3(0.75f, 0.875f);
            _sleepyGameObjectRef.GetComponent<SpriteRenderer>().color = Color.clear;
            _sleepyGameObjectRef.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.5f);

            StartCoroutine(delayBeforeNextTurn(1f, userIndex));
        }

        //Sunlance Strike
        else if (skillIndex == 2)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);
        }
        //Chrono Slash
        else if (skillIndex == 4)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);
        }
        //Midnight Elegance
        else if(skillIndex == 5)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);
        }

        //Lunar Blessing
        else if (skillIndex == 6)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);

            //healing effect
            int[] addedHP = new int[4];

            for(int i = 0; i < _hero.Count; i++)
            {
                addedHP[i] = Random.Range(25, 41) * _hero[i].getNetMaxHP() / 100;
                int maxHP = _hero[i].getNetMaxHP();
                int newHP = addedHP[i] + _hero[i].getHP();
                if (newHP > maxHP) newHP = maxHP;
                _hero[i].setHP(newHP);
            }

            StartCoroutine(healAnimAll(addedHP, userIndex));
        }

        //Firestorm / daisycutter
        else if (skillIndex == 7 || skillIndex == 8)
        {
            //pay AP cost
            _hero[userIndex].setAP(_hero[userIndex].getAP() - _skillList._skillList[skillIndex]._APNeeded);
            PlayerGelud(userIndex, -1);
        }
    }

    void setUsedSkillBarCaption(int index)
    {
        if(index == -1)
        {
            _skillUsedBar.SetActive(false);
        }
        else
        {
            _skillUsedBar.SetActive(true);
            _skillUsedName.text = _skillList._skillList[index]._skillName;
        }
    }

    void enemyGeludCaption(string caption)
    {
        _skillUsedBar.SetActive(true);
        _skillUsedName.text = caption;
    }

    #endregion

    IEnumerator delayBeforeNextTurn(float delay, int userIndex)
    {
        yield return new WaitForSeconds(1f + delay);
        endPlayerTurn(userIndex);
    }

    IEnumerator healAnim(int addedHP, int userIndex, int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);
        InstantiateDamage(addedHP, _hero[targetIndex].gameObject, true);
        yield return new WaitForSeconds(1f);
        endPlayerTurn(userIndex);
    }

    IEnumerator healAnimAll(int[] addedHP, int userIndex)
    {
        yield return new WaitForSeconds(0.5f);

        for(int i = 0; i < _hero.Count; i++)
            InstantiateDamage(addedHP[i], _hero[i].gameObject, true);

        _sfx.lunarBlessing();

        yield return new WaitForSeconds(1f);
        endPlayerTurn(userIndex);
    }

    //Menus Navigations
    #region Action Menu
    void changeActionMenu(int dir)
    {
        _sfx.cursor();
        arrowStartTime = Time.time;
        if (dir == 1) _selectedAction++;
        else if (dir == -1) _selectedAction--;
        if (_selectedAction < 0) _selectedAction = 2;
        if (_selectedAction > 2) _selectedAction = 0;
        setActionMenu();
    }

    void setActionMenu()
    {
        clearActionMenu();
        action[_selectedAction].color = Color.yellow;
    }

    void clearActionMenu()
    {
        foreach (Text t in action)
        {
            t.color = Color.white;
        }
    }
    #endregion

    #region Hero Menu
    //hero menu for healing
    void changeHeroMenu(int dir)
    {
        _sfx.cursor();
        arrowStartTime = Time.time;
        if (dir == 1) _selectedHero++;
        else if (dir == -1) _selectedHero--;
        if (_selectedHero < 0) _selectedHero = _hero.Count - 1;
        if (_selectedHero >= _hero.Count) _selectedHero = 0;
        setHeroMenu();
    }

    void setHeroMenu()
    {
        clearHeroMenu(false);
        _heroNames[_selectedHero].color = _hero[_selectedHero].getHP() == _hero[_selectedHero].getNetMaxHP() ? Color.red : Color.yellow;
        if(_hero[_selectedHero].getHP() < _hero[_selectedHero].getNetMaxHP())
          arrow.transform.position = _hero[_selectedHero].transform.position + new Vector3(-1, 0, 0);
    }

    void clearHeroMenu(bool colorClear)
    {
        for(int i=0; i< _hero.Count; i++)
        {
            if (colorClear) _heroNames[i].color = Color.white;
            else _heroNames[i].color = _hero[i].getHP() == _hero[i].getNetMaxHP() ? Color.gray : Color.white;
        }
    }
    #endregion

    #region Skill Menu
    void changeSkillMenu(int dir)
    {
        _sfx.cursor();
        arrowStartTime = Time.time;
        if (dir == 1) _selectedSkillMenu++;
        else if (dir == -1) _selectedSkillMenu--;

        int reps = _hero[_selectedHero]._equippedArmor._slots;
        int max = 0;
        for(int i = 0; i < reps; i++)
            if (_hero[_selectedHero]._equippedArmor._skills[i] != -1) max++;

        if (_selectedSkillMenu < 0) _selectedSkillMenu = max - 1;
        if (_selectedSkillMenu >= max) _selectedSkillMenu = 0;

        _selectedSkill = _hero[_selectedHero]._equippedArmor._skills[_selectedSkillMenu];
        setSkillMenu();
    }

    void setSkillMenu()
    {
        clearSkillMenu();

        if (_hero[0].getAP() < _skillList._skillList[_selectedSkill]._APNeeded)
            _skillNames[_selectedSkillMenu].color = Color.red;
        else
            _skillNames[_selectedSkillMenu].color = Color.yellow;

        _skillDesc.text = _skillList._skillList[_selectedSkill]._skillDesc.ToString();
        _APNeeded.text = _skillList._skillList[_selectedSkill]._APNeeded.ToString();
    }

    void clearSkillMenu()
    {
        int reps = _hero[_selectedHero]._equippedArmor._slots;
        for (int i = 0; i < reps; i++)
        {
            int index = _hero[_selectedHero]._equippedArmor._skills[i];
            if (index == -1) continue;

            if (_hero[0].getAP() < _skillList._skillList[index]._APNeeded)
                _skillNames[i].color = Color.gray;
            else
                _skillNames[i].color = Color.white;
        }
    }
    #endregion

    #region Enemy Menu
    void changeEnemyMenu(int dir)
    {
        _sfx.cursor();
        arrowStartTime = Time.time;
        if (dir == 1) _selectedEnemy++;
        else if (dir == -1) _selectedEnemy--;
        if (_selectedEnemy < 0) _selectedEnemy = _enemy.Count - 1;
        if (_selectedEnemy >= _enemy.Count) _selectedEnemy = 0;
        setEnemyMenu();
    }

    void setEnemyMenu()
    {
        clearEnemyMenu();
        _enemyNames[_selectedEnemy].color = Color.yellow;
        arrow.transform.position = _enemy[_selectedEnemy].transform.position + new Vector3(-1, 0, 0);
    }

    void clearEnemyMenu()
    {
        foreach (Text e in _enemyNames)
        {
            e.color = Color.white;
        }
    }
    #endregion

    //Player Attack
    #region Gelud
    //Player Gelud
    void PlayerGelud(int attackerIndex, int enemyIndex)
    {
        npcAttackInProgress = true;

        float _MAtkMultiplier = 1f, _RAtkMultiplier = 1f, _RDefMultiplier = 1f;

        if (_selectedSkill == -1) _MAtkMultiplier = 1f;
        else if (_selectedSkill == 4) _MAtkMultiplier = 2.5f;
        else if (_selectedSkill == 5) _MAtkMultiplier = 3f;
        else if (_selectedSkill == 8) _RAtkMultiplier = 2.5f;

        if (_selectedSkill == -1) _RAtkMultiplier = 1f;
        else if (_selectedSkill == 2) _RAtkMultiplier = 4f;
        else if (_selectedSkill == 7) _RAtkMultiplier = 2f;

        if (_selectedSkill == -1) _RDefMultiplier = 1f;
        else if (_selectedSkill == 7) _RDefMultiplier = 0.75f;

        int playerDamage, enemyDefense;
        float[] totalDamages = new float[4];
        float totalDamage = 0;

        if (_selectedSkill != 7 && _selectedSkill != 8)
        {
            playerDamage = (int)(
                (Random.Range(_hero[attackerIndex].getMAtk().min, _hero[attackerIndex].getMAtk().max + 1) * _MAtkMultiplier) +
                (Random.Range(_hero[attackerIndex].getRAtk().min, _hero[attackerIndex].getRAtk().max + 1) * _RAtkMultiplier) +
                _hero[attackerIndex]._equippedWeapon._statsGiven[1] +
                _hero[attackerIndex]._equippedWeapon._statsGiven[2]);

            enemyDefense = (int)
                (Random.Range(_enemy[enemyIndex].getMDef().min, _enemy[enemyIndex].getMDef().max + 1) +
                (Random.Range(_enemy[enemyIndex].getRDef().min, _enemy[enemyIndex].getRDef().max + 1) * _RDefMultiplier));

            totalDamage = _scalingMultiplier * (100 - enemyDefense) * playerDamage / 100;
            _enemy[enemyIndex].setHP(_enemy[enemyIndex].getHP() - (int)totalDamage);
        }
        else
        {
            for (int i = 0; i < _enemy.Count; i++)
            {
                playerDamage = (int)(
                    (Random.Range(_hero[attackerIndex].getMAtk().min, _hero[attackerIndex].getMAtk().max + 1) * _MAtkMultiplier) +
                    (Random.Range(_hero[attackerIndex].getRAtk().min, _hero[attackerIndex].getRAtk().max + 1) * _RAtkMultiplier) +
                    _hero[attackerIndex]._equippedWeapon._statsGiven[1] +
                    _hero[attackerIndex]._equippedWeapon._statsGiven[2]);

                enemyDefense = (int)
                    (Random.Range(_enemy[i].getMDef().min, _enemy[i].getMDef().max + 1) +
                    (Random.Range(_enemy[i].getRDef().min, _enemy[i].getRDef().max + 1) * _RDefMultiplier));

                totalDamages[i] = _scalingMultiplier * (100 - enemyDefense) * playerDamage / 100;
                _enemy[i].setHP(_enemy[i].getHP() - (int)totalDamages[i]);
            }
        }

        if (_selectedSkill != 7 && _selectedSkill != 8)
            StartCoroutine(PlayerGeludAnim(attackerIndex, (int)totalDamage, enemyIndex));
        else
            StartCoroutine(PlayerTawuranAnim(attackerIndex, totalDamages));
    }

    IEnumerator PlayerTawuranAnim(int attackerIndex, float[] totalDamages)
    {
        _hero[attackerIndex].dealDamageAnim();

        if (_selectedSkill == 7) _sfx.firestorm();
        else if (_selectedSkill == 8) _sfx.daisycutter();

        yield return new WaitForSecondsRealtime(2f);
        for(int i = 0; i < _enemy.Count; i++)
            _enemy[i].takeDamageAnim();

        yield return new WaitForSecondsRealtime(0.8f);
        for (int i = 0; i < _enemy.Count; i++)
            InstantiateDamage((int)totalDamages[i], _enemy[i].gameObject, false);

        yield return new WaitForSecondsRealtime(1f);
        updateGameCondition();

        if (_gameState != GameState.battleEnd) endPlayerTurn(attackerIndex);
        npcAttackInProgress = false;
    }

    IEnumerator PlayerGeludAnim(int attackerIndex, int totalDamage, int enemyIndex)
    {
        _hero[attackerIndex].dealDamageAnim();

        if (_selectedSkill == -1) _sfx.attack();
        else if (_selectedSkill == 2) _sfx.sunlanceStrike();
        else if (_selectedSkill == 4) _sfx.chronoSlash();  
        else if (_selectedSkill == 5) _sfx.midnightElegance();

        yield return new WaitForSecondsRealtime(1f);
        _enemy[enemyIndex].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);
        InstantiateDamage(totalDamage, _enemy[enemyIndex].gameObject, false);
        yield return new WaitForSecondsRealtime(1f);
        updateGameCondition();
        if (_gameState != GameState.battleEnd) endPlayerTurn(-1);
        npcAttackInProgress = false;
    }

    //Enemy Gelud
    void EnemyGelud(int enemyIndex, int heroIndex)
    {
        enemyAttackInProgress = true;
        int enemyDamage, playerDefense;
        float totalDamage = 0;
        float[] totalDamages = new float[4];

        bool singleAttack = true;

        //normal enemy attack
        if (!_enemyTawuran)
        {
            enemyDamage =
            Random.Range(_enemy[enemyIndex].getMAtk().min, _enemy[enemyIndex].getMAtk().max + 1) +
            Random.Range(_enemy[enemyIndex].getRAtk().min, _enemy[enemyIndex].getRAtk().max + 1);

            playerDefense =
                _hero[heroIndex].getMDef() +
                _hero[heroIndex].getRDef() +
                _hero[heroIndex]._equippedArmor._statsGiven[1] +
                _hero[heroIndex]._equippedArmor._statsGiven[2] +
                _hero[heroIndex]._equippedWeapon._statsGiven[3] +
                _hero[heroIndex]._equippedWeapon._statsGiven[4];

            totalDamage = _scalingMultiplier * (100 - playerDefense) * enemyDamage / 100;
            if (_prerseveranceTurnsRemaining > 0) totalDamage -= 30;
            if (totalDamage < 0) totalDamage = 0;

            _hero[heroIndex].setHP(_hero[heroIndex].getHP() - (int)totalDamage);
        }
        else
        {
            enemyGeludCaption("Special Attack!");
            if(_enemy[enemyIndex]._name == "Rogue" || _enemy[enemyIndex]._name == "Golem")
            {
                totalDamage = Random.Range(0.4f, 0.6f) * _hero[heroIndex].getNetMaxHP();
                if (_prerseveranceTurnsRemaining > 0) totalDamage -= 30;
                if (totalDamage < 0) totalDamage = 0;

                _hero[heroIndex].setHP(_hero[heroIndex].getHP() - (int)totalDamage);
            }
            else
            {
                singleAttack = false;

                for (int i = 0; i < _hero.Count; i++)
                {
                    enemyDamage =
                        Random.Range(_enemy[enemyIndex].getMAtk().min, _enemy[enemyIndex].getMAtk().max + 1) +
                        Random.Range(_enemy[enemyIndex].getRAtk().min, _enemy[enemyIndex].getRAtk().max + 1);

                    playerDefense =
                        _hero[i].getMDef() +
                        _hero[i].getRDef() +
                        _hero[i]._equippedArmor._statsGiven[1] +
                        _hero[i]._equippedArmor._statsGiven[2] +
                        _hero[i]._equippedWeapon._statsGiven[3] +
                        _hero[i]._equippedWeapon._statsGiven[4];

                    if (_prerseveranceTurnsRemaining > 0) totalDamages[i] -= 30;
                    if (totalDamages[i] < 0) totalDamages[i] = 0;

                    _hero[i].setHP(_hero[i].getHP() - (int)totalDamage);
                }
            }
        }

        if (singleAttack)
        {
            StartCoroutine(EnemyGeludAnim((int)totalDamage, enemyIndex, heroIndex));
        }
        else
        {
            StartCoroutine(EnemyTawuranAnim(totalDamages, enemyIndex));
        }
    }

    IEnumerator EnemyTawuranAnim(float[] totalDamages, int enemyIndex)
    {
        _enemy[enemyIndex].dealDamageAnim();
        _sfx.enemySpecialAttackTwo();

        yield return new WaitForSecondsRealtime(1f);

        for(int i=0;i<_hero.Count;i++)
         _hero[i].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);

        for (int i = 0; i < _hero.Count; i++)
            InstantiateDamage((int) totalDamages[i], _hero[i].gameObject, false);
        yield return new WaitForSecondsRealtime(1f);

        setUsedSkillBarCaption(-1);
        updateGameCondition();

        enemyAttackInProgress = false;
    }

    IEnumerator EnemyGeludAnim(int totalDamage, int enemyIndex, int heroIndex)
    {
        _enemy[enemyIndex].dealDamageAnim();

        if (_enemyTawuran) _sfx.enemySpecialAttackOne();
        else _sfx.attack();

        yield return new WaitForSecondsRealtime(1f);
        _hero[heroIndex].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);
        InstantiateDamage(totalDamage, _hero[heroIndex].gameObject, false);
        yield return new WaitForSecondsRealtime(1f);

        setUsedSkillBarCaption(-1);
        updateGameCondition();
        
        enemyAttackInProgress = false;
    }
    #endregion

    //Update Game Condition
    #region Update Game Condition
    void updateGameCondition()
    {
        //check kondisi enemy
        List<Enemy> _deadEnemies = new List<Enemy>();
        for (int i = 0; i < _enemy.Count; i++)
        {
            if (_enemy[i].getHP() < 0)
            {
                _enemy[i].defeated();
                _sfx.enemyDie();
                _deadEnemies.Add(_enemy[i]);
            }
        }

        foreach(Enemy _ded in _deadEnemies)
            _enemy.Remove(_ded);

        updateEnemyNames();

        //cek kondisi player
        List<Hero> _deadHeroes = new List<Hero>();
        bool dedsound = false;
        for (int i = 0; i < _hero.Count; i++)
        {
            if (_hero[i].getHP() < 0)
            {
                _hero[i].defeated();
                if (!dedsound)
                {
                    dedsound = true;
                    _sfx.enemyDie();
                }
                _deadHeroes.Add(_hero[i]);
            }
        }

        foreach (Hero _ded in _deadHeroes)
            _hero.Remove(_ded);

        if (_hero.Count == 0)
        {
            //game over
            _gameState = GameState.battleEnd;
            StartCoroutine(gameLoseAnim());
        }
        updateHeroNames();
        UpdateHPAP();

        //cek kalo enemy udh ga ada semua = battle won
        if (_enemy.Count == 0)
        {
            _gameState = GameState.battleEnd;
            StartCoroutine(gameWinAnim());
        }
    }

    //update HP and AP (and weapon)
    void UpdateHPAP()
    {
        for (int i = 0; i < _hero.Count; i++)
        {
            heroStats[i].name.text = _hero[i]._name;
            heroStats[i].HP.text = _hero[i].getHP().ToString() + "/" + _hero[i].getNetMaxHP().ToString();
            heroStats[i].AP.text = _hero[i].getAP().ToString() + "/20";
            heroStats[i]._weapon.sprite = _hero[i]._equippedWeapon._icon;
            heroStats[i]._weapon.color = Color.white;
        }
    }

    //update nama enemy baru
    void updateEnemyNames()
    {    
        foreach (Text t in _enemyNames)
            t.gameObject.SetActive(false);
        for (int i = 0; i < _enemy.Count; i++)
        {
            _enemyNames[i].gameObject.SetActive(true);
            _enemyNames[i].text = _enemy[i]._name;
        }
    }

    //update nama player baru
    void updateHeroNames()
    {
        for (int i = 0; i < heroStats.Length; i++)
        {
            heroStats[i].name.text = heroStats[i].HP.text = heroStats[i].AP.text = "";
            heroStats[i]._weapon.sprite = null;
            heroStats[i]._weapon.color = Color.clear;
        }
        UpdateHPAP();
    }

    #endregion

    #region game win/lose

    [HideInInspector] public GameObject _endGameHeader;
    IEnumerator gameLoseAnim()
    {
        yield return new WaitForSecondsRealtime(1f);
        gameLose();
    }
    
    void gameLose()
    {
        _music.gameOver();

        foreach (LevelUpStats stat in _levelUpText)
            stat.Name.text = stat.HP.text = "";

        _levelUpBar.SetActive(true);
        _levelUpBar.GetComponent<Image>().color = Color.clear;

        _endGameHeader = Instantiate(BigMessage);
        _endGameHeader.transform.GetChild(0).GetComponent<Text>().text = "Annihilated!";
        _endGameHeader.transform.position = new Vector2(-9.125f, -3.52f);
        _endGameHeader.transform.DOMoveX(0.625f, 0.5f).OnComplete(() => {
            _levelUpBar.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0.75f), 0.5f).OnComplete(() => {
            
                _levelUpText[0].Name.color = _levelUpText[0].HP.color = Color.clear;
          
                _levelUpText[0].Name.text = "Waves Survived:";
                _levelUpText[0].HP.text = _wave.ToString();
                
                _levelUpText[0].Name.DOColor(Color.white, 0.5f);
                _levelUpText[0].HP.DOColor(Color.white, 0.5f);
            });
        });

        //clear all saved data (nanti weapon gw tambahin)
        PlayerPrefs.DeleteAll();

        StartCoroutine(DelayBeforePromptInput());
    }

    IEnumerator gameWinAnim()
    {
        yield return new WaitForSecondsRealtime(1f);
        gameWin();
    }

    void gameWin()
    {
        _music.fanfare();
        _pressAnyKeyMessage.gameObject.SetActive(false);

        foreach (LevelUpStats stat in _levelUpText)
            stat.Name.text = stat.HP.text = "";

        _levelUpBar.SetActive(true);
        _levelUpBar.GetComponent<Image>().color = Color.clear;

        int[] HP = new int[4];
        for (int i = 0; i < _hero.Count; i++)
        {
            int levelUpBonus = Utils.playerScaling();
            HP[i] = _hero[i].getMaxHP() * levelUpBonus / 100;
            _hero[i].setMaxHP(_hero[i].getMaxHP() + HP[i]);
        }

        _endGameHeader = Instantiate(BigMessage);
        _endGameHeader.transform.position = new Vector2(-9.125f, -3.52f);
        _endGameHeader.transform.DOMoveX(0.625f, 0.5f).OnComplete(() => {
            _levelUpBar.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0.75f), 0.5f).OnComplete(() => {
                for (int i = 0; i < _hero.Count; i++)
                {
                    _levelUpText[i].Name.color = _levelUpText[i].HP.color = Color.clear;
                    _levelUpText[i].Name.text = _hero[i]._name.ToString();
                    _levelUpText[i].HP.text = "HP + " + HP[i] + "  (" + (_hero[i].getNetMaxHP() - HP[i]) + "->" + _hero[i].getNetMaxHP() + ")";
                    _levelUpText[i].Name.DOColor(Color.white, 0.5f);
                    _levelUpText[i].HP.DOColor(Color.white, 0.5f);
                }
            });
        });

        //first message
        saveGame();
        int chance = PlayerPrefs.GetInt("NewHeroSpawnChance", 0);
        if (_newHeroSpawned)
        {
            PlayerPrefs.SetInt("NewHeroSpawnChance", 0);
        }
        else
        {
            int partyCount = _hero.Count;
            if (partyCount == 1) PlayerPrefs.SetInt("NewHeroSpawnChance", chance + 10);
            else if (partyCount == 2) PlayerPrefs.SetInt("NewHeroSpawnChance", chance + 5);
            else if (partyCount == 3) PlayerPrefs.SetInt("NewHeroSpawnChance", chance + 2);
        }
        StartCoroutine(DelayBeforePromptInput());
    }

    public void secondMessage()
    {
        _pressAnyKeyMessage.gameObject.SetActive(false);

        for(int i = 0; i < _enemyCount; i++)
        {
            bool _dropItem = Random.Range(1, 101) <= 30 ? false : true;
            if (!_dropItem) continue;

            bool weapons = Random.Range(0, 2) == 0 ? true : false;
            int _indexRNG = Random.Range(0, 4 + 1);
            if (weapons)
                _weaponDrops.Add(generateWeapon(_indexRNG));
            else
                _armorDrops.Add(generateArmor(_indexRNG));
        }

        for (int i = 1; i < _levelUpText.Length; i++)
            _levelUpText[i].Name.text = _levelUpText[i].HP.text  = "";

        _endGameHeader.transform.GetChild(0).GetComponent<Text>().text = "Items";

        _levelUpText[0].Name.text = "Obtained:";
        _levelUpText[0].HP.text = "";

        List<string> _items = new List<string>();
        List<int> _itemAmount = new List<int>();

        foreach(Weapon w in _weaponDrops)
        {
            bool identical = false;
            for(int i = 0;i < _items.Count; i++)
            {
                if(w._name == _items[i])
                {
                    identical = true;
                    _itemAmount[i]++;
                    break;
                }
            }
            if (!identical)
            {
                _items.Add(w._name);
                _itemAmount.Add(1);
            }
        }
        foreach (Armor a in _armorDrops)
        {
            bool identical = false;
            for (int i = 0; i < _items.Count; i++)
            {
                if (a._name == _items[i])
                {
                    identical = true;
                    _itemAmount[i]++;
                    break;
                }
            }
            if (!identical)
            {
                _items.Add(a._name);
                _itemAmount.Add(1);
            }
        }

        if (_items.Count == 0)
        {
            _levelUpBar.SetActive(false);
            SceneManager.LoadScene(0);
        }

        for (int i = 0; i < _items.Count; i++)
        {
            _levelUpText[i+1].Name.text = _itemAmount[i] + " x";
            _levelUpText[i+1].HP.text = _items[i];
        }

        StartCoroutine(DelayBeforePromptInput());
    }

    public void saveGame()
    {
        int _aliveHeroIdx = -1;
        //game saving
        for (int i = 0; i < _heroPool.Length; i++)
        {
            bool alive = false;
            for (int j = 0; j < _hero.Count; j++)
            {
                if (_hero[j]._name == _heroPool[i]._name)
                {
                    alive = true;
                    _aliveHeroIdx++;
                    break;
                }
            }
            if (!alive) continue;

            //save all stats
            PlayerPrefs.SetInt("HeroIsAlive" + i, 1);

            PlayerPrefs.SetInt("HeroHP" + i, _hero[_aliveHeroIdx].getHP());
            PlayerPrefs.SetInt("HeroAP" + i, _hero[_aliveHeroIdx].getAP());
            PlayerPrefs.SetInt("HeroMaxHP" + i, _hero[_aliveHeroIdx].getMaxHP());

            Hero h = _hero[_aliveHeroIdx];
            int weaponIndex = 0, armorIndex = 0;
            for (int j = 0; j < _equippable._weapons.Length; j++)
            {
                if (_equippable._weapons[j]._name == h._equippedWeapon._name)
                {
                    weaponIndex = j;
                    break;
                }
            }
            for (int j = 0; j < _equippable._armors.Length; j++)
            {
                if (_equippable._armors[j]._name == h._equippedArmor._name)
                {
                    armorIndex = j;
                    break;
                }
            }
            PlayerPrefs.SetInt("HeroWeaponIdx" + i, weaponIndex);

            for (int j = 0; j < 5; j++) //copy tiap stats
            {
                PlayerPrefs.SetInt("HeroWeaponIdx" + i + "_wsIdx" + j, h._equippedWeapon._statsGiven[j]);
            }

            PlayerPrefs.SetInt("HeroArmorIdx" + i, armorIndex);

            for (int j = 0; j < 3; j++) //copy tiap stats
            {
                PlayerPrefs.SetInt("HeroArmorIdx" + i + "_asIdx" + j, h._equippedArmor._statsGiven[j]);
            }

            for (int j = 0; j < 3; j++) //copy tiap skills
            {
                PlayerPrefs.SetInt("HeroSkills" + i + "_asIdx" + j, h._equippedArmor._skills[j]);
            }
        }

        PlayerPrefs.SetInt("Wave", _wave + 1);

        //after that make sure to tell save data exist and not doing the loop in the pre battle manager
        PlayerPrefs.SetInt("GameStarted", 1);

        Debug.Log("Game Saved");
    }

    IEnumerator DelayBeforePromptInput()
    {
        yield return new WaitForSecondsRealtime(3f);
        PromptInput();
    }

    void PromptInput()
    {
        _pressAnyKeyMessage.gameObject.SetActive(true);
        _pressAnyKeyMessage.color = Color.clear;
        _pressAnyKeyMessage.DOColor(Color.white, 0.5f);
        _gameState = GameState.waitingForInput;
    }

    #endregion

    #region Enums
    public enum GameState
    {
        none,
        preBattle,
        heroTurn,
        enemyTurn,
        npcTurn,
        battleEnd,
        waitingForInput
    };

    public enum menuState { 
        none,
        actionSelect,
        skillSelect,
        enemySelect,
        heroSelect,
        attackInProgress,
        skillInProgress,
    };

    #endregion
}