using UnityEngine;
using UnityEngine.UI;
using Wit.BaiduAip.Speech;

public class TtsDemo : MonoBehaviour
{
    public string APIKey = "";
    public string SecretKey = "";
    public Button SynthesisButton;
    public Text Input;

    private Tts _asr;
    private AudioSource _audioSource;
    private bool _startPlaying;

    void Start()
    {
        _asr = new Tts(APIKey, SecretKey);
        StartCoroutine(_asr.GetAccessToken());

        _audioSource = gameObject.AddComponent<AudioSource>();


        SynthesisButton.onClick.AddListener(OnClickSynthesisButton);
    }

    private void OnClickSynthesisButton()
    {
        SynthesisButton.gameObject.SetActive(false);

        StartCoroutine(_asr.Synthesis(Input.text, s =>
        {
            if (s.Success)
            {
                _audioSource.clip = s.clip;
                _audioSource.Play();

                _startPlaying = true;
            }
            else
            {
				SynthesisButton.gameObject.SetActive(true);
            }
        }));
    }

    void Update()
    {
        if (_startPlaying)
        {
            if (!_audioSource.isPlaying)
            {
                _startPlaying = false;
                SynthesisButton.gameObject.SetActive(true);
            }
        }
    }
}
