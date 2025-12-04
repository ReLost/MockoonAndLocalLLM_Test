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

        private void Awake()
        {
            whisper.OnNewSegment += OnNewSegment;
            
            microphoneRecord.OnRecordStop += OnRecordStop;
            
            recordButton.onClick.AddListener(OnButtonPressed);
        }

        public void StartStopRecording()
        {
            if (microphoneRecord.IsRecording == false)
            {
                microphoneRecord.StartRecord();
                buttonText.text = "<color=red>Stop • [`~]</color>";
            }
            else
            {
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
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || inputField == null ) 
                return;
            
            inputField.ActivateInputField();
            inputField.text = res.Result;
        }
        
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (streamSegments == false || inputField == null)
                return;

            _buffer += segment.Text;
            inputField.text = _buffer + "...";
        }

        private void OnDestroy()
        {
            whisper.OnNewSegment -= OnNewSegment;
            
            microphoneRecord.OnRecordStop -= OnRecordStop;
        }
    }
}