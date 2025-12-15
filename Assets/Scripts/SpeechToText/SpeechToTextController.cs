using System.Diagnostics;
using TMPro;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;

namespace Immersion.MetaCouch.SpeechToText
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class SpeechToTextController : MonoBehaviour
    {
        [SerializeField] private WhisperManager whisper;
        [SerializeField] private MicrophoneRecord microphoneRecord;
        [SerializeField] private bool streamSegments = true;

        [Header("UI")] 
        [SerializeField] private Button recordButton;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private TMP_InputField inputField;
        
        private string _buffer;
        
        private WhisperStream _stream;

        private void Awake()
        {
            microphoneRecord.OnRecordStop += OnRecordStop;
            
            recordButton.onClick.AddListener(OnButtonPressed);
        }
        
        private async void Start()
        {
            if (streamSegments  == false|| whisper == null || microphoneRecord == null)
                return;

            _stream = await whisper.CreateStream(microphoneRecord);

            _stream.OnSegmentUpdated += OnStreamSegmentUpdated;
            _stream.OnSegmentFinished += OnStreamSegmentFinished;
        }
        
        private void OnStreamSegmentUpdated(WhisperResult result)
        {
            if (inputField == null || result == null)
                return;
            
            _buffer += result.Result;
            inputField.text = result.Result + "...";
        }

        private void OnStreamSegmentFinished(WhisperResult result)
        {
            if (inputField == null || result == null)
                return;

            inputField.ActivateInputField();
            inputField.text = _buffer;
        }

        public void StartStopRecording()
        {
            if (microphoneRecord.IsRecording == false)
            {
                _buffer = "";
                microphoneRecord.StartRecord();
                
                if (streamSegments && _stream != null)
                {
                    _stream.StartStream();
                }
                
                buttonText.text = "<color=red>Stop • [`~]</color>";
            }
            else
            {
                if (streamSegments && _stream != null)
                {
                    _stream.StopStream();
                }
                
                microphoneRecord.StopRecord();
                buttonText.text = "Record [`~]";
            }
        }
        
        private void OnButtonPressed()
        {
            StartStopRecording();
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
            
            // In streaming mode we do NOT run one-shot transcription on stop
            if (streamSegments)
                return;
            
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || inputField == null ) 
                return;
            
            inputField.ActivateInputField();
            inputField.text = res.Result;
        }
        

        private void OnDestroy()
        {
            microphoneRecord.OnRecordStop -= OnRecordStop;
            
            if (_stream != null)
            {
                _stream.OnSegmentUpdated -= OnStreamSegmentUpdated;
                _stream.OnSegmentFinished -= OnStreamSegmentFinished;
            }
        }
    }
}