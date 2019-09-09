using UnityEngine;

namespace Example.Photon.CloudAnchor
{
    [RequireComponent(typeof(GoogleARCore.Examples.Common.DetectedPlaneVisualizer))]
    public class PlaneCollider : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            if (_meshCollider == null)
            {
                _meshCollider = this.gameObject.AddComponent<MeshCollider>();
            }

            _meshCollider.sharedMesh = _meshFilter.sharedMesh;
        }
    }
}
