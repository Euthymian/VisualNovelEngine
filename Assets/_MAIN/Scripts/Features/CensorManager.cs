using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


// We can inject this censor to inputPanel -> OnAcceptInput
public class CensorManager
{
    // Standalone bad words
    private static Dictionary<string, string> badWords = new Dictionary<string, string>()
    {
        { "badword1", "b[a@4]dw[o0]rd1" },
        { "stinking", "[s\\$]t[i1]nk[i1]n[9g]" },
    };

    // Censor even bad words are part of a larger word
    private static Dictionary<string, string> hardBlock = new Dictionary<string, string>()
    {
        { "tofu", "t[o0]fu" },
    };

    public static bool Censor(ref string text)
    {
        bool isCensored = false;

        foreach(var pair in hardBlock)
        {
            // Ignore word boundaries for hard blocks
            Regex regex = new Regex(pair.Value, RegexOptions.IgnoreCase);

            if (regex.IsMatch(text))
            {
                isCensored = true;
                text = regex.Replace(text, match => new string('*', match.Length));
            }
        }

        foreach (var pair in badWords)
        {
            //  No word boundary check
            string pattern = $"(?<=\\W|^){pair.Value}(?=\\W|$)";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if(regex.IsMatch(text))
            {
                isCensored = true;
                text = regex.Replace(text, match => new string('*', match.Length));
            }
        }

        return isCensored;
    }
}
