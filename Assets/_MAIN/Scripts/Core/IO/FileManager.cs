using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Handle saving, loading and encrypting files

public class FileManager
{
    private const string COMMENT_LINE_ID = "//";
    private const string COMMECT_SECTION_START_ID = "/*";
    private const string COMMENT_SECTION_END_ID = "*/";

    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if(!filePath.StartsWith("/"))
            filePath = FilePaths.root + filePath;
        
        List<string> lines = new List<string>();
        bool inCommentSection = false;
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (!inCommentSection)
                    {
                        if (line.StartsWith(COMMECT_SECTION_START_ID))
                        {
                            inCommentSection = true;
                            continue;
                        }

                        if (line.StartsWith(COMMENT_LINE_ID))
                            continue;

                        if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                            lines.Add(line);
                    }
                    else
                    {
                        if (line.StartsWith(COMMENT_SECTION_END_ID))
                            inCommentSection = false;
                    }
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
            bool inCommentSection = false;
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();

                if (!inCommentSection)
                {
                    if (line.StartsWith(COMMECT_SECTION_START_ID))
                    {
                        inCommentSection = true;
                        continue;
                    }

                    if (line.StartsWith(COMMENT_LINE_ID))
                        continue;

                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
                else
                {
                    if (line.StartsWith(COMMENT_SECTION_END_ID))
                        inCommentSection = false;
                }
            }
        }
        return lines;
    }
}
