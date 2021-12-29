using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : MonoBehaviour
{
    public string _name;
    public Sprite _icon;
    public int _slots;

    //stats (index = 0-4)
    [SerializeField] private rangedInt _HP;
    [SerializeField] private rangedInt _MDef;
    [SerializeField] private rangedInt _RDef;

    //stats HP MDef RDef
    public int[] _statsGiven = { 0, 0, 0 };
    public int[] _skills = { 0, -1, -1 };

    //getter
    public rangedInt getHP() { return this._HP; }
    public rangedInt getMDef() { return this._MDef; }
    public rangedInt getRDef() { return this._RDef; }
}
