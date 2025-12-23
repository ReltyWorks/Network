using Game.Lobbies;
using System;
using TMPro;
using UnityEngine;

namespace Game.Net.Client.Lobbies
{
    public class UserInLobbySlot : MonoBehaviour
    {
        public UserInLobbyInfo cachedUserInfo { get; private set; }

        [SerializeField] TMP_Text _userId;
        [SerializeField] TMP_Text _isReady;


        public void Refresh(UserInLobbyInfo userInfo)
        {
            cachedUserInfo = userInfo;
            _userId.text = userInfo.UserId;

            if (userInfo.CustomProperties.TryGetValue("IsReady", out string value))
            {
                _isReady.enabled = Convert.ToBoolean(value);
            }
            else
            {
                _isReady.enabled = false;
            }
        }

        public void Clear()
        {
            cachedUserInfo = null;
            _userId.text = string.Empty;
            _isReady.enabled = false;
        }
    }
}