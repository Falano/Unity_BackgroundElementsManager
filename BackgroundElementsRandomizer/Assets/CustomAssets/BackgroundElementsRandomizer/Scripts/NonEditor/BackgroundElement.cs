using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nolanfa.BackgroundElementsRandomizer
{
    public class BackgroundElement : MonoBehaviour
    {
        [SerializeField]
        BackgroundTypes _type;

        [SerializeField]
        bool _isAspectLocked;

        [SerializeField]
        bool _isRotationScaleLocked;

        [SerializeField]
        int _unwalkableColliderHeight;

        [SerializeField]
        bool _toBeInitialized = true;

        GameObject _placeholder;
        GameObject _prettyBg;
        MeshFilter _prettyMesh;
        Collider _prettyCollider;
        MeshRenderer _prettyRenderer;
        BoxCollider _unwalkableCollider;
        Vector3 _unwalkableColliderCenter = Vector3.zero;
        Vector3 _unwalkableColliderSize;
        Vector3 _baseScale;
        float _unwalkableColliderBaseScale;
        float _unwalkableColliderOffset;

        Vector3 currentRotation;

        #region Properties
        public BackgroundTypes Type
        {
            get
            {
                return _type;
            }
        }

        public bool IsAspectLocked
        {
            get
            {
                return _isAspectLocked;
            }
        }

        public bool IsRotationScaleLocked
        {
            get
            {
                return _isRotationScaleLocked;
            }
        }
        #endregion

        /// <summary>
        /// find all the background element's components;
        /// if already done, automatically skips it;
        /// but you can manually tell it to initialize it again anyway
        /// </summary>
        public void Initialize(bool verbose = true)
        {
            if (!_toBeInitialized)
            {
                return;
            }

            foreach (Transform child in transform)
            {
                if (child.tag == "EditorOnly")
                {
                    _placeholder = child.gameObject;
                }
                else
                {
                    _prettyBg = child.gameObject;
                }
            }

            _prettyMesh = _prettyBg.GetComponent<MeshFilter>();
            _prettyRenderer = _prettyBg.GetComponent<MeshRenderer>();
            if (BackgroundElementsManager.BackgroundTypesDictionary[Type].CanCollide)
            {
                _prettyCollider = _prettyBg.GetComponent<Collider>();
            }

            if (_placeholder == null || _prettyBg == null || _prettyMesh == null || (_prettyCollider == null && BackgroundElementsManager.BackgroundTypesDictionary[Type].CanCollide) || _prettyRenderer == null)
            {
                Debug.LogError("Element " + gameObject.name + " could not be fully initialized. Please check that it has two children, " +
                    "one of which is tagged 'EditorOnly' and holds a simple geometric mesh, " +
                    "and the other holds a MeshFilter, a Collider and a MeshRenderer.");

            }

            if (BackgroundElementsManager.BackgroundTypesDictionary[Type].ShouldBlockUpwards)
            {
                _unwalkableCollider = GetComponent<BoxCollider>();
                _unwalkableColliderBaseScale = _unwalkableCollider.size.y;
                _unwalkableColliderOffset = _unwalkableCollider.center.y;
            }

            _baseScale = _prettyBg.transform.localScale;

            _toBeInitialized = false;
        }

        public void ChangeMesh(Mesh mesh)
        {
            _prettyMesh.mesh = mesh;
            // set _prettyCollider if it's a mesh collider
            if (_prettyCollider as MeshCollider != null)
            {
                (_prettyCollider as MeshCollider).sharedMesh = mesh;
            }
        }

        public void ChangeScale(int scale)
        {
            _prettyBg.transform.localScale = _baseScale + (_baseScale * scale);
        }

        public void ChangeMat(Material mat)
        {
            _prettyRenderer.material = mat;
        }

        /// <summary>
        /// change rotation (on its Y axis) of the pretty element
        /// </summary>
        /// <param name="rotY">local rotation of the pretty element along the Y axis</param>
        public void ChangeRotation(int rotY)
        {
            currentRotation = transform.localRotation.eulerAngles;
            currentRotation.y = rotY;
            _prettyBg.transform.Rotate(currentRotation);
        }

        public BackgroundTypes GetBackgroundType()
        {
            return _type;
        }

        public void SetUnwalkableCollider()
        {
            if (_unwalkableCollider == null)
            {
                Debug.LogError("You're trying to set the unwalkable collider for " + gameObject.name + ", who doesn't have one, or at least doesn't know it.");
                return;
            }

            _unwalkableCollider.enabled = true;

            _unwalkableColliderCenter.y = (((float) _unwalkableColliderHeight / 2) * _unwalkableColliderBaseScale) - 1 +
                (_unwalkableColliderHeight == 1 ? _unwalkableColliderOffset + .5f : _unwalkableColliderOffset);
            _unwalkableCollider.center = _unwalkableColliderCenter;
            _unwalkableColliderSize = _unwalkableCollider.size;
            _unwalkableColliderSize.y = _unwalkableColliderHeight * _unwalkableColliderBaseScale;
            _unwalkableCollider.size = _unwalkableColliderSize;
        }

        public void DeactivateUnwalkableCollider()
        {
            if (_unwalkableCollider != null)
            {
                _unwalkableCollider.enabled = false;
            }
        }

        public bool CheckPrettyState()
        {
            if (_placeholder.activeSelf == _prettyBg.activeSelf)
            {
                Debug.LogError("Careful, both versions (pretty and geometric) of the element " + gameObject.name + "are active (or inactive). Only one should. You should activate or deactivate Pretty Elements in the editor's Background Elements Manager.");
            }
            return !_placeholder.activeSelf;
        }

        /// <summary>
        /// activate pretty element and hide geometric one
        /// </summary>
        /// <param name="isPretty"></param>
        public void ActivatePrettyBackground(bool isPretty = true)
        {
            _placeholder.SetActive(!isPretty);
            _prettyBg.SetActive(isPretty);
        }

        /// <summary>
        /// reset parameters and locks and the view to geometric
        /// </summary>
        public void Reset()
        {
            ActivatePrettyBackground(false);
            ChangeRotation(0);
            ChangeScale(0);
            _isAspectLocked = false;
            _isRotationScaleLocked = false;
            _toBeInitialized = true;
        }
    }
}
