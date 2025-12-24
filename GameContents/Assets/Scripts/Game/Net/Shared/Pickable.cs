using Unity.Netcode;
using UnityEngine;

namespace Game.Net.Shared
{
    public class Pickable : NetworkBehaviour
    {
        public Mesh mesh => _mesh;
        public Material[] materials => _renderer.materials;
        public ulong pickerObjectId => _pickerObjectId;

        private ulong _pickerObjectId;
        private Rigidbody _rigidbody;
        private Collider[] _colliders;
        private Renderer _renderer;
        private Mesh _mesh;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            _renderer = GetComponentInChildren<Renderer>();
            _mesh = GetComponentInChildren<MeshFilter>().mesh;
        }

        public bool TryPickUp(ulong pickerObjectId)
        {
            if (_pickerObjectId > 0)
                return false;

            SetPhysics(false);
            HideRpc();
            _pickerObjectId = pickerObjectId;
            return true;
        }

        private void SetPhysics(bool active)
        {
            _rigidbody.isKinematic = !active;

            foreach (var collider in _colliders)
            {
                collider.enabled = active;
            }
        }

        private void SetRenderer(bool active)
        {
            _renderer.enabled = active;
        }

        [Rpc(SendTo.NotServer)]
        void HideRpc()
        {
            SetRenderer(false);
        }
    }
}