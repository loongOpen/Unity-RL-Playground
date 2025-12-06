using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Wit.BaiduAip.Speech
{
    [Serializable]
    public class TtsResponse
    {
        public int err_no;
        public string err_msg;
        public string sn;
        public int idx;

        public bool Success
        {
            get { return err_no == 0; }
        }

        public AudioClip clip;
    }

    public class Tts : Base
    {
        public enum Pronouncer
        {
            Female, // 0为普通女声
            Male, // 1为普通男声
            Duxiaoyao, // 3为情感合成-度逍遥
            Duyaya // 4为情感合成-度丫丫
        }

        private const string UrlTts = "https://tsn.baidu.com/text2audio";

        public Tts(string apiKey, string secretKey) : base(apiKey, secretKey)
        {
        }

        public IEnumerator Synthesis(string text, Action<TtsResponse> callback, int speed = 5, int pit = 5, int vol = 5,
            Pronouncer per = Pronouncer.Female)
        {
            yield return PreAction();

            if (tokenFetchStatus == Base.TokenFetchStatus.Failed)
            {
                Debug.LogError("Token was fetched failed. Please check your APIKey and SecretKey");
                callback(new TtsResponse()
                {
                    err_no = -1,
                    err_msg = "Token was fetched failed. Please check your APIKey and SecretKey"
                });
                yield break;
            }

            var param = new Dictionary<string, string>();
            param.Add("tex", text);
            param.Add("tok", Token);
            param.Add("cuid", SystemInfo.deviceUniqueIdentifier);
            param.Add("ctp", "1");
            param.Add("lan", "zh");
            param.Add("spd", Mathf.Clamp(speed, 0, 9).ToString());
            param.Add("pit", Mathf.Clamp(pit, 0, 9).ToString());
            param.Add("vol", Mathf.Clamp(vol, 0, 15).ToString());
            param.Add("per", ((int) per).ToString());

            string url = UrlTts;
            int i = 0;
            foreach (var p in param)
            {
                url += i != 0 ? "&" : "?";
                url += p.Key + "=" + p.Value;
                i++;
            }

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
            var www = UnityWebRequest.Get(url);
#else
            var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
#endif
            Debug.Log(www.url);
            yield return www.SendWebRequest();


            if (string.IsNullOrEmpty(www.error))
            {
                var type = www.GetResponseHeader("Content-Type");
                Debug.Log("response type: " + type);

                if (type == "audio/mp3")
                {
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
                    var clip = GetAudioClipFromMP3ByteArray(www.downloadHandler.data);
                    var response = new TtsResponse {clip = clip};
#else
                    var response = new TtsResponse {clip = DownloadHandlerAudioClip.GetContent(www) };
#endif
                    callback(response);
                }
                else
                {
                    Debug.LogError(www.downloadHandler.text);
                    callback(JsonUtility.FromJson<TtsResponse>(www.downloadHandler.text));
                }
            }
            else
                Debug.LogError(www.error);
        }


#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
        /// <summary>
        /// 将mp3格式的字节数组转换为audioclip
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private AudioClip GetAudioClipFromMP3ByteArray(byte[] mp3Data)
        {
            var mp3MemoryStream = new MemoryStream(mp3Data);
            MP3Sharp.MP3Stream mp3Stream = new MP3Sharp.MP3Stream(mp3MemoryStream);

            //Get the converted stream data
            MemoryStream convertedAudioStream = new MemoryStream();
            byte[] buffer = new byte[2048];
            int bytesReturned = -1;
            int totalBytesReturned = 0;

            while (bytesReturned != 0)
            {
                bytesReturned = mp3Stream.Read(buffer, 0, buffer.Length);
                convertedAudioStream.Write(buffer, 0, bytesReturned);
                totalBytesReturned += bytesReturned;
            }

            Debug.Log("MP3 file has " + mp3Stream.ChannelCount + " channels with a frequency of " +
                      mp3Stream.Frequency);

            byte[] convertedAudioData = convertedAudioStream.ToArray();

            //bug of mp3sharp that audio with 1 channel has right channel data, to skip them
            byte[] data = new byte[convertedAudioData.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                data[i] = convertedAudioData[2 * i];
                data[i + 1] = convertedAudioData[2 * i + 1];
            }

            Wav wav = new Wav(data, mp3Stream.ChannelCount, mp3Stream.Frequency);

            AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);

            //保存录音
            string outPath;
            WavUtility.FromAudioClip(audioClip,out outPath, true);

            return audioClip;
        }
#endif
    }
}
