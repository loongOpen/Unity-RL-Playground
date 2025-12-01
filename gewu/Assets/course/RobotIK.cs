using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RobotIK : MonoBehaviour
{
    [Header("Robot Configuration")]
    public Transform robot;
    
    [Header("IK Tip")]
    public ArticulationBody tip;

    [Header("IK Tar")]
    public Transform tar;
    
    [Header("IK Settings")]
    [Tooltip("按住按钮时每秒移动的距离（单位：米/秒）")]
    public float moveSpeed = 0.1f;
    
    [Header("UI Settings")]
    [Tooltip("UI 的父对象，如果为空则自动查找或创建 Canvas")]
    public Transform uiParent;
    
    [Tooltip("UI 距离左下角的偏移（X为距离左边缘，Y为距离下边缘）")]
    public Vector2 offsetFromBottomLeft = new Vector2(20f, 20f);
    
    [Tooltip("Button 之间的间距")]
    public float buttonSpacing = 20f;
    
    [Tooltip("Button 的尺寸")]
    public Vector2 buttonSize = new Vector2(100f, 50f);
    
    private Canvas canvas;
    private Toggle enableIKToggle;
    private List<Button> directionButtons = new List<Button>();
    private Dictionary<string, bool> buttonPressedStates = new Dictionary<string, bool>(); // 跟踪按钮按下状态
    
    void Start()
    {
        // 初始化tar的显示状态（默认隐藏）
        if (tar != null)
        {
            tar.gameObject.SetActive(false);
        }
        
        if (robot != null)
        {
            CreateUI();
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
                    CreateUI();
                }
            };
        }
        #endif
    }
    
    void OnDestroy()
    {
        ClearUI();
    }
    
    void CreateUI()
    {
        // 检查是否已经存在UI，如果存在则跳过
        if (CheckExistingUI())
        {
            return;
        }
        
        // 清理可能存在的旧UI
        ClearUI();
        
        // 确保有 Canvas
        EnsureCanvas();
        
        // 先创建 6 个 Button（每行2个，3行）
        CreateDirectionButtons();
        
        // 然后创建 Toggle（放在button上面）
        CreateToggle();
    }
    
    bool CheckExistingUI()
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
                            // 确保按钮可交互
                            button.interactable = true;
                            
                            // 初始化按钮按下状态
                            buttonPressedStates[label] = false;
                            
                            // 重新绑定 EventTrigger
                            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
                            if (trigger == null)
                            {
                                trigger = buttonObj.gameObject.AddComponent<EventTrigger>();
                            }
                            
                            // 清除旧的触发器
                            trigger.triggers.Clear();
                            
                            // PointerDown 事件
                            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
                            pointerDown.eventID = EventTriggerType.PointerDown;
                            string direction = label; // 保存方向字符串
                            pointerDown.callback.AddListener((data) => { OnButtonPointerDown(direction); });
                            trigger.triggers.Add(pointerDown);
                            
                            // PointerUp 事件
                            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
                            pointerUp.eventID = EventTriggerType.PointerUp;
                            pointerUp.callback.AddListener((data) => { OnButtonPointerUp(direction); });
                            trigger.triggers.Add(pointerUp);
                            
                            // PointerExit 事件
                            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                            pointerExit.eventID = EventTriggerType.PointerExit;
                            pointerExit.callback.AddListener((data) => { OnButtonPointerUp(direction); });
                            trigger.triggers.Add(pointerExit);
                            
                            directionButtons.Add(button);
                        }
                    }
                }
                
                // 如果找到所有按钮，则认为UI已存在
                if (directionButtons.Count == 6)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void EnsureCanvas()
    {
        // 如果指定了父对象，尝试从中找到 Canvas
        if (uiParent != null)
        {
            canvas = uiParent.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = uiParent.GetComponent<Canvas>();
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
            
        }
        
        // 确保有 EventSystem（UI交互必需）
        EnsureEventSystem();
        
        // 设置 uiParent
        if (uiParent == null)
        {
            uiParent = canvas.transform;
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
        }
    }
    
    void CreateToggle()
    {
        // 使用Unity的DefaultControls创建默认Toggle
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
            Debug.LogError("RobotIK: 无法创建Toggle");
            return;
        }
        
        toggleObj.name = "EnableIK_Toggle";
        toggleObj.transform.SetParent(uiParent, false);
        
        // 设置位置到左下角，在button上方
        RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            
            // 增大toggle的scale
            rectTransform.localScale = new Vector3(2f, 2f, 1f);
            
            // 计算toggle位置：在button上方
            // button总高度 = 3行 * (button高度 + 间距) - 间距
            float buttonTotalHeight = 3 * (buttonSize.y + buttonSpacing) - buttonSpacing;
            float toggleY = offsetFromBottomLeft.y + buttonTotalHeight + buttonSpacing;
            
            rectTransform.anchoredPosition = new Vector2(offsetFromBottomLeft.x, toggleY);
        }
        
        // 获取Toggle组件
        Toggle toggle = toggleObj.GetComponent<Toggle>();
        toggle.isOn = false; // 默认不勾选
        
        // 添加Toggle状态变化监听
        toggle.onValueChanged.AddListener(OnIKToggleChanged);
        
        // 设置标签文本
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
        
        // 使用Unity的DefaultControls创建默认Button
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        #if UNITY_EDITOR
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        #else
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        #endif
        
        // 计算起始位置（从左下角开始）
        float startY = offsetFromBottomLeft.y;
        float startX = offsetFromBottomLeft.x;
        
        // 创建6个按钮，每行2个，共3行
        for (int i = 0; i < 6; i++)
        {
            GameObject buttonObj = DefaultControls.CreateButton(resources);
            if (buttonObj == null)
            {
                Debug.LogError($"RobotIK: 无法创建Button {i + 1}");
                continue;
            }
            
            buttonObj.name = $"Button_{buttonLabels[i]}";
            buttonObj.transform.SetParent(uiParent, false);
            
            // 设置位置
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.sizeDelta = buttonSize;
                
                // 计算行列位置
                int row = i / 2; // 行（0, 1, 2）
                int col = i % 2; // 列（0, 1）
                
                rectTransform.anchoredPosition = new Vector2(
                    startX + col * (buttonSize.x + buttonSpacing),
                    startY + row * (buttonSize.y + buttonSpacing)
                );
            }
            
            // 获取Button组件
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"RobotIK: Button {i + 1} 缺少Button组件");
                continue;
            }
            
            // 确保按钮可交互
            button.interactable = true;
            
            // 设置按钮文本（使用DefaultControls创建的Text组件）
            Transform textObj = buttonObj.transform.Find("Text");
            if (textObj != null)
            {
                Text buttonText = textObj.GetComponent<Text>();
                if (buttonText != null)
                {
                    // 设置文本内容为对应的标签
                    buttonText.text = buttonLabels[i];
                    // 设置字号为20
                    buttonText.fontSize = 24;
                    // 确保使用Legacy字体
                    if (buttonText.font == null)
                    {
                        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
                else
                {
                    Debug.LogWarning($"RobotIK: 按钮 {i + 1} 的Text组件为空");
                }
            }
            else
            {
                // 尝试查找所有Text组件
                Text[] allTexts = buttonObj.GetComponentsInChildren<Text>(true);
                if (allTexts.Length > 0)
                {
                    allTexts[0].text = buttonLabels[i];
                    allTexts[0].fontSize = 20;
                }
                else
                {
                    Debug.LogError($"RobotIK: 按钮 {i + 1} 完全找不到Text组件");
                }
            }
            
            // 初始化按钮按下状态
            string direction = buttonLabels[i];
            buttonPressedStates[direction] = false;
            
            // 添加 EventTrigger 来检测按下和释放
            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buttonObj.gameObject.AddComponent<EventTrigger>();
            }
            
            // PointerDown 事件：按钮被按下
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnButtonPointerDown(direction); });
            trigger.triggers.Add(pointerDown);
            
            // PointerUp 事件：按钮被释放
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnButtonPointerUp(direction); });
            trigger.triggers.Add(pointerUp);
            
            // PointerExit 事件（当鼠标移出按钮时也释放）
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
            return; // 如果 Enable IK 未选中，不激活也不移动 tar
        }
        
        // 如果 tar 未激活，先激活它（只有在 Enable IK 选中时才会执行到这里）
        if (!tar.gameObject.activeSelf)
        {
            tar.gameObject.SetActive(true);
        }
        
        // 解析方向
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
        
        // 计算移动后的位置
        Vector3 moveDelta = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 newPosition = tar.position + moveDelta;
        
        // 检查 tar 和 tip 的距离限制
        if (tip != null)
        {
            float currentDistance = Vector3.Distance(tar.position, tip.transform.position);
            float newDistance = Vector3.Distance(newPosition, tip.transform.position);
            
            // 如果当前距离超过 0.02，且移动后距离增大，则阻止移动
            if (currentDistance > 0.002f && newDistance > currentDistance)
            {
                return; // 阻止向差值增大方向移动
            }
            // 如果移动后距离减小，或者当前距离 <= 0.02，则允许移动
        }
        
        // 检查 tar 是否有 Rigidbody 或 ArticulationBody 组件
        Rigidbody rb = tar.GetComponent<Rigidbody>();
        ArticulationBody ab = tar.GetComponent<ArticulationBody>();
        
        if (rb != null)
        {
            // 如果有 Rigidbody，使用 MovePosition（适用于 isKinematic）
            rb.MovePosition(newPosition);
        }
        else if (ab != null)
        {
            // 如果有 ArticulationBody，直接设置位置
            tar.position = newPosition;
        }
        else
        {
            // 普通 Transform，直接设置位置
            tar.position = newPosition;
        }
    }
    
    void OnIKToggleChanged(bool isOn)
    {
        if (tar != null)
        {
            if (isOn)
            {
                // 当启用IK时，先移动tar的位置到tip处，再激活
                if (tip != null)
                {
                    tar.position = tip.transform.position;
                    tar.rotation = tip.transform.rotation;
                    
                    // 移动完成后再激活
                    tar.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("RobotIK: tip未设置，无法移动tar");
                    tar.gameObject.SetActive(true);
                }
            }
            else
            {
                // 当禁用IK时，先隐藏tar
                tar.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("RobotIK: tar未设置，无法显示/隐藏");
        }
    }
    
    void Update()
    {
        // 检查所有按钮的按下状态，如果被按住则持续移动tar
        foreach (var kvp in buttonPressedStates)
        {
            if (kvp.Value) // 如果按钮被按住
            {
                MoveTarInDirection(kvp.Key);
            }
        }
    }
    
    void ClearUI()
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
}

