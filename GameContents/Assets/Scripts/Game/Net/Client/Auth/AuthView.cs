using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Net.Client.Auth
{
    public class AuthView : MonoBehaviour
    {
        [SerializeField] TMP_InputField _id;
        [SerializeField] TMP_InputField _pw;
        [SerializeField] Button _confirm;
        [SerializeField] GameObject _alertPanel;

        public event Action<string, string> onConfirm;

        private void Awake()
        {
            _confirm.onClick.AddListener(() => onConfirm?.Invoke(_id.text, _pw.text));
        }

        public void SetLoginInteractables(bool interactable)
        {
            _id.interactable = interactable;
            _pw.interactable = interactable;
            _confirm.interactable = interactable;
        }

        public void ShowAlertPanel(string content)
        {
            _alertPanel.GetComponentInChildren<TextMeshProUGUI>().text = content;
            _alertPanel.SetActive(true);
        }

        public void HideAlertPanel()
        {
            _alertPanel.SetActive(false);
        }
    }
}