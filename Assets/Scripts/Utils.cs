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

    public static int[] _enemySpawn(int _wave, int _partyMembersCount)
    {
        //rogue = 0, wraith = 1, golem = 2, yeti = 3
        int[] _enemyIndex = { -1, -1, -1 };
        int rng;

        //wave 1-3 : 1 rogue
        if (_wave >= 1 && _wave <= 3)
        {
            _enemyIndex[0] = 0;
            _enemyIndex[1] = _enemyIndex[2] = -1;
        }
        //wave 4-6 : 2 rogue
        else if (_wave >= 4 && _wave <= 6)
        {
            _enemyIndex[0] = _enemyIndex[1] = 0;
            _enemyIndex[2] = -1;
        }
        //wave 7-10 : 1 rogue 1 wraith
        else if (_wave >= 4 && _wave <= 7)
        {
            _enemyIndex[0] = 0;
            _enemyIndex[1] = 1;
            _enemyIndex[2] = -1;
        }
        //wave 11-15 : 1 wraith 2 rogue / 2 wraith 1 rogue
        else if (_wave >= 11 && _wave <= 15)
        {
            if(Random.Range(0,2) == 0)
            {
                _enemyIndex[0] = 0;
                _enemyIndex[1] = 1;
                _enemyIndex[2] = 1;
            }
            else
            {
                _enemyIndex[0] = 0;
                _enemyIndex[1] = 0;
                _enemyIndex[2] = 1;
            }
        }
        //Wave 16-20 : 3 opsi yaitu :1 golem 1 wraith 30%; 1 golem 1 rogue 30 %; 1 wraith 2 rogue / 2 wraith 1 rogue 40 %
        else if (_wave >= 16 && _wave <= 20)
        {
            rng = Random.Range(1, 101);
            if(rng >= 1 && rng <= 30)
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = 1;
                _enemyIndex[2] = -1;
            }
            else if (rng >= 31 && rng <= 60)
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = 0;
                _enemyIndex[2] = -1;
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    _enemyIndex[0] = 0;
                    _enemyIndex[1] = 1;
                    _enemyIndex[2] = 1;
                }
                else
                {
                    _enemyIndex[0] = 0;
                    _enemyIndex[1] = 0;
                    _enemyIndex[2] = 1;
                }
            }
        }
        //Wave 21-25 : 1 yeti 25 %; 1 golem 2 wraith / 1 golem 2 rogue / 1 golem 1rogue 1 wraith 65 %;  3 wraith / 3 rogue 10 %
        else if (_wave >= 21 && _wave <= 25)
        {
            rng = Random.Range(1, 101);
            if (rng >= 1 && rng <= 25)
            {
                _enemyIndex[0] = 3;
                _enemyIndex[1] = -1;
                _enemyIndex[2] = -1;
            }
            else if (rng >= 26 && rng <= 90)
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = (Random.Range(0, 2) == 0) ? 0 : 1;
                _enemyIndex[2] = (Random.Range(0, 2) == 0) ? 0 : 1;
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    _enemyIndex[0] = 1;
                    _enemyIndex[1] = 1;
                    _enemyIndex[2] = 1;
                }
                else
                {
                    _enemyIndex[0] = 0;
                    _enemyIndex[1] = 0;
                    _enemyIndex[2] = 0;
                }
            }
        }
        //Wave 26-30 :1 yeti 1 golem 20%; 2 golem 30 %; 2 golem 1 wraith 50 %
        else if (_wave >= 26 && _wave <= 30)
        {
            rng = Random.Range(1, 101);
            if (rng >= 1 && rng <= 20)
            {
                _enemyIndex[0] = 3;
                _enemyIndex[1] = 2;
                _enemyIndex[2] = -1;
            }
            else if (rng >= 21 && rng <= 50)
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = 2;
                _enemyIndex[2] = -1;
            }
            else
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = 1;
                _enemyIndex[2] = 0;
            }
        }
        //Wave 31-40 : 2 yeti 30%; 3 golem 35 %; 2 golem 1 yeti 35 %
        else if (_wave >= 31 && _wave <= 40)
        {
            rng = Random.Range(1, 101);
            if (rng >= 1 && rng <= 30)
            {
                _enemyIndex[0] = 3;
                _enemyIndex[1] = 3;
                _enemyIndex[2] = -1;
            }
            else if (rng >= 31 && rng <= 65)
            {
                _enemyIndex[0] = 2;
                _enemyIndex[1] = 2;
                _enemyIndex[2] = 2;
            }
            else
            {
                _enemyIndex[0] = 3;
                _enemyIndex[1] = 2;
                _enemyIndex[2] = 2;
            }
        }
        //wave 41+
        else
        {
            rng = Random.Range(1, 101);
            if(_partyMembersCount >= 2)
            {
                int limit;
                if (_partyMembersCount == 4) limit = 25;
                else if (_partyMembersCount == 3) limit = 33;
                else limit = 50;

                if (rng >= 1 && rng <= limit)
                {
                    _enemyIndex[0] = Random.Range(0, 4);
                    _enemyIndex[1] = Random.Range(0, 4);
                    _enemyIndex[2] = -1;
                }
                else
                {
                    _enemyIndex[0] = Random.Range(0, 4);
                    _enemyIndex[1] = Random.Range(0, 4);
                    _enemyIndex[2] = Random.Range(0, 4);
                }
            }
            else
            {
                if (rng >= 1 && rng <= 50)
                {
                    _enemyIndex[0] = Random.Range(0, 4);
                    _enemyIndex[1] = -1;
                    _enemyIndex[2] = -1;
                }
                else if (rng >= 51 && rng <= 85)
                {
                    _enemyIndex[0] = Random.Range(0, 4);
                    _enemyIndex[1] = Random.Range(0, 4);
                    _enemyIndex[2] = -1;
                }
                else
                {
                    _enemyIndex[0] = Random.Range(0, 4);
                    _enemyIndex[1] = Random.Range(0, 4);
                    _enemyIndex[2] = Random.Range(0, 4);
                }
            }
        }

        return _enemyIndex;
    }
}
