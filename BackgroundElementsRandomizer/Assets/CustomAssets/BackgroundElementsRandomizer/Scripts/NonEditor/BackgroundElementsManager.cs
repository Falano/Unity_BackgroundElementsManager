using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

// documentation on creating menu items: https://blog.redbluegames.com/guide-to-extending-unity-editors-menus-b2de47a746db
// documentation on creating buttons in editor scripts: https://learn.unity.com/tutorial/editor-scripting?language=en#5c7f8528edbc2a002053b5f8
namespace Nolanfa.BackgroundElementsRandomizer
{
    public static class BackgroundElementsManager
    {
        public static BackgroundElement[] BackgroundElements;

        public static Dictionary<BackgroundTypes, BackgroundType> BackgroundTypesDictionary;

        public static List<string> AlternateMaterialsFoldersPaths = new List<string>();
        public static char Separator { get { return Path.AltDirectorySeparatorChar; } }
        public static bool isDebugging = false;



        // to create a new type of background:
        // - create a new background type in the BackgroundTypes enum
        // - create a new scriptable object of type BackgroundType
        // (Assets > Create > Scriptable Object > Background Type)
        // and fill it out appropriately
        // - automatically create a prefab using the CreatePrefabs button ( TODO: button)

        
        
        [MenuItem("CustomScripts/BackgroundElements/InitializeAll", false, 0)]
        public static void InitializeAll()
        {
            InitializeManager();
            FindAllElements();
            InitializeAllElements();
        }



        /// <summary>
        /// randomize background elements' pretty look
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/RandomizeBackground", false, 0)]
        public static void RandomizeBackground()
        {
            BackgroundTypes currentType;
            int meshNumber;
            int matNumber;
            foreach (BackgroundElement element in BackgroundElements)
            {
                // start showing pretty background elements
                element.ActivatePrettyBackground();

                if (element.IsAspectLocked || BackgroundTypesDictionary[element.GetBackgroundType()].IsLocked)
                {
                    continue;
                }

                currentType = element.GetBackgroundType();
                // choose random mesh
                meshNumber = Random.Range(0, BackgroundTypesDictionary[currentType].Meshes.Count);
                element.ChangeMesh(BackgroundTypesDictionary[currentType].Meshes[meshNumber]);
                // choose random material
                matNumber = Random.Range(0, BackgroundTypesDictionary[currentType].NewMaterials.Count);
                element.ChangeMat(BackgroundTypesDictionary[currentType].NewMaterials[matNumber]);

                if (element.IsRotationScaleLocked)
                {
                    continue;
                }
                // if allowed, choose random rotation
                switch (BackgroundTypesDictionary[currentType].RotationalAbility)
                {
                    case RotationalAbility.None:
                        break;
                    case RotationalAbility.By180Degrees:
                        element.ChangeRotation(RandomRotationValue(180));
                        break;
                    case RotationalAbility.By90Degrees:
                        element.ChangeRotation(RandomRotationValue(90));
                        break;
                    case RotationalAbility.By30Degrees:
                        element.ChangeRotation(RandomRotationValue(30));
                        break;
                    case RotationalAbility.Anything:
                        element.ChangeRotation(RandomRotationValue(1));
                        break;
                }

                // choose random scale
                if (BackgroundTypesDictionary[currentType].Scalability != 0)
                {
                    element.ChangeScale(Random.Range(-BackgroundTypesDictionary[currentType].Scalability / 100, BackgroundTypesDictionary[currentType].Scalability / 100));
                }
            }
        }



        /// <summary>
        /// switch between geometric and pretty views
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/Switch view", false, 0)]
        public static void SwitchView()
        {
            foreach (BackgroundElement element in BackgroundElements)
            {
                element.ActivatePrettyBackground(!element.CheckPrettyState());
            }
        }

        /*
        [MenuItem("CustomScripts/BackgroundElements/Reset All", true, 0)]
        public static void ResetAllConfirm()
        {
            Rect window = GUI.Window(0, window, ResetAllDraw, "Reset All");
        }

    */
        
        
        
        // TODO: add a confirmation window
        [MenuItem("CustomScripts/BackgroundElements/Reset All", false, 0)]
        public static void ResetAll()
        {
            RemoveAlternateMaterials();
            ResetElements();
            ResetManager();
        }



        // TODO: everything
        [MenuItem("CustomScripts/BackgroundElements/Information", false, 0)]
        public static void BackgroundElementsInfo()
        {
            // has manager been initialized? when?
            // if so, which types does it know?
            // have elements been initialized? when? how many?
            // have elements been randomized? When? how many?
            // number of geometric elements?
            // number of pretty elements?
            // some mini-tuto shoud be available here.
        }



        /// <summary>
        /// update a list of all different background types available
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/More/InitializeManager", false, 1)]
        public static void InitializeManager()
        {
            // get all background types automatically

            // this is faster (if harder to read) than Resources.FindObjectsOfTypeAll
            // because it only searches a few folders instead of the whole project

            string[] typesPaths = AssetDatabase.FindAssets("t: BackgroundType", new[] {GeneralInformation.Paths.TypesFolder });
            foreach (string path in typesPaths)
            {
                BackgroundType type = AssetDatabase.LoadAssetAtPath<BackgroundType>(AssetDatabase.GUIDToAssetPath(path));
                if(BackgroundTypesDictionary == null)
                {
                    BackgroundTypesDictionary = new Dictionary<BackgroundTypes, BackgroundType>();
                }
                if (!BackgroundTypesDictionary.ContainsValue(type))
                {
                    BackgroundTypesDictionary.Add(type.Type, type);

                    // for each background type, get the meshes from the mesh folder

                    // since the script adds all files that contain the type's name,
                    // it is case-sensitive to lower the risk of errors with too-similar names
                    string[] meshesPaths = AssetDatabase.FindAssets(
                        type.FindMeshesByName?
                        "t: Mesh name: " + type.Type.ToString():
                        "t: Mesh"
                        , new[] { type.MeshesFolder });

                    foreach (string meshPath in meshesPaths)
                    {
                        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                        if (!type.Meshes.Contains(mesh))
                        {
                            type.Meshes.Add(mesh);
                        }
                    }
                }
            }


            // create x versions of each material per type, ensure it has the right scale
            // and offset it x different ways
            // we need to use Database.CreateAsset so there is a path on disk to the new materials;
            // otherwise the ScriptableObject trying to link to the new materials throws an error
            Vector2 tiling;
            Material newMat;
            string matPath;
            string matFolder;
            foreach(BackgroundType type in BackgroundTypesDictionary.Values)
            {
                if (!type.ShouldOffsetTexture)
                {
                    type.NewMaterials = type.Materials;
                }

                else if (type.NewMaterials == null || type.NewMaterials.Count == 0)
                {
                    // we find the folder containing the materials
                    // and create a subfolder for the variations
                    if(type.Materials == null || type.Materials.Count == 0)
                    {
                        Debug.LogError("You forgot to add materials to the element type " + type.Type + "!");
                        continue;
                    }
                    matPath = AssetDatabase.GetAssetPath(type.Materials[0]);
                    int lastSlash = matPath.LastIndexOf(Separator);
                    matPath = matPath.Remove(lastSlash);
                    matFolder = matPath + Separator + GeneralInformation.Paths.AlternativeMaterialsFolderName;
                    GeneralOperations.CreateFolderIfEmpty(matFolder);
                    // for cleaning up purposes later
                    if (!AlternateMaterialsFoldersPaths.Contains(matFolder))
                    {
                        AlternateMaterialsFoldersPaths.Add(matFolder);
                        if (isDebugging)
                        {
                            Debug.Log("created folder " + matFolder);
                        }
                    }

                    tiling = type.TextureTiling;
                    foreach (Material mat in type.Materials)
                    {
                        mat.mainTextureScale = tiling;
                        for (int i = 0; i < 3; i++)
                        {
                            newMat = new Material(mat)
                            {
                                mainTextureOffset = new Vector2(Random.Range((float)0, (float)1), Random.Range((float)0, (float)1))
                            };
                            AssetDatabase.CreateAsset(newMat, matPath + Separator + GeneralInformation.Paths.AlternativeMaterialsFolderName + Separator + (mat.name + "_" + type.name + "_v" + i + ".mat"));
                            type.NewMaterials.Add(newMat);
                            if (isDebugging)
                            {
                                Debug.Log("added material " + newMat + " to the list.");
                            }
                        }
                        //and then reset it so as not to commit useless stuff each time you make one small change
                        mat.mainTextureScale = Vector2.one;
                    }
                    if (isDebugging)
                    {
                        Debug.Log("Just treated " + type.name + " background type.");
                    }
                }
            }
        }



        public static void CreateBackgroundElementPrefabs()
        {
            string prefabPath = GeneralInformation.Paths.PrefabsFolder;
            GeneralOperations.CreateFolderIfEmpty(prefabPath);
            // create a prefab for each type
            foreach(KeyValuePair<BackgroundTypes, BackgroundType> pair in BackgroundTypesDictionary)
            {
                // create parent empty object
                GameObject gameObject = new GameObject(pair.Key.ToString());
                gameObject.AddComponent<BackgroundElement>();
                if (pair.Value.ShouldBlockUpwards)
                {
                    gameObject.AddComponent<BoxCollider>();
                }
                // create geometric placeholder child
                CreateBasicObject(name: "Geometric", parent: gameObject.transform, tag: "EditorOnly", mesh: pair.Value.geometricMesh);
                // create pretty inGame child
                GameObject prettyChild = CreateBasicObject(name: "Pretty", parent: gameObject.transform, mesh: pair.Value.Meshes[0]);
                if (pair.Value.CanCollide)
                {
                    MeshCollider collider = prettyChild.AddComponent<MeshCollider>();
                    // TODO: add an option for a non-automatic collider?
                    collider.convex = true;
                }

                // make a prefab out of it
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            }
        }



        /// <summary>
        /// update list of background elements present in the scene
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/More/Find All Elements", false, 1)]
        public static void FindAllElements()
        {            
            BackgroundElements = Object.FindObjectsOfType<BackgroundElement>();
        }



        /// <summary> 
        /// initialize all background elements found so that they find the components they need to work
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/More/Initialize Elements", false, 1)]
        public static void InitializeAllElements()
        {
            foreach (BackgroundElement element in BackgroundElements)
            {
                element.Initialize(verbose: false);

                if(BackgroundTypesDictionary[element.Type].ShouldBlockUpwards)
                {
                    element.SetUnwalkableCollider();
                }
                else
                {
                    element.DeactivateUnwalkableCollider();
                }
            }
        }



        /// <summary>
        /// reset each element's parameters and locks and the view to geometric
        /// </summary>
        // TODO: add a confirmation window
        [MenuItem("CustomScripts/BackgroundElements/More/Reset Background", false, 50)]
        public static void ResetElements()
        {
            foreach (BackgroundElement element in BackgroundElements)
            {
                element.Reset();
            }
        }



        /// <summary>
        /// remove alternate materials created to have several different texture offsets
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/More/Remove Alternate Materials", false, 50)]
        public static void RemoveAlternateMaterials()
        {
            foreach(string path in AlternateMaterialsFoldersPaths)
            {
                Directory.Delete(path, true);
            }
            AssetDatabase.Refresh();
            AlternateMaterialsFoldersPaths.Clear();
            foreach (BackgroundType type in BackgroundTypesDictionary.Values)
            {
                type.NewMaterials.Clear();
            }
        }



        /// <summary>
        /// reset manager's types list and each type info to default empty state
        /// </summary>
        [MenuItem("CustomScripts/BackgroundElements/More/ResetManager", false, 50)]
        public static void ResetManager()
        {
            BackgroundTypesDictionary.Clear();
        }

        // TODO: progress bar

        
        
        /// <summary>
        /// returns a random rotation taken from each -step- degrees; 
        /// if it can only turn 90 degrees, il will return either 
        /// 0, 90, 180 or 240 degrees
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int RandomRotationValue(int step)
        {
            return Random.Range(0, (360 / step)) * step;
        }



        /// <summary>
        /// create game object with just a mesh (mesh filter and mesh renderer)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static GameObject CreateBasicObject(string name, Mesh mesh, Transform parent = null, string tag = "")
        {
            GameObject gameObject = new GameObject(name);
            if(parent != null)
            {
                gameObject.transform.SetParent(gameObject.transform);
            }
            if (tag != "")
            {
                gameObject.tag = tag;
            }
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            gameObject.AddComponent<MeshRenderer>();
            return gameObject;
        }
    }
}