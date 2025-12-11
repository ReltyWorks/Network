using UnityEngine;
using TMPro;
using System;

namespace Game.Net.Chat
{
    public class ChatView : MonoBehaviour
    {
        [Header("Logs")]
        [SerializeField] RectTransform _content;
        [SerializeField] TextMeshProUGUI _history;

        [Header("Message")]
        [SerializeField] TMP_InputField _text;


        public event Action<string> onTextSubmit;

        private void Awake()
        {
            _text.onSubmit.AddListener(value => onTextSubmit?.Invoke(value));
        }

        public void SetHistory(string history)
        {
            _history.text = history;
        }

        public void ClearText()
        {
            _text.text = string.Empty;
        }
    }
}