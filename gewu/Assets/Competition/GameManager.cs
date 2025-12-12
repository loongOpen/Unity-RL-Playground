using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoretext;
    public Transform ball;
    public Transform goalblue;
    public Transform goalpurple;
    public ArticulationBody player1;
    public ArticulationBody player2;
    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    
    // 音效
    public AudioClip backgroundMusic;  // 背景音效
    public AudioClip cheerSound;       // 欢呼声音效
    public AudioClip victorySound;    // 胜利音效
    
    // 胜利图片
    public Image bluewin;   // Blue获胜图片
    public Image purplewin; // Purple获胜图片
    
    // Start按钮
    public Button startButton;  // 开始按钮
    
    int scoreblue;
    int scorepurple;
    
    // 防止重复计分的标记
    private bool hasScoredBlue = false;
    private bool hasScoredPurple = false;
    
    // Collider引用，用于检测碰撞
    private Collider ballCollider;
    private Collider goalBlueCollider;
    private Collider goalPurpleCollider;
    
    // 球的初始位置和Rigidbody
    private Vector3 ballInitialPosition;
    private Rigidbody ballRigidbody;
    
    // 玩家的初始位置和旋转
    private Vector3 player1InitialPosition;
    private Quaternion player1InitialRotation;
    private Vector3 player2InitialPosition;
    private Quaternion player2InitialRotation;
    
    // 是否正在复位球
    private bool isResettingBall = false;
    
    // AudioSource组件
    private AudioSource audioSource;
    private AudioSource backgroundAudioSource;
    
    // 是否已经播放过胜利音效
    private bool hasPlayedVictorySound = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // 获取或创建AudioSource组件（用于欢呼和胜利音效）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // 设置音效音量更大（最大为1.0，但PlayOneShot可以使用volumeScale超过1.0）
        audioSource.volume = 1.0f;
        
        // 创建专门的背景音乐AudioSource（循环播放）
        backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        backgroundAudioSource.loop = true;
        backgroundAudioSource.playOnAwake = false;
        
        // 播放背景音效
        if (backgroundMusic != null && backgroundAudioSource != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.Play();
        }
        // 获取Collider组件
        if (ball != null)
        {
            ballCollider = ball.GetComponent<Collider>();
            if (ballCollider == null)
            {
                ballCollider = ball.GetComponentInChildren<Collider>();
            }
            
            // 记录球的初始位置
            ballInitialPosition = ball.position;
            
            // 获取Rigidbody组件（用于重置速度）
            ballRigidbody = ball.GetComponent<Rigidbody>();
            if (ballRigidbody == null)
            {
                ballRigidbody = ball.GetComponentInChildren<Rigidbody>();
            }
        }
        
        if (goalblue != null)
        {
            goalBlueCollider = goalblue.GetComponent<Collider>();
            if (goalBlueCollider == null)
            {
                goalBlueCollider = goalblue.GetComponentInChildren<Collider>();
            }
        }
        
        if (goalpurple != null)
        {
            goalPurpleCollider = goalpurple.GetComponent<Collider>();
            if (goalPurpleCollider == null)
            {
                goalPurpleCollider = goalpurple.GetComponentInChildren<Collider>();
            }
        }
        
        // 记录玩家的初始位置和旋转
        if (player1 != null)
        {
            player1InitialPosition = player1.transform.position;
            player1InitialRotation = player1.transform.rotation;
        }
        
        if (player2 != null)
        {
            player2InitialPosition = player2.transform.position;
            player2InitialRotation = player2.transform.rotation;
        }
        
        // 为Start按钮添加点击事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        player1.GetJointPositions(P0);
        player1.GetJointVelocities(W0);
        
        UpdateScoreText();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ball == null || goalblue == null || goalpurple == null)
            return;
        
        if (ballCollider == null || goalBlueCollider == null || goalPurpleCollider == null)
            return;
        
        // 如果正在复位球，暂停碰撞检测
        if (isResettingBall)
            return;
        
        // 检测 ball 和 goalblue 的接触（使用边界框重叠检测）
        if (IsColliding(ballCollider, goalBlueCollider))
        {
            if (!hasScoredBlue)
            {
                scorepurple++;
                hasScoredBlue = true;
                UpdateScoreText();
                
                // 播放欢呼声音效
                PlayCheerSound();
                
                // 检查是否达到3分，播放胜利音效并显示胜利图片
                if (scorepurple >= 3 && !hasPlayedVictorySound)
                {
                    PlayVictorySound();
                    hasPlayedVictorySound = true;
                    // 显示Purple获胜图片
                    ShowVictoryImage(true);
                }
                
                // 启动复位协程
                StartCoroutine(ResetBallAfterGoal());
            }
        }
        else
        {
            hasScoredBlue = false;
        }
        
        // 检测 ball 和 goalpurple 的接触（使用边界框重叠检测）
        if (IsColliding(ballCollider, goalPurpleCollider))
        {
            if (!hasScoredPurple)
            {
                scoreblue++;
                hasScoredPurple = true;
                UpdateScoreText();
                
                // 播放欢呼声音效
                PlayCheerSound();
                
                // 检查是否达到3分，播放胜利音效并显示胜利图片
                if (scoreblue >= 3 && !hasPlayedVictorySound)
                {
                    PlayVictorySound();
                    hasPlayedVictorySound = true;
                    // 显示Blue获胜图片
                    ShowVictoryImage(false);
                }
                
                // 启动复位协程
                StartCoroutine(ResetBallAfterGoal());
            }
        }
        else
        {
            hasScoredPurple = false;
        }
    }
    
    // 检测两个Collider是否接触（使用边界框重叠）
    bool IsColliding(Collider collider1, Collider collider2)
    {
        if (collider1 == null || collider2 == null)
            return false;
        
        // 使用边界框（Bounds）检测重叠
        Bounds bounds1 = collider1.bounds;
        Bounds bounds2 = collider2.bounds;
        
        return bounds1.Intersects(bounds2);
    }
    
    // 进球后等待1秒，然后复位球和玩家
    IEnumerator ResetBallAfterGoal()
    {
        isResettingBall = true;
        
        // 等待1秒
        yield return new WaitForSeconds(1.0f);
        
        // 复位球和玩家
        ResetBallAndPlayers();
        
        isResettingBall = false;
    }
    
    void UpdateScoreText()
    {
        if (scoretext != null)
        {
            scoretext.text = $"{scorepurple}         {scoreblue}";
        }
    }
    
    // 播放欢呼声音效（音量更大）
    void PlayCheerSound()
    {
        if (cheerSound != null && audioSource != null)
        {
            // 使用volumeScale参数增加音量（1.5倍）
            audioSource.PlayOneShot(cheerSound, 5f);
        }
    }
    
    // 播放胜利音效（音量更大）
    void PlayVictorySound()
    {
        if (victorySound != null && audioSource != null)
        {
            // 使用volumeScale参数增加音量（1.5倍）
            audioSource.PlayOneShot(victorySound, 5f);
        }
    }
    
    // 显示胜利图片
    // isPurple: true表示Purple获胜，false表示Blue获胜
    void ShowVictoryImage(bool isPurple)
    {
        if (isPurple)
        {
            // Purple获胜，显示purplewin，隐藏bluewin
            if (purplewin != null)
            {
                purplewin.gameObject.SetActive(true);
            }
            if (bluewin != null)
            {
                bluewin.gameObject.SetActive(false);
            }
        }
        else
        {
            // Blue获胜，显示bluewin，隐藏purplewin
            if (bluewin != null)
            {
                bluewin.gameObject.SetActive(true);
            }
            if (purplewin != null)
            {
                purplewin.gameObject.SetActive(false);
            }
        }
    }
    
    // 复位球和玩家到初始位置
    void ResetBallAndPlayers()
    {
        // 复位球的位置
        if (ball != null)
        {
            ball.position = ballInitialPosition;
            
            // 如果有Rigidbody，重置速度和角速度
            if (ballRigidbody != null)
            {
                ballRigidbody.velocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
            }
        }
        
        // 使用TeleportRoot复位玩家1
        if (player1 != null)
        {
            player1.TeleportRoot(player1InitialPosition, player1InitialRotation);
            // 重置速度
            player1.velocity = Vector3.zero;
            player1.angularVelocity = Vector3.zero;
            player1.SetJointPositions(P0);
            player1.SetJointVelocities(W0);
        }
        
        // 使用TeleportRoot复位玩家2
        if (player2 != null)
        {
            player2.TeleportRoot(player2InitialPosition, player2InitialRotation);
            // 重置速度
            player2.velocity = Vector3.zero;
            player2.angularVelocity = Vector3.zero;
            player2.SetJointPositions(P0);
            player2.SetJointVelocities(W0);
        }
        
        // 重置计分标记
        hasScoredBlue = false;
        hasScoredPurple = false;
    }
    
    // Start按钮点击事件处理
    void OnStartButtonClicked()
    {
        // 隐藏胜利图片
        if (bluewin != null)
        {
            bluewin.gameObject.SetActive(false);
        }
        
        if (purplewin != null)
        {
            purplewin.gameObject.SetActive(false);
        }
        
        // 将比分清零
        scoreblue = 0;
        scorepurple = 0;
        
        // 重置胜利音效标记
        hasPlayedVictorySound = false;
        
        // 复位球和玩家
        ResetBallAndPlayers();
        
        // 更新分数显示
        UpdateScoreText();
    }
}
