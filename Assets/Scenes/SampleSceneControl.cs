using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class SampleSceneControl : MonoBehaviour
{
    private void Start()
    {
        // Unity 2022.3.9f1 returns a length of 8.202 seconds
        // Unity 2023.2.3f1 returns a length of 4.153 seconds. Thus, the issue seems to be fixed in Unity 2023.2.
        string relativePath = "Scenes/mp3-TestAudio-4sec.mp3";
        string fullPath = Path.Combine(Application.dataPath, relativePath);

        AudioClip audioClip = LoadAudioClipImmediately(fullPath, true);

        float expectedLengthInSeconds = 4;
        Debug.Log($"audioClip.length: {audioClip.length} seconds, expected length: {expectedLengthInSeconds} seconds");
        if (Math.Abs(audioClip.length - expectedLengthInSeconds) > 0.5f)
        {
            Debug.LogError("audioClip.length is not as expected.");
        }
    }

    public static AudioClip LoadAudioClipImmediately(string uri, bool streamAudio)
    {
        Uri uriHandle = new Uri(uri);
        using UnityWebRequest webRequest = CreateAudioClipRequest(uriHandle, streamAudio);
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            Debug.LogWarning("Waiting for AudioClip to load via Thread.Sleep");
            Thread.Sleep(10);
        }

        if (webRequest.result
            is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error Loading Audio: " + uri);
            Debug.LogError(webRequest.error);
        }

        AudioClip audioClip = (webRequest.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
        string fileName = Path.GetFileName(uriHandle.LocalPath);
        audioClip.name = $"Audio file '{fileName}'";
        return audioClip;
    }

    public static UnityWebRequest CreateAudioClipRequest(Uri uriHandle, bool streamAudio)
    {
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uriHandle, AudioType.UNKNOWN);
        DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
        downloadHandler.streamAudio = streamAudio;
        return webRequest;
    }
}