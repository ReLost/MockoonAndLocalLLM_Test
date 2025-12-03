using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Immersion.MetaCouch.Networking
{
    [CreateAssetMenu(fileName = "NetworkHandlerSO", menuName = "Networking/NetworkHandler")]
    public class NetworkHandler : ScriptableObject
    {
        [SerializeField] private string url = "http://localhost:3000/completition";

        [SerializeField, Tooltip("Timeout in seconds")]
        private float timeout = 5f;

        public event Action OnResponseWaiting;
        public event Action<string> OnResponseReceivedSuccess;
        public event Action OnResponseReceivedFailure;
        public event Action OnResponseTimeout;

        public async Task SendRequest(CancellationToken cancellationToken = default)
        {
            OnResponseWaiting?.Invoke();

            try
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    var operation = webRequest.SendWebRequest();

                    Task requestTask = WebRequestToTask(operation);
                    Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout), cancellationToken);

                    Task finished = await Task.WhenAny(requestTask, timeoutTask);

                    if (finished == requestTask)
                    {
                        await requestTask;

                        HandleWebRequestFinished(webRequest);
                    }
                    else
                    {
                        HandleTimeout(webRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Network request failed: {ex.Message}");
                OnResponseReceivedFailure?.Invoke();
            }
        }

        private Task WebRequestToTask(UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += _ => tcs.SetResult(null);
            return tcs.Task;
        }

        private void HandleWebRequestFinished(UnityWebRequest webRequest)
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string json = webRequest.downloadHandler.text;
                OnResponseReceivedSuccess?.Invoke(json);
            }
            else
            {
                OnResponseReceivedFailure?.Invoke();
            }
        }

        private void HandleTimeout(UnityWebRequest webRequest)
        {
            try
            {
                webRequest.Abort();
            }
            catch
            {
                // ignored
            }

            OnResponseTimeout?.Invoke();
        }
    }
}