using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Nolanfa
{
    public static class GeneralOperations
    {

        public static void CreateFolderIfEmpty(string path, int depth = 0, string origFolder = "", string origPath = "")
        {
            string errorMessage = "You're trying to create the folder " + origFolder + " at " + origPath + ", but " + origPath + "doesn't exist.";
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf(Path.AltDirectorySeparatorChar);
                if (lastSlash == -1)
                {
                    if(depth == 0)
                    {
                        Debug.LogError("Trying to create folder " + path + "; invalid path.");
                    }
                    else
                    {
                        Debug.LogError(errorMessage);
                    }
                    return;
                }
                string[] splitPath = new string[2];
                splitPath[0] = path.Substring(0, lastSlash);
                splitPath[1] = path.Substring(lastSlash + 1);
                if (!AssetDatabase.IsValidFolder(splitPath[0]))
                {
                    if(depth > 5)
                    {
                        Debug.LogError(errorMessage);
                        return;
                    }
                    else
                    {
                        if (depth == 0)
                        {
                            CreateFolderIfEmpty(splitPath[0], depth + 1, splitPath[1], splitPath[0]);
                        }
                        else
                        {
                            CreateFolderIfEmpty(splitPath[0], depth + 1, origFolder, origPath);
                        }
                    }
                }
                AssetDatabase.CreateFolder(splitPath[0], splitPath[1]);
            }
        }
    }
}