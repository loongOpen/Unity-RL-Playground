using LLMUnity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Wit.BaiduAip.Speech;

public class SpeechToTextAndLLM : MonoBehaviour
{
    [Header("UI References")]
    public Button startAudioBtn;    // 按钮
    public Text resultText;         // 显示文本的框
    public Dropdown voiceDropdown; // 用于选择语音类型的下拉框

    [Header("Audio Settings")]
    public AudioSource audioSource; // 用来播放语音的 AudioSource
    private AudioClip recordedClip; // 录音的 AudioClip
    private bool isRecording = false;

    private string accessToken;

    // 百度语音识别配置
    private string apiKey = "pGuaQEWVNMTnWWQrFI76h4ok";
    private string secretKey = "Rdmq5orQ1kktzzbsmkHvHQHnL20vMx9K";
    private string cuid = "240a906f2b88794fd0426442c4136a5a57bf5c01";
    private int dev_pid = 1537;        // 普通话模型

    private Tts tts;  // 百度TTS实例
    private LLMCharacter llmCharacter; // LLM实例

    public string recognizedText = "";
    private Tts.Pronouncer selectedVoice;  // 当前选择的语音类型
    void Start()
    {
        // 获取 AccessToken
        StartCoroutine(GetAccessToken());

        // 初始化 TTS
        tts = new Tts(apiKey, secretKey);

        // 初始化 LLMCharacter
        llmCharacter = GetComponent<LLMCharacter>();

        // 按钮点击事件
        startAudioBtn.onClick.AddListener(OnRecordButtonClick);

        // 初始化下拉框选择事件
        voiceDropdown.onValueChanged.AddListener(OnVoiceSelected);
    }

    // 下拉框选择事件
    private void OnVoiceSelected(int index)
    {
        selectedVoice = GetPronouncerFromIndex(index);  // 更新选择的语音类型
    }
    // 将下拉框索引转换为 Pronouncer 枚举
    private Tts.Pronouncer GetPronouncerFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Tts.Pronouncer.Female;  // 普通女声
            case 1:
                return Tts.Pronouncer.Male;    // 普通男声
            case 2:
                return Tts.Pronouncer.Duxiaoyao; // 度逍遥
            case 3:
                return Tts.Pronouncer.Duyaya;   // 度丫丫
            default:
                return Tts.Pronouncer.Female;   // 默认返回普通女声
        }
    }

    private void OnRecordButtonClick()
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

    private void StartRecording()
    {
        isRecording = true;
        resultText.text = "录音中...";

        // 开始录音，时长10秒，采样率16000Hz
        recordedClip = Microphone.Start(null, false, 10, 16000);

        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
        }
        startAudioBtn.GetComponentInChildren<Text>().text = "停止录音";
    }

    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(null); // 停止录音

        resultText.text = "识别中...";

        // 录音完成后，开始语音识别
        StartCoroutine(RecognizeFromClip(recordedClip, onSuccess =>
        {
            // 在此处对识别到的文字进行处理 12.5号添加
            recognizedText = onSuccess;
            print(recognizedText);//打印出识别的文字

            resultText.text = "识别结果: " + onSuccess;

            // 语音识别成功后，将文本发送给 LLM
            StartCoroutine(SendToLLM(onSuccess, llmResponse =>
            {
                resultText.text = "LLM 回复: " + llmResponse;
                // LLM 回复后，将文本转为语音播放
                PlayTextToSpeech(llmResponse);
            },
            onError =>
            {
                resultText.text = "LLM 请求失败: " + onError;
            }));
        },
        onError =>
        {
            resultText.text = "识别失败: " + onError;
        }));

        startAudioBtn.GetComponentInChildren<Text>().text = "开始录音";
    }

    // 获取百度 AccessToken
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
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(request.downloadHandler.text);
                accessToken = tokenResponse.access_token;
                Debug.Log("Access Token 获取成功");
            }
            else
            {
                Debug.LogError("获取 Access Token 失败: " + request.error);
            }
        }
    }

    // 语音识别并返回文本
    public IEnumerator RecognizeFromClip(AudioClip clip, Action<string> onSuccess, Action<string> onError)
    {
        if (accessToken == null)
        {
            onError?.Invoke("AccessToken 未获取");
            yield break;
        }

        byte[] pcmData = ConvertClipToPCM16(clip);
        if (pcmData == null)
        {
            onError?.Invoke("音频格式错误");
            yield break;
        }

        string base64Audio = Convert.ToBase64String(pcmData);

        var requestData = new
        {
            format = "pcm",
            rate = 16000,
            channel = 1,
            cuid = cuid,
            token = accessToken,
            dev_pid = dev_pid,
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

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("网络错误: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text;
                var result = JsonConvert.DeserializeObject<ASRResponse>(responseText);
                if (result.err_no == 0)
                    onSuccess?.Invoke(string.Join("", result.result));
                else
                    onError?.Invoke($"识别失败: {result.err_msg}");
            }
        }
    }

    // 将识别的文本发送给 LLM 并获取回复
    public IEnumerator SendToLLM(string inputText, Action<string> onSuccess, Action<string> onError)
    {
        if (llmCharacter == null)
        {
            onError?.Invoke("LLMCharacter 组件未找到");
            yield break;
        }
        string finalText = "";
        yield return llmCharacter.Chat(
            inputText,
            partial =>
            {
                finalText = partial;
            },
            () =>
            {
                if (!string.IsNullOrEmpty(finalText))
                    onSuccess?.Invoke(finalText);
                else
                    onError?.Invoke("LLM 返回为空");
            }
            );
        //yield return llmCharacter.Chat(inputText, partial =>
        //{
        //    // 处理大模型的输出
        //    onSuccess?.Invoke(partial);
        //},
        //() =>
        //{
        //    onError?.Invoke("LLM 请求失败");
        //});
    }

    // 将 LLM 输出的文本转为语音并播放
    private void PlayTextToSpeech(string text)
    {
        StartCoroutine(tts.Synthesis(text, response =>
        {
            if (response.Success && response.clip != null)
            {
                audioSource.clip = response.clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("语音合成失败: " + response.err_msg);
            }
        }, per: selectedVoice)); // 使用用户选择的语音
    }

    // 语音格式转换方法，转为 PCM16 格式
    private byte[] ConvertClipToPCM16(AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        byte[] pcm = new byte[samples.Length * 2];
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
        public int err_no;
        public string err_msg;
        public string sn;
        public string[] result;
    }

    [Serializable]
    public class TokenResponse
    {
        public string access_token;
    }
}
