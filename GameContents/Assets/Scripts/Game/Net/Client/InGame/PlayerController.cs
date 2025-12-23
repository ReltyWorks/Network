using UnityEngine;
using Unity.Netcode;
using Game.Net.Shared;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Game.Net.Client.InGame
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] LayerMask _interactableMask;
        [SerializeField] float _interactionRange = 0.5f;
        [SerializeField] private float _speed = 2.0f;
        [SerializeField] private float _rotateSpeed = 360f;

        [Header("Pickable")]
        [SerializeField] private MeshFilter _pickableMeshFilter;
        [SerializeField] private MeshRenderer _pickableMeshRenderer;
        private Vector3 _cachedMoveInput;
        private InGameInputActions _inputActions;
        private Rigidbody _rigidbody;
        private CinemachineCamera _cinemachineCamera;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
            _inputActions = new InGameInputActions();
        }

        private void OnEnable()
        {
            InGameManager.singleton.state.OnValueChanged += OnInGameStateChanged;
        }

        private void OnDisable()
        {
            InGameManager.singleton.state.OnValueChanged -= OnInGameStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                _cinemachineCamera.gameObject.SetActive(true);
                _inputActions.Player.Move.performed += OnMove;
                _inputActions.Player.Move.canceled += OnStop;
                _inputActions.Player.Interact.started += OnInteract;
                _inputActions.Player.Enable();
            }
            else
            {
                _cinemachineCamera.gameObject.SetActive(false);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsOwner)
            {
                _cinemachineCamera.gameObject.SetActive(false);
                _inputActions.Player.Move.performed -= OnMove;
                _inputActions.Player.Move.canceled -= OnStop;
                _inputActions.Player.Interact.started -= OnInteract;
                _inputActions.Player.Disable();
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            Move();
            SmoothRotate();
        }

        void Move()
        {
            Vector3 velocity = _cachedMoveInput.normalized * _speed;
            _rigidbody.MovePosition(_rigidbody.position + velocity * Time.fixedDeltaTime);
        }

        void SmoothRotate()
        {
            if (_cachedMoveInput.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(_cachedMoveInput);
            _rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, _rotateSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime);
        }

        void OnInGameStateChanged(InGameState oldState, InGameState newState)
        {
            switch (newState)
            {
                case InGameState.None:
                    break;
                case InGameState.WaitUntilAllClientsAreConntected:
                    break;
                case InGameState.StartContent:
                    OnStartContent();
                    break;
                case InGameState.WaitUntilContentFinished:
                    break;
                default:
                    break;
            }
        }

        void OnStartContent()
        {
            _inputActions.Enable();
        }

        void OnMove(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            _cachedMoveInput = new Vector3(value.x, 0f, value.y);
        }

        void OnStop(InputAction.CallbackContext context)
        {
            _cachedMoveInput = Vector3.zero;
        }

        void OnInteract(InputAction.CallbackContext context)
        {
            // 정확한 판정이 필요하면 아래 로직 전부다 서버에서 하는게 맞다 
            Collider[] colliders = Physics.OverlapSphere(_rigidbody.position, _interactionRange, _interactableMask);
            Collider closest = null;
            float closestSqrMagnitude = float.MaxValue;

            // 가장 가까운 것 찾기
            foreach (var collider in colliders)
            {
                float sqr = (_rigidbody.position - collider.transform.position).sqrMagnitude;

                if (sqr < closestSqrMagnitude)
                {
                    closest = collider;
                    closestSqrMagnitude = sqr;
                }
            }

            if (closest != null)
            {
                NetworkObject pickable = closest.gameObject.GetComponent<NetworkObject>();
                PickUpRequest request = new PickUpRequest
                {
                    InteractableObjectId = pickable.NetworkObjectId,
                    InteractorObjectId = NetworkObjectId
                };
                PickUpRequestRpc(request);
            }
        }

        [Rpc(SendTo.Server)]
        void PickUpRequestRpc(PickUpRequest request)
        {
            bool success = false;
            ulong interactableObjectId = 0;
            ulong interactorObjectId = 0;

            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(request.InteractableObjectId, out NetworkObject networkObject))
            {
                interactableObjectId = networkObject.NetworkObjectId;
                Pickable pickable = networkObject.GetComponent<Pickable>();

                if (pickable.TryPickUp(request.InteractorObjectId))
                    success = true;

                interactorObjectId = pickable.pickerObjectId;
            }

            PickUpResponse response = new PickUpResponse
            {
                Success = success,
                InteractableObjectId = interactableObjectId,
                InteractorObjectId = interactorObjectId
            };
            PickUpResponseRpc(response);
        }

        [Rpc(SendTo.NotServer)]
        void PickUpResponseRpc(PickUpResponse response)
        {
            if (response.Success)
            {
                if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(response.InteractableObjectId, out NetworkObject pickableObject) &&
                    NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(response.InteractorObjectId, out NetworkObject pickerObject))
                {
                    Pickable pickable = pickableObject.GetComponent<Pickable>();
                    PlayerController picker = pickerObject.GetComponent<PlayerController>();
                    picker._pickableMeshFilter.mesh = pickable.mesh;
                    picker._pickableMeshRenderer.materials = pickable.materials;
                    picker._pickableMeshFilter.transform.localScale = pickable.transform.localScale;
                }
            }
        }

        /// <summary>
        /// PickUp RPC Client Message
        /// </summary>
        struct PickUpRequest : INetworkSerializable
        {
            public ulong InteractableObjectId;
            public ulong InteractorObjectId;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref InteractableObjectId);
                serializer.SerializeValue(ref InteractorObjectId);
            }
        }

        struct PickUpResponse : INetworkSerializable
        {
            public bool Success;
            public ulong InteractableObjectId;
            public ulong InteractorObjectId;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Success);
                serializer.SerializeValue(ref InteractableObjectId);
                serializer.SerializeValue(ref InteractorObjectId);
            }
        }
    }
}