using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Handle saving, loading and encrypting files

public class FileManager
{
    // This encryption mechanism is called XOR encryption. Since XOR is a symmetric encryption algorithm, the same key is used for both encryption and decryption.
    // Its fast and simple but not secure for sensitive data.
    private const string KEY = "wwwencryptkey";

    private const string COMMENT_LINE_ID = "//";
    private const string COMMECT_SECTION_START_ID = "/*";
    private const string COMMENT_SECTION_END_ID = "*/";

    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if (!filePath.StartsWith("/"))
            filePath = FilePaths.root + filePath;

        List<string> lines = new List<string>();
        bool inCommentSection = false;
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
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
        catch (FileNotFoundException e)
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

    public static bool TryCreateDirFromPath(string path)
    {
        if (Directory.Exists(path) || File.Exists(path))
            return true;

        if (path.Contains("."))
        {
            path = Path.GetDirectoryName(path);
            if (Directory.Exists(path))
                return true;
        }

        if (path == string.Empty)
            return false;

        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception error)
        {
            Debug.LogError($"Failed to create directory at {path}: {error.Message}");
            return false;
        }
    }

    public static void Save(string filePath, string JSONData, bool encrypt = false)
    {
        if (!TryCreateDirFromPath(filePath))
        {
            Debug.LogError($"Failed to save file {filePath}");
            return;
        }

        if (encrypt)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(JSONData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
            byte[] encryptedBytes = XOR(dataBytes, keyBytes);

            File.WriteAllBytes(filePath, encryptedBytes);
        }
        else
        {
            StreamWriter sw = new StreamWriter(filePath);
            sw.Write(JSONData);
            sw.Close();
        }

        Debug.Log($"File saved successfully at {filePath}");
    }

    public static T Load<T>(string filePath, bool encrypt = false)
    {
        if (File.Exists(filePath))
        {
            if (encrypt)
            {
                byte[] encryptedBytes = File.ReadAllBytes(filePath);
                byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
                byte[] decryptedBytes = XOR(encryptedBytes, keyBytes);

                string JSONDataFromDecryptedBytes = Encoding.UTF8.GetString(decryptedBytes);
                return JsonUtility.FromJson<T>(JSONDataFromDecryptedBytes);
            }
            else
            {
                string JSONData = File.ReadAllLines(filePath)[0];
                return JsonUtility.FromJson<T>(JSONData);
            }
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
            return default(T);
        }
    }

    private static byte[] XOR(byte[] input, byte[] key)
    {
        byte[] output = new byte[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            output[i] = (byte)(input[i] ^ key[i % key.Length]);
        }

        return output;
    }
}
