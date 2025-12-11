using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Gewu.Flower
{
    /// <summary>
    /// 随机设置 Transform 的位置和旋转
    /// </summary>
    public class RandomPose : MonoBehaviour
    {
        [Header("Target Transform")]
        public Transform targetTransform;
        
        [Header("Position Range")]
        [Tooltip("位置 X 的最小值")]
        public float minX = -1f;
        [Tooltip("位置 X 的最大值")]
        public float maxX = 1f;
        [Tooltip("位置 Y 的最小值")]
        public float minY = -1f;
        [Tooltip("位置 Y 的最大值")]
        public float maxY = 1f;
        [Tooltip("位置 Z 的最小值")]
        public float minZ = -1f;
        [Tooltip("位置 Z 的最大值")]
        public float maxZ = 1f;
        
        [Header("Rotation Range (Euler Angles)")]
        [Tooltip("旋转 X 的最小值（度）")]
        public float minRotX = -180f;
        [Tooltip("旋转 X 的最大值（度）")]
        public float maxRotX = 180f;
        [Tooltip("旋转 Y 的最小值（度）")]
        public float minRotY = -180f;
        [Tooltip("旋转 Y 的最大值（度）")]
        public float maxRotY = 180f;
        [Tooltip("旋转 Z 的最小值（度）")]
        public float minRotZ = -180f;
        [Tooltip("旋转 Z 的最大值（度）")]
        public float maxRotZ = 180f;
        
        [Header("UI Settings")]
        [Tooltip("UI 的父对象，如果为空则自动查找或创建 Canvas")]
        public Transform uiParent;
        [Tooltip("按钮距离右下角的偏移（X为距离右边缘，Y为距离下边缘）")]
        public Vector2 offsetFromBottomRight = new Vector2(-120f, 20f);
        [Tooltip("按钮的尺寸")]
        public Vector2 buttonSize = new Vector2(100f, 50f);
        
        private Canvas canvas;
        private Button randomButton;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool hasInitialized = false;
        
        void Start()
        {
            InitializeInitialPose();
            CreateUI();
        }
        
        void InitializeInitialPose()
        {
            if (targetTransform != null && !hasInitialized)
            {
                initialPosition = targetTransform.position;
                initialRotation = targetTransform.rotation;
                hasInitialized = true;
            }
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
            CreateRandomButton();
        }
        
        bool CheckExistingUI()
        {
            if (uiParent == null)
                return false;
            
            Transform buttonObj = uiParent.Find("RandomPose_Button");
            if (buttonObj != null)
            {
                randomButton = buttonObj.GetComponent<Button>();
                if (randomButton != null)
                {
                    // 确保添加了监听器
                    randomButton.onClick.RemoveListener(OnRandomButtonClick);
                    randomButton.onClick.AddListener(OnRandomButtonClick);
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
        
        void CreateRandomButton()
        {
            DefaultControls.Resources resources = new DefaultControls.Resources();
            
            #if UNITY_EDITOR
            resources.standard = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            resources.background = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            #else
            resources.standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
            resources.background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
            #endif
            
            // 创建 Random 按钮
            GameObject buttonObj = DefaultControls.CreateButton(resources);
            if (buttonObj != null)
            {
                buttonObj.name = "RandomPose_Button";
                buttonObj.transform.SetParent(uiParent, false);
                
                RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    rectTransform.sizeDelta = buttonSize;
                    rectTransform.anchoredPosition = offsetFromBottomRight;
                }
                
                randomButton = buttonObj.GetComponent<Button>();
                if (randomButton != null)
                {
                    randomButton.interactable = true;
                    randomButton.onClick.AddListener(OnRandomButtonClick);
                }
                
                // 设置按钮文本
                Transform textObj = buttonObj.transform.Find("Text");
                if (textObj != null)
                {
                    Text buttonText = textObj.GetComponent<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = "Random";
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
                    Text[] allTexts = buttonObj.GetComponentsInChildren<Text>(true);
                    if (allTexts.Length > 0)
                    {
                        allTexts[0].text = "Random";
                        allTexts[0].fontSize = 24;
                        if (allTexts[0].font == null)
                        {
                            allTexts[0].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        }
                    }
                }
            }
        }
        
        void OnRandomButtonClick()
        {
            RandomizePose();
        }
        
        /// <summary>
        /// 随机设置 Transform 的位置和旋转（位置相对于初始位置）
        /// </summary>
        public void RandomizePose()
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("RandomPose: targetTransform is null");
                return;
            }
            
            // 确保已初始化初始位置
            if (!hasInitialized)
            {
                InitializeInitialPose();
            }
            
            // 随机位置偏移（相对于初始位置）
            float randomOffsetX = Random.Range(minX, maxX);
            float randomOffsetY = Random.Range(minY, maxY);
            float randomOffsetZ = Random.Range(minZ, maxZ);
            targetTransform.position = initialPosition + new Vector3(randomOffsetX, randomOffsetY, randomOffsetZ);
            
            // 随机旋转（欧拉角，相对于初始旋转）
            float randomRotX = Random.Range(minRotX, maxRotX);
            float randomRotY = Random.Range(minRotY, maxRotY);
            float randomRotZ = Random.Range(minRotZ, maxRotZ);
            targetTransform.rotation = initialRotation * Quaternion.Euler(randomRotX, randomRotY, randomRotZ);
            
            Debug.Log($"RandomPose: Set position offset to ({randomOffsetX:F2}, {randomOffsetY:F2}, {randomOffsetZ:F2}), rotation offset to ({randomRotX:F2}, {randomRotY:F2}, {randomRotZ:F2})");
        }
        
        void ClearUI()
        {
            if (randomButton != null && randomButton.gameObject != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(randomButton.gameObject);
                }
                else
                #endif
                {
                    Destroy(randomButton.gameObject);
                }
            }
            
            randomButton = null;
        }
    }
}

