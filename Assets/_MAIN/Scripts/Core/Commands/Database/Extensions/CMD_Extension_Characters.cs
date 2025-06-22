using CHARACTER;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_Characters : CMD_Extension
    {
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enable" };
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string PARAM_XPOS => "-x";
        private static string PARAM_YPOS => "-y";
        private static string[] PARAM_SMOOTH => new string[] { "-sm", "-smooth" };
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed" };
        private static string[] PARAM_COLOR => new string[] { "-c", "-color" };
        private static string[] PARAM_SPRITE => new string[] { "-spt", "-sprite" };
        private static string[] PARAM_LAYER => new string[] { "-l", "-layer" };

        new public static void Extend(CommandDatabase cmdDatabase)
        {
            cmdDatabase.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));
            cmdDatabase.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            cmdDatabase.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            cmdDatabase.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            cmdDatabase.AddCommand("setcharacterspriority", new Action<string[]>(SetCharactersPriority));
            cmdDatabase.AddCommand("highlightcharacters", new Func<string[], IEnumerator>(HighlightAll));
            cmdDatabase.AddCommand("unhighlightcharacters", new Func<string[], IEnumerator>(UnHighlightAll));

            //BASE CHARACTER COMMANDS
            CommandDatabase baseSubCommandDb = CommandManager.Instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTER_BASE);
            baseSubCommandDb.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseSubCommandDb.AddCommand("show", new Func<string[], IEnumerator>(Show));
            baseSubCommandDb.AddCommand("hide", new Func<string[], IEnumerator>(Hide));
            baseSubCommandDb.AddCommand("setpriority", new Action<string[]>(SetPriority));
            baseSubCommandDb.AddCommand("setcolor", new Func<string[], IEnumerator>(SetColor));
            baseSubCommandDb.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));
            baseSubCommandDb.AddCommand("unhighlight", new Func<string[], IEnumerator>(Unhighlight));

            //SPRITE CHARACTER COMMANDS
            CommandDatabase spriteSubCommandDb = CommandManager.Instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTER_SPRITE);
            spriteSubCommandDb.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));
        }

        #region GLOBAL COMMANDS
        private static void CreateCharacter(string[] data)
        {
            string characterName = data[0];
            bool enable = false;
            bool immediateShow = false;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediateShow, defaultValue: false);

            Character character = CharacterManager.Instance.CreateCharacter(characterName);

            if (!enable)
                return;

            if(immediateShow)
                character.isVisible = true;
            else
                character.Show();
        }

        private static IEnumerator ShowAll(string[] data)
        {
            // There are 2 options to show characters:
            // 1. Show by Show() method which grandually shows the character
            // 2. Show by isVisible property which immediately shows the character without fading animation

            // data will be a list of character names seperated by SPACE

            List<Character> characters = new List<Character>();
            bool immediate = false;
            float speed = 1;

            foreach (string each in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(each, createIfDoentExist: false);
                if(character != null)
                    characters.Add(character);
            }

            if(characters.Count == 0)
                yield break;

            // Get params
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // call logic on each character
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;
                else
                    character.Show(speed);
            }

            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                    {
                        character.isVisible = true;
                    }
                });

                while (characters.Any(c => c.isShowing))
                    yield return null;
            }
        }

        private static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediateShow = false;

            foreach (string each in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(each, createIfDoentExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;

            // Get params
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediateShow, defaultValue: false);

            // call logic on each character
            foreach (Character character in characters)
            {
                if (immediateShow)
                    character.isVisible = false;
                else
                    character.Hide();
            }

            if (!immediateShow)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                    {
                        character.isVisible = false;
                    }
                });

                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }

        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.Instance.GetCharacter(characterName, createIfDoentExist: false);

            if(character == null)
            {
                Debug.LogWarning($"Character '{characterName}' does not exist.");
                yield break;
            }

            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;

            CommandParameters parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_XPOS, out x, defaultValue: 0f);
            parameters.TryGetValue(PARAM_YPOS, out y, defaultValue: 0f);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Vector2 pos = new Vector2(x, y);

            if (immediate)
                character.SetPosition(pos);
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(pos); });
                yield return character.MoveToPosition(pos, speed, smooth);
            }
        }

        private static void SetCharactersPriority(string[] data)
        {
            CharacterManager.Instance.SortCharacters(data);
        }

        private static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;

            foreach (string each in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(each, createIfDoentExist: false);
                if (character != null)
                {
                    characters.Add(character);
                }
            }

            if (characters.Count == 0)
                yield break;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out float speed, defaultValue: 1f);

            foreach (Character character in characters)
            {
                character.Highlight(immediate: immediate, speed:speed);
            }

            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                    {
                        character.Highlight(immediate: true);
                    }
                });
                while (characters.Any(c => c.isHighlighting))
                    yield return null;
            }
        }

        private static IEnumerator UnHighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;

            foreach (string each in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(each, createIfDoentExist: false);
                if (character != null)
                {
                    characters.Add(character);
                }
            }

            if (characters.Count == 0)
                yield break;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out float speed, defaultValue: 1f);

            foreach (Character character in characters)
            {
                character.UnHighlight(immediate: immediate, speed: speed);
            }

            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                    {
                        character.UnHighlight(immediate: true);
                    }
                });
                while (characters.Any(c => c.isUnHighlighting))
                    yield return null;
            }
        }
        #endregion

        #region BASE CHARACTER COMMAND
        private static IEnumerator Show(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.Instance.GetCharacter(characterName, createIfDoentExist: false);
            if (character == null)
            {
                Debug.LogWarning($"Character '{characterName}' does not exist.");
                yield break;
            }

            bool immediateShow = false;
            float speed = 1f;

            CommandParameters parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediateShow, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            if (immediateShow)
                character.isVisible = true;
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = true; });
                yield return character.Show(speed);
            }
        }

        private static IEnumerator Hide(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.Instance.GetCharacter(characterName, createIfDoentExist: false);
            if (character == null)
            {
                Debug.LogWarning($"Character '{characterName}' does not exist.");
                yield break;
            }

            bool immediateShow = false;
            float speed = 1f;

            CommandParameters parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediateShow, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            if (immediateShow)
                character.isVisible = false;
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = false; });
                yield return character.Hide(speed);
            }
        }

        private static void SetPriority(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], createIfDoentExist: false);
            int priority;

            if(character == null || data.Length < 2)
            {
                Debug.LogWarning($"Character '{data[0]}' does not exist or priority is not specified.");
                return;
            }

            if (!int.TryParse(data[1], out priority))
                priority = 0;

            character.SetPriority(priority);
        }

        private static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], createIfDoentExist: false);
            string colorName;
            float speed;
            bool immediate;

            if(character == null || data.Length < 2)
            {
                Debug.LogWarning($"Character '{data[0]}' does not exist or color is not specified.");
                yield break;
            }

            // startIndex = 1 becuase when we call function by character, character name will be the first param,
            // then if we not specify the param id (-c, ...) there is no matching type for character name
            var parameters = ConvertDataToParameters(data, startingIndex:1);
            parameters.TryGetValue(PARAM_COLOR, out colorName);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Color color = Color.white;
            // this is color extension method
            color = color.GetColorByName(colorName);

            if (immediate)
            {
                character.SetColor(color);
                Debug.Log("immediate set colro");
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.SetColor(color); });
                yield return character.TransitionColor(color, speed);
            }
        }

        private static IEnumerator Highlight(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], createIfDoentExist: false);
            if (character == null)
            {
                Debug.LogWarning($"Character '{data[0]}' does not exist.");
                yield break;
            }

            bool immediate = false;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
                character.Highlight(immediate: true);
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.Highlight(immediate: true); });
                yield return character.Highlight(immediate: false);
            }
        }

        private static IEnumerator Unhighlight(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], createIfDoentExist: false);
            if (character == null)
            {
                Debug.LogWarning($"Character '{data[0]}' does not exist.");
                yield break;
            }

            bool immediate = false;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
            {
                character.UnHighlight(immediate: true);
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.UnHighlight(immediate: true); });
                yield return character.UnHighlight(immediate: false);
            }
        }
        #endregion

        #region SPRITE CHARACTER COMMANDS
        private static IEnumerator SetSprite(string[] data)
        {
            Character_Sprite character = CharacterManager.Instance.GetCharacter(data[0], createIfDoentExist: false) as Character_Sprite;
            if (character == null)
            {
                Debug.LogWarning($"Character '{data[0]}' does not exist.");
                yield break;
            }

            int layer;
            string spriteName;
            float speed;
            bool immediate;

            CommandParameters parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);
            parameters.TryGetValue(PARAM_SPRITE, out spriteName);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Sprite sprite = character.GetSprite(spriteName);

            if (sprite == null)
            {
                Debug.LogWarning($"Sprite '{spriteName}' does not exist for character '{data[0]}'.");
                yield break;
            }

            if (immediate)
            {
                character.SetSprite(sprite, layer);
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character?.SetSprite(sprite, layer); });
                yield return character.TransitionSprite(sprite, layer, speed);
            }
        }
        #endregion
    }
}