using Immersion.MetaCouch.Networking;
using Immersion.MetaCouch.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Immersion.MetaCouch.UI
{
    public class CharacterUI : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField] private NetworkHandler networkHandler;
        [SerializeField] private DataParser dataParser;
        
        [Header("UI References")]
        [SerializeField, Space(5)] private Image satisfactionProgressBar;

        private void Start()
        {
            networkHandler.OnResponseReceivedSuccess += UpdateCharacterUI;
        }

        private void OnDestroy()
        {
            networkHandler.OnResponseReceivedSuccess -= UpdateCharacterUI;
        }

        private void UpdateCharacterUI(string responseMessage)
        {
            var data = dataParser.GetCharacterData(responseMessage, out var success);
            
            if (success)
            {
                var satisfactionValueNormalized = data.satisfaction / 100.0f;
                satisfactionProgressBar.fillAmount = satisfactionValueNormalized;
                satisfactionProgressBar.color = Color.Lerp(Color.red, Color.green, satisfactionValueNormalized);
            }
        }
    }
}