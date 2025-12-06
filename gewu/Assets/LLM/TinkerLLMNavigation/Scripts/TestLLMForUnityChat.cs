using LLMUnity;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;   // 若用 TMP：using TMPro;
using Wit.BaiduAip.Speech;

public class TestLLMForUnityChat : MonoBehaviour
{
    [Header("LLM for Unity")]
    public LLMCharacter llmCharacter;

    [Header("UI References")]
    public Text inputText;
    public Text answerText; 
    public Button sendButton;

    [Header("Typewriter Settings")]
    [Tooltip("每个字符的时间间隔（秒/字符）")]
    public float typingSpeed = 0.03f;
    [Tooltip("新请求到来时是否清空旧回答")]
    public bool clearOnNewRequest = true;
    private Tts tts;

    // —— 打字机内部状态 ——
    private readonly StringBuilder _buffer = new StringBuilder(); // 当前应显示的“完整文本”
    private int _shownCount = 0;                                   // 已显示到 UI 的字符数
    private Coroutine _typingCo;
    private bool _streamFinished = false;

    // —— 处理 partial 的关键状态 ——
    private string _lastFull = "";   // 上一次收到的“完整前缀”
    private bool _chatInProgress = false;


    void Start()
    {
        tts = new Tts("MH12KtETqHzMM1E8T8Ncr8t9", "jZzroIRC1ouXJtwu08hrMJhpKwZyFuUr");
        if (sendButton != null)
            sendButton.onClick.AddListener(OnClickSend);

        if (answerText != null)
            answerText.text = "";
    }

    private void OnClickSend()
    {
        if (_chatInProgress) return; // 防止并发

        string message = inputText != null ? inputText.text : "";
        if (string.IsNullOrWhiteSpace(message)) return;

        if (clearOnNewRequest && answerText != null)
            answerText.text = "";

        StartChat(message);

        if (inputText != null) inputText.text = "";
    }

    private async void StartChat(string message)
    {
        // 每次新对话前重置
        ResetTypewriter();
        _lastFull = "";
        _chatInProgress = true;
        if (sendButton) sendButton.interactable = false;

        try
        {
            await llmCharacter.Chat(
                message,
                partial =>
                {
                    if (string.IsNullOrEmpty(partial)) return;

                    // 绝大多数实现：partial 是“到目前为止的完整前缀”
                    if (partial.StartsWith(_lastFull))
                    {
                        string delta = partial.Substring(_lastFull.Length); // 只取新增
                        AppendDelta(delta);
                        _lastFull = partial;
                    }
                    else
                    {
                        // 兜底：直接覆盖到最新全量
                        OverrideTo(partial);
                        _lastFull = partial;
                    }
                },
                () =>
                {
                    _streamFinished = true;
                    Debug.Log("[AI] 完成");
                    // 将生成的文本转换为语音并播放
                    PlayTextToSpeech(_buffer.ToString());
                }
            );
        }
        finally
        {
            _chatInProgress = false;
            if (sendButton) sendButton.interactable = true;
        }
    }
    private void PlayTextToSpeech(string text)
    {
        // 调用TTS接口合成语音并播放
        StartCoroutine(tts.Synthesis(text, response =>
        {
            if (response.Success && response.clip != null)
            {
                // 播放生成的语音
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.clip = response.clip;
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogError("语音合成失败: " + response.err_msg);
            }
        }));
    }
    // 仅把“增量”追加到缓冲，不重启协程
    private void AppendDelta(string delta)
    {
        if (string.IsNullOrEmpty(delta)) return;

        _buffer.Append(delta);
        if (_typingCo == null)
            _typingCo = StartCoroutine(TypewriterLoop());
    }

    // 覆盖到最新全量文本（处理乱序/实现差异）
    private void OverrideTo(string full)
    {
        _buffer.Clear();
        _buffer.Append(full);
        _shownCount = Mathf.Min(_shownCount, _buffer.Length);

        if (_typingCo == null)
            _typingCo = StartCoroutine(TypewriterLoop());
    }

    private void ResetTypewriter()
    {
        _buffer.Clear();
        _shownCount = 0;
        _streamFinished = false;

        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }

        if (answerText != null)
            answerText.text = "";
    }

    private IEnumerator TypewriterLoop()
    {
        while (true)
        {
            if (_shownCount < _buffer.Length)
            {
                _shownCount++;
                if (answerText != null)
                    answerText.text = _buffer.ToString(0, _shownCount);

                yield return new WaitForSeconds(typingSpeed);
            }
            else
            {
                if (_streamFinished)
                {
                    _typingCo = null;
                    yield break;
                }
                yield return null;
            }
        }
    }

    private void OnDisable()
    {
        llmCharacter?.CancelRequests();

        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }
    }
}
