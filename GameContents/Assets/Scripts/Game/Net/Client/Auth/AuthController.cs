using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NetworkStatus;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Game.Net.Client.Auth
{
    public class AuthController : MonoBehaviour
    {
        public string jwt => _jwt;
        public bool isLoggedIn => string.IsNullOrEmpty(_jwt) == false;

        const string BASE_URL = "http://localhost:5083";
        string _jwt;
        string _userId;

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
                        var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                        _jwt = response.Jwt;
                        _userId = response.UserId;
                        NetworkBlackboard.userId = _userId.ToString();
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
                yield return SceneManager.LoadSceneAsync("Lobbies", LoadSceneMode.Additive);
                yield return SceneManager.UnloadSceneAsync("Auth");
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
            public string UserId;
            public string Nickname;
        }

        [Serializable]
        public class DeleteRequest
        {
            public string UserId;
            public string Id;
            public string Pw;
        }

        // JWT 사용예시 (User API 등 에서 DeleteUser 와같이 소유자 권한이 필요한 요청에 사용)
        IEnumerator C_DeleteUser(string userId, string id, string pw)
        {
            var deleteDto = new DeleteRequest { UserId = userId, Id = id, Pw = pw };
            string json = JsonUtility.ToJson(deleteDto);
            bool success = false;

            using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/user/delete{Guid.Parse(userId)}", "DELETE"))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_jwt}"); // 요거 한줄 추가해야함 
                byte[] body = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                }
            }
        }
    }
}
