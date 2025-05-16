using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Control how text appear

public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;
    public string currentText => tmpro.text;
    public string targetText { get; private set; } = "";
    public string preText { get; private set; } = "";
    private int preTextLength = 0;
    public string fullTargetText => preText + targetText;

    public enum BuildMethod { instant, typewriter, fade }
    public BuildMethod buildMethod = BuildMethod.typewriter;
    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    private const float baseSpeed = 1;
    private float speedMultiplier = 1;
    public float speed { get { return speedMultiplier * baseSpeed; } set { speedMultiplier = value; } }

    private int characterMultiplier = 1;
    public int characterPerCycle { get { return speed <= 2 ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }

    public bool hurryUp = false;

    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    public Coroutine Build(string text)
    {
        preText = "";
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    private IEnumerator Building()
    {
        Prepare();
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_TypeWriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }

        OnComplete();
    }

    private void Prepare()
    {
        switch(buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_TypeWriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color; // Force re-init color in all vertices before start fading
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void Prepare_TypeWriter()
    {
        tmpro.color = tmpro.color; 
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        tmpro.text = preText;
        if(preText != "")
        {
            tmpro.ForceMeshUpdate();
            preTextLength = tmpro.textInfo.characterCount;
        }
        else preTextLength = 0;

        tmpro.text += targetText;
        tmpro.maxVisibleCharacters = int.MaxValue; // if we switch to fade, we need to show all characters (typewriter will hide them)
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;

        Color visibleColor = new Color(textColor.r, textColor.g, textColor.b, 1);
        Color hiddenColor = new Color(textColor.r, textColor.g, textColor.b, 0);

        /*
         TextMeshPro stores the color of all vertices (4 each character) in a long Color32 array.
        Below is how we can access the color of each vertex of each character.
         */
        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        for(int i=0;i<textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if(!charInfo.isVisible) continue; // This is because anytime we access a invisible character, the index will go back to 0 -> the first character will behave weirdly (always visible or flicky)

            if (i < preTextLength)
            {
                for(int v = 0; v < 4; v++)
                {
                    vertexColors[charInfo.vertexIndex + v] = visibleColor;
                }
            }
            else
            {
                for (int v = 0; v < 4; v++)
                {
                    vertexColors[charInfo.vertexIndex + v] = hiddenColor;
                }
            }
        }

        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32); // Force update the vertex colors
    }


    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }

    public void ForceComplete()
    {
        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;
        }

        Stop();
        OnComplete();
    }

    private IEnumerator Build_TypeWriter()
    {
        while(tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? characterPerCycle * 5 : characterPerCycle;
            yield return new WaitForSeconds(0.015f / speed);
        }
    }

    private IEnumerator Build_Fade()
    {
        // We arent going to fade character by character, but we are going to fade a range of characters
        int minRange = preTextLength;
        int maxRange = minRange + 1;

        // When last character of the range reaches a threshold (character doesnt completely appear)4, we will increase the range
        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = tmpro.textInfo;

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        // By default, alpha is stored in byte, we cant grandually increase byte (decimal), it isnt gonna look good
        // -> define array of float to lerp the alpha value of each character
        float[] alphas = new float[textInfo.characterCount];

        while (true)
        {
            float fadeSpeed = (hurryUp ? characterPerCycle * 5 : characterPerCycle) * speed * 4f;

            for (int i=minRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;
                
                int vertexIndex = charInfo.vertexIndex;
                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];

                if (alphas[i] >= 255)
                    minRange++;
            }

            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            
            bool lastCharIsInvisible = !textInfo.characterInfo[maxRange-1].isVisible;
            if (alphas[maxRange-1] > alphaThreshold || lastCharIsInvisible)
            {
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphas[maxRange - 1] >= 255 || lastCharIsInvisible)
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;

    public void Stop()
    {
        if(!isBuilding) return;
        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }
}
