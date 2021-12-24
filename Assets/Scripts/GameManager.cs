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

    [Header("Enemy")]
    public Text[] _enemyNames;
    public Enemy[] _enemyPool;
    private List<Enemy> _enemy;
    
    [Header("Popups")]
    public GameObject Damage;
    public GameObject BigMessage;

    [Header("Level Up")]
    public GameObject _levelUpBar;
    public LevelUpStats[] _levelUpText;

    //game related stuff
    private int _wave = 1;
    private GameState _gameState;
    private int _initPlayerCount;
    private int _initEnemyCount;

    //UI related stuff
    private menuState _selectedMenu;
    private int _selectedAction = 0;
    private int _selectedEnemy = 0;

    // Start is called before the first frame update
    void Start()
    {
        //GameManager Init
        _gameState = GameState.heroTurn;
        _hero = new List<Hero>();
        _enemy = new List<Enemy>();

        //Player Init
        _initPlayerCount = 1;
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
        _initEnemyCount = 1;
        foreach (Text t in _enemyNames)
            t.gameObject.SetActive(false);
        for (int i = 0; i < _initEnemyCount; i++)
        {
            _enemyNames[i].gameObject.SetActive(true);
            Enemy e = Instantiate(_enemyPool[0]);
            e.setHP(Random.Range(e.getMaxHP().min, e.getMaxHP().max));
            Debug.Log(e.getHP());
            _enemy.Add(e);
            _enemyNames[i].text = e._name;
           Transform epos = e.gameObject.transform;
            epos.position = new Vector2(-7, 0);
            epos.transform.DOMove(Positions[i+4].transform.position, 0.5f);
        }

        //UI Init
        _levelUpBar.SetActive(false);
        _levelUpBar.GetComponent<Image>().color = Color.clear;

        updateGameCondition();
        _selectedMenu = menuState.actionSelect;
        setActionMenu();
    }


    void InstantiateDamage(int amount, GameObject character)
    {
        GameObject dmgText = Instantiate(Damage);
        dmgText.transform.position = character.transform.position;
        dmgText.transform.GetChild(0).GetComponent<Text>().text = amount.ToString();
        dmgText.transform.DOMoveY(dmgText.transform.position.y + 0.5f, 0.3f).OnComplete(() => {
            dmgText.transform.DOMoveY(dmgText.transform.position.y - 0.5f, 0.3f).OnComplete(() => {
                Destroy(dmgText,1f);
            });
        });
    }


    private float arrowStartTime;
    private bool enemyAttackInProgress;
    private bool enemyAttackStarted;
    void Update()
    {
        if(_gameState == GameState.heroTurn)
        {
            if(_selectedMenu == menuState.actionSelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeActionMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeActionMenu(1);
                if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
                {
                    if (_selectedAction == 0)
                    {
                        arrowStartTime = Time.time;
                        _selectedMenu = menuState.enemySelect;
                        clearActionMenu();
                        setEnemyMenu();
                    }
                    if( _selectedAction == 2)
                    {
                        arrowStartTime = Time.time;
                        clearActionMenu();
                        if (_hero.Count > 1) ; //AI turn
                        else _gameState = GameState.enemyTurn;
                    }
                }
            }
            if(_selectedMenu == menuState.enemySelect)
            {
                if (Input.GetKey(KeyCode.UpArrow) && (Time.time - arrowStartTime) > 0.175f) changeEnemyMenu(-1);
                else if (Input.GetKey(KeyCode.DownArrow) && (Time.time - arrowStartTime) > 0.175f) changeEnemyMenu(1);
                if (Input.GetKey(KeyCode.Escape) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _selectedMenu = menuState.actionSelect;
                    clearEnemyMenu();
                    setActionMenu();
                }
                if (Input.GetKey(KeyCode.Return) && (Time.time - arrowStartTime) > 0.175f)
                {
                    arrowStartTime = Time.time;
                    _selectedMenu = menuState.attackInProgress;
                    clearEnemyMenu();
                    PlayerGelud(_selectedEnemy);
                }
            }
        }
        if (_gameState == GameState.enemyTurn && !enemyAttackStarted)
        {
            StartCoroutine(SemuaEnemyGelud());
        }
        if (_gameState == GameState.battleEnd)
        {
            clearActionMenu();
            clearEnemyMenu();
        }
    }

    IEnumerator SemuaEnemyGelud()
    {
        enemyAttackStarted = true;
        yield return new WaitForSecondsRealtime(1.5f);
        for (int i = 0; i < _enemy.Count; i++)
        {
            //bakal beda2 tergantung enemy apa
            EnemyGelud(i, Random.Range(0, _hero.Count - 1));
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
            setActionMenu();
        }
        enemyAttackStarted = false;
    }

    #region Action Menu
    void changeActionMenu(int dir)
    {
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

    #region Enemy Menu
    void changeEnemyMenu(int dir)
    {
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

    #region Gelud
    //Player Gelud
    void PlayerGelud(int enemyIndex)
    {
        int playerDamage = Random.Range(_hero[0].getMAtk().min, _hero[0].getMAtk().max) + Random.Range(_hero[0].getRAtk().min, _hero[0].getRAtk().max);
        int enemyDefense = Random.Range(_enemy[enemyIndex].getMDef().min, _enemy[enemyIndex].getMDef().max) + Random.Range(_enemy[enemyIndex].getRDef().min, _enemy[enemyIndex].getRDef().max);
        int totalDamage = (100 - enemyDefense) * playerDamage / 100;
        _enemy[enemyIndex].setHP(_enemy[enemyIndex].getHP() - totalDamage);
        Debug.Log(_enemy[enemyIndex].getHP());
        StartCoroutine(PlayerGeludAnim(totalDamage, enemyIndex)); 
    }

    IEnumerator PlayerGeludAnim(int totalDamage, int enemyIndex)
    {
        _hero[0].dealDamageAnim();
        yield return new WaitForSecondsRealtime(1f);
        _enemy[enemyIndex].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);
        InstantiateDamage(totalDamage, _enemy[enemyIndex].gameObject);
        yield return new WaitForSecondsRealtime(1f);
        updateGameCondition();
        if (_gameState != GameState.battleEnd)
        {
            if (_hero.Count > 1) ; //AI turn
            else _gameState = GameState.enemyTurn;
        }
    }

    //Enemy Gelud
    void EnemyGelud(int enemyIndex, int heroIndex)
    {
        enemyAttackInProgress = true;
        int enemyDamage = Random.Range(_enemy[enemyIndex].getMAtk().min, _enemy[enemyIndex].getMAtk().max) + Random.Range(_enemy[enemyIndex].getRAtk().min, _enemy[0].getRAtk().max);
        int playerDefense = _hero[heroIndex].getMDef() + _hero[heroIndex].getRDef();
        int totalDamage = (100 - playerDefense) * enemyDamage / 100;
        _hero[heroIndex].setHP(_hero[heroIndex].getHP() - totalDamage);
        StartCoroutine(EnemyGeludAnim(totalDamage, enemyIndex, heroIndex));
    }
    IEnumerator EnemyGeludAnim(int totalDamage, int enemyIndex, int heroIndex)
    {
        _enemy[enemyIndex].dealDamageAnim();
        yield return new WaitForSecondsRealtime(1f);
        _hero[heroIndex].takeDamageAnim();
        yield return new WaitForSecondsRealtime(0.8f);
        InstantiateDamage(totalDamage, _hero[heroIndex].gameObject);
        yield return new WaitForSecondsRealtime(1f);
        updateGameCondition();
        enemyAttackInProgress = false;
    }
    #endregion

    void updateGameCondition()
    {
        //update HP and AP
        for (int i = 0; i < _hero.Count; i++)
        {
            heroStats[i].name.text = _hero[i]._name;
            heroStats[i].HP.text = _hero[i].getHP().ToString() + "/" + _hero[i].getMaxHP().ToString();
            heroStats[i].AP.text = _hero[i].getAP().ToString() + "/20";
        }
        //check kondisi enemy
        for (int i = 0; i < _enemy.Count; i++)
        {
            if (_enemy[i].getHP() < 0)
            {
                _enemy[i].defeated();
                _enemy.Remove(_enemy[i]);
            }
        }

        //update nama enemy baru
        foreach (Text t in _enemyNames)
            t.gameObject.SetActive(false);
        for (int i = 0; i < _enemy.Count; i++)
        {
            _enemyNames[i].gameObject.SetActive(true);
            _enemyNames[i].text = _enemy[i]._name;
        }

        //cek kalo enemy udh ga ada semua = battle won
        if(_enemy.Count == 0)
        {
            _gameState = GameState.battleEnd;
            StartCoroutine(gameWinAnim());
        }
    }

    IEnumerator gameWinAnim()
    {
        yield return new WaitForSecondsRealtime(1f);
        gameWin();
    }

    void gameWin()
    {
        foreach(LevelUpStats stat in _levelUpText)
        {
            stat.Name.text = "";
            stat.HP.text = "";
        }

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

                    _levelUpText[i].Name.color = Color.clear;
                    _levelUpText[i].HP.color = Color.clear;

                    _levelUpText[i].Name.text = _hero[i]._name.ToString();
                    _levelUpText[i].HP.text = "HP + " + HP + "  (" + (_hero[i].getMaxHP() - HP) + "->" + _hero[i].getMaxHP() + ")";

                    _levelUpText[i].Name.DOColor(Color.white, 0.5f);
                    _levelUpText[i].HP.DOColor(Color.white, 0.5f);
                }
            });
        });
    }

    public enum GameState
    {
        heroTurn,
        enemyTurn,
        npcTurn,
        battleEnd
    };

    public enum menuState { 
        actionSelect,
        skillSelect,
        enemySelect,
        attackInProgress
    };
}


