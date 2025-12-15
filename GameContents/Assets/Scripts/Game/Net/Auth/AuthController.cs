using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Net.Auth
{
    public class AuthController : MonoBehaviour
    {
        public string jwt => _jwt;
        public bool isLoggedIn => string.IsNullOrEmpty(_jwt) == false;

        const string BASE_URL = "http://localhost:5083";
        string _jwt;

        [SerializeField] AuthView _view;

        private void OnEnable()
        {
            _view.onConfirm += Login;
        }

        private void OnDisable()
        {
            _view.onConfirm -= Login;
        }

        void Login(string id, string pw)
        {            
            StartCoroutine(C_Login(id, pw));
        }

        IEnumerator C_Login(string id, string pw)
        {
            _view.SetLoginInteractables(false);
            var loginDto = new LoginRequest { Id = id, Pw = pw };
            string json = JsonUtility.ToJson(loginDto);
            bool success = false;

            using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/auth/login", "POST"))
            {
                byte[] body = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                        _jwt = response.Jwt;
                        success = true;
                        _view.ShowAlertPanel("Logged In !");
                    }
                    catch (Exception e)
                    {
                        _view.ShowAlertPanel("Wrong response");
                    }
                }
                else
                {
                    _view.ShowAlertPanel("Failed to log in");
                }
            }

            yield return new WaitForSeconds(2f);

            if (success)
            {
                // TODO : Move next scene
            }
            else
            {
                _view.HideAlertPanel();
                _view.SetLoginInteractables(true);
            }
        }

        [Serializable]
        public class LoginRequest
        {
            public string Id;
            public string Pw;
        }

        [Serializable]
        public class LoginResponse
        {
            public string Jwt;
        }
    }
}
