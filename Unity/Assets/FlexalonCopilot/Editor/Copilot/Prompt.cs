#if UNITY_TMPRO && UNITY_UI

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlexalonCopilot.Editor
{
    internal class Prompt
    {
        public event Action OnUpdate;

        public string Id;
        public string PreviousId;
        public PromptContext PromptContext;
        public SceneUpdater SceneUpdater;
        public CancellationToken CancellationToken;

        private string _errorMessage;
        public string ErrorMessage => _errorMessage;

        private GetVersionResponse _upgradeResponse = null;
        public GetVersionResponse UpgradeResponse => _upgradeResponse;

        public async Task SendAsync(string prompt)
        {
            await SendAndCatchErrors(async () =>
            {
                await Api.Instance.SendPromptAsync(
                    Id, PreviousId, prompt, PromptContext, CancellationToken, OnData);
            });
        }

        public async Task SendTestAsync(string gptResponse)
        {
            await SendAndCatchErrors(async () =>
            {
                await Api.Instance.SendPromptTestAsync(gptResponse, PromptContext, OnData);
            });
        }

        public async Task SendRepeatAsync(string promptId)
        {
            await SendAndCatchErrors(async () =>
            {
                await Api.Instance.SendPromptRepeatAsync(promptId, PreviousId, PromptContext, CancellationToken, OnData);
            });
        }

        public async Task SendAndCatchErrors(Func<Task> sendAction)
        {
            _errorMessage = "";

            SceneUpdater.Init();

            try
            {
                await sendAction();
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Api.UpgradeRequiredException ex)
            {
                _upgradeResponse = ex.VersionResponse;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                _errorMessage = ex.Message;
            }
            finally
            {
                await SceneUpdater.PostUpdate();
            }
        }

        private void OnData(PromptResponseData data)
        {
            try
            {
                ProcessResponse(data);
                OnUpdate?.Invoke();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void ProcessResponse(PromptResponseData data)
        {
            switch (data.cmd)
            {
                case "create":
                    SceneUpdater.CreateGameObject(data.id, data.a, data.b);
                    break;
                case "move":
                    SceneUpdater.MoveGameObject(data.id, data.a, data.b);
                    break;
                case "destroy":
                    SceneUpdater.DestroyGameObject(data.id);
                    break;
                case "addComponent":
                    SceneUpdater.AddComponent(data.id, data.a, data.b);
                    break;
                case "removeComponent":
                    SceneUpdater.RemoveComponent(data.id);
                    break;
                case "setProperty":
                    SceneUpdater.SetComponentProperty(data.id, data.a, data.b);
                    break;
                case "clearProperty":
                    SceneUpdater.ClearComponentProperty(data.id, data.a);
                    break;
                case "setStyleProperty":
                    SceneUpdater.SetStyleProperty(data.id, data.a, data.b, data.c);
                    break;
                case "clearStyleProperty":
                    SceneUpdater.ClearStyleProperty(data.id, data.a, data.b);
                    break;
                case "setRectSizeToZero":
                    SceneUpdater.SetRectSizeToZero(data.id, data.a == "width");
                    break;
                case "message":
                    SceneUpdater.Message(data.id, data.a);
                    break;
            }

            SceneUpdater.PostCommand();
        }
    }
}

#endif