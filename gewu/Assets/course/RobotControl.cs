using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class JointInitialAngle
{
    public string jointName;
    public float initialAngle; // 初始角度（度）
    
    public JointInitialAngle()
    {
        jointName = "";
        initialAngle = 0f;
    }
    
    public JointInitialAngle(string name, float angle)
    {
        jointName = name;
        initialAngle = angle;
    }
}

public class RobotControl : MonoBehaviour
{
    [Header("Robot Configuration")]
    public Transform robot;
    
    [Header("UI Settings")]
    [Tooltip("Slider 的父对象，如果为空则自动查找或创建 Canvas")]
    public Transform sliderParent;
    
    [Tooltip("Slider 之间的垂直间距")]
    public float sliderSpacing = 50f;
    
    [Tooltip("Slider 距离左上角的偏移（X为距离左边缘，Y为距离上边缘）")]
    public Vector2 offsetFromTopLeft = new Vector2(200f, -50f);
    
    [Header("Joint Settings")]
    public float stiffness = 2000f;
    public float damping = 200f;
    
    [Header("Joint Initial Angles")]
    [Tooltip("每个关节的初始角度（度）。在Start时会自动应用这些值。")]
    public List<JointInitialAngle> jointInitialAngles = new List<JointInitialAngle>();
    
    private List<ArticulationBody> revoluteJoints = new List<ArticulationBody>();
    private List<Slider> sliders = new List<Slider>();
    private List<Text> angleValueTexts = new List<Text>(); // 存储角度值文本
    private Canvas canvas;
    private bool slidersCreated = false;
    
    void Start()
    {
        if (robot != null)
        {
            FindRevoluteJoints();
            ApplyInitialAngles();
            CreateSliders();
            // 初始化时将slider的值设为joint initial angles
            SetSlidersToInitialAngles();
        }
    }
    
    void SetSlidersToInitialAngles()
    {
        if (jointInitialAngles == null || jointInitialAngles.Count == 0)
            return;
        
        for (int i = 0; i < Mathf.Min(sliders.Count, revoluteJoints.Count, jointInitialAngles.Count); i++)
        {
            if (sliders[i] != null && revoluteJoints[i] != null)
            {
                var drive = revoluteJoints[i].xDrive;
                float lowerLimit = drive.lowerLimit;
                float upperLimit = drive.upperLimit;
                float initialAngle = jointInitialAngles[i].initialAngle;
                
                // 计算slider值
                float sliderValue = 0.5f;
                if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
                {
                    sliderValue = (initialAngle - lowerLimit) / (upperLimit - lowerLimit);
                    sliderValue = Mathf.Clamp01(sliderValue);
                }
                
                // 设置slider值
                sliders[i].value = sliderValue;
                
                // 更新角度值文本显示
                if (i < angleValueTexts.Count && angleValueTexts[i] != null)
                {
                    angleValueTexts[i].text = $"{initialAngle:F1}°";
                }
                
                Debug.Log($"RobotControl: 设置 Slider {i + 1} 值为 {sliderValue:F3}，对应角度 {initialAngle:F1}°");
            }
        }
    }
    
    void OnDestroy()
    {
        ClearSliders();
    }
    
    void OnValidate()
    {
        // 在编辑器中，当 robot 被赋值时自动创建 sliders
        #if UNITY_EDITOR
        if (robot != null && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && robot != null)
                {
                    // 重置标志，允许在编辑器中重新创建
                    slidersCreated = false;
                    FindRevoluteJoints();
                    CreateSliders();
                }
            };
        }
        #endif
    }
    
    void FindRevoluteJoints()
    {
        revoluteJoints.Clear();
        if (robot == null) return;
        
        ArticulationBody[] allBodies = robot.GetComponentsInChildren<ArticulationBody>(true);
        foreach (ArticulationBody body in allBodies)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                revoluteJoints.Add(body);
            }
        }
        
        Debug.Log($"RobotControl: 找到 {revoluteJoints.Count} 个旋转关节");
        
        // 自动初始化关节初始角度列表（如果为空或数量不匹配）
        InitializeJointInitialAngles();
    }
    
    void InitializeJointInitialAngles()
    {
        // 如果列表为空或数量不匹配，自动创建
        if (jointInitialAngles == null || jointInitialAngles.Count != revoluteJoints.Count)
        {
            jointInitialAngles = new List<JointInitialAngle>();
            for (int i = 0; i < revoluteJoints.Count; i++)
            {
                if (revoluteJoints[i] != null)
                {
                    // 获取当前关节角度作为初始值
                    float currentAngle = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                    jointInitialAngles.Add(new JointInitialAngle(
                        revoluteJoints[i].gameObject.name,
                        currentAngle
                    ));
                }
            }
        }
        else
        {
            // 更新关节名称（如果名称不匹配）
            for (int i = 0; i < Mathf.Min(jointInitialAngles.Count, revoluteJoints.Count); i++)
            {
                if (revoluteJoints[i] != null)
                {
                    jointInitialAngles[i].jointName = revoluteJoints[i].gameObject.name;
                }
            }
        }
    }
    
    private void ApplyInitialAngles()
    {
        if (jointInitialAngles == null || jointInitialAngles.Count == 0)
            return;
        
        for (int i = 0; i < Mathf.Min(jointInitialAngles.Count, revoluteJoints.Count); i++)
        {
            if (revoluteJoints[i] != null)
            {
                float initialAngle = jointInitialAngles[i].initialAngle;
                SetJointTargetDeg(revoluteJoints[i], initialAngle);
                Debug.Log($"RobotControl: 设置关节 {revoluteJoints[i].gameObject.name} 初始角度为 {initialAngle} 度");
            }
        }
    }
    
    void CreateSliders()
    {
        if (revoluteJoints.Count == 0)
        {
            Debug.LogWarning("RobotControl: 未找到旋转关节，无法创建 Sliders");
            slidersCreated = false;
            return;
        }
        
        // 确保有 Canvas
        EnsureCanvas();
        
        // 检查是否已经存在sliders
        if (CheckExistingSliders())
        {
            Debug.Log("RobotControl: 检测到已存在的 Sliders，跳过创建");
            slidersCreated = true;
            return;
        }
        
        // 如果已经创建过，先清理旧的 sliders
        if (slidersCreated)
        {
            ClearSliders();
        }
        
        // 清理所有可能存在的旧 sliders（通过名称查找）
        ClearSlidersByName();
        
        // 为每个关节创建 Slider
        for (int i = 0; i < revoluteJoints.Count; i++)
        {
            CreateSliderForJoint(i);
        }
        
        slidersCreated = true;
        Debug.Log($"RobotControl: 已创建 {sliders.Count} 个 Sliders");
    }
    
    bool CheckExistingSliders()
    {
        if (sliderParent == null)
            return false;
        
        // 查找所有以 "Slider_Joint_" 开头的GameObject
        List<GameObject> existingSliders = new List<GameObject>();
        foreach (Transform child in sliderParent)
        {
            if (child.name.StartsWith("Slider_Joint_"))
            {
                Slider slider = child.GetComponent<Slider>();
                if (slider != null)
                {
                    existingSliders.Add(child.gameObject);
                }
            }
        }
        
        // 如果找到的slider数量与关节数量匹配，则认为已经存在
        if (existingSliders.Count == revoluteJoints.Count && existingSliders.Count > 0)
        {
            // 填充sliders和angleValueTexts列表
            sliders.Clear();
            angleValueTexts.Clear();
            
            // 按名称排序，确保顺序正确
            existingSliders.Sort((a, b) => 
            {
                int indexA = ExtractJointIndex(a.name);
                int indexB = ExtractJointIndex(b.name);
                return indexA.CompareTo(indexB);
            });
            
            for (int i = 0; i < existingSliders.Count; i++)
            {
                Slider slider = existingSliders[i].GetComponent<Slider>();
                if (slider != null)
                {
                    sliders.Add(slider);
                    
                    // 查找角度值文本
                    Transform angleValueObj = existingSliders[i].transform.Find("AngleValue");
                    if (angleValueObj != null)
                    {
                        Text angleValueText = angleValueObj.GetComponent<Text>();
                        angleValueTexts.Add(angleValueText);
                    }
                    else
                    {
                        angleValueTexts.Add(null);
                    }
                }
            }
            
            return true;
        }
        
        return false;
    }
    
    int ExtractJointIndex(string sliderName)
    {
        // 从 "Slider_Joint_1" 中提取数字
        string[] parts = sliderName.Split('_');
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[2], out int index))
            {
                return index;
            }
        }
        return 0;
    }
    
    void ClearSlidersByName()
    {
        // 清理所有以 "Slider_Joint_" 开头的对象
        if (sliderParent != null)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in sliderParent)
            {
                if (child.name.StartsWith("Slider_Joint_"))
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            
            foreach (GameObject obj in toDestroy)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(obj);
                }
                else
                #endif
                {
                    Destroy(obj);
                }
            }
        }
    }
    
    void EnsureCanvas()
    {
        // 如果指定了父对象，尝试从中找到 Canvas
        if (sliderParent != null)
        {
            canvas = sliderParent.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = sliderParent.GetComponent<Canvas>();
            }
        }
        
        // 如果还没找到，尝试在场景中查找
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        // 如果仍然没有，创建一个新的 Canvas
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("RobotControl: 自动创建了新的 Canvas");
        }
        
        // 确保有 EventSystem（UI交互必需）
        EnsureEventSystem();
        
        // 设置 sliderParent
        if (sliderParent == null)
        {
            sliderParent = canvas.transform;
        }
    }
    
    void EnsureEventSystem()
    {
        // 检查场景中是否已有 EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            Debug.Log("RobotControl: 自动创建了新的 EventSystem");
        }
    }
    
    void CreateSliderForJoint(int jointIndex)
    {
        if (jointIndex >= revoluteJoints.Count || revoluteJoints[jointIndex] == null)
            return;
        
        ArticulationBody joint = revoluteJoints[jointIndex];
        
        // 使用Unity的DefaultControls创建默认Slider（与菜单创建的完全一致）
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        // 获取Unity默认的所有sprite资源，确保使用默认颜色和外观
        #if UNITY_EDITOR
        // 在编辑器中，使用AssetDatabase获取所有默认资源
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        resources.knob = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        #else
        // 运行时，尝试使用Resources获取
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        resources.knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob");
        
        // 如果路径不对，尝试其他可能的路径
        if (resources.standard == null) resources.standard = Resources.GetBuiltinResource<Sprite>("UISprite");
        if (resources.background == null) resources.background = Resources.GetBuiltinResource<Sprite>("Background");
        if (resources.knob == null) resources.knob = Resources.GetBuiltinResource<Sprite>("Knob");
        #endif
        
        // 创建默认Slider结构（会自动使用默认颜色）
        GameObject sliderObj = DefaultControls.CreateSlider(resources);
        
        if (sliderObj == null)
        {
            Debug.LogError("RobotControl: 无法创建Slider");
            return;
        }
        
        // 确保所有部分都使用默认sprite（如果之前没有获取到）
        #if !UNITY_EDITOR
        // 运行时，如果sprite为null，再次尝试获取
        if (resources.knob == null)
        {
            Transform handleArea = sliderObj.transform.Find("Handle Slide Area");
            if (handleArea != null)
            {
                Transform handle = handleArea.Find("Handle");
                if (handle != null)
                {
                    Image handleImage = handle.GetComponent<Image>();
                    if (handleImage != null && handleImage.sprite == null)
                    {
                        Sprite circleSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob");
                        if (circleSprite == null) circleSprite = Resources.GetBuiltinResource<Sprite>("Knob");
                        if (circleSprite != null) handleImage.sprite = circleSprite;
                    }
                }
            }
        }
        #endif
        
        // 设置名称和父对象
        sliderObj.name = $"Slider_Joint_{jointIndex + 1}";
        sliderObj.transform.SetParent(sliderParent, false);
        
        // 设置位置到左上角
        RectTransform rectTransform = sliderObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 设置anchor和pivot为左上角
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            // 从左上角开始，向下排列
            rectTransform.anchoredPosition = new Vector2(
                offsetFromTopLeft.x, 
                offsetFromTopLeft.y - jointIndex * sliderSpacing
            );
        }
        
        // 获取Slider组件
        Slider slider = sliderObj.GetComponent<Slider>();
        
        // 创建标签文本（不影响Slider本身）
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        // 标签在slider左侧，anchor设置为左侧中间
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(1f, 0.5f); // pivot在右侧，这样文本右对齐
        labelRect.sizeDelta = new Vector2(100f, 20f);
        // 标签在slider左侧，距离slider左边缘一定距离
        labelRect.anchoredPosition = new Vector2(-10f, 0f);
        Text labelText = labelObj.AddComponent<Text>();
        // 使用revolute joint的GameObject名称作为标签文本
        labelText.text = joint.gameObject.name;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 18;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleRight;
        
        // 初始化 Slider 值
        var drive = joint.xDrive;
        float lowerLimit = drive.lowerLimit;
        float upperLimit = drive.upperLimit;
        
        // 优先使用jointInitialAngles中的初始角度值
        float targetAngleDeg = joint.jointPosition[0] * Mathf.Rad2Deg; // 默认使用当前角度
        if (jointInitialAngles != null && jointIndex < jointInitialAngles.Count)
        {
            targetAngleDeg = jointInitialAngles[jointIndex].initialAngle;
        }
        
        float sliderValue = 0.5f;
        if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
        {
            sliderValue = (targetAngleDeg - lowerLimit) / (upperLimit - lowerLimit);
            sliderValue = Mathf.Clamp01(sliderValue);
        }
        slider.value = sliderValue;
        
        // 创建角度值文本（在slider右侧）
        GameObject angleValueObj = new GameObject("AngleValue");
        angleValueObj.transform.SetParent(sliderObj.transform, false);
        RectTransform angleValueRect = angleValueObj.AddComponent<RectTransform>();
        // 角度值在slider右侧
        angleValueRect.anchorMin = new Vector2(1f, 0.5f);
        angleValueRect.anchorMax = new Vector2(1f, 0.5f);
        angleValueRect.pivot = new Vector2(0f, 0.5f); // pivot在左侧，这样文本左对齐
        angleValueRect.sizeDelta = new Vector2(80f, 20f);
        angleValueRect.anchoredPosition = new Vector2(10f, 0f);
        Text angleValueText = angleValueObj.AddComponent<Text>();
        angleValueText.text = $"{targetAngleDeg:F1}°";
        angleValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        angleValueText.fontSize = 18;
        angleValueText.color = Color.white;
        angleValueText.alignment = TextAnchor.MiddleLeft;
        
        sliders.Add(slider);
        angleValueTexts.Add(angleValueText);
    }
    
    void ClearSliders()
    {
        // 清理列表中的 sliders
        foreach (Slider slider in sliders)
        {
            if (slider != null && slider.gameObject != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(slider.gameObject);
                }
                else
                #endif
                {
                    Destroy(slider.gameObject);
                }
            }
        }
        sliders.Clear();
        angleValueTexts.Clear();
        
        // 也清理所有可能遗留的 sliders（通过名称查找）
        ClearSlidersByName();
        
        slidersCreated = false;
    }
    
    void FixedUpdate()
    {
        // 根据 Slider 值控制关节
        for (int i = 0; i < Mathf.Min(sliders.Count, revoluteJoints.Count); i++)
        {
            if (sliders[i] != null && revoluteJoints[i] != null)
            {
                var drive = revoluteJoints[i].xDrive;
                float lowerLimit = drive.lowerLimit;
                float upperLimit = drive.upperLimit;
                
                float sliderValue = sliders[i].value;
                float angleDeg = Mathf.Lerp(lowerLimit, upperLimit, sliderValue);
                
                SetJointTargetDeg(revoluteJoints[i], angleDeg);
                
                // 更新角度值文本显示
                if (i < angleValueTexts.Count && angleValueTexts[i] != null)
                {
                    angleValueTexts[i].text = $"{angleDeg:F1}°";
                }
            }
        }
    }
    
    void SetJointTargetDeg(ArticulationBody joint, float angleDeg)
    {
        var drive = joint.xDrive;
        drive.stiffness = stiffness;
        drive.damping = damping;
        drive.target = angleDeg;
        joint.xDrive = drive;
    }
    
    /// <summary>
    /// 手动刷新 Sliders（当 robot 被重新赋值后调用）
    /// </summary>
    public void RefreshSliders()
    {
        FindRevoluteJoints();
        CreateSliders();
    }
}

