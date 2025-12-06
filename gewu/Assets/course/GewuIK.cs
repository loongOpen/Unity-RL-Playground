using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GewuIK : MonoBehaviour
{
    [Header("Robot Configuration")]
    public Transform robot;
    
    [Header("IK Tip")]
    public ArticulationBody tip;

    [Header("IK Tar")]
    public Transform tar;
    
    [Header("IK Base")]
    public Transform baseTransform;
    
    [Header("IK Settings")]
    [Tooltip("按住按钮时每秒移动的距离（单位：米/秒）")]
    public float moveSpeed = 0.1f;
    
    [Header("Slider UI Settings")]
    [Tooltip("Slider 的父对象，如果为空则自动查找或创建 Canvas")]
    public Transform sliderParent;
    
    [Tooltip("Slider 之间的垂直间距")]
    public float sliderSpacing = 50f;
    
    [Tooltip("Slider 距离左上角的偏移（X为距离左边缘，Y为距离上边缘）")]
    public Vector2 offsetFromTopLeft = new Vector2(200f, -50f);
    
    [Header("IK UI Settings")]
    [Tooltip("IK UI 的父对象，如果为空则自动查找或创建 Canvas")]
    public Transform uiParent;
    
    [Tooltip("IK UI 距离左下角的偏移（X为距离左边缘，Y为距离下边缘）")]
    public Vector2 offsetFromBottomLeft = new Vector2(20f, 20f);
    
    [Tooltip("Button 之间的间距")]
    public float buttonSpacing = 20f;
    
    [Tooltip("Button 的尺寸")]
    public Vector2 buttonSize = new Vector2(100f, 50f);
    
    [Header("Joint Settings")]
    public float stiffness = 2000f;
    public float damping = 200f;
    
    [Header("Joint Initial Angles")]
    [Tooltip("每个关节的初始角度（度）。在Start时会自动应用这些值。")]
    public List<JointInitialAngle> jointInitialAngles = new List<JointInitialAngle>();
    
    // Slider 相关
    private List<ArticulationBody> revoluteJoints = new List<ArticulationBody>();
    private List<Slider> sliders = new List<Slider>();
    private List<Text> angleValueTexts = new List<Text>();
    private bool slidersCreated = false;
    
    // IK 相关
    private Canvas canvas;
    private Toggle enableIKToggle;
    private List<Button> directionButtons = new List<Button>();
    private Dictionary<string, bool> buttonPressedStates = new Dictionary<string, bool>();
    
    void Start()
    {
        // 初始化tar的显示状态（默认隐藏）
        if (tar != null)
        {
            tar.gameObject.SetActive(false);
        }
        
        // 初始化base的显示状态（默认隐藏）
        if (baseTransform != null)
        {
            baseTransform.gameObject.SetActive(false);
        }
        
        if (robot != null)
        {
            FindRevoluteJoints();
            ApplyInitialAngles();
            CreateSliders();
            SetSlidersToInitialAngles();
            CreateIKUI();
        }
    }
    
    void OnValidate()
    {
        // 在编辑器中，当 robot 被赋值时自动创建 UI
        #if UNITY_EDITOR
        if (robot != null && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && robot != null)
                {
                    FindRevoluteJoints();
                    CreateSliders();
                    CreateIKUI();
                }
            };
        }
        #endif
    }
    
    void OnDestroy()
    {
        ClearSliders();
        ClearIKUI();
    }
    
    // ==================== RobotControl 功能 ====================
    
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
        
        Debug.Log($"GewuIK: 找到 {revoluteJoints.Count} 个旋转关节");
        
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
                    float currentAngle = 0f;
                    if (revoluteJoints[i].jointPosition.dofCount > 0)
                    {
                        currentAngle = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                    }
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
                Debug.Log($"GewuIK: 设置关节 {revoluteJoints[i].gameObject.name} 初始角度为 {initialAngle} 度");
            }
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
            }
        }
    }
    
    void CreateSliders()
    {
        if (revoluteJoints.Count == 0)
        {
            Debug.LogWarning("GewuIK: 未找到旋转关节，无法创建 Sliders");
            slidersCreated = false;
            return;
        }
        
        // 确保有 Canvas
        EnsureCanvas();
        
        // 检查是否已经存在sliders
        if (CheckExistingSliders())
        {
            Debug.Log("GewuIK: 检测到已存在的 Sliders，跳过创建");
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
        Debug.Log($"GewuIK: 已创建 {sliders.Count} 个 Sliders");
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
    
    void CreateSliderForJoint(int jointIndex)
    {
        if (jointIndex >= revoluteJoints.Count || revoluteJoints[jointIndex] == null)
            return;
        
        ArticulationBody joint = revoluteJoints[jointIndex];
        
        // 使用Unity的DefaultControls创建默认Slider
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        #if UNITY_EDITOR
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        resources.knob = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        #else
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        resources.knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob");
        
        if (resources.standard == null) resources.standard = Resources.GetBuiltinResource<Sprite>("UISprite");
        if (resources.background == null) resources.background = Resources.GetBuiltinResource<Sprite>("Background");
        if (resources.knob == null) resources.knob = Resources.GetBuiltinResource<Sprite>("Knob");
        #endif
        
        GameObject sliderObj = DefaultControls.CreateSlider(resources);
        
        if (sliderObj == null)
        {
            Debug.LogError("GewuIK: 无法创建Slider");
            return;
        }
        
        // 设置名称和父对象
        sliderObj.name = $"Slider_Joint_{jointIndex + 1}";
        sliderObj.transform.SetParent(sliderParent, false);
        
        // 设置位置到左上角
        RectTransform rectTransform = sliderObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(
                offsetFromTopLeft.x, 
                offsetFromTopLeft.y - jointIndex * sliderSpacing
            );
        }
        
        // 获取Slider组件
        Slider slider = sliderObj.GetComponent<Slider>();
        
        // 创建标签文本
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(1f, 0.5f);
        labelRect.sizeDelta = new Vector2(100f, 20f);
        labelRect.anchoredPosition = new Vector2(-10f, 0f);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = joint.gameObject.name;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 18;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleRight;
        
        // 初始化 Slider 值
        var drive = joint.xDrive;
        float lowerLimit = drive.lowerLimit;
        float upperLimit = drive.upperLimit;
        
        float targetAngleDeg = 0f;
        if (joint.jointPosition.dofCount > 0)
        {
            targetAngleDeg = joint.jointPosition[0] * Mathf.Rad2Deg;
        }
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
        
        // 创建角度值文本
        GameObject angleValueObj = new GameObject("AngleValue");
        angleValueObj.transform.SetParent(sliderObj.transform, false);
        RectTransform angleValueRect = angleValueObj.AddComponent<RectTransform>();
        angleValueRect.anchorMin = new Vector2(1f, 0.5f);
        angleValueRect.anchorMax = new Vector2(1f, 0.5f);
        angleValueRect.pivot = new Vector2(0f, 0.5f);
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
        
        ClearSlidersByName();
        slidersCreated = false;
    }
    
    void SetJointTargetDeg(ArticulationBody joint, float angleDeg)
    {
        var drive = joint.xDrive;
        drive.stiffness = stiffness;
        drive.damping = damping;
        drive.target = angleDeg;
        joint.xDrive = drive;
    }
    
    public void RefreshSliders()
    {
        FindRevoluteJoints();
        CreateSliders();
    }
    
    /// <summary>
    /// 设置IK的启用状态
    /// </summary>
    public void SetIKEnabled(bool enabled)
    {
        if (enableIKToggle != null)
        {
            enableIKToggle.isOn = enabled;
        }
    }
    
    // ==================== RobotIK 功能 ====================
    
    void CreateIKUI()
    {
        // 检查是否已经存在UI，如果存在则跳过
        if (CheckExistingIKUI())
        {
            return;
        }
        
        // 清理可能存在的旧UI
        ClearIKUI();
        
        // 确保有 Canvas
        EnsureCanvas();
        
        // 先创建 6 个 Button（每行2个，3行）
        CreateDirectionButtons();
        
        // 然后创建 Toggle（放在button上面）
        CreateToggle();
    }
    
    bool CheckExistingIKUI()
    {
        if (uiParent == null)
            return false;
        
        // 检查是否存在 Toggle
        Transform toggleObj = uiParent.Find("EnableIK_Toggle");
        if (toggleObj != null)
        {
            enableIKToggle = toggleObj.GetComponent<Toggle>();
            if (enableIKToggle != null)
            {
                // 确保添加了监听器
                enableIKToggle.onValueChanged.RemoveListener(OnIKToggleChanged);
                enableIKToggle.onValueChanged.AddListener(OnIKToggleChanged);
                
                // 查找所有按钮
                directionButtons.Clear();
                string[] buttonLabels = { "x+", "x-", "y+", "y-", "z+", "z-" };
                foreach (string label in buttonLabels)
                {
                    Transform buttonObj = uiParent.Find($"Button_{label}");
                    if (buttonObj != null)
                    {
                        Button button = buttonObj.GetComponent<Button>();
                        if (button != null)
                        {
                            button.interactable = true;
                            buttonPressedStates[label] = false;
                            
                            // 重新绑定 EventTrigger
                            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
                            if (trigger == null)
                            {
                                trigger = buttonObj.gameObject.AddComponent<EventTrigger>();
                            }
                            
                            trigger.triggers.Clear();
                            
                            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
                            pointerDown.eventID = EventTriggerType.PointerDown;
                            string direction = label;
                            pointerDown.callback.AddListener((data) => { OnButtonPointerDown(direction); });
                            trigger.triggers.Add(pointerDown);
                            
                            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
                            pointerUp.eventID = EventTriggerType.PointerUp;
                            pointerUp.callback.AddListener((data) => { OnButtonPointerUp(direction); });
                            trigger.triggers.Add(pointerUp);
                            
                            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                            pointerExit.eventID = EventTriggerType.PointerExit;
                            pointerExit.callback.AddListener((data) => { OnButtonPointerUp(direction); });
                            trigger.triggers.Add(pointerExit);
                            
                            directionButtons.Add(button);
                        }
                    }
                }
                
                if (directionButtons.Count == 6)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void CreateToggle()
    {
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        #if UNITY_EDITOR
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        resources.checkmark = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        #else
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        resources.checkmark = Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark");
        #endif
        
        GameObject toggleObj = DefaultControls.CreateToggle(resources);
        if (toggleObj == null)
        {
            Debug.LogError("GewuIK: 无法创建Toggle");
            return;
        }
        
        toggleObj.name = "EnableIK_Toggle";
        toggleObj.transform.SetParent(uiParent, false);
        
        RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.localScale = new Vector3(2f, 2f, 1f);
            
            float buttonTotalHeight = 3 * (buttonSize.y + buttonSpacing) - buttonSpacing;
            float toggleY = offsetFromBottomLeft.y + buttonTotalHeight + buttonSpacing;
            
            rectTransform.anchoredPosition = new Vector2(offsetFromBottomLeft.x, toggleY);
        }
        
        Toggle toggle = toggleObj.GetComponent<Toggle>();
        toggle.isOn = false;
        toggle.onValueChanged.AddListener(OnIKToggleChanged);
        
        Transform labelObj = toggleObj.transform.Find("Label");
        if (labelObj != null)
        {
            Text labelText = labelObj.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.text = "Enable IK";
            }
        }
        
        enableIKToggle = toggle;
    }
    
    void CreateDirectionButtons()
    {
        string[] buttonLabels = { "x+", "x-", "y+", "y-", "z+", "z-" };
        
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        #if UNITY_EDITOR
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        #else
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        #endif
        
        float startY = offsetFromBottomLeft.y;
        float startX = offsetFromBottomLeft.x;
        
        for (int i = 0; i < 6; i++)
        {
            GameObject buttonObj = DefaultControls.CreateButton(resources);
            if (buttonObj == null)
            {
                Debug.LogError($"GewuIK: 无法创建Button {i + 1}");
                continue;
            }
            
            buttonObj.name = $"Button_{buttonLabels[i]}";
            buttonObj.transform.SetParent(uiParent, false);
            
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.sizeDelta = buttonSize;
                
                int row = i / 2;
                int col = i % 2;
                
                rectTransform.anchoredPosition = new Vector2(
                    startX + col * (buttonSize.x + buttonSpacing),
                    startY + row * (buttonSize.y + buttonSpacing)
                );
            }
            
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"GewuIK: Button {i + 1} 缺少Button组件");
                continue;
            }
            
            button.interactable = true;
            
            Transform textObj = buttonObj.transform.Find("Text");
            if (textObj != null)
            {
                Text buttonText = textObj.GetComponent<Text>();
                if (buttonText != null)
                {
                    buttonText.text = buttonLabels[i];
                    buttonText.fontSize = 24;
                    if (buttonText.font == null)
                    {
                        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
            }
            else
            {
                Text[] allTexts = buttonObj.GetComponentsInChildren<Text>(true);
                if (allTexts.Length > 0)
                {
                    allTexts[0].text = buttonLabels[i];
                    allTexts[0].fontSize = 20;
                }
            }
            
            string direction = buttonLabels[i];
            buttonPressedStates[direction] = false;
            
            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buttonObj.gameObject.AddComponent<EventTrigger>();
            }
            
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnButtonPointerDown(direction); });
            trigger.triggers.Add(pointerDown);
            
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnButtonPointerUp(direction); });
            trigger.triggers.Add(pointerUp);
            
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnButtonPointerUp(direction); });
            trigger.triggers.Add(pointerExit);
            
            directionButtons.Add(button);
        }
    }
    
    void OnButtonPointerDown(string direction)
    {
        buttonPressedStates[direction] = true;
    }
    
    void OnButtonPointerUp(string direction)
    {
        buttonPressedStates[direction] = false;
    }
    
    void MoveTarInDirection(string direction)
    {
        if (tar == null)
        {
            return;
        }
        
        // 只有在 Enable IK 被选中时，才允许移动 tar
        if (enableIKToggle == null || !enableIKToggle.isOn)
        {
            return;
        }
        
        if (!tar.gameObject.activeSelf)
        {
            tar.gameObject.SetActive(true);
        }
        
        Vector3 moveDirection = Vector3.zero;
        
        switch (direction)
        {
            case "x+":
                moveDirection = Vector3.right;
                break;
            case "x-":
                moveDirection = Vector3.left;
                break;
            case "y+":
                moveDirection = Vector3.up;
                break;
            case "y-":
                moveDirection = Vector3.down;
                break;
            case "z+":
                moveDirection = Vector3.forward;
                break;
            case "z-":
                moveDirection = Vector3.back;
                break;
            default:
                return;
        }
        
        Vector3 moveDelta = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 newPosition = tar.position + moveDelta;
        
        // 检查 tar 和 tip 的距离限制
        if (tip != null)
        {
            float currentDistance = Vector3.Distance(tar.position, tip.transform.position);
            float newDistance = Vector3.Distance(newPosition, tip.transform.position);
            
            if (currentDistance > 0.005f && newDistance > currentDistance)
            {
                return;
            }
        }
        
        Rigidbody rb = tar.GetComponent<Rigidbody>();
        ArticulationBody ab = tar.GetComponent<ArticulationBody>();
        
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else if (ab != null)
        {
            tar.position = newPosition;
        }
        else
        {
            tar.position = newPosition;
        }
    }
    
    void OnIKToggleChanged(bool isOn)
    {
        if (tar != null)
        {
            if (isOn)
            {
                if (tip != null)
                {
                    tar.position = tip.transform.position;
                    tar.rotation = tip.transform.rotation;
                    tar.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("GewuIK: tip未设置，无法移动tar");
                    tar.gameObject.SetActive(true);
                }
            }
            else
            {
                tar.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("GewuIK: tar未设置，无法显示/隐藏");
        }
        
        // 控制base的显示/隐藏
        if (baseTransform != null)
        {
            baseTransform.gameObject.SetActive(isOn);
        }
    }
    
    void ClearIKUI()
    {
        if (enableIKToggle != null && enableIKToggle.gameObject != null)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(enableIKToggle.gameObject);
            }
            else
            #endif
            {
                Destroy(enableIKToggle.gameObject);
            }
        }
        
        foreach (Button button in directionButtons)
        {
            if (button != null && button.gameObject != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(button.gameObject);
                }
                else
                #endif
                {
                    Destroy(button.gameObject);
                }
            }
        }
        
        directionButtons.Clear();
    }
    
    // ==================== 共享功能 ====================
    
    void EnsureCanvas()
    {
        // 优先使用 sliderParent 或 uiParent
        Transform parentToCheck = sliderParent != null ? sliderParent : uiParent;
        
        if (parentToCheck != null)
        {
            canvas = parentToCheck.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = parentToCheck.GetComponent<Canvas>();
            }
        }
        
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        EnsureEventSystem();
        
        // 设置父对象
        if (sliderParent == null)
        {
            sliderParent = canvas.transform;
        }
        if (uiParent == null)
        {
            uiParent = canvas.transform;
        }
    }
    
    void EnsureEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }
    
    void FixedUpdate()
    {
        // 检查 Enable IK 是否被选中
        bool ikEnabled = enableIKToggle != null && enableIKToggle.isOn;
        
        for (int i = 0; i < Mathf.Min(sliders.Count, revoluteJoints.Count); i++)
        {
            if (sliders[i] != null && revoluteJoints[i] != null)
            {
                var drive = revoluteJoints[i].xDrive;
                float lowerLimit = drive.lowerLimit;
                float upperLimit = drive.upperLimit;
                
                if (ikEnabled)
                {
                    // 当 Enable IK 被选中时，将 slider 的值设为当前关节角
                    float currentAngleRad = 0f;
                    if (revoluteJoints[i].jointPosition.dofCount > 0)
                    {
                        currentAngleRad = revoluteJoints[i].jointPosition[0];
                    }
                    float currentAngleDeg = currentAngleRad * Mathf.Rad2Deg;
                    
                    // 计算slider值
                    float sliderValue = 0.5f;
                    if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
                    {
                        sliderValue = (currentAngleDeg - lowerLimit) / (upperLimit - lowerLimit);
                        sliderValue = Mathf.Clamp01(sliderValue);
                    }
                    
                    // 设置slider值
                    sliders[i].value = sliderValue;
                    
                    // 更新角度值文本显示
                    if (i < angleValueTexts.Count && angleValueTexts[i] != null)
                    {
                        angleValueTexts[i].text = $"{currentAngleDeg:F1}°";
                    }
                }
                else
                {
                    // 当 Enable IK 未被选中时，根据 Slider 值控制关节
                    float sliderValue = sliders[i].value;
                    float angleDeg = Mathf.Lerp(lowerLimit, upperLimit, sliderValue);
                    
                    SetJointTargetDeg(revoluteJoints[i], angleDeg);
                    
                    if (i < angleValueTexts.Count && angleValueTexts[i] != null)
                    {
                        angleValueTexts[i].text = $"{angleDeg:F1}°";
                    }
                }
            }
        }
    }
    
    void Update()
    {
        // 检查所有按钮的按下状态，如果被按住则持续移动tar
        foreach (var kvp in buttonPressedStates)
        {
            if (kvp.Value)
            {
                MoveTarInDirection(kvp.Key);
            }
        }
    }
}

