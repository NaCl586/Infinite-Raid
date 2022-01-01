using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public string _name;
    public Sprite _icon;

    //weapon type
    [SerializeField] private bool _healer;
    [SerializeField] private bool _buffer;

    //stats (index = 0-4)
    [SerializeField] private rangedInt _HP;
    [SerializeField] private rangedInt _MAtk;
    [SerializeField] private rangedInt _RAtk;
    [SerializeField] private rangedInt _MDef;
    [SerializeField] private rangedInt _RDef;

    //stats HP Matk Ratk Mdef Rdef;
    public int[] _statsGiven = {0,0,0,0,0};

    //getter
    public rangedInt getHP() { return this._HP; }
    public rangedInt getMAtk() { return this._MAtk; }
    public rangedInt getRAtk() { return this._RAtk; }
    public rangedInt getMDef() { return this._MDef; }
    public rangedInt getRDef() { return this._RDef; }

    public bool isBuffer() { return _buffer; }
    public bool isHealer() { return _healer; }
}
