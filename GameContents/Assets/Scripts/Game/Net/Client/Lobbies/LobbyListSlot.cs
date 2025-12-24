using Game.Lobbies;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Net.Client.Lobbies
{
    public class LobbyListSlot : MonoBehaviour
    {
        public LobbyInfo cachedLobbyInfo { get; private set; }

        [SerializeField] TMP_Text _lobbyId;
        [SerializeField] TMP_Text _maxUser;
        [SerializeField] TMP_Text _numUser;
        [SerializeField] TMP_Text _title;
        Button _button;
        Outline _isSelected;

        public event Action onClick;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => onClick?.Invoke());
            _isSelected = GetComponent<Outline>();
        }

        public void Refresh(LobbyInfo lobbyInfo)
        {
            cachedLobbyInfo = lobbyInfo;
            _lobbyId.text = lobbyInfo.LobbyId.ToString();
            _maxUser.text = lobbyInfo.MaxClient.ToString();
            _numUser.text = lobbyInfo.NumClient.ToString();

            if (lobbyInfo.CustomProperties.TryGetValue("Title", out string title))
            {
                _title.text = title;
            }
            else
            {
                _title.text = string.Empty;
            }
        }

        public void Select(bool selected)
        {
            _isSelected.enabled = selected;
        }
    }
}