/*
 * 作者：关尔
 * 时间：2017年12月1日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Wit.BaiduAip.Speech
{
    [Serializable]
    public class AsrResponse
    {
        public int err_no;
        public string err_msg;
        public string sn;
        public string[] result;
    }

    public class Asr : Base
    {
        private const string UrlAsr = "https://vop.baidu.com/server_api";

        public Asr(string apiKey, string secretKey) : base(apiKey, secretKey)
        {
        }

        public IEnumerator Recognize(byte[] data, Action<AsrResponse> callback)
        {
			yield return PreAction ();

			if (tokenFetchStatus == Base.TokenFetchStatus.Failed) {
				Debug.LogError("Token fetched failed, please check your APIKey and SecretKey");
				yield break;
			}

            var uri = string.Format("{0}?lan=zh&cuid={1}&token={2}", UrlAsr, SystemInfo.deviceUniqueIdentifier, Token);

            var form = new WWWForm();
            form.AddBinaryData("audio", data);
            var www = UnityWebRequest.Post(uri, form);
            www.SetRequestHeader("Content-Type", "audio/pcm;rate=16000");
            yield return www.SendWebRequest();

            if (string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.downloadHandler.text);
                callback(JsonUtility.FromJson<AsrResponse>(www.downloadHandler.text));
            }
            else
                Debug.LogError(www.error);
        }

        /// <summary>
        /// 将Unity的AudioClip数据转化为PCM格式16bit数据
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static byte[] ConvertAudioClipToPCM16(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            var samples_int16 = new short[samples.Length];

            for (var index = 0; index < samples.Length; index++)
            {
                var f = samples[index];
                samples_int16[index] = (short) (f * short.MaxValue);
            }

            var byteArray = new byte[samples_int16.Length * 2];
            Buffer.BlockCopy(samples_int16, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }
    }
}