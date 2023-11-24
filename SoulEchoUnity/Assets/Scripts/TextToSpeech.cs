using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TextToSpeechNamespace{
public class TextToSpeech
{
    private UnityWebRequest www;
    private string url;

    public IEnumerator GenerateSpeech(string phrase, double octave, string option, string apikey, System.Action<AudioClip> onComplete)
    {
        if (apikey == null)
        {
            Debug.LogError("Please contact the support");
            yield break;
        }

        if (phrase == null || phrase == "")
        {
            Debug.LogError("Text is empty");
            yield break;
        }

        www = null;

        WWWForm form = new WWWForm();
        form.AddField("sentence", phrase);
        form.AddField("octave", octave.ToString());

        string link = $"https://ariel-api.xandimmersion.com/tts/{option}";
        www = UnityWebRequest.Post(link, form);
        www.SetRequestHeader("Authorization", "Api-Key " + apikey);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error While Sending: " + www.error);
        }
        else
        {
            var info = JsonUtility.FromJson<Synthese>(www.downloadHandler.text);
            url = "https://rocky-taiga-14840.herokuapp.com/" + info.audio;

            using (UnityWebRequest wwwAudio = UnityWebRequestMultimedia.GetAudioClip(info.audio, AudioType.WAV))
            {
                wwwAudio.SetRequestHeader("x-requested-with", "http://127.0.0.1:8080");
                yield return wwwAudio.SendWebRequest();

                if (wwwAudio.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(wwwAudio);
                    onComplete?.Invoke(audioClip);
                }
                else
                {
                    Debug.LogError(wwwAudio.error);
                }
            }
        }
    }
}
}