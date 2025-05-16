using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Handle saving, loading and encrypting files

public class FileManager
{
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if(!filePath.StartsWith("/"))
            filePath = FilePaths.root + filePath;
        
        List<string> lines = new List<string>();
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line)) 
                        lines.Add(line);
                }
            }
        }
        catch(FileNotFoundException e)
        {
            Debug.LogError("File not found: " + e);
        }

        return lines;
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(filePath);
        if (textAsset == null)
        {
            Debug.LogError("Text asset not found: " + filePath);
            return null;
        }

        return ReadTextAsset(textAsset, includeBlankLines);
    }

    public static List<string> ReadTextAsset(TextAsset textAsset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(textAsset.text))
        {
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }
        return lines;
    }
}
