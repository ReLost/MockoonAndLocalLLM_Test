using System;
using System.Text;
using UnityEngine.Networking;

namespace Immersion.MetaCouch.Networking
{
    public class StreamingDownloadHandler : DownloadHandlerScript
    {
        private event Action<string> OnReceiveData;
        private readonly StringBuilder fullResponse = new();
        
        public StreamingDownloadHandler(Action<string> onReceiveData)
        {
            OnReceiveData = onReceiveData;
        }
        
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0) return false;

            string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            
            fullResponse.Append(chunk);
            OnReceiveData?.Invoke(chunk);
            return true;
        }

        protected override string GetText()
        {
            return fullResponse.ToString();
        }
    }
}