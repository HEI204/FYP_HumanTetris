#if UNITY_TMPRO && UNITY_UI

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Threading;

namespace FlexalonCopilot.Editor
{
    [Serializable]
    internal class PromptResponseData
    {
        public string cmd;
        public string id;
        public string a;
        public string b;
        public string c;
    }

    [Serializable]
    internal class SignupRequest
    {
        public string email;
    }

    [Serializable]
    internal class LoginRequest
    {
        public string email;
        public string code;
    }

    [Serializable]
    internal class LoginResponse
    {
        public string token;
    }

    [Serializable]
    internal class GetUserRequest
    {
        public string token;
    }

    [Serializable]
    internal class GetUserResponse
    {
        public string email;
        public string subType;
        public string subEndDate;
        public DateTime subEndDateTime;
    }

    [Serializable]
    internal class PromptRequest
    {
        public string version;
        public string token;
        public string id;
        public string previousId;
        public string prompt;
        public PromptContext context;
    }

    [Serializable]
    internal class PromptRepeatRequest
    {
        public string token;
        public string id;
        public string previousId;
        public PromptContext context;
    }

    [Serializable]
    internal class PromptTestRequest // TODO: Move to test assembly
    {
        public PromptContext context;
        public string gptResponse;
    }

    [Serializable]
    internal class FeedbackRequest
    {
        public string token;
        public string id;
        public int vote;
        public string feedback;
    }

    [Serializable]
    internal class UpdateUserRequest
    {
        public string token;
        public string firstName;
        public string lastName;
        public string profession;
        public string company;
        public string usage;
        public bool marketing;
    }

    [Serializable]
    internal class GetVersionResponse
    {
        public string version;
        public string downloadLink;
    }

    internal class Api
    {
        private static Api _instance;
        public static Api Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Api();
                }

                return _instance;
            }
        }

        private GetUserResponse _userInfo;
        public GetUserResponse UserInfo => _userInfo;

        public event Action LoginStateChanged;

        private string _token;
        public string Token => _token;
        private string _endpoint;

        public Api()
        {
            _token = EditorPrefs.GetString("FlexalonCopilotToken");

#if FLEXALON_COPILOT_TEST
            _endpoint = "http://localhost:7071/api/";
#else
            _endpoint = "https://ai.flexalon.com/api/";
#endif
        }

        public async void RefreshUserInfo(Action onComplete)
        {
            _userInfo = null;
            if (IsLoggedIn)
            {
                await GetUserAsync();
                DateTime.TryParse(_userInfo.subEndDate, out _userInfo.subEndDateTime);
            }

            onComplete?.Invoke();
        }

        public async Task SignupAsync(string email)
        {
            await SendAsync<SignupRequest>("signup", new SignupRequest { email = email });
        }

        public async Task LoginAsync(string email, string code)
        {
            var response = await SendAsync<LoginResponse>("login", new LoginRequest { email = email, code = code });
            EditorPrefs.SetString("FlexalonCopilotToken", response.token);
            _token = response.token;
            LoginStateChanged?.Invoke();
        }

        public async Task UpdateUserAsync(bool marketing)
        {
            await SendAsync("updateUser", new UpdateUserRequest
            {
                token = _token,
                marketing = marketing
            });
        }

        public void Logout()
        {
            EditorPrefs.DeleteKey("FlexalonCopilotToken");
            _token = null;
            LoginStateChanged?.Invoke();
        }

        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_token);

        private async Task GetUserAsync()
        {
            _userInfo = await SendAsync<GetUserResponse>("getUser", new GetUserRequest { token = _token });
        }

        public async Task SendPromptAsync(string id, string previousId, string prompt,
            PromptContext context, CancellationToken token, Action<PromptResponseData> onData)
        {
            var response = await SendAsync("prompt", new PromptRequest {
                version = WindowUtil.GetVersion(),
                token = _token, id = id,
                previousId = previousId,
                prompt = prompt,
                context = context
            });

            await HandlePromptResponse(response, token, onData);
        }

        public async Task SendPromptRepeatAsync(
            string id, string previousId,
            PromptContext context, CancellationToken token,
             Action<PromptResponseData> onData)
        {
            var response = await SendAsync("prompt-repeat", new PromptRepeatRequest { token = _token, id = id, previousId = previousId, context = context });
            await HandlePromptResponse(response, token, onData);
        }

        public async Task SendPromptTestAsync(
            string gptResponse, PromptContext context, Action<PromptResponseData> onData)
        {
            var response = await SendAsync("prompt-test", new PromptTestRequest { context = context, gptResponse = gptResponse });
            await HandlePromptResponse(response, CancellationToken.None, onData);
        }

        public async Task SendFeedbackAsync(string promptId, int vote, string feedback)
        {
            await SendAsync("feedback", new FeedbackRequest { token = _token, id = promptId, vote = vote, feedback = feedback });
        }

        private async Task HandlePromptResponse(HttpResponseMessage response, CancellationToken token, Action<PromptResponseData> onData)
        {
            token.ThrowIfCancellationRequested();

            var stream = await response.Content.ReadAsStreamAsync();
            token.ThrowIfCancellationRequested();

            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await WithCancellation(reader.ReadLineAsync(), token)) != null)
                {
                    if (line.StartsWith("data:"))
                    {
                        var json = line.Substring(5).Trim(); // remove "data:" prefix
                        var data = JsonUtility.FromJson<PromptResponseData>(json);
                        onData?.Invoke(data);
                    }
                    else if (line.StartsWith("error:"))
                    {
                        var error = line.Substring(6).Trim(); // remove "error:" prefix
                        throw new Exception(error);
                    }
                }
            }
        }

        // https://ticehurst.com/2021/08/27/unity-httpclient.html
        // Might need to update approach if DNS changes happen.
        private HttpClient _httpClient = new HttpClient();

        internal class UpgradeRequiredException : Exception
        {
            public GetVersionResponse VersionResponse;
        }

        public async Task<HttpResponseMessage> SendAsync(string func, object body, CancellationToken token = default)
        {
            var startTime = DateTime.Now;
            using (var request = new HttpRequestMessage(HttpMethod.Post, _endpoint + func))
            {
                if (body != null)
                {
                    request.Content = new StringContent(Serialization.Serialize(body), Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await WithCancellation(response.Content.ReadAsStringAsync(), token);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        if (message == "Invalid access token")
                        {
                            Logout();
                            LoginStateChanged?.Invoke();
                            throw new Exception("Your login has expired. Please log in again.");
                        }

                        throw new Exception(message);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.UpgradeRequired)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var versionResponse = JsonUtility.FromJson<GetVersionResponse>(json);
                    throw new UpgradeRequiredException { VersionResponse = versionResponse };
                }

                throw new Exception("Error response: " + (int)response.StatusCode + " " + response.ReasonPhrase);
            }
        }

        public async Task<T> SendAsync<T>(string func, object body = null)
        {
            var response = await SendAsync(func, body);
            var json = await response.Content.ReadAsStringAsync();
            return JsonUtility.FromJson<T>(json);
        }

        private Task<T> WithCancellation<T>(Task<T> task, CancellationToken cancellationToken)
        {
            return task.IsCompleted
                ? task
                : task.ContinueWith(
                    completedTask => completedTask.GetAwaiter().GetResult(),
                    cancellationToken);
        }

        public async Task DownloadFileAsync(string url, string path)
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception((int)response.StatusCode + " " + response.ReasonPhrase);
            }

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var fileStream = File.Create(path))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        public async Task<GetVersionResponse> GetVersionAsync()
        {
            return await SendAsync<GetVersionResponse>("getVersion");
        }
    }
}

#endif