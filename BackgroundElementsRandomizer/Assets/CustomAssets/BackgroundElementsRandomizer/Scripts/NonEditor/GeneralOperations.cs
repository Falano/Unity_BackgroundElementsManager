using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Nolanfa
{
    public static class GeneralOperations
    {

        public static void CreateFolderIfEmpty(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf(Path.AltDirectorySeparatorChar);
                string[] splitPath = new string[2];
                splitPath[0] = path.Substring(0, lastSlash);
                splitPath[1] = path.Substring(lastSlash + 1);

                AssetDatabase.CreateFolder(splitPath[0], splitPath[1]);
            }
        }
    }
}