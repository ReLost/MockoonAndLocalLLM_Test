using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Immersion.MetaCouch.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Immersion.MetaCouch.Networking
{
    [CreateAssetMenu(fileName = "NetworkHandlerSO", menuName = "Networking/NetworkHandler")]
    public class NetworkHandler : ScriptableObject
    {
        [SerializeField] private string url = "http://localhost:3000/completition";
        [SerializeField] private bool askOllama;

        [SerializeField, Tooltip("Timeout in seconds, 0 means no timeout")]
        private float timeout = 5f;

        public event Action OnResponseWaiting;
        public event Action<string> OnResponseReceivedSuccess;
        public event Action OnResponseReceivedFailure;
        public event Action OnResponseTimeout;

        public void SendRequest(string prompt, CancellationToken cancellationToken = default)
        {
            OnResponseWaiting?.Invoke();

            _ = askOllama
                ? SendRequestInternal(CreateOllamaRequest(prompt), cancellationToken)
                : SendRequestInternal(CreateLocalhostRequest(), cancellationToken);
        }

        private UnityWebRequest CreateOllamaRequest(string prompt)
        {
            var request = new OllamaRequestData
            {
                model = "llama3", 
                prompt = prompt
            };
            string json = JsonUtility.ToJson(request);

            var webRequest = new UnityWebRequest(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            return webRequest;
        }

        private UnityWebRequest CreateLocalhostRequest()
        {
            return UnityWebRequest.Get(url);
        }

        private async Task SendRequestInternal(UnityWebRequest webRequest, CancellationToken cancellationToken)
        {
            try
            {
                var operation = webRequest.SendWebRequest();
                Task requestTask = WebRequestToTask(operation);

                Task finished = timeout > 0
                    ? await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(timeout), cancellationToken))
                    : await Task.WhenAny(requestTask);

                if (timeout > 0 && finished != requestTask)
                {
                    HandleTimeout(webRequest);
                    return;
                }

                await requestTask;

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    OnResponseReceivedSuccess?.Invoke(webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"Request error: {webRequest.error}");
                    OnResponseReceivedFailure?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Network request failed: {ex.Message}");
                OnResponseReceivedFailure?.Invoke();
            }
            finally
            {
                webRequest.Dispose();
            }
        }

        private Task WebRequestToTask(UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += _ => tcs.SetResult(null);
            return tcs.Task;
        }

        private void HandleTimeout(UnityWebRequest webRequest)
        {
            try
            {
                webRequest.Abort();
            }
            catch
            {
            }

            OnResponseTimeout?.Invoke();
        }
    }
}