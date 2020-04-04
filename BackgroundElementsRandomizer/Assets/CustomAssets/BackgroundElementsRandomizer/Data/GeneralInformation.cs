using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Nolanfa
{
    public static class GeneralInformation
    {
        public static class Paths
        {

            private static char _correctSeparator;

            public static string TypesFolder = "Assets/CustomAssets/BackgroundElementsRandomizer/Data/Tree.asset";
            public static string PrefabsFolder;
            public static string AlternativeMaterialsFolderName = "alternativeVersions";



            public static char CorrectSeparator
            {
                get {
                    if(_correctSeparator == char.MinValue)
                    {
                        _correctSeparator = TypesFolder.LastIndexOf(Path.DirectorySeparatorChar) > 0 ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar;
                    }
                    return _correctSeparator;
                }
            }
        }
    }
}
