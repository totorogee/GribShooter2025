using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Utility class for file operations in Unity, providing methods for reading/writing files
/// and loading textures/sprites from the file system.
/// </summary>
public static class FilePath
{
    /// <summary>
    /// Writes a string to a file in the appropriate documents directory for the current platform.
    /// </summary>
    /// <param name="str">The string content to write to the file</param>
    /// <param name="filename">The name of the file to write to</param>
    public static void WriteStringToFile(string str, string filename)
    {
        string path = PathForDocumentsFile(filename);
        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);

        StreamWriter sw = new StreamWriter(file);
        sw.WriteLine(str);

        sw.Close();
        file.Close();
    }

    /// <summary>
    /// Reads a string from a file in the appropriate documents directory for the current platform.
    /// </summary>
    /// <param name="filename">The name of the file to read from</param>
    /// <returns>The string content of the file, or null if the file doesn't exist</returns>
    public static string ReadStringFromFile(string filename)
    {
        string path = PathForDocumentsFile(filename);

        if (File.Exists(path))
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string str = null;
            str = sr.ReadLine();

            sr.Close();
            file.Close();

            return str;
        }

        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the appropriate file path for documents based on the current platform.
    /// For iOS, uses the Documents folder. For other platforms, uses persistentDataPath.
    /// </summary>
    /// <param name="fullAddress">The full file address or just filename</param>
    /// <returns>The platform-appropriate file path</returns>
    public static string PathForDocumentsFile(string fullAddress)
    {
        var filename = Path.GetFileName(fullAddress);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // For iOS, navigate to the Documents folder
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else
        {
            // For other platforms, use persistentDataPath
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }

    /// <summary>
    /// Loads a sprite from a file path with customizable parameters.
    /// </summary>
    /// <param name="FilePath">The path to the image file</param>
    /// <param name="PixelsPerUnit">Pixels per unit for the sprite (default: 100.0f)</param>
    /// <param name="spriteType">The sprite mesh type (default: Tight)</param>
    /// <returns>A new Sprite object, or null if the file couldn't be loaded</returns>
    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    /// <summary>
    /// Loads a Texture2D from a file path.
    /// </summary>
    /// <param name="FilePath">The path to the image file</param>
    /// <returns>A Texture2D object, or null if the file doesn't exist or couldn't be loaded</returns>
    public static Texture2D LoadTexture(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData))
            {
                return Tex2D;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a file exists in the appropriate documents directory for the current platform.
    /// </summary>
    /// <param name="filename">The name of the file to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    public static bool FileExists(string filename)
    {
        string path = PathForDocumentsFile(filename);
        return File.Exists(path);
    }
}
