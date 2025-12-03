using System;
using System.Text;
using Immersion.MetaCouch.Data;
using UnityEngine;

namespace Immersion.MetaCouch.Utils
{
    [CreateAssetMenu(fileName = "DataParser", menuName = "Immersion/DataParser")]
    public class DataParser : ScriptableObject
    {
        private StringBuilder ollamaResponse = new StringBuilder();
        
        public ResponseData GetResponseData(string json)
        {
            var extractedJson = ExtractJson(json);
            return JsonUtility.FromJson<ResponseData>(extractedJson);
        }

        public CharacterData GetCharacterData(string json, out bool success)
        {
            var extractedJson = ExtractJson(json);
            var responseData = JsonUtility.FromJson<ResponseData>(extractedJson);
            success = IsCharacterDataEmpty() == false;
            return responseData.character;
            
            bool IsCharacterDataEmpty()
            {
                return string.IsNullOrEmpty(responseData.character.name) &&
                       responseData.character.mood == 0 &&
                       responseData.character.satisfaction == 0;
            }
        }

        private string ExtractJson(string text)
        {
            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');

            if (start == -1 || end == -1 || end < start)
                return null;

            return text.Substring(start, end - start + 1);
        }

        public string GetStatus(string text)
        {
            return ExtractStatus(text);
        }

        private string ExtractStatus(string text)
        {
            int idx = text.IndexOf("Status:", StringComparison.Ordinal);

            if (idx == -1)
                return string.Empty;

            string statusLine = text.Substring(idx).Trim();

            int startIndex = statusLine.IndexOf('[');
            int endIndex = statusLine.IndexOf(']');

            if (startIndex == -1 || endIndex == -1)
                return string.Empty;

            return statusLine.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
        
        public string GetOllamaResponse(string rawResponse)
        {
            var lines = rawResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ollamaResponse.Clear();

            foreach (var line in lines)
            {
                try
                {
                    OllamaResponseData chunk = JsonUtility.FromJson<OllamaResponseData>(line);
                    
                    if (string.IsNullOrEmpty(chunk.response) == false)
                    {
                        ollamaResponse.Append(chunk.response);
                    }
                    if (chunk.done)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to parse line: " + line + " Error: " + e.Message);
                }
            }

            return ollamaResponse.ToString();
        }
    }
}