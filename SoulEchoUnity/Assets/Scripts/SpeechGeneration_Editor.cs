using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using TMPro;
using System.Threading;
using System.Linq;
using UnityEngine.UIElements;

public class Tts
{
    public string Phrase;

    public double Octave;

    public string Email;
    
    public string Title;

    public Dictionary<string, string[]> Options = new Dictionary<string, string[]>
    {
        { "Arabic",new string[] { "Farah", } },
        { "Chinese (Mandarin)",new string[] { "Daiyu", } },
        { "Danish",new string[] { "Emma", "Oscar", } },
        { "Dutch",new string[] { "Anke","Adriaan", } },
        { "English (Australian)",new string[] { "Mia","Grace","Jack", } },
        { "English (British)",new string[] { "Charlotte","Sophia","Elijah", } },
        { "English (Indian)",new string[] { "Advika","Onkar", } },
        { "English (New Zealand)",new string[] { "Ruby", } },
        { "English (South African)",new string[] { "Elna", } },
        { "English (US)",new string[] { "Mary","Linda","Patricia","Barbara","Susan","Paul","Michael","William","Thomas", } },
        { "English (Welsh)",new string[] { "Aeron", } },
        { "French",new string[] { "Capucine","Alix","Arnaud", } },
        { "French (Canadian)",new string[] { "Stephanie","Celine", } },
        { "German",new string[] { "Maria","Theresa","Felix", } },
        { "Hindi",new string[] { "Chhaya", } },
        { "Icelandic",new string[] { "Anna","Sigriour", } },
        { "Italian",new string[] { "Gabriella","Bella","Lorenzo", } },
        { "Japanese",new string[] { "Rika","Tanaka", } },
        { "Korean",new string[] { "Ji-Ho", } },
        { "Norwegian",new string[] { "Camilla", } },
        { "Polish",new string[] { "Katarzyna","Malgorzata","Piotr","Jan", } },
        { "Portuguese (Brazilian)",new string[] { "Tabata","Juliana","Pedro", } },
        { "Portuguese (European)",new string[] { "Pati","Adriano", } },
        { "Romanian",new string[] { "Alexandra", } },
        { "Russian",new string[] { "Inessa","Viktor", } },
        { "Spanish (European)",new string[] { "Francisca","Margarita","Mateo", } },
        { "Spanish (Mexican)",new string[] { "Leticia", } },
        { "Spanish (US)",new string[] { "Josefina","Rosa","Miguel", } },
        { "Swedish",new string[] { "Eva", } },
        { "Turkish",new string[] { "Mesut", } },
        { "Welsh",new string[] { "Angharad", } },
    };

    public string[] Langues = new string[]
        {
            "Arabic", "Chinese (Mandarin)", "Danish", "Dutch", "English (Australian)", "English (British)", "English (Indian)", "English (New Zealand)", "English (South African)", "English (US)", "English (Welsh)", "French", "French (Canadian)", "German", "Hindi", "Icelandic", "Italian", "Japanese", "Korean", "Norwegian", "Polish", "Portuguese (Brazilian)", "Portuguese (European)", "Romanian", "Russian", "Spanish (European)", "Spanish (Mexican)", "Spanish (US)", "Swedish", "Turkish", "Welsh",
        };
    
    public int Selected_O = 0;
    public int Selected_L = 0;
}
public class Synthese
{
    public string sentence;
    public string audio;

}
public class Authorization
{
    public string api_key = "kGOdv5yl.oqUd2gMibwhBVp5C6ed57Wpxvs2LUKOW";

}

public class SpeechGenerationForNPCs : EditorWindow
{
    private Tts _tts = new Tts();

    public string url;
    UnityWebRequest www;

    public AudioClip son_pilou;

    private Synthese info = null;

    string s_octave;

    int selectedOctave = 0;
    string[] selStrings = { "normal pitch (0)", "low pitch (-0.5)", "high pitch (+0.5)", "custom pitch between -1 and +1" };

    //Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Window/SpeechGenerationForNPCs", false, 1)]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<SpeechGenerationForNPCs>();
        wnd.titleContent = new GUIContent("SpeechGenerationForNPCs");
        wnd.minSize = new Vector2(750, 280);
        wnd.maxSize = new Vector2(1000, 350);
    }

    public void TextToAudio(string option, string phrase, string title, double octave, string apikey)
    {
        if (apikey == null)
        {
            UnityEngine.Debug.LogError("Please contact the support");
            return;
        }

        if (phrase == null || phrase == "")
        {
            UnityEngine.Debug.LogError("Text is empty");
            return;
        }
        www = null;
        
        WWWForm form = new WWWForm();
        form.AddField("sentence", phrase);
        s_octave = octave.ToString();
        form.AddField("octave", s_octave.Replace(',', '.'));
        //form.AddField("speed", "1");
        string lien = $"https://ariel-api.xandimmersion.com/tts/{option}";
        www = UnityWebRequest.Post(lien, form);
        www.SetRequestHeader("Authorization", "Api-Key " + apikey);

        www.SendWebRequest();
        while (!www.isDone)
        {
            continue;
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError("Error While Sending: " + www.error);
        }
        else
        {
            info = JsonUtility.FromJson<Synthese>(www.downloadHandler.text);
       
        }

        url = "https://rocky-taiga-14840.herokuapp.com/" + info.audio;
        using (UnityWebRequest www_audio = UnityWebRequestMultimedia.GetAudioClip(info.audio, AudioType.WAV))
        {
            www_audio.SetRequestHeader("x-requested-with", "http://127.0.0.1:8080");

            www_audio.SendWebRequest();


            while (!www_audio.isDone)
            {
                continue; //ajouter une barre de chargement
            }


            if (www_audio.isNetworkError)
            {
                UnityEngine.Debug.LogError(www_audio.error);
            }
            else
            {
                AudioClip son_pilou = DownloadHandlerAudioClip.GetContent(www_audio);
                if (title == null) 
                {
                    UnityEngine.Debug.Log("No file name : file will be saved at untitled-gen.wav");
                    title = "untitled";
                }
                UnityEngine.Debug.Log("Audio Generation In Progress.");
                SavWav.Save($"SpeechGenerationForNPCs/SavedAudioFiles/{title}-gen", son_pilou);

            }
        }

        

        info = null;
    }

    public void OnGUI()
    {
        LayoutItem(_tts);
    }

    private void LayoutItem(Tts tts)
    {
        if (tts == null)
        {
            return;
        }

        tts.Phrase = EditorGUILayout.TextField("Text to Audio file", tts.Phrase, GUI.skin.textArea, GUILayout.Height(100));

        selectedOctave = GUILayout.SelectionGrid(selectedOctave, selStrings, 2, GUI.skin.toggle);

        switch (selectedOctave)
        {
            case 0:
                tts.Octave = 0;
                break;
            case 1:
                tts.Octave = -0.5;
                break;
            case 2:
                tts.Octave = 0.5;
                break;
            case 3:
                tts.Octave = EditorGUILayout.DoubleField("Custom Octave", tts.Octave, GUI.skin.textArea);
                break;
            default:
                tts.Octave = 0;
                break;
        }

        tts.Title = EditorGUILayout.TextField("File name : ''...-gen.wav'' ", tts.Title, GUI.skin.textArea);

        tts.Selected_L = EditorGUILayout.Popup("Select Language", tts.Selected_L, tts.Langues);

        tts.Selected_O = EditorGUILayout.Popup("Select Voice", tts.Selected_O, tts.Options[tts.Langues[tts.Selected_L].ToString()]);


        if (GUILayout.Button("Save to wav"))
        {
            TextToAudio(tts.Options[tts.Langues[tts.Selected_L].ToString()][tts.Selected_O], tts.Phrase, tts.Title, tts.Octave, "kGOdv5yl.oqUd2gMibwhBVp5C6ed57Wpxvs2LUKOW");
        }

        GUILayout.Label("First use : Please take a look at the notice (SpeechGenerationForNPCs/NoticeAssetUnity) \n" +
            "If your audio file is not showing in the SavedAudioFiles folder : take a look through your file explorer as Unity may need to be reload \n \n"+
            "The assets produced with this software are licensed under the MIT license.");

    }
}
