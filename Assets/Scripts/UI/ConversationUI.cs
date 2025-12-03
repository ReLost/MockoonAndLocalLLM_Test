using System.Collections;
using System.Text;
using Immersion.MetaCouch.Networking;
using Immersion.MetaCouch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Immersion.MetaCouch.UI
{
    public class ConversationUI : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField] private NetworkHandler  networkHandler;
        [SerializeField] private DataParser dataParser;
        
        [Header("UI References")]
        [SerializeField, Space(5)] private TMP_Text conversationHistory;
        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private Button sendMessageButton;
        
        [Header("Input")]
        [SerializeField] private InputAction sendMessageInput;
        
        private StringBuilder currentHistory = new();
        
        private StringBuilder waitingDotsBuilder = new();
        private Coroutine waitingCoroutine;
        private bool isWaitingForResponse;
        private WaitForSeconds delayBetweenDots;

        private void Start()
        {
            delayBetweenDots  = new WaitForSeconds(0.5f);
            
            conversationHistory.text = string.Empty;

            networkHandler.OnResponseWaiting += ShowWaitingForResponseIndicator;
            networkHandler.OnResponseReceivedSuccess += UpdateMessage;
            networkHandler.OnResponseReceivedFailure += OnResponseError;
            networkHandler.OnResponseTimeout  += OnResponseTimeout;
            
            sendMessageButton.onClick.AddListener(SendRequest);
            HandleSendButtonBehaviour();
            
            sendMessageInput.Enable();
            sendMessageInput.performed += SendRequestOnButtonAction;
            
            messageInputField.ActivateInputField();
        }

        private void SendRequestOnButtonAction(InputAction.CallbackContext obj)
        {
            SendRequest();
        }

        private void OnDestroy()
        {
            networkHandler.OnResponseWaiting -= ShowWaitingForResponseIndicator;
            networkHandler.OnResponseReceivedSuccess -= UpdateMessage;
            networkHandler.OnResponseReceivedFailure -= OnResponseError;
            networkHandler.OnResponseTimeout  -= OnResponseTimeout;
            
            sendMessageInput.Disable();
            sendMessageInput.performed -= SendRequestOnButtonAction;
        }

        private void SendRequest()
        {
            var message = messageInputField.text;
            AddToConversation(message);
            _ = networkHandler.SendRequest();
            messageInputField.text = string.Empty;
            messageInputField.ActivateInputField();
        }

        private void ShowWaitingForResponseIndicator()
        {
            if (waitingCoroutine != null)
                StopCoroutine(waitingCoroutine);

            waitingCoroutine = StartCoroutine(WaitingIndicatorCoroutine());
        }
        
        private IEnumerator WaitingIndicatorCoroutine()
        {
            int dotCount = 0;

            while (true)
            {
                dotCount = (dotCount % 3) + 1;
                
                waitingDotsBuilder.Length = 0;
                waitingDotsBuilder.Append('.', dotCount);
                
                conversationHistory.text = currentHistory.ToString() + waitingDotsBuilder;

                yield return delayBetweenDots;
            }
        }

        private void UpdateMessage(string responseMessage)
        {
            var data = dataParser.GetCharacterData(responseMessage, out var success);
            var message = success ? 
                $"Hi, I'm {data.name}, my mood today, in scale 0-6, is {data.mood}" :
                "<color=red>Something went wrong with CharacterData</color>";
            AddToConversation(message);
        }
        
        private void OnResponseError()
        {
            var message = "<color=red>Some connection/server error</color>";
            AddToConversation(message);
        }

        private void OnResponseTimeout()
        {
            var message = "<color=red>Connection timeout</color>";
            AddToConversation(message);
        }
        
        private void AddToConversation(string message)
        {
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
                waitingDotsBuilder.Length = 0;
            }

            currentHistory.Append($"{message}\n");
            conversationHistory.text = currentHistory.ToString();
        }

        public void HandleSendButtonBehaviour()
        {
            sendMessageButton.interactable = messageInputField.text.Length > 0;
        }
    }
}