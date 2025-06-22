using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtension
{
    public static Color SetAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static Color GetColorByName(this Color original, string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "black":
                return Color.black;
            case "white":
                return Color.white;
            case "orange":
                return new Color(1.0f, 0.5f, 0.0f); // Custom orange color
            default:
                Debug.LogWarning($"Color '{colorName}' not recognized. Returning original color.");
                return original;
        }
    }
}
