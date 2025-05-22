using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

namespace DIALOGUE
{
    public class DL_DIALOGUE_DATA
    {
        private const string SEGMENT_IDENTIFIER_REGEX_PATTERN = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

        public List<DIALOGUE_SEGMENT> segments;

        public DL_DIALOGUE_DATA(string rawDialogue)
        {
            segments = RipSegments(rawDialogue);
        }

        public List<DIALOGUE_SEGMENT> RipSegments(string rawDialogue)
        {
            List<DIALOGUE_SEGMENT> segments = new List<DIALOGUE_SEGMENT>();
            MatchCollection matches = Regex.Matches(rawDialogue, SEGMENT_IDENTIFIER_REGEX_PATTERN);

            DIALOGUE_SEGMENT segment = new DIALOGUE_SEGMENT();

            // Handle the very first segment with no signal
            segment.signalDelay = 0f;
            segment.startSignal = DIALOGUE_SEGMENT.StartSignal.NONE;
            if (matches.Count == 0)
                segment.dialogue = rawDialogue;
            else
                segment.dialogue = rawDialogue.Substring(0, matches[0].Index);
            segments.Add(segment);

            int currentSignalIdentifierStartIndex = default;

            if (matches.Count == 0)
                return segments;
            else
                currentSignalIdentifierStartIndex = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DIALOGUE_SEGMENT();

                string signal = match.Value;
                signal = signal.Substring(1, signal.Length - 2); // Remove the brackets
                string[] signalParts = signal.Split(' ');

                // Get the signal
                segment.startSignal = Enum.Parse<DIALOGUE_SEGMENT.StartSignal>(signalParts[0].ToUpper());

                // Get the delay
                if (signalParts.Length < 2)
                    segment.signalDelay = 0f;
                else
                    float.TryParse(signalParts[1], out segment.signalDelay);

                // Get the dialogue
                int nextSignalIdentifierStartIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(currentSignalIdentifierStartIndex + match.Length, nextSignalIdentifierStartIndex - (currentSignalIdentifierStartIndex + match.Length));
                currentSignalIdentifierStartIndex = nextSignalIdentifierStartIndex;
                segments.Add(segment);
            }

            return segments;
        }

        public struct DIALOGUE_SEGMENT
        {
            public string dialogue;
            public StartSignal startSignal;
            public float signalDelay;
            public enum StartSignal { NONE, C, A, WC, WA }

            public bool append => startSignal == StartSignal.A || startSignal == StartSignal.WA ? true : false;
        }
    }
}