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
    /// <summary>
    /// 用户解析token的json数据
    /// </summary>
    [Serializable]
    class TokenResponse
    {
        public string access_token = null;
    }

    public class Base
	{
		protected enum TokenFetchStatus
		{
			NotFetched,
			Fetching,
			Success,
			Failed
		}

		public string SecretKey { get; private set; }

		public string APIKey { get; private set; }

		public string Token { get; private set; }

		protected TokenFetchStatus tokenFetchStatus = TokenFetchStatus.NotFetched;

		public Base (string apiKey, string secretKey)
		{
			APIKey = apiKey;
			SecretKey = secretKey;
		}

		public IEnumerator GetAccessToken ()
		{
			Debug.Log ("Start fetching token...");
			tokenFetchStatus = TokenFetchStatus.Fetching;

			var uri =
				string.Format (
					"https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}",
					APIKey, SecretKey);
			var www = UnityWebRequest.Get(uri);
			yield return www.SendWebRequest();

			if (string.IsNullOrEmpty (www.error)) {
                Debug.Log(www.downloadHandler.text);
				var result = JsonUtility.FromJson<TokenResponse> (www.downloadHandler.text);
				Token = result.access_token;
				Debug.Log ("Token has been fetched successfully");
				tokenFetchStatus = TokenFetchStatus.Success;
			} else {
				Debug.LogError (www.error);
				Debug.LogError("Token was fetched failed. Please check your APIKey and SecretKey");
				tokenFetchStatus = TokenFetchStatus.Failed;
			}
		}

		protected IEnumerator PreAction ()
		{
			if (tokenFetchStatus == TokenFetchStatus.NotFetched) {
				Debug.Log ("Token has not been fetched, now fetching...");
				yield return GetAccessToken ();
			}

			if (tokenFetchStatus == TokenFetchStatus.Fetching)
				Debug.Log ("Token is still being fetched, waiting...");

			while (tokenFetchStatus == TokenFetchStatus.Fetching) {
				yield return null;
			}
		}
	}
}