using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpJointPublisher : MonoBehaviour
{
    public JointAngleMonitor monitor;
    public string remoteIp = "127.0.0.1";
    public int remotePort = 18888;
    public bool useFixedUpdate = true;
    
    [Header("手掌数据发布")]
    [Tooltip("是否发布手掌数据")]
    public bool publishHandData = true;
    
    [Tooltip("手掌数据发布端口（与手臂数据分开）")]
    public int handDataPort = 18889;

    private UdpClient client;
    private UdpClient handClient;
    private string cachedNamesJson = null;
    private string cachedHandNamesJson = null;
    private bool printedHandDebug = false;

    void Start()
    {
        client = new UdpClient();
        if (publishHandData)
        {
            handClient = new UdpClient();
        }
        CacheNames();
        CacheHandNames();
    }

    void OnDestroy()
    {
        client?.Dispose();
        handClient?.Dispose();
    }

    void CacheNames()
    {
        if (monitor == null) return;
        var names = monitor.GetJointNames();
        var sb = new StringBuilder();
        sb.Append("{\"names\":[");
        for (int i = 0; i < names.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('"').Append(names[i]).Append('"');
        }
        sb.Append("],\"rad\":[]}");
        cachedNamesJson = sb.ToString();
    }

    void CacheHandNames()
    {
        if (monitor == null || !publishHandData) return;
        
        var sb = new StringBuilder();
        sb.Append("{\"hand_names\":[");
        
        // 左手关节名称
        string[] leftHandNames = {
            "left_hand_thumb_0_link", "left_hand_thumb_1_link", "left_hand_thumb_2_link",
            "left_hand_middle_0_link", "left_hand_middle_1_link", 
            "left_hand_index_0_link", "left_hand_index_1_link"
        };
        
        // 右手关节名称
        string[] rightHandNames = {
            "right_hand_thumb_0_link", "right_hand_thumb_1_link", "right_hand_thumb_2_link",
            "right_hand_middle_0_link", "right_hand_middle_1_link", 
            "right_hand_index_0_link", "right_hand_index_1_link"
        };
        
        // 添加左手名称
        for (int i = 0; i < leftHandNames.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('"').Append(leftHandNames[i]).Append('"');
        }
        
        // 添加右手名称
        for (int i = 0; i < rightHandNames.Length; i++)
        {
            sb.Append(',');
            sb.Append('"').Append(rightHandNames[i]).Append('"');
        }
        
        sb.Append("],\"left_hand\":[],\"right_hand\":[]}");
        cachedHandNamesJson = sb.ToString();
    }

    void FixedUpdate()
    {
        if (useFixedUpdate) 
        {
            Publish();
            if (publishHandData) PublishHandData();
        }
    }

    void Update()
    {
        if (!useFixedUpdate) 
        {
            Publish();
            if (publishHandData) PublishHandData();
        }
    }

    void Publish()
    {
        if (monitor == null || client == null) return;
        var rad = monitor.GetAnglesRad();
        if (cachedNamesJson == null) CacheNames();

        var sb = new StringBuilder(cachedNamesJson);
        // 替换尾部空数组
        int idx = sb.ToString().LastIndexOf("[]}");
        if (idx >= 0)
        {
            sb.Remove(idx, 3);
            sb.Append('[');
            for (int i = 0; i < rad.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(rad[i].ToString("F6"));
            }
            sb.Append("]}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        client.Send(bytes, bytes.Length, remoteIp, remotePort);
    }

    void PublishHandData()
    {
        if (monitor == null || handClient == null || !publishHandData) return;
        
        var leftHandAngles = monitor.GetLeftHandAnglesNormalized();
        var rightHandAngles = monitor.GetRightHandAnglesNormalized();
        
        // 直接构建JSON字符串，避免复杂的字符串替换
        var sb = new StringBuilder();
        sb.Append("{\"hand_names\":[");
        
        // 左手关节名称
        string[] leftHandNames = {
            "left_hand_thumb_0_link", "left_hand_thumb_1_link", "left_hand_thumb_2_link",
            "left_hand_middle_0_link", "left_hand_middle_1_link", 
            "left_hand_index_0_link", "left_hand_index_1_link"
        };
        
        // 右手关节名称
        string[] rightHandNames = {
            "right_hand_thumb_0_link", "right_hand_thumb_1_link", "right_hand_thumb_2_link",
            "right_hand_middle_0_link", "right_hand_middle_1_link", 
            "right_hand_index_0_link", "right_hand_index_1_link"
        };
        
        // 添加左手名称
        for (int i = 0; i < leftHandNames.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('"').Append(leftHandNames[i]).Append('"');
        }
        
        // 添加右手名称
        for (int i = 0; i < rightHandNames.Length; i++)
        {
            sb.Append(',');
            sb.Append('"').Append(rightHandNames[i]).Append('"');
        }
        
        // 添加左手数据
        sb.Append("],\"left_hand\":[");
        for (int i = 0; i < leftHandAngles.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(leftHandAngles[i].ToString("F6"));
        }
        
        // 添加右手数据
        sb.Append("],\"right_hand\":[");
        for (int i = 0; i < rightHandAngles.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(rightHandAngles[i].ToString("F6"));
        }
        
        sb.Append("]}");

        var jsonString = sb.ToString();
        var bytes = Encoding.UTF8.GetBytes(jsonString);
        handClient.Send(bytes, bytes.Length, remoteIp, handDataPort);
        
        // 调试输出（仅第一次）
        if (!printedHandDebug)
        {
            Debug.Log($"[UdpJointPublisher] Sending hand data: {jsonString}");
            Debug.Log($"[UdpJointPublisher] Left hand angles: [{string.Join(", ", leftHandAngles.Select(x => x.ToString("F3")))}]");
            Debug.Log($"[UdpJointPublisher] Right hand angles: [{string.Join(", ", rightHandAngles.Select(x => x.ToString("F3")))}]");
            printedHandDebug = true;
        }
    }
}


