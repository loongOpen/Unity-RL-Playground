using Assimp.Configs;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Recorder.Timeline;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ASR : MonoBehaviour
{
    public static ASR instance;
    private void Awake()
    {
        instance = this;
    }

    [Header("百度语音识别配置")]
    [SerializeField] private string apiKey = "pGuaQEWVNMTnWWQrFI76h4ok";
    [SerializeField] private string secretKey = "Rdmq5orQ1kktzzbsmkHvHQHnL20vMx9K";
    [SerializeField] private string accessToken;
    [Space]

    [Header("短语音识别标准版参数设置")]
    [SerializeField] private string format = "pcm";
    [SerializeField] private int rate = 16000;
    [SerializeField] private int channel = 1;
    [SerializeField] private string cuid = "240a906f2b88794fd0426442c4136a5a57bf5c01";
    [SerializeField] private int dev_pid = 1537;
    [Space]
    [Space]

    [Header("UI相关")]
    //public Button buttonStartASR;
    public Button buttonRecord;
    public Text textResult;

    [Header("录音配置")]
    public AudioSource audioSource;
    public AudioClip recordedClip;
    private bool isRecording;



    // Start is called before the first frame update
    void Start()
    {
        //一开始就进行鉴权
        StartCoroutine(GetAccessToken());
        //开始识别
        //buttonStartASR.onClick.AddListener(() =>
        //{
        //    print("开始识别");
        //    StartCoroutine(RecognizeFromClip(audioSource.clip,
        //        onSuccess => { textResult.text = onSuccess; },
        //        onError => { Debug.Log(onError); }));
        //});

        //录音
        buttonRecord.onClick.AddListener(() =>
        {
            ToggleRecording();
        });
       buttonRecord.transform.GetChild(0).GetComponent<Text>().text = "开始录音";
    }
    /// <summary>
    /// 开始/停止录音
    /// </summary>
    private void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }
    /// <summary>
    /// 开始录音
    /// </summary>
    private void StartRecording()
    {
        isRecording = true;
        textResult.text = "录音中...";

        // 开始录音，录音时长为10秒，采样率为16000Hz
        recordedClip = Microphone.Start(null, false, 10, 16000);

        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
        }
        buttonRecord.transform.GetChild(0).GetComponent<Text>().text = "停止录音";

    }
    /// <summary>
    /// 停止录音
    /// </summary>
    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(null); // 停止录音

        textResult.text = "识别中...";

        // 录音完成后，开始识别音频
        StartCoroutine(RecognizeFromClip(recordedClip,
            onSuccess => { textResult.text = onSuccess; },
            onError => { textResult.text = "识别失败: " + onError; }
        ));
        buttonRecord.transform.GetChild(0).GetComponent<Text>().text = "开始录音";

    }

    #region 短语音识别相关

    /// <summary>
    /// 短语音识别方法
    /// </summary>
    /// <param name="clip">待识别语音</param>
    /// <param name="onSuccess">识别成功返回结果（文本）</param>
    /// <param name="onError">识别成功返回问题</param>
    /// <returns></returns>
    public IEnumerator RecognizeFromClip(AudioClip clip, Action<string> onSuccess, Action<string> onError)
    {
        if (accessToken == null)
        {
            onError?.Invoke("accessToken未获取");
            yield break;
        }

        // 转换 clip 为 PCM 数据（16bit）
        byte[] pcmData = ConvertClipToPCM16(clip);
        if (pcmData == null)
        {
            onError?.Invoke("音频格式错误或转换失败");
            yield break;
        }

        string base64Audio = Convert.ToBase64String(pcmData);

        var requestData = new
        {
            format = format,
            rate = rate,
            channel = channel,
            cuid = cuid,
            token = accessToken,
            dev_pid = dev_pid,// 普通话输入法模型
            speech = base64Audio,
            len = pcmData.Length
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://vop.baidu.com/server_api", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("网络错误: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("🎤 识别结果: " + responseText);

                var result = JsonConvert.DeserializeObject<ASRResponse>(responseText);
                if (result.err_no == 0)
                    onSuccess?.Invoke(string.Join("", result.result));
                else
                    onError?.Invoke($"识别失败（错误码{result.err_no}）：{result.err_msg}");
            }
        }
    }

    /// <summary>
    /// 语音格式转换方法，转为 PCM16 格式
    /// </summary>
    /// <param name="clip">需要转换的音频</param>
    /// <returns>返回转换后的音频结果</returns>
    // 将 AudioClip 转为 PCM16 格式
    private byte[] ConvertClipToPCM16(AudioClip clip)
    {
        if (clip.channels != 1 || clip.frequency != 16000)
        {
            Debug.LogError("❌ 仅支持 16kHz 单通道音频");
            return null;
        }

        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        byte[] pcm = new byte[samples.Length * 2]; // 16-bit = 2 bytes
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(samples[i] * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(value);
            pcm[i * 2] = bytes[0];
            pcm[i * 2 + 1] = bytes[1];
        }
        return pcm;
    }

    [Serializable]
    public class ASRResponse
    {
        /// <summary>
        /// 短文本语音识别返回结构
        /// </summary>
        public int err_no;
        public string err_msg;
        public string sn;
        public string[] result;
    }
    #endregion


    #region 鉴权相关
    /// <summary>
    /// 鉴权方法
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// 获取百度 AccessToken（已使用 using 自动释放资源）
    /// </summary>
    public IEnumerator GetAccessToken()
    {
        string url = "https://aip.baidubce.com/oauth/2.0/token";

        WWWForm form = new WWWForm();
        form.AddField("grant_type", "client_credentials");
        form.AddField("client_id", apiKey);
        form.AddField("client_secret", secretKey);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(request.downloadHandler.text);
                    accessToken = tokenResponse.access_token;
                    Debug.Log("✅ 短语音识别获取 AccessToken 成功: " + accessToken);
                }
                catch (Exception ex)
                {
                    Debug.LogError("❌ 短语音识别AccessToken 解析失败: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("❌ 短语音识别获取 AccessToken 失败: " + request.error);
            }
        }
    }

    [Serializable]
    public class TokenResponse
    {
        /// <summary>
        /// 鉴权返回结构
        /// </summary>
        public string access_token;
    }
    #endregion
}
