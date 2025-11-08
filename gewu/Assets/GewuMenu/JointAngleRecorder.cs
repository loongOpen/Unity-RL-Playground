using UnityEngine;
using XCharts.Runtime;
using System.Collections.Generic;

[System.Serializable]
public class JointInfo
{
    public ArticulationBody joint;
    public bool isSelected = true;
    public string jointName;
    
    public JointInfo(ArticulationBody j)
    {
        joint = j;
        jointName = j.gameObject.name;
        isSelected = true;
    }
}

[RequireComponent(typeof(ArticulationBody))]
public class JointAngleRecorder : MonoBehaviour
{
    public LineChart lineChart;
    public int maxDataCount = 100;
    
    [Header("纵轴设置")]
    public bool yAxisAutoRange = true; // 纵轴是否自动调节范围
    public float yAxisMin = -180f; // 纵轴最小值（度）
    public float yAxisMax = 180f; // 纵轴最大值（度）
    
    [Header("背景设置")]
    [Range(0f, 1f)]
    public float backgroundAlpha = 1f; // 背景透明度（0=完全透明，1=完全不透明）
    
    [SerializeField]
    public List<JointInfo> jointInfos = new List<JointInfo>();
    
    private ArticulationBody[] joints; // 只包含选中的关节
    private float time;
    private float firstDataTime = -1f; // 第一个数据点的绝对时间
    private Queue<float> timeQueue = new Queue<float>(); // 存储每个数据点的绝对时间

    void Start()
    {
        // 如果关节列表为空，初始化它
        if (jointInfos.Count == 0)
        {
            InitializeJointInfos();
        }
        
        // 根据选中状态筛选关节
        UpdateSelectedJoints();
        
        // 为每个选中的关节创建曲线
        for (int i = 0; i < joints.Length; i++)
        {
            var serie = lineChart.AddSerie<Line>(joints[i].gameObject.name);
            serie.symbol.show = false;
            // 不使用 maxCache，手动管理数据点以控制横坐标起点
        }
        
        // 设置坐标轴
        var xAxis = lineChart.GetChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value;
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom; // 使用自定义范围以实现滚动效果
        xAxis.axisLabel.numericFormatter = "F2"; // 横坐标显示两位小数
        
        var yAxis = lineChart.GetChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "关节角(°)";
        
        // 设置纵轴范围
        if (yAxisAutoRange)
        {
            yAxis.minMaxType = Axis.AxisMinMaxType.MinMax; // 自动调节范围
        }
        else
        {
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom; // 手动设置范围
            yAxis.min = yAxisMin;
            yAxis.max = yAxisMax;
        }
        
        // 设置背景透明度
        UpdateBackgroundAlpha();
    }
    
    void UpdateBackgroundAlpha()
    {
        if (lineChart == null) return;
        
        // 确保 Background 组件存在
        var background = lineChart.EnsureChartComponent<Background>();
        if (background != null)
        {
            // 关闭自动颜色，使用手动设置的颜色
            background.autoColor = false;
            
            // 获取当前背景颜色（如果之前没有设置过，使用主题背景色）
            var color = background.imageColor;
            if (color.a == 0 && backgroundAlpha > 0)
            {
                // 如果颜色是透明的，使用主题背景色作为基础
                color = lineChart.theme.backgroundColor;
            }
            
            // 设置透明度
            color.a = backgroundAlpha;
            background.imageColor = color;
            background.show = true; // 确保背景显示
            
            // 刷新图表（运行时和编辑器模式都支持）
            lineChart.RefreshChart();
            
#if UNITY_EDITOR
            // 在编辑器模式下，标记场景为已修改
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(lineChart);
            }
#endif
        }
    }
    
    void OnValidate()
    {
        // 在 Inspector 中修改值时立即更新背景透明度（运行时和编辑器模式都生效）
        if (lineChart != null)
        {
            UpdateBackgroundAlpha();
        }
    }
    
    public void InitializeJointInfos()
    {
        jointInfos.Clear();
        
        // 获取所有子节点的ArticulationBody（排除自身）
        var allJoints = GetComponentsInChildren<ArticulationBody>();
        
        foreach (var joint in allJoints)
        {
            // 排除自身，只记录旋转关节
            if (joint != GetComponent<ArticulationBody>() && 
                joint.jointType == ArticulationJointType.RevoluteJoint)
            {
                // 排除包含 hip、knee、ankle、hand 的关节
                string jointName = joint.gameObject.name.ToLowerInvariant();
                if (!jointName.Contains("hip") && 
                    !jointName.Contains("knee") && 
                    !jointName.Contains("ankle") && 
                    !jointName.Contains("hand"))
                {
                    jointInfos.Add(new JointInfo(joint));
                }
            }
        }
    }
    
    void UpdateSelectedJoints()
    {
        var selectedList = new List<ArticulationBody>();
        foreach (var info in jointInfos)
        {
            if (info.isSelected && info.joint != null)
            {
                selectedList.Add(info.joint);
            }
        }
        joints = selectedList.ToArray();
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        
        // 记录第一个数据点的时间
        if (firstDataTime < 0f)
        {
            firstDataTime = time;
        }
        
        // 检查是否需要移除最早的数据点
        bool needRemove = false;
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].dofCount > 0 && lineChart.GetSerie(i).dataCount >= maxDataCount)
            {
                needRemove = true;
                break;
            }
        }
        
        // 如果数据点超过限制，移除最早的数据点
        if (needRemove)
        {
            // 移除所有系列的第一个数据点
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].dofCount > 0 && lineChart.GetSerie(i).dataCount > 0)
                {
                    lineChart.GetSerie(i).RemoveData(0);
                }
            }
            
            // 从队列中移除最早的时间
            if (timeQueue.Count > 0)
            {
                timeQueue.Dequeue();
            }
        }
        
        // 添加新数据点（使用绝对时间作为横坐标）
        timeQueue.Enqueue(time); // 记录当前时间
        for (int i = 0; i < joints.Length; i++)
        {
            // 安全检查：确保关节有自由度
            if (joints[i].dofCount > 0)
            {
                float angle = joints[i].jointPosition[0] * Mathf.Rad2Deg;
                lineChart.AddData(i, time, angle);
            }
        }
        
        // 更新横坐标范围以实现滚动效果
        var xAxis = lineChart.GetChartComponent<XAxis>();
        if (timeQueue.Count > 0)
        {
            // 使用队列中的最早时间作为窗口起点，当前时间作为窗口终点
            xAxis.min = timeQueue.Peek();
            xAxis.max = time;
        }
        else
        {
            // 如果没有数据，使用第一个数据点时间
            xAxis.min = firstDataTime;
            xAxis.max = time;
        }
        
        // 更新纵坐标范围
        var yAxis = lineChart.GetChartComponent<YAxis>();
        if (yAxisAutoRange)
        {
            yAxis.minMaxType = Axis.AxisMinMaxType.MinMax; // 自动调节范围
        }
        else
        {
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom; // 手动设置范围
            yAxis.min = yAxisMin;
            yAxis.max = yAxisMax;
        }
        
        lineChart.RefreshChart();
    }
}
