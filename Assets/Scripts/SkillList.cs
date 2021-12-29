using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public string _skillName;
    [TextArea(1, 2)] public string _skillDesc;
    public int _APNeeded;
    public int[] _usableWeapons;
}

public class SkillList : MonoBehaviour
{
    public Skill[] _skillList;
}