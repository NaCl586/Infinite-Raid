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
    //public references
    [Header("Character Placements")]
    public GameObject[] Positions;
    //0-3 = player; 4-6 enemy

    [Header("Buttons")]
    public Text[] action;
    public Text[] enemy;

    [Header("Hero")]
    public HeroStats[] heroStats;
    public Hero[] _heroPool;
    private List<Hero> _hero;
    public Text[] _heroNames;

    [Header("Enemy")]
    public Text[] _enemyNames;
    public Enemy[] _enemyPool;
    private List<Enemy> _enemy;
    
    [Header("Popups")]
    public GameObject Damage;
    public GameObject BigMessage;

    [Header("Skill")]
    public SkillList _skillList;
    public GameObject _skillBar;
    public GameObject _upperBar;
    public Text _skillDesc;
    public Text _APNeeded;
    public Text[] _skillNames;
    public List<Skill> _skill;

    [Header("Level Up")]
    public GameObject _levelUpBar;
    public LevelUpStats[] _levelUpText;

    [Header("SFX")]
    public SFX _sfx;
    public music _music;

    //game related stuff
    private int _wave = 1;
    private GameState _gameState;
    private int _initPlayerCount;
    private int _initEnemyCount;
    private int _initSkillCount;

    //UI related stuff
    private menuState _selectedMenu;
    private int _selectedAction = 0;
    private int _selectedEnemy = 0;
    private int _selectedSkill = -1; //-1 artinya normal attack
    private int _selectedHero = 0; //for skill, krn pasti player cuma bs control siapapun di index 0

    // Start is called before the first frame update
    void Start()
    {
        _music.battle();

        //GameManager Init
        _gameState = GameState.none;
        _hero = new List<Hero>();
        _enemy = new List<Enemy>();

        //Player Init
        _initPlayerCount = 2;
        foreach(HeroStats h in heroStats)
            h.gameObject.SetActive(false);
        for (int i = 0; i < _initPlayerCount; i++)
        {
            heroStats[i].gameObject.SetActive(true);
            Hero h = Instantiate(_heroPool[i]);
            _hero.Add(h);
            Transform hpos = h.gameObject.transform;
            hpos.position = new Vector2(12, 0);
            hpos.transform.DOMove(Positions[i].transform.position, 0.5f);
        }

        //Enemy Init
        _initEnemyCount = 3;
        foreach (Text t in _enemyNames)
            t.gameObject.SetActive(false);
        for (int i = 0; i < _initEnemyCount; i++)
        {
            _enemyNames[i].gameObject.SetActive(true);
            Enemy e = Instantiate(_enemyPool[0]);
            e.setHP(Random.Range(e.getMaxHP().min, e.getMaxHP().max));
            _enemy.Add(e);
            _enemyNames[i].text = e._name;
           Transform epos = e.gameObject.transform;
            epos.position = new Vector2(-7, 0);
            epos.transform.DOMove(Positions[i+4].transform.position, 0.5f);
        }

        //init skill list
        _initSkillCount = 1;
        foreach(Text t in _skillNames)
            t.gameObject.SetActive(false);
        
        //prayer of healing
        _skill.Add(_skillList._skillList[0]);

        for (int i = 0; i < _initSkillCount; i++)
        {
            _skillNames[i].gameObject.SetActive(true);
            _skillNames[i].text = _skill[i]._skillName;
        }

        //UI Init
        _levelUpBar.SetActive(false);
        _levelUpBar.GetComponent<Image>().color = Color.clear;
        displaySkillMenuAnim(false, false);

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
        }
        else
        {
            _skillBar.transform.DOMoveX(_skillBar.transform.position.x - 11.5f, dur).OnComplete(() => {_skillBar.SetActive(false); });
            _upperBar.transform.DOMoveY(_upperBar.transform.position.y + 2.25f, dur).OnComplete(() => { _upperBar.SetActive(false); });
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
                        _selectedMenu = menuState.enemySelect;
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
                        regenerateAP(false);
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
                    _selectedSkill = 0;
                    clearHeroMenu(true);
                    setSkillMenu();
                }
                if (Input.GetKey(KeyCode.Return) && _hero[_selectedHero].getHP() != _hero[_selectedHero].getMaxHP() && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.healingInProgress;
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
                    clearEnemyMenu();
                    setActionMenu();
                }
                if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _sfx.cursor();
                    _selectedMenu = menuState.attackInProgress;
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
    }

    void endPlayerTurn()
    {
        regenerateAP(true);
        if (_hero.Count > 1) _gameState = GameState.npcTurn; //AI turn
        else _gameState = GameState.enemyTurn;
    }

    void regenerateAP(bool partial)
    {
        if (partial)
        {
            for (int i = 0; i < _hero.Count; i++)
            {
                if (_selectedSkill != -1 && i == _selectedHero) continue;
                int newAP = _hero[i].getAP() + Random.Range(1, 6);
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

    //NPC satu2 gelud
    IEnumerator NPCGelud()
    {
        npcAttackStarted = true;
        yield return new WaitForSecondsRealtime(1.5f);
        for(int i = 1; i < _hero.Count; i++)
        {
            bool healFlag = false;
            //AP > 13 = bisa heal
            if (_hero[i].getAP() >= 13)
            {
                //cari yg bisa diheal
                for(int j = 0; j < _hero.Count; j++)
                {
                    if((_hero[j].getHP() * 100 / _hero[j].getMaxHP()) <= 30){
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
            PlayerGelud(i, Random.Range(1, _enemy.Count)-1);
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
            regenerateAP(true);
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
            regenerateAP(true);
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
            int addedHP = Random.Range(25, 60) * _hero[targetIndex].getMaxHP() / 100;
            int maxHP = _hero[targetIndex].getMaxHP();
            int newHP = addedHP + _hero[targetIndex].getHP();
            if (newHP > maxHP) newHP = maxHP;
            _hero[targetIndex].setHP(newHP);
            StartCoroutine(healAnim(addedHP, targetIndex));
        }
    }

    IEnumerator healAnim(int addedHP, int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);
        InstantiateDamage(addedHP, _hero[targetIndex].gameObject, true);
        yield return new WaitForSeconds(1f);
        updateGameCondition();
        endPlayerTurn();
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
        _heroNames[_selectedHero].color = _hero[_selectedHero].getHP() == _hero[_selectedHero].getMaxHP() ? Color.red : Color.yellow;
    }

    void clearHeroMenu(bool colorClear)
    {
        for(int i=0; i< _hero.Count; i++)
        {
            if (colorClear) _heroNames[i].color = Color.white;
            else _heroNames[i].color = _hero[i].getHP() == _hero[i].getMaxHP() ? Color.gray : Color.white;
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
        if (_hero[0].getAP() < _skill[_selectedSkill]._APNeeded)
            _skillNames[_selectedSkill].color = Color.red;
        else
            _skillNames[_selectedSkill].color = Color.yellow;
        _skillDesc.text = _skill[_selectedSkill]._skillDesc.ToString();
        _APNeeded.text = _skill[_selectedSkill]._APNeeded.ToString();
    }

    void clearSkillMenu()
    {
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
    }

    void clearEnemyMenu()
    {
        foreach (Text e in _enemyNames)
        {
            e.color = Color.white;
        }
    }
    #endregion

    //Battle
    #region Gelud
    //Player Gelud
    void PlayerGelud(int attackerIndex, int enemyIndex)
    {
        npcAttackInProgress = true;
        int playerDamage = Random.Range(_hero[attackerIndex].getMAtk().min, _hero[attackerIndex].getMAtk().max) + Random.Range(_hero[attackerIndex].getRAtk().min, _hero[attackerIndex].getRAtk().max);
        int enemyDefense = Random.Range(_enemy[enemyIndex].getMDef().min, _enemy[enemyIndex].getMDef().max) + Random.Range(_enemy[enemyIndex].getRDef().min, _enemy[enemyIndex].getRDef().max);
        int totalDamage = (100 - enemyDefense) * playerDamage / 100;
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
        npcAttackInProgress = false;
        if (_gameState != GameState.battleEnd) endPlayerTurn();
    }

    //Enemy Gelud
    void EnemyGelud(int enemyIndex, int heroIndex)
    {
        enemyAttackInProgress = true;
        int enemyDamage = Random.Range(_enemy[enemyIndex].getMAtk().min, _enemy[enemyIndex].getMAtk().max) + Random.Range(_enemy[enemyIndex].getRAtk().min, _enemy[enemyIndex].getRAtk().max);
        int playerDefense = _hero[heroIndex].getMDef() + _hero[heroIndex].getRDef();
        int totalDamage = (100 - playerDefense) * enemyDamage / 100;
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

    //update HP and AP
    void UpdateHPAP()
    {
        for (int i = 0; i < _hero.Count; i++)
        {
            heroStats[i].name.text = _hero[i]._name;
            heroStats[i].HP.text = _hero[i].getHP().ToString() + "/" + _hero[i].getMaxHP().ToString();
            heroStats[i].AP.text = _hero[i].getAP().ToString() + "/20";
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
            _levelUpBar.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0.3843137f), 0.5f).OnComplete(() => {
            
                _levelUpText[0].Name.color = _levelUpText[0].HP.color = Color.clear;
          
                _levelUpText[0].Name.text = "Waves Survived:";
                _levelUpText[0].HP.text = _wave.ToString();
                
                _levelUpText[0].Name.DOColor(Color.white, 0.5f);
                _levelUpText[0].HP.DOColor(Color.white, 0.5f);
            });
        });
        UpdateHPAP();
    }

    IEnumerator gameWinAnim()
    {
        yield return new WaitForSecondsRealtime(1f);
        gameWin();
    }

    void gameWin()
    {
        _music.fanfare();

        foreach (LevelUpStats stat in _levelUpText)
            stat.Name.text = stat.HP.text = "";

        _levelUpBar.SetActive(true);
        _levelUpBar.GetComponent<Image>().color = Color.clear;

        GameObject message = Instantiate(BigMessage);
        message.transform.position = new Vector2(-9.125f, -3.52f);
        message.transform.DOMoveX(0.625f, 0.5f).OnComplete(() => {
            _levelUpBar.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0.3843137f), 0.5f).OnComplete(()=> { 
                for(int i = 0; i < _hero.Count; i++)
                {
                    int levelUpBonus = Utils.playerScaling();
                    int HP = _hero[i].getMaxHP() * levelUpBonus / 100;
                    _hero[i].setMaxHP(_hero[i].getMaxHP() + HP);

                    _levelUpText[i].Name.color = _levelUpText[i].HP.color = Color.clear;

                    _levelUpText[i].Name.text = _hero[i]._name.ToString();
                    _levelUpText[i].HP.text = "HP + " + HP + "  (" + (_hero[i].getMaxHP() - HP) + "->" + _hero[i].getMaxHP() + ")";

                    _levelUpText[i].Name.DOColor(Color.white, 0.5f);
                    _levelUpText[i].HP.DOColor(Color.white, 0.5f);
                }
            });
        });
        updateHeroNames();
    }

    #endregion

    #region Enums
    public enum GameState
    {
        none,
        heroTurn,
        enemyTurn,
        npcTurn,
        battleEnd
    };

    public enum menuState { 
        none,
        actionSelect,
        skillSelect,
        enemySelect,
        heroSelect,
        attackInProgress,
        healingInProgress
    };

    #endregion
}