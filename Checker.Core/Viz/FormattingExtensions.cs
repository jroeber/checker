using System;

namespace Checker.Viz
{
    public static class FormattingExtensions
    {
        public static string SimpleElapsed(this DateTime dateTime)
        {
            // Guaranteed to be 3 chars wide
            var elapsed = DateTime.Now - dateTime;
            
            if (elapsed.Days > 9)    return elapsed.Days.ToString() + "d";
            if (elapsed.Days > 0)    return " " + elapsed.Days.ToString() + "d";
            if (elapsed.Hours > 9)   return elapsed.Hours.ToString() + "h";
            if (elapsed.Hours > 0)   return " " + elapsed.Hours.ToString() + "h";
            if (elapsed.Minutes > 9) return elapsed.Minutes.ToString() + "m";
            if (elapsed.Minutes > 0) return " " + elapsed.Minutes.ToString() + "m";
            if (elapsed.Seconds > 9) return elapsed.Seconds.ToString() + "s";
            return " " + elapsed.Seconds.ToString() + "s";
        }

        public static string SetWidth(this string str, int finalLength)
        {
            if (str.Length <= finalLength)
            {
                return str.PadRight(finalLength);
            }
            else if (str.Length > finalLength && finalLength > 3)
            {
                return str.Substring(0, finalLength - 3) + "[…]";
            }
            else if (finalLength <= 0) return "";
            else if (finalLength == 1) return "…";
            else if (finalLength == 2) return "[…";
            else if (finalLength == 3) return "[…]";
            
            return str;
        }
    }
}