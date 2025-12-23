using Game.Lobbies;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Net.Client.Lobbies
{
    public class LobbiesView : MonoBehaviour
    {
        [Header("Canvas - BeforeJoinLobby")]
        [SerializeField] Canvas _beforeJoinLobby;
        [SerializeField] RectTransform _lobbyList;
        [SerializeField] Button _create;
        [SerializeField] Button _join;
        [SerializeField] Button _refresh;
        LobbyListSlot _lobbyListSlot;
        LobbyListSlot _selectedLobbyListSlot;
        List<LobbyListSlot> _lobbyListSlots = new(); // TODO : Reserving

        [Header("Canvas - CreateLobbySettings")]
        [SerializeField] Canvas _createLobbySettings;
        [SerializeField] TMP_InputField _title;
        [SerializeField] TMP_InputField _maxUser;
        [SerializeField] Button _confirm;
        [SerializeField] Button _close;

        [Header("Canvas - AfterJoinLobby")]
        [SerializeField] Canvas _afterJoinLobby;
        [SerializeField] Button _start;
        [SerializeField] Button _ready;
        [SerializeField] Button _leave;
        [SerializeField] RectTransform _users;
        List<UserInLobbySlot> _userInLobbySlots = new(4);

        public event Action<string, int> onCreateLobbyConfirm;
        public event Action<int> onJoinButtonClicked;
        public event Action onRefreshButtonClicked;
        public event Action onStartClicked;
        public event Action onReadyClicked;
        public event Action onLeaveClicked;

        private void Awake()
        {
            _lobbyListSlot = _lobbyList.GetChild(0).GetComponent<LobbyListSlot>();
            _lobbyListSlot.gameObject.SetActive(false);

            for (int i = 0; i < 4; i++)
                _userInLobbySlots.Add(_users.GetChild(i).GetComponent<UserInLobbySlot>());

            _create.onClick.AddListener(() => _createLobbySettings.enabled = true);
            _close.onClick.AddListener(() => _createLobbySettings.enabled = false);
            _confirm.onClick.AddListener(() =>
            {
                if (_title.text.Length == 0)
                    return;

                int maxUser = Convert.ToInt32(_maxUser.text);

                if (maxUser <= 0)
                    return;

                onCreateLobbyConfirm?.Invoke(_title.text, maxUser);
                _createLobbySettings.enabled = false;
            });
            _join.onClick.AddListener(() =>
            {
                if (_selectedLobbyListSlot == null)
                {
                    _join.interactable = false;
                    // TODO : 로비가 존재하지 않는다는 내용의 팝업 알림
                    return;
                }

                onJoinButtonClicked?.Invoke(_selectedLobbyListSlot.cachedLobbyInfo.LobbyId);
            });
            _refresh.onClick.AddListener(() => onRefreshButtonClicked?.Invoke());
            _start.onClick.AddListener(() => onStartClicked?.Invoke());
            _ready.onClick.AddListener(() => onReadyClicked?.Invoke());
            _leave.onClick.AddListener(() => onLeaveClicked?.Invoke());
        }

        public void SetBeforeJoinLobbyCanvas(bool enable)
        {
            _beforeJoinLobby.enabled = enable;
        }

        public void SetAfterJoinLobbyCanvas(bool enable)
        {
            _afterJoinLobby.enabled = enable;
        }

        public void OnGetLobbyList(IEnumerable<LobbyInfo> lobbyInfos)
        {
            _selectedLobbyListSlot?.Select(false);
            _selectedLobbyListSlot = null;
            _join.interactable = false;

            // 기존슬롯 파괴
            for (int i = _lobbyListSlots.Count - 1; i >= 0; i--)
            {
                Destroy(_lobbyListSlots[i].gameObject);
                _lobbyListSlots.RemoveAt(i);
            }

            foreach (var lobbyInfo in lobbyInfos)
            {
                LobbyListSlot slot = Instantiate(_lobbyListSlot, _lobbyList);
                slot.gameObject.SetActive(true);
                slot.Refresh(lobbyInfo);
                slot.onClick += () =>
                {
                    _selectedLobbyListSlot?.Select(false); // 이전에 선택된거 있으면 선택해제
                    _selectedLobbyListSlot = slot; // 선택슬롯 갱신
                    _selectedLobbyListSlot.Select(true); // 선택
                    _join.interactable = true;
                };
                _lobbyListSlots.Add(slot);
            }
        }

        public void RefreshUserSlots(IEnumerable<UserInLobbyInfo> userInfos)
        {
            using (IEnumerator<UserInLobbySlot> eUserSlot = _userInLobbySlots.GetEnumerator())
            using (IEnumerator<UserInLobbyInfo> eUserInfo = userInfos.GetEnumerator())
            {
                while (eUserSlot.MoveNext())
                {
                    if (eUserInfo.MoveNext())
                    {
                        eUserSlot.Current.Refresh(eUserInfo.Current);
                    }
                    else
                    {
                        eUserSlot.Current.Clear();
                    }
                }
            }
        }
    }
}