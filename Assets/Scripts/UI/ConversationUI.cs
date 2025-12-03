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
        [SerializeField] private NetworkHandler networkHandler;
        [SerializeField] private NetworkHandler ollamaNetworkHandler;

        [SerializeField] private DataParser dataParser;

        [Header("UI References")] [SerializeField, Space(5)]
        private TMP_Text conversationHistory;

        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private Button sendMessageButton;
        [SerializeField] private Button sendMessageToOllamaButton;

        [Header("Input")] 
        [SerializeField] private InputAction sendMessageInput;
        [SerializeField] private bool inputSendMessageToOllama = true;

        private readonly StringBuilder currentHistory = new();

        private readonly StringBuilder waitingDotsBuilder = new();
        private Coroutine waitingCoroutine;
        private bool isWaitingForResponse;
        private WaitForSeconds delayBetweenDots;

        private bool askedOllama;

        private void Start()
        {
            delayBetweenDots = new WaitForSeconds(0.5f);

            conversationHistory.text = string.Empty;

            AttachEventsToNetworkHandlers();

            sendMessageButton.onClick.AddListener(() => SendRequest(false));
            sendMessageToOllamaButton.onClick.AddListener(() => SendRequest(true));
            HandleSendButtonBehaviour();

            sendMessageInput.Enable();
            sendMessageInput.performed += SendRequestOnButtonAction;

            messageInputField.ActivateInputField();
        }

        private void AttachEventsToNetworkHandlers()
        {
            networkHandler.OnResponseWaiting += ShowWaitingForResponseIndicator;
            networkHandler.OnResponseReceivedSuccess += UpdateMessage;
            networkHandler.OnResponseReceivedFailure += OnResponseError;
            networkHandler.OnResponseTimeout += OnResponseTimeout;
            
            ollamaNetworkHandler.OnResponseWaiting += ShowWaitingForResponseIndicator;
            ollamaNetworkHandler.OnResponseReceivedSuccess += UpdateMessage;
            ollamaNetworkHandler.OnResponseReceivedFailure += OnResponseError;
            ollamaNetworkHandler.OnResponseTimeout += OnResponseTimeout;
        }

        private void SendRequestOnButtonAction(InputAction.CallbackContext obj)
        {
            SendRequest(inputSendMessageToOllama);
        }

        private void OnDestroy()
        {
            networkHandler.OnResponseWaiting -= ShowWaitingForResponseIndicator;
            networkHandler.OnResponseReceivedSuccess -= UpdateMessage;
            networkHandler.OnResponseReceivedFailure -= OnResponseError;
            networkHandler.OnResponseTimeout -= OnResponseTimeout;

            sendMessageInput.Disable();
            sendMessageInput.performed -= SendRequestOnButtonAction;
        }

        private void SendRequest(bool askOllama)
        {
            var message = messageInputField.text;
            AddToConversation($"<color=red>[YOU]</color> {messageInputField.text}");
            askedOllama = askOllama;
            var currentNetworkHandler = askOllama ? ollamaNetworkHandler : networkHandler;
            currentNetworkHandler.SendRequest(message);
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
            var message = GetParsedMessage(responseMessage);
            AddToConversation(message);
        }

        private string GetParsedMessage(string responseMessage)
        {
            if (askedOllama)
            {
                return $"<color=purple>[OLLAMA]</color> {dataParser.GetOllamaResponse(responseMessage)}";;
            }

            var data = dataParser.GetCharacterData(responseMessage, out var success);
            return success
                ? $"<color=blue>[BOT]</color> Hi, I'm {data.name}, my mood today, in scale 0-6, is {data.mood}"
                : "<color=red>Something went wrong with CharacterData</color>";
        }

        private void OnResponseError(string errorMessage)
        {
            var message = $"<color=red>Some connection/server error: {errorMessage}</color>";
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
            sendMessageToOllamaButton.interactable = messageInputField.text.Length > 0;
        }
    }
}