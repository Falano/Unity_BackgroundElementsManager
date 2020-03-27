using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a scriptable object can be changed from within the editor
// and is useful for holding data that won't be changed in-game
// mostly used for level design, I think
// like to switch between difficulty presets for example
// or graphical themes
namespace Nolanfa.BackgroundElementsRandomizer
{
    [CreateAssetMenu(fileName = "BackgroundType", menuName = "ScriptableObjects/Background Type", order = 1)]
    public class BackgroundType : ScriptableObject
    {
        public BackgroundTypes Type;
        public RotationalAbility RotationalAbility;
        [Tooltip("Scalability in % either way; with a scalability of 20, an object can be from 80% to 120% big")]
        public int Scalability;
        public List<Mesh> Meshes;
        public List<Material> Materials;
        [Tooltip("Different versions of the materials created through script")]
        public List<Material> NewMaterials;
        public Vector2 TextureTiling;
        public bool CanCollide;
        public bool ShouldOffsetTexture;
        public bool ShouldBlockUpwards;
    }

    public enum BackgroundTypes
    {
        House,
        Tree
    };

    public enum RotationalAbility
    {
        None,
        By180Degrees,
        By90Degrees,
        By30Degrees,
        Anything
    }
}