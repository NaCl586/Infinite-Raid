using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string _name;
    [SerializeField] private int _HP;
    [SerializeField] private rangedInt _MaxHP;
    [SerializeField] private int _AP = 20;
    [SerializeField] private rangedInt _MAtk;
    [SerializeField] private rangedInt _RAtk;
    [SerializeField] private rangedInt _MDef;
    [SerializeField] private rangedInt _RDef;

    private GameObject _gameObject;
    private void Start()
    {
        _gameObject = this.gameObject;
    }

    //Getters and setters
    public int getHP() { return _HP; }
    public void setHP(int HP) { this._HP = HP; }
    public rangedInt getMaxHP() { return _MaxHP; }
    public void setMaxHP(rangedInt HP) { this._MaxHP = HP; }
    public int getAP() { return _AP; }
    public void setAP(int AP) { this._AP = AP; }
    public rangedInt getMAtk() { return _MAtk; }
    public void setMAtk(rangedInt MAtk) { this._MAtk = MAtk; }
    public rangedInt getRAtk() { return _RAtk; }
    public void setRAtk(rangedInt RAtk) { this._RAtk = RAtk; }
    public rangedInt getMDef() { return _MDef; }
    public void setMDef(rangedInt MDef) { this._MDef = MDef; }
    public rangedInt getRDef() { return _RDef; }
    public void setRDef(rangedInt RDef) { this._RDef = RDef; }

    public void dealDamageAnim()
    {
        this.gameObject.transform.DOMoveX(this.gameObject.transform.position.x + 0.5f, 0.4f).OnComplete(() => {
            this.gameObject.transform.DOMoveX(this.gameObject.transform.position.x - 0.5f, 0.4f);
        });
    }

    public void takeDamageAnim()
    {
        this.gameObject.transform.DOMoveX(this.gameObject.transform.position.x - 0.5f, 0.4f).OnComplete(() => {
            this.gameObject.transform.DOMoveX(this.gameObject.transform.position.x + 0.5f, 0.4f);
        });
    }

    public void defeated()
    {
        this.gameObject.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.4f).OnComplete(() => {
            Destroy(this.gameObject);
        });
    }
}
