using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Net.Auth
{
    public class AuthView : MonoBehaviour
    {
        [SerializeField] TMP_InputField _id;
        [SerializeField] TMP_InputField _pw;
        [SerializeField] Button _confirm;

        public event Action<string, string> onConfirm;

        private void Awake()
        {
            _confirm.onClick.AddListener(() => onConfirm(_id.text, _pw.text));
        }
    }
}