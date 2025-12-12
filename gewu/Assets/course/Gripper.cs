using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Gripper : MonoBehaviour
{
    [Header("Gripper Configuration")]
    [Tooltip("左侧夹爪关节")]
    public ArticulationBody leftFinger;
    
    [Tooltip("右侧夹爪关节")]
    public ArticulationBody rightFinger;
    
    [Header("Gripper Angles")]
    [Tooltip("打开时的角度（度）")]
    public float openAngle = 0f;
    
    [Tooltip("抓取时的角度（度）")]
    public float graspAngle = 45f;
    
    [Header("UI Settings")]
    [Tooltip("UI 的父对象，如果为空则自动查找或创建 Canvas")]
    public Transform uiParent;
    
    [Tooltip("按钮距离右下角的偏移（X为距离右边缘，Y为距离下边缘）")]
    public Vector2 offsetFromBottomRight = new Vector2(-120f, 20f);
    
    [Tooltip("按钮之间的间距")]
    public float buttonSpacing = 10f;
    
    [Tooltip("按钮的尺寸")]
    public Vector2 buttonSize = new Vector2(100f, 50f);
    
    private Canvas canvas;
    private Button openButton;
    private Button graspButton;
    
    void Start()
    {
        CreateUI();
        
        // 初始化时让夹爪打开
        SetGripperPosition(openAngle, openAngle);
    }
    
    void OnValidate()
    {
        // 在编辑器中，自动创建 UI
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
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
        // 检查是否已经存在UI
        if (CheckExistingUI())
        {
            return;
        }
        
        // 清理可能存在的旧UI
        ClearUI();
        
        // 确保有 Canvas
        EnsureCanvas();
        
        // 创建按钮
        CreateButtons();
    }
    
    bool CheckExistingUI()
    {
        if (uiParent == null)
            return false;
        
        Transform openBtnObj = uiParent.Find("Gripper_Open_Button");
        Transform graspBtnObj = uiParent.Find("Gripper_Grasp_Button");
        
        if (openBtnObj != null && graspBtnObj != null)
        {
            openButton = openBtnObj.GetComponent<Button>();
            graspButton = graspBtnObj.GetComponent<Button>();
            
            if (openButton != null && graspButton != null)
            {
                // 重新绑定事件
                openButton.onClick.RemoveAllListeners();
                openButton.onClick.AddListener(OnOpenButtonClick);
                
                graspButton.onClick.RemoveAllListeners();
                graspButton.onClick.AddListener(OnGraspButtonClick);
                
                return true;
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
    
    void CreateButtons()
    {
        DefaultControls.Resources resources = new DefaultControls.Resources();
        
        #if UNITY_EDITOR
        resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        #else
        resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
        resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        #endif
        
        // 创建 Open 按钮
        GameObject openButtonObj = DefaultControls.CreateButton(resources);
        if (openButtonObj != null)
        {
            openButtonObj.name = "Gripper_Open_Button";
            openButtonObj.transform.SetParent(uiParent, false);
            
            RectTransform rectTransform = openButtonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(1f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.sizeDelta = buttonSize;
                rectTransform.anchoredPosition = new Vector2(
                    offsetFromBottomRight.x,
                    offsetFromBottomRight.y
                );
            }
            
            openButton = openButtonObj.GetComponent<Button>();
            if (openButton != null)
            {
                openButton.interactable = true;
                openButton.onClick.AddListener(OnOpenButtonClick);
            }
            
            // 设置按钮文本
            Transform textObj = openButtonObj.transform.Find("Text");
            if (textObj != null)
            {
                Text buttonText = textObj.GetComponent<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "Open";
                    buttonText.fontSize = 24;
                    if (buttonText.font == null)
                    {
                        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
            }
            else
            {
                // 如果找不到Text子对象，尝试查找所有Text组件
                Text[] allTexts = openButtonObj.GetComponentsInChildren<Text>(true);
                if (allTexts.Length > 0)
                {
                    allTexts[0].text = "Open";
                    allTexts[0].fontSize = 24;
                    if (allTexts[0].font == null)
                    {
                        allTexts[0].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
            }
        }
        
        // 创建 Grasp 按钮
        GameObject graspButtonObj = DefaultControls.CreateButton(resources);
        if (graspButtonObj != null)
        {
            graspButtonObj.name = "Gripper_Grasp_Button";
            graspButtonObj.transform.SetParent(uiParent, false);
            
            RectTransform rectTransform = graspButtonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(1f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.sizeDelta = buttonSize;
                rectTransform.anchoredPosition = new Vector2(
                    offsetFromBottomRight.x,
                    offsetFromBottomRight.y + buttonSize.y + buttonSpacing
                );
            }
            
            graspButton = graspButtonObj.GetComponent<Button>();
            if (graspButton != null)
            {
                graspButton.interactable = true;
                graspButton.onClick.AddListener(OnGraspButtonClick);
            }
            
            // 设置按钮文本
            Transform textObj = graspButtonObj.transform.Find("Text");
            if (textObj != null)
            {
                Text buttonText = textObj.GetComponent<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "Grasp";
                    buttonText.fontSize = 24;
                    if (buttonText.font == null)
                    {
                        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
            }
            else
            {
                // 如果找不到Text子对象，尝试查找所有Text组件
                Text[] allTexts = graspButtonObj.GetComponentsInChildren<Text>(true);
                if (allTexts.Length > 0)
                {
                    allTexts[0].text = "Grasp";
                    allTexts[0].fontSize = 24;
                    if (allTexts[0].font == null)
                    {
                        allTexts[0].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
            }
        }
    }
    
    void OnOpenButtonClick()
    {
        // Open: left = openAngle, right = -openAngle
        SetGripperPosition(openAngle, openAngle);
    }
    
    void OnGraspButtonClick()
    {
        // Grasp: left = graspAngle, right = -graspAngle
        SetGripperPosition(graspAngle, graspAngle);
    }
    
    public void SetGripperPosition(float leftPosition, float rightPosition)
    {
        // 设置左侧夹爪位置（prismatic joint）
        if (leftFinger != null)
        {
            var drive = leftFinger.xDrive;
            drive.stiffness = 1000f;
            drive.damping = 100f;
            drive.target = leftPosition;
            leftFinger.xDrive = drive;
        }
        
        // 设置右侧夹爪位置（prismatic joint）
        if (rightFinger != null)
        {
            var drive = rightFinger.xDrive;
            drive.stiffness = 2000f;
            drive.damping = 200f;
            drive.target = rightPosition;
            rightFinger.xDrive = drive;
        }
    }
    
    void ClearUI()
    {
        if (openButton != null && openButton.gameObject != null)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(openButton.gameObject);
            }
            else
            #endif
            {
                Destroy(openButton.gameObject);
            }
        }
        
        if (graspButton != null && graspButton.gameObject != null)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(graspButton.gameObject);
            }
            else
            #endif
            {
                Destroy(graspButton.gameObject);
            }
        }
    }
}

