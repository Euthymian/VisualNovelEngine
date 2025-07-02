using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

namespace DIALOGUE
{
    public class TagManager
    {
        private readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>();
        private readonly Regex tagRegex = new Regex(@"<\w+>");

        public TagManager()
        {
            InitializeTags();
        }

        private void InitializeTags()
        {
            tags["<mainChar>"] = () => "Avira";
            tags["<time>"] = () => DateTime.Now.ToString("hh:mm tt");
            tags["<playerLevel>"] = () => "15";
            tags["<tempVal1>"] = () => "42";
            tags["<input>"] = () => InputPanel.Instance.lastInput;
        }

        public string Inject(string text)
        {
            if (tagRegex.IsMatch(text))
            {
                foreach (Match match in tagRegex.Matches(text))
                {
                    if (tags.TryGetValue(match.Value, out var tagValueFunc))
                    {
                        text = text.Replace(match.Value, tagValueFunc());
                    }
                }
            }

            return text;
        }
    }
}