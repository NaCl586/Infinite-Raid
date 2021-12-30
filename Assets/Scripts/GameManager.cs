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
    [SerializeField] [Range(1, 4)] private int _initPlayerCount;
    [SerializeField] [Range(1, 3)] private int _initEnemyCount;
    private int _initSkillCount;
    [SerializeField] [Range(1, 4)] private int _initEnemyVariation; //variasi enemy yg ada, makin tinggi makin susah (1-4)
    private int _scalingMultiplier = 1;

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
    [HideInInspector] public List<Hero> _hero;
    public Text[] _heroNames;

    [Header("Enemy")]
    public Text[] _enemyNames;
    public Enemy[] _enemyPool;
    private List<Enemy> _enemy;
    
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
    public List<Skill> _skill;

    [Header("Equippable List")]
    public Equippable _equippable;

    [Header("Level Up")]
    public GameObject _levelUpBar;
    public LevelUpStats[] _levelUpText;
    public Text _pressAnyKeyMessage;
    public Text _waveText;

    [Header("SFX")]
    public SFX _sfx;
    public music _music;

    //game related stuff
    private int _wave;
    private GameState _gameState;

    //UI related stuff
    private menuState _selectedMenu;
    private int _selectedAction = 0;
    private int _selectedEnemy = 0;
    private int _selectedSkill = -1; //-1 artinya normal attack
    private int _selectedHero = 0; //for skill, krn pasti player cuma bs control siapapun di index 0

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        //GameManager Init
        _gameState = GameState.none;
        _hero = new List<Hero>();
        _enemy = new List<Enemy>();

        //Wave init
        _wave = PlayerPrefs.GetInt("Wave", 1);
        _waveText.text = "Wave " + _wave;

        _scalingMultiplier = Utils.scalingMultiplier(_wave);

        game.SetActive(false);
        preBattle.SetActive(true);
        _music.preBattle();
        _gameState = GameState.preBattle;
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
        int _weaponIndexRNG = Random.Range(0, _equippable._weapons.Length);
        int _armorIndexRNG = Random.Range(0, _equippable._armors.Length);

        //weapon
        h._equippedWeapon = _equippable._weapons[_weaponIndexRNG];
        Weapon w = h._equippedWeapon;

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

        //armor
        h._equippedArmor = _equippable._armors[_armorIndexRNG];
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

    public void initGame()
    { 
        _music.battle();

        game.SetActive(true);
        preBattle.SetActive(false);
        
        //this is to instantiate additional hero
        instantiateNewHero();

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

        for (int i = 0; i < _initEnemyCount; i++)
        {
            _enemyNames[i].gameObject.SetActive(true);
            int rng = Random.Range(1, 100) % _initEnemyVariation;
            Enemy e = Instantiate(_enemyPool[rng]);
            int initHP = Random.Range(e.getMaxHP().min, e.getMaxHP().max + 1);

            initHP = initHP * _scalingMultiplier;
            e.setHP(initHP);
            _enemy.Add(e);
            _enemyNames[i].text = e._name;
            Transform epos = e.gameObject.transform;
            epos.position = new Vector2(-7, 0);
            epos.transform.DOMove(Positions[i + 4].transform.position, 0.5f);
        }

        //init skill list
        //_initSkillCount = 1;

        //UI Init
        _levelUpBar.SetActive(false);
        _levelUpBar.GetComponent<Image>().color = Color.clear;
        displaySkillMenuAnim(false, false);
        arrow.SetActive(false);

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
        if (heal) _sfx.heal();
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

            foreach(int n in _hero[0]._equippedArmor._skills)
            {
                if(n != -1) _skill.Add(_skillList._skillList[n]);
            }

            foreach (Text t in _skillNames)
                t.gameObject.SetActive(false);

            for (int i = 0; i < _skill.Count; i++)
            {
                _skillNames[i].gameObject.SetActive(true);
                _skillNames[i].text = _skill[i]._skillName;
            }

        }
        else
        {
            _skillBar.transform.DOMoveX(_skillBar.transform.position.x - 11.5f, dur).OnComplete(() => {_skillBar.SetActive(false); });
            _upperBar.transform.DOMoveY(_upperBar.transform.position.y + 2.25f, dur).OnComplete(() => { _upperBar.SetActive(false); });

            //clear all, then add idx 0 again
            _skill.Clear();
        }
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
                        _selectedEnemy = 0;
                        _selectedMenu = menuState.enemySelect;
                        arrow.SetActive(true);
                        _selectedSkill = -1;
                        clearActionMenu();
                        setEnemyMenu();
                    }
                    if (_selectedAction == 1)
                    {
                        arrowStartTime = Time.time;
                        _sfx.cursor();
                        _selectedMenu = menuState.skillSelect;
                        _selectedSkill = 0;
                        clearActionMenu();
                        displaySkillMenuAnim(true, true);
                        setSkillMenu();
                    }
                    if( _selectedAction == 2)
                    {
                        arrowStartTime = Time.time;
                        _sfx.cursor();
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
                    _selectedSkill = -1;
                    setActionMenu();
                }
                if (Input.GetKey(KeyCode.Return) && _hero[0].getAP() >= _skill[_selectedSkill]._APNeeded && (Time.time - arrowStartTime) > 0.175f)
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
                    _selectedSkill = 0;
                    clearHeroMenu(true);
                    setSkillMenu();
                }
                if (Input.GetKey(KeyCode.Return) && _hero[_selectedHero].getHP() != _hero[_selectedHero].getMaxHP() && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.healingInProgress;
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
                    if(_selectedSkill == -1) PlayerGelud(0,_selectedEnemy);
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
            if (Input.anyKey)
                SceneManager.LoadScene(0);
        }
    }

    void endPlayerTurn(int user)
    {
        regenerateAP(true, user);
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
    IEnumerator NPCGelud()
    {
        npcAttackStarted = true;
        yield return new WaitForSecondsRealtime(1.5f);
        for(int i = 1; i < _hero.Count; i++)
        {
            //kalo udah battle end, jgn diterusin
            if (_gameState == GameState.battleEnd) break;

            bool healFlag = false;
            //AP > 13 = bisa heal
            if (_hero[i].getAP() >= 13)
            {
                //cari yg bisa diheal
                for(int j = 0; j < _hero.Count; j++)
                {
                    if((_hero[j].getHP() * 100 / _hero[j].getNetMaxHP()) <= 30){
                        _selectedSkill = 0;
                        useSkill(0,i,j);
                        healFlag = true;
                        break;
                    }
                }
            }
            if (healFlag)
            {
                yield return new WaitForSecondsRealtime(3.8f);
                continue;
            }
            int rng = Random.Range(0, _enemy.Count);
            PlayerGelud(i, Random.Range(0, _enemy.Count));
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
            //kalo udah battle end, jgn diterusin
            if (_gameState == GameState.battleEnd) break;

            //bakal beda2 tergantung enemy apa
            EnemyGelud(i, Random.Range(1, 100) % _hero.Count);
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
        }
        enemyAttackStarted = false;
    }

    //Use Skill
    void useSkill(int skillIndex, int userIndex, int targetIndex)
    {
        //Prayer of Healing
        if(skillIndex == 0)
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
    }

    #endregion

    IEnumerator healAnim(int addedHP, int userIndex, int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);
        InstantiateDamage(addedHP, _hero[targetIndex].gameObject, true);
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
        if(_hero[_selectedHero].getHP() != _hero[_selectedHero].getNetMaxHP())
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
        if (dir == 1) _selectedSkill++;
        else if (dir == -1) _selectedSkill--;
        if (_selectedSkill < 0) _selectedSkill = _skill.Count - 1;
        if (_selectedSkill >= _skill.Count) _selectedSkill = 0;
        setSkillMenu();
    }

    void setSkillMenu()
    {
        clearSkillMenu();
        if (_skill.Count == 0) return;

        if (_hero[0].getAP() < _skill[_selectedSkill]._APNeeded)
            _skillNames[_selectedSkill].color = Color.red;
        else
            _skillNames[_selectedSkill].color = Color.yellow;
        _skillDesc.text = _skill[_selectedSkill]._skillDesc.ToString();
        _APNeeded.text = _skill[_selectedSkill]._APNeeded.ToString();
    }

    void clearSkillMenu()
    {
        if (_skill.Count == 0) return;

        foreach (Text t in _skillNames)
        {
            if (_hero[0].getAP() < _skill[_selectedSkill]._APNeeded)
                t.color = Color.gray;
            else
                t.color = Color.white;
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

        int playerDamage =
            Random.Range(_hero[attackerIndex].getMAtk().min, _hero[attackerIndex].getMAtk().max + 1) +
            Random.Range(_hero[attackerIndex].getRAtk().min, _hero[attackerIndex].getRAtk().max + 1) +
            _hero[attackerIndex]._equippedWeapon._statsGiven[1] +
            _hero[attackerIndex]._equippedWeapon._statsGiven[2];

        int enemyDefense =
            Random.Range(_enemy[enemyIndex].getMDef().min, _enemy[enemyIndex].getMDef().max + 1) +
            Random.Range(_enemy[enemyIndex].getRDef().min, _enemy[enemyIndex].getRDef().max + 1);

        int totalDamage = _scalingMultiplier * (100 - enemyDefense) * playerDamage / 100;
        _enemy[enemyIndex].setHP(_enemy[enemyIndex].getHP() - totalDamage);
        StartCoroutine(PlayerGeludAnim(attackerIndex, totalDamage, enemyIndex)); 
    }

    IEnumerator PlayerGeludAnim(int attackerIndex, int totalDamage, int enemyIndex)
    {
        _hero[attackerIndex].dealDamageAnim();
        _sfx.attack();
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

        int enemyDamage = 
            Random.Range(_enemy[enemyIndex].getMAtk().min, _enemy[enemyIndex].getMAtk().max + 1) + 
            Random.Range(_enemy[enemyIndex].getRAtk().min, _enemy[enemyIndex].getRAtk().max + 1);

        int playerDefense = 
            _hero[heroIndex].getMDef() + 
            _hero[heroIndex].getRDef() +
            _hero[heroIndex]._equippedArmor._statsGiven[1] +
            _hero[heroIndex]._equippedArmor._statsGiven[2] +
            _hero[heroIndex]._equippedWeapon._statsGiven[3] +
            _hero[heroIndex]._equippedWeapon._statsGiven[4];

        int totalDamage = _scalingMultiplier * (100 - playerDefense) * enemyDamage / 100;
        _hero[heroIndex].setHP(_hero[heroIndex].getHP() - totalDamage);
        StartCoroutine(EnemyGeludAnim(totalDamage, enemyIndex, heroIndex));
    }
    IEnumerator EnemyGeludAnim(int totalDamage, int enemyIndex, int heroIndex)
    {
        _enemy[enemyIndex].dealDamageAnim();
        _sfx.attack();
        yield return new WaitForSecondsRealtime(1f);
        _hero[heroIndex].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);
        InstantiateDamage(totalDamage, _hero[heroIndex].gameObject, false);
        yield return new WaitForSecondsRealtime(1f);
        updateGameCondition();
        enemyAttackInProgress = false;
    }
    #endregion

    //Update Game Condition
    #region Update Game Condition
    void updateGameCondition()
    {
        //check kondisi enemy
        for (int i = 0; i < _enemy.Count; i++)
        {
            if (_enemy[i].getHP() < 0)
            {
                _enemy[i].defeated();
                _sfx.enemyDie();
                _enemy.Remove(_enemy[i]);
            }
        }
        updateEnemyNames();

        //cek kondisi player
        for (int i = 0; i < _hero.Count; i++)
        {
            if (_hero[i].getHP() < 0)
            {
                _hero[i].defeated();
                _sfx.enemyDie();
                _hero.Remove(_hero[i]);
            }
        }
        if(_hero.Count == 0)
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

        GameObject message = Instantiate(BigMessage);
        message.transform.GetChild(0).GetComponent<Text>().text = "Annihilated!";
        message.transform.position = new Vector2(-9.125f, -3.52f);
        message.transform.DOMoveX(0.625f, 0.5f).OnComplete(() => {
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

        GameObject message = Instantiate(BigMessage);
        message.transform.position = new Vector2(-9.125f, -3.52f);
        message.transform.DOMoveX(0.625f, 0.5f).OnComplete(() => {
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

        int _aliveHeroIdx = -1;
        //game saving
        for (int i = 0; i < _heroPool.Length; i++)
        {
            bool alive = false;
            for(int j = 0; j < _hero.Count; j++)
            {
                if(_hero[j]._name == _heroPool[i]._name)
                {
                    alive = true;
                    _aliveHeroIdx++;
                    break;
                }
            }
            if (!alive) continue;
            
            //save all stats
            PlayerPrefs.SetInt("HeroIsAlive" + i, 1);

            PlayerPrefs.SetInt("HeroHP" + i, _hero[i].getHP());
            PlayerPrefs.SetInt("HeroAP" + i, _hero[i].getAP());
            PlayerPrefs.SetInt("HeroMaxHP" + i, _hero[i].getMaxHP());
            
            Hero h = _hero[_aliveHeroIdx];
            int weaponIndex = 0, armorIndex = 0;
            for(int j = 0; j < _equippable._weapons.Length; j++)
            {
                if(_equippable._weapons[j] == h._equippedWeapon)
                {
                    weaponIndex = j;
                    break;
                }
            }
            for (int j = 0; j < _equippable._armors.Length; j++)
            {
                if (_equippable._armors[j] == h._equippedArmor)
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
        StartCoroutine(DelayBeforePromptInput());
    }

    IEnumerator DelayBeforePromptInput()
    {
        yield return new WaitForSecondsRealtime(5f);
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
        healingInProgress,
    };

    #endregion
}