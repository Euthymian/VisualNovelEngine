using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    public class DL_SPEAKER_DATA
    {
        private const string NAME_CAST_ID = " as ";
        private const string POSITION_CAST_ID = " at ";
        private const string EXPRESSION_CAST_ID = @" [";
        private string SPEAKER_DATA_REGEX_PATTERN = @$"{NAME_CAST_ID}|{POSITION_CAST_ID}|{EXPRESSION_CAST_ID.Insert(EXPRESSION_CAST_ID.Length - 1, @"\")}";
        private const char AXIS_SEPERATOR = ':';
        private const char LAYERS_SEPERATOR = ',';
        private const char LAYER_EXPRESSION_SEPERATOR = ':';
        private const string ENTER_KEYWORD = "enter ";

        public string speakerName;
        public string speakerCastName;
        public string displayName => hasCastingName ? speakerCastName : speakerName;
        public Vector2 castPos;
        public List<(int layer, string expression)> CastExpressions { get; set; }

        public bool makeCharacterEnter = false;

        public bool hasCastingName => speakerCastName != string.Empty;
        public bool hasCastingPosition = false;
        public bool hasCastingExpressions => CastExpressions != null && CastExpressions.Count > 0;

        private string ProcessKeywords(string rawSpeaker)
        {
            if (rawSpeaker.StartsWith(ENTER_KEYWORD))
            {
                rawSpeaker = rawSpeaker.Substring(ENTER_KEYWORD.Length);
                makeCharacterEnter = true;
            }

            return rawSpeaker;
        }

        public DL_SPEAKER_DATA(string rawSpeaker)
        {
            rawSpeaker = ProcessKeywords(rawSpeaker);

            MatchCollection matches = Regex.Matches(rawSpeaker, SPEAKER_DATA_REGEX_PATTERN);

            speakerCastName = "";
            castPos = Vector2.zero;
            CastExpressions = new List<(int, string)>();

            if (matches.Count == 0)
            {
                speakerName = rawSpeaker;
                return;
            }

            int currentMatchIndex = matches[0].Index;

            speakerName = rawSpeaker.Substring(0, currentMatchIndex);

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startDataIndex = -1, endDataIndex = -1;

                if (match.Value == NAME_CAST_ID)
                {
                    startDataIndex = match.Index + NAME_CAST_ID.Length;
                    endDataIndex = i + 1 == matches.Count ? rawSpeaker.Length - 1 : matches[i + 1].Index - 1;
                    speakerCastName = rawSpeaker.Substring(startDataIndex, endDataIndex - startDataIndex + 1);
                }
                else if (match.Value == POSITION_CAST_ID)
                {
                    hasCastingPosition = true;

                    startDataIndex = match.Index + POSITION_CAST_ID.Length;
                    endDataIndex = i + 1 == matches.Count ? rawSpeaker.Length - 1 : matches[i + 1].Index - 1;
                    string[] pos = rawSpeaker.Substring(startDataIndex, endDataIndex - startDataIndex + 1)
                                             .Split(AXIS_SEPERATOR, System.StringSplitOptions.RemoveEmptyEntries);
                    // there is a case where we dont specify the y-axis even though we have :, flag System.StringSplitOptions.RemoveEmptyEntries will sevre as safeguard

                    float.TryParse(pos[0], out castPos.x);
                    if (pos.Length > 1)
                        float.TryParse(pos[1], out castPos.y);
                }
                else if (match.Value == EXPRESSION_CAST_ID)
                {
                    startDataIndex = match.Index + EXPRESSION_CAST_ID.Length;
                    endDataIndex = i + 1 == matches.Count ? rawSpeaker.Length - 1 : matches[i + 1].Index - 1;
                    string rawExpression = rawSpeaker.Substring(startDataIndex, endDataIndex - startDataIndex); // removed brackets

                    CastExpressions = rawExpression.Split(LAYERS_SEPERATOR)
                        .Select(x =>
                        {
                            var parts = x.Trim().Split(LAYER_EXPRESSION_SEPERATOR);

                            // initially, when we cast expressions, we will specify layer and expression name
                            // but, in fact, sprite characters only have one layer, so we can just return the expression name and default layer 0
                            if (parts.Length == 2)
                                return (int.Parse(parts[0]), parts[1]);
                            else
                                return (0, parts[0]);

                        }).ToList();
                }
            }

        }

    }
}