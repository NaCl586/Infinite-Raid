using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    //Leveling player
    public static int playerScaling()
    {
        int RNG = Random.Range(1, 100);
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

}
