﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// container class for all the extension methods used in this project
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// returns the int as a roman numeral string.  Supports 0-3999, inclusive. For numbers outside that range, the regular number.toString() is returned instead.
    /// </summary>
    public static string ToRomanNumeral(this int number)
    {
        //very slight modification of this answer on stack overflow: http://stackoverflow.com/a/11749642

        //special cases
        if ((number < 0) || (number > 3999)) return number.ToString(); //out of range
        if (number == 0) return "";                                    //0 is an empty string

        //general case: find the find the beginning of the number and recurse to get the rest
        if (number >= 1000) return "M" + ToRomanNumeral(number - 1000);
        if (number >= 900) return "CM" + ToRomanNumeral(number - 900);
        if (number >= 500) return "D" + ToRomanNumeral(number - 500);
        if (number >= 400) return "CD" + ToRomanNumeral(number - 400);
        if (number >= 100) return "C" + ToRomanNumeral(number - 100);
        if (number >= 90) return "XC" + ToRomanNumeral(number - 90);
        if (number >= 50) return "L" + ToRomanNumeral(number - 50);
        if (number >= 40) return "XL" + ToRomanNumeral(number - 40);
        if (number >= 10) return "X" + ToRomanNumeral(number - 10);
        if (number >= 9) return "IX" + ToRomanNumeral(number - 9);
        if (number >= 5) return "V" + ToRomanNumeral(number - 5);
        if (number >= 4) return "IV" + ToRomanNumeral(number - 4);
        if (number >= 1) return "I" + ToRomanNumeral(number - 1);

        //we shouldn't be able to get here
        Debug.LogWarning("something went wrong in extension function ToRoman().");
        return "???";
    }

    /// <summary>
    /// returns the opposite of the ConnectionDirection
    /// </summary>
    public static ConnectionDirection Inverse(this ConnectionDirection original)
    {
        switch (original)
        {
            case ConnectionDirection.UP:    return ConnectionDirection.DOWN; 
            case ConnectionDirection.RIGHT: return ConnectionDirection.LEFT; 
            case ConnectionDirection.DOWN:  return ConnectionDirection.UP;   
            case ConnectionDirection.LEFT:  return ConnectionDirection.RIGHT;
            default: throw new InvalidOperationException("Can't find an inverse for ConnectionDirection " + original);
        }
    }
}
