using System;
using System.Text;
using Immersion.MetaCouch.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Immersion.MetaCouch.Networking
{
    [CreateAssetMenu(fileName = "NetworkHandlerSO", menuName = "Networking/OllamaNetworkHandler")]
    public class OllamaNetworkHandler : NetworkHandler
    {
        [Header("Ollama")]
        [SerializeField] 
        private bool streamingResponse;
        
        public bool StreamingResponse => streamingResponse;
        
        public event Action<string> OnResponseChunkReceived;
        
        protected override UnityWebRequest CreateRequest(string prompt)
        {
            var request = new OllamaRequestData
            {
                model = "llama3", 
                prompt = prompt,
                stream = streamingResponse,
            };
            string json = JsonUtility.ToJson(request);

            var webRequest = new UnityWebRequest(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            webRequest.downloadHandler = GetDownloadHandler();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            return webRequest;
        }

        private DownloadHandler GetDownloadHandler()
        {
            if (streamingResponse)
            {
                return new StreamingDownloadHandler(OnResponseChunkReceived);
            }

            return new DownloadHandlerBuffer();
        }
    }
}
