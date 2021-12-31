using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    //Leveling player
    public static int playerScaling()
    {
        int RNG = Random.Range(1, 101);
        if (RNG >= 1 && RNG <= 10) return 1;
        if (RNG >= 11 && RNG <= 22) return 2;
        if (RNG >= 23 && RNG <= 36) return 3;
        if (RNG >= 37 && RNG <= 54) return 4;
        if (RNG >= 55 && RNG <= 72) return 5;
        if (RNG >= 73 && RNG <= 84) return 6;
        if (RNG >= 85 && RNG <= 90) return 7;
        if (RNG >= 91 && RNG <= 85) return 8;
        if (RNG >= 96 && RNG <= 98) return 9;
        return 10;
    }

    public static float scalingMultiplier(int _wave)
    {
        float num = 1;
        int reps = (_wave) / 5;
        for (int i = 0; i < reps; i++)
        {
            num = (num * 125) / 100;
        }
        return num;
    }

    public static int[] skillRandomizer(int _weaponIdx, int _slots)
    {
        int[] _skills = { 0, -1, -1 };
        if(_slots == 1)
        {
            //do nothing
        }
        else if(_slots > 1)
        {
            List<int> _skillList = new List<int>();
            if (_weaponIdx == 0) { _skillList.Add(4); _skillList.Add(5); } //sword
            else if (_weaponIdx == 1) { _skillList.Add(3); _skillList.Add(7); } //bow
            else if (_weaponIdx == 2) { _skillList.Add(2); _skillList.Add(6); } //wand
            else if (_weaponIdx == 3) { _skillList.Add(1); _skillList.Add(5); } //gauntlet
            else if (_weaponIdx == 4) { _skillList.Add(5); _skillList.Add(8); }//morningStar
            if (_slots == 2)
            {
                int rng = Random.Range(0, 2);
                _skills[1] = _skillList[rng];
            }
            else if (_slots == 3)
            {
                _skills[1] = _skillList[0];
                _skills[2] = _skillList[1];
            }
        }
        return _skills;
    }

}
