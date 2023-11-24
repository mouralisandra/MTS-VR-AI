using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TextToSpeechNamespace;

public class PlayHT_TTS : MonoBehaviour
{
    public AudioSource audioSource;
    public string url = "http://localhost:3000/tts";

    private TextToSpeech textToSpeech = new TextToSpeech();

    void OnEnable()
    {
        SpeechRecognitionTest.OnConversation += OnConversation;
    }

    private void OnConversation(string input)
    {
        string newUri = url + "?input=" + input;
        print(newUri);
        StartCoroutine(MakePlayHTRequest(newUri));
    }

    IEnumerator MakePlayHTRequest(string URI)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(URI);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string response = webRequest.downloadHandler.text;
            ResponseHandler(response);
        }
        else
        {
            Debug.LogWarning("Error: " + webRequest.error);
        }
    }

    private void ResponseHandler(string response)
    {
        try
        {
            print(response);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(response);

            // Access the text from the response
            var text = jsonObject["text"].ToString();

            if (!string.IsNullOrEmpty(text))
            {
                ConvertTextToSpeech(text);
            }
            else
            {
                Debug.LogWarning("Cannot get text from the response");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void ConvertTextToSpeech(string text)
    {
        textToSpeech.GenerateSpeech(text, 0, "English (US)", "kGOdv5yl.oqUd2gMibwhBVp5C6ed57Wpxvs2LUKOW", audioClip =>
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        });
    }
}
