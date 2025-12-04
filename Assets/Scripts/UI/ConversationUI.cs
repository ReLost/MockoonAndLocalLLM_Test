using System;
using System.Collections;
using System.Text;
using Immersion.MetaCouch.Networking;
using Immersion.MetaCouch.SpeechToText;
using Immersion.MetaCouch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Immersion.MetaCouch.UI
{
    public class ConversationUI : MonoBehaviour
    {
        [Header("External References")] [SerializeField]
        private NetworkHandler networkHandler;

        [SerializeField] private NetworkHandler ollamaNetworkHandler;

        [SerializeField] private DataParser dataParser;

        [Header("UI References")] [SerializeField, Space(5)]
        private TMP_Text conversationHistory;

        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private Button sendMessageButton;
        [SerializeField] private Button sendMessageToOllamaButton;

        [SerializeField, Space(5)] private Toggle textToSpeechToggle;
        [SerializeField] private Button stopSpeakingButton;

        [Header("Text To Speech")] 
        [SerializeField] private TextToSpeechController textToSpeechController;
        [Header("Speech To Text")] 
        [SerializeField] private SpeechToTextController speechToTextController;

        [Header("Input")] 
        [SerializeField] private InputAction sendMessageInput;
        [SerializeField] private bool inputSendMessageToOllama = true;
        [SerializeField, Space(5)] private InputAction stopSpeakingInput;
        [SerializeField, Space(5)] private InputAction recordingInput;

        private readonly StringBuilder currentHistory = new();

        private readonly StringBuilder waitingDotsBuilder = new();
        private Coroutine waitingCoroutine;
        private bool isWaitingForResponse;
        private WaitForSeconds delayBetweenDots;

        private Coroutine speakingCoroutine;

        private bool askedOllama;

        private void Start()
        {
            delayBetweenDots = new WaitForSeconds(0.5f);

            conversationHistory.text = string.Empty;

            AttachEventsToNetworkHandlers();

            sendMessageButton.onClick.AddListener(() => SendRequest(false));
            sendMessageToOllamaButton.onClick.AddListener(() => SendRequest(true));
            HandleSendButtonBehaviour();

            AttachInputs();

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

        private void AttachInputs()
        {
            sendMessageInput.Enable();
            sendMessageInput.performed += SendRequestOnButtonAction;

            stopSpeakingInput.Enable();
            stopSpeakingInput.performed += StopSpeakingOnButtonAction;
            
            recordingInput.Enable();
            recordingInput.performed += StartStopRecordingOnButtonAction;
        }

        private void OnDestroy()
        {
            networkHandler.OnResponseWaiting -= ShowWaitingForResponseIndicator;
            networkHandler.OnResponseReceivedSuccess -= UpdateMessage;
            networkHandler.OnResponseReceivedFailure -= OnResponseError;
            networkHandler.OnResponseTimeout -= OnResponseTimeout;

            sendMessageInput.Disable();
            sendMessageInput.performed -= SendRequestOnButtonAction;
            stopSpeakingInput.Disable();
            stopSpeakingInput.performed -= StopSpeakingOnButtonAction;
            recordingInput.Disable();
            recordingInput.performed -= StartStopRecordingOnButtonAction;
        }

        private void SendRequest(bool askOllama)
        {
            var message = messageInputField.text;
            
            if(string.IsNullOrEmpty(message) == false)
            {
                AddToConversation($"<color=red>[YOU]</color> {messageInputField.text}");
                askedOllama = askOllama;
                var currentNetworkHandler = askOllama ? ollamaNetworkHandler : networkHandler;
                currentNetworkHandler.SendRequest(message);
                messageInputField.text = string.Empty;
                messageInputField.ActivateInputField();
            }
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
            var prefix = askedOllama ? "<color=purple>[OLLAMA]</color>" : "<color=blue>[BOT]</color>";
            AddToConversation($"{prefix} {message}");
            TryReadResponse(message);
        }

        private void TryReadResponse(string message)
        {
            if (textToSpeechToggle.isOn)
            {
                try
                {
                    textToSpeechController.TextToSpeech(message, out var audioSize);
                    stopSpeakingButton.gameObject.SetActive(true);

                    if (speakingCoroutine != null)
                        StopCoroutine(speakingCoroutine);

                    speakingCoroutine = StartCoroutine(DisableStopSpeakingButtonAfterSpeech(audioSize));
                }
                catch (Exception ex)
                {
                    //Debug.LogError($"Error with Reading text: {ex.Message}");
                    textToSpeechController.ResetModel();
                }
            }
        }

        private IEnumerator DisableStopSpeakingButtonAfterSpeech(float audioSize)
        {
            yield return new WaitForSeconds(audioSize);

            stopSpeakingButton.gameObject.SetActive(false);
        }

        private string GetParsedMessage(string responseMessage)
        {
            if (askedOllama)
            {
                return dataParser.GetOllamaResponse(responseMessage);
            }

            var data = dataParser.GetCharacterData(responseMessage, out var success);
            return success
                ? $"Hi, I'm {data.name}, my mood today, in scale 0-6, is {data.mood}"
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

        public void StopSpeaking()
        {
            if (speakingCoroutine != null)
                StopCoroutine(speakingCoroutine);

            textToSpeechController.StopSpeaking();
            stopSpeakingButton.gameObject.SetActive(false);
        }

        private void StartStopRecording()
        {
            speechToTextController.StartStopRecording();
        }
        
        private void SendRequestOnButtonAction(InputAction.CallbackContext obj)
        {
            SendRequest(inputSendMessageToOllama);
        }

        private void StopSpeakingOnButtonAction(InputAction.CallbackContext obj)
        {
            StopSpeaking();
        }

        private void StartStopRecordingOnButtonAction(InputAction.CallbackContext obj)
        {
            StartStopRecording();
        }
    }
}