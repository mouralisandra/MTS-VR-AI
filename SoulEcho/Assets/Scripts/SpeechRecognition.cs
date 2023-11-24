using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HuggingFace.API;
using UnityEngine.Events;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI conversationText;

    // New serialized fields for personality prompt and bot name
    [SerializeField] private string avatarName = "Mootez: ";

    public static UnityAction<string> OnConversation;

    private Conversation conversation = new Conversation();

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        text.color = Color.white;
        text.text = "Recording...";
        startButton.interactable = false;
        stopButton.interactable = true;
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }

    private void SendRecording()
    {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            text.color = Color.white;
            text.text = response;
            startButton.interactable = true;
            ConversationText();

        }, error =>
        {
            text.color = Color.red;
            text.text = error;
            startButton.interactable = true;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    private string ConversationText()
    {
        string reply = "";
        HuggingFaceAPI.Conversation(text.text, response =>
        {
            reply = conversation.GetLatestResponse();
            conversationText.text = conversationText.text.TrimEnd($"{avatarName} is typing...\n".ToCharArray());

            // Customize the output based on personality or prompt
            conversationText.text += $"\n<color=#{444444}>{avatarName}{reply}</color>\n\n";

            OnConversation?.Invoke(reply);

            Canvas.ForceUpdateCanvases();
        }, error =>
        {
            conversationText.text = conversationText.text.TrimEnd($"{avatarName} is typing...\n".ToCharArray());
            conversationText.text += $"\n<color=#{222222}>Error: {error}</color>\n\n";
            reply = "Error";
            Canvas.ForceUpdateCanvases();
        }, conversation);

        if (reply == "")
        {
            reply = "Didn't get it";
        }
        return reply;
    }
}
