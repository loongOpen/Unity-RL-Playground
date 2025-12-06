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
public class RobotPlot : MonoBehaviour
{
    public Transform robot;
    public ArticulationBody rootArticulationBody; // Auto-filled
    
    public LineChart jointAngleChart;
    public LineChart eulerAngleChart;
    public LineChart linearVelocityChart;
    
    public int maxDataCount = 500;
    
    [Range(0f, 1f)]
    public float backgroundAlpha = 1f; // 0=fully transparent, 1=fully opaque
    
    [SerializeField]
    public List<JointInfo> jointInfos = new List<JointInfo>();
    
    private ArticulationBody[] joints; // 只包含选中的关节
    private float time;
    private float firstDataTime = -1f; // 第一个数据点的绝对时间
    private Queue<float> timeQueue = new Queue<float>(); // 存储每个数据点的绝对时间
    private Queue<float> eulerTimeQueue = new Queue<float>(); // 欧拉角时间队列
    private Queue<float> velocityTimeQueue = new Queue<float>(); // 线速度时间队列

    void Start()
    {
        // 如果关节列表为空，且 robot 已设置，自动初始化
        if (jointInfos.Count == 0 && robot != null)
        {
            InitializeJointInfos();
        }
        
        // 根据选中状态筛选关节
        UpdateSelectedJoints();
        
        // 确保 Root ArticulationBody 已设置
        if (robot != null && rootArticulationBody == null)
        {
            // 如果还没有设置，尝试从 robot 节点获取
            rootArticulationBody = robot.GetComponent<ArticulationBody>();
        }
        
        // 设置关节角图表
        if (jointAngleChart != null)
        {
            // 为每个选中的关节创建曲线
            for (int i = 0; i < joints.Length; i++)
            {
                var serie = jointAngleChart.AddSerie<Line>(joints[i].gameObject.name);
                serie.symbol.show = false;
            }
            SetupChart(jointAngleChart, "Joint Angle (°)");
        }
        
        // 设置欧拉角图表
        if (eulerAngleChart != null)
        {
            eulerAngleChart.AddSerie<Line>("X").symbol.show = false;
            eulerAngleChart.AddSerie<Line>("Y").symbol.show = false;
            eulerAngleChart.AddSerie<Line>("Z").symbol.show = false;
            SetupChart(eulerAngleChart, "Euler Angle (°)");
        }
        
        // 设置线速度图表
        if (linearVelocityChart != null)
        {
            linearVelocityChart.AddSerie<Line>("X").symbol.show = false;
            linearVelocityChart.AddSerie<Line>("Y").symbol.show = false;
            linearVelocityChart.AddSerie<Line>("Z").symbol.show = false;
            SetupChart(linearVelocityChart, "Linear Velocity (m/s)");
        }
        
        // 设置背景透明度
        UpdateBackgroundAlpha();
    }
    
    void SetupChart(LineChart chart, string yAxisName)
    {
        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value;
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        xAxis.axisLabel.numericFormatter = "F2";
        
        var yAxis = chart.GetChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = yAxisName;
        yAxis.axisLabel.numericFormatter = "F2"; // 纵轴显示两位小数
        
        // 设置图表标题为英文
        var title = chart.EnsureChartComponent<Title>();
        if (yAxisName.Contains("Joint Angle"))
        {
            title.text = "Joint Angle";
        }
        else if (yAxisName.Contains("Euler Angle"))
        {
            title.text = "Euler Angle";
        }
        else if (yAxisName.Contains("Linear Velocity"))
        {
            title.text = "Linear Velocity";
        }
        title.show = true;
        
        // 使用自动范围
        yAxis.minMaxType = Axis.AxisMinMaxType.MinMax;
    }
    
    void UpdateBackgroundAlpha()
    {
        UpdateChartBackgroundAlpha(jointAngleChart);
        UpdateChartBackgroundAlpha(eulerAngleChart);
        UpdateChartBackgroundAlpha(linearVelocityChart);
    }
    
    void UpdateChartBackgroundAlpha(LineChart chart)
    {
        if (chart == null) return;
        
        // 确保 Background 组件存在
        var background = chart.EnsureChartComponent<Background>();
        if (background != null)
        {
            // 关闭自动颜色，使用手动设置的颜色
            background.autoColor = false;
            
            // 获取当前背景颜色（如果之前没有设置过，使用主题背景色）
            var color = background.imageColor;
            if (color.a == 0 && backgroundAlpha > 0)
            {
                // 如果颜色是透明的，使用主题背景色作为基础
                color = chart.theme.backgroundColor;
            }
            
            // 设置透明度
            color.a = backgroundAlpha;
            background.imageColor = color;
            background.show = true; // 确保背景显示
            
            // 刷新图表（运行时和编辑器模式都支持）
            chart.RefreshChart();
            
#if UNITY_EDITOR
            // 在编辑器模式下，标记场景为已修改
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(chart);
            }
#endif
        }
    }
    
    void OnValidate()
    {
        // 在 Inspector 中修改值时立即更新背景透明度（运行时和编辑器模式都生效）
        UpdateBackgroundAlpha();
        
        // 如果 robot 已设置但关节列表为空，自动初始化（仅在编辑器模式下）
#if UNITY_EDITOR
        if (robot != null && (jointInfos == null || jointInfos.Count == 0))
        {
            // 使用延迟调用，避免在序列化过程中修改
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (robot != null && (jointInfos == null || jointInfos.Count == 0))
                {
                    InitializeJointInfos();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            };
        }
#endif
    }
    
    public void InitializeJointInfos()
    {
        jointInfos.Clear();
        
        // 如果指定了 robot，从 robot 的子节点中检测
        // 否则从当前对象的子节点中检测（排除自身）
        ArticulationBody[] allJoints;
        
        if (robot != null)
        {
            // 从指定的 robot Transform 的子节点中获取所有 ArticulationBody
            allJoints = robot.GetComponentsInChildren<ArticulationBody>();
            
            // 自动设置根节点 ArticulationBody：使用第一个找到的 ArticulationBody
            if (allJoints.Length > 0)
            {
                rootArticulationBody = allJoints[0];
            }
        }
        else
        {
            // 从当前对象的子节点中获取（排除自身）
            allJoints = GetComponentsInChildren<ArticulationBody>();
        }
        
        ArticulationBody selfArticulationBody = GetComponent<ArticulationBody>();
        
        foreach (var joint in allJoints)
        {
            // 排除自身，只记录旋转关节
            if (joint != selfArticulationBody && 
                joint.jointType == ArticulationJointType.RevoluteJoint)
            {
                jointInfos.Add(new JointInfo(joint));
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
        
        // 更新关节角图表
        if (jointAngleChart != null)
        {
            UpdateJointAngleChart();
        }
        
        // 更新欧拉角图表
        if (eulerAngleChart != null && rootArticulationBody != null)
        {
            UpdateEulerAngleChart();
        }
        
        // 更新线速度图表
        if (linearVelocityChart != null && rootArticulationBody != null)
        {
            UpdateLinearVelocityChart();
        }
    }
    
    void UpdateJointAngleChart()
    {
        // 检查是否需要移除最早的数据点
        bool needRemove = false;
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].dofCount > 0 && jointAngleChart.GetSerie(i).dataCount >= maxDataCount)
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
                if (joints[i].dofCount > 0 && jointAngleChart.GetSerie(i).dataCount > 0)
                {
                    jointAngleChart.GetSerie(i).RemoveData(0);
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
                jointAngleChart.AddData(i, time, angle);
            }
        }
        
        // 更新横坐标范围以实现滚动效果
        var xAxis = jointAngleChart.GetChartComponent<XAxis>();
        if (timeQueue.Count > 0)
        {
            xAxis.min = timeQueue.Peek();
            xAxis.max = time;
        }
        else
        {
            xAxis.min = firstDataTime;
            xAxis.max = time;
        }
        
        // 更新纵坐标范围（自动）
        var yAxis = jointAngleChart.GetChartComponent<YAxis>();
        yAxis.minMaxType = Axis.AxisMinMaxType.MinMax;
        
        jointAngleChart.RefreshChart();
    }
    
    void UpdateEulerAngleChart()
    {
        // 检查是否需要移除最早的数据点
        bool needRemove = false;
        for (int i = 0; i < 3; i++)
        {
            if (eulerAngleChart.GetSerie(i).dataCount >= maxDataCount)
            {
                needRemove = true;
                break;
            }
        }
        
        if (needRemove)
        {
            for (int i = 0; i < 3; i++)
            {
                if (eulerAngleChart.GetSerie(i).dataCount > 0)
                {
                    eulerAngleChart.GetSerie(i).RemoveData(0);
                }
            }
            if (eulerTimeQueue.Count > 0)
            {
                eulerTimeQueue.Dequeue();
            }
        }
        
        // 获取欧拉角
        Vector3 eulerAngles = rootArticulationBody.transform.eulerAngles;
        // 转换为 -180 到 180 范围
        eulerAngles.x = NormalizeAngle(eulerAngles.x);
        eulerAngles.y = NormalizeAngle(eulerAngles.y);
        eulerAngles.z = NormalizeAngle(eulerAngles.z);
        
        eulerTimeQueue.Enqueue(time);
        eulerAngleChart.AddData(0, time, eulerAngles.x);
        eulerAngleChart.AddData(1, time, eulerAngles.y);
        eulerAngleChart.AddData(2, time, eulerAngles.z);
        
        // 更新横坐标范围
        var xAxis = eulerAngleChart.GetChartComponent<XAxis>();
        if (eulerTimeQueue.Count > 0)
        {
            xAxis.min = eulerTimeQueue.Peek();
            xAxis.max = time;
        }
        
        eulerAngleChart.RefreshChart();
    }
    
    void UpdateLinearVelocityChart()
    {
        // 检查是否需要移除最早的数据点
        bool needRemove = false;
        for (int i = 0; i < 3; i++)
        {
            if (linearVelocityChart.GetSerie(i).dataCount >= maxDataCount)
            {
                needRemove = true;
                break;
            }
        }
        
        if (needRemove)
        {
            for (int i = 0; i < 3; i++)
            {
                if (linearVelocityChart.GetSerie(i).dataCount > 0)
                {
                    linearVelocityChart.GetSerie(i).RemoveData(0);
                }
            }
            if (velocityTimeQueue.Count > 0)
            {
                velocityTimeQueue.Dequeue();
            }
        }
        
        // 获取线速度
        Vector3 linearVelocity = rootArticulationBody.velocity;
        
        velocityTimeQueue.Enqueue(time);
        linearVelocityChart.AddData(0, time, linearVelocity.x);
        linearVelocityChart.AddData(1, time, linearVelocity.y);
        linearVelocityChart.AddData(2, time, linearVelocity.z);
        
        // 更新横坐标范围
        var xAxis = linearVelocityChart.GetChartComponent<XAxis>();
        if (velocityTimeQueue.Count > 0)
        {
            xAxis.min = velocityTimeQueue.Peek();
            xAxis.max = time;
        }
        
        linearVelocityChart.RefreshChart();
    }
    
    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
