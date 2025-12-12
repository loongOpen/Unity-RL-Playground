using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Gewu.Flower
{
    /// <summary>
    /// DoubaoImageQuery: 打开浏览器访问豆包网站并上传图片
    /// </summary>
    public class DoubaoImageQuery : MonoBehaviour
    {
        [Header("图片设置")]
        [Tooltip("要查询的图片（Texture2D）")]
        public Texture2D image;
        
        [Header("浏览器设置")]
        [Tooltip("浏览器路径（留空则使用默认浏览器）\n" +
                 "Windows: C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\n" +
                 "Linux: /usr/bin/google-chrome 或 /usr/bin/chromium-browser\n" +
                 "macOS: /Applications/Google Chrome.app/Contents/MacOS/Google Chrome")]
        public string browserPath = "";
        
        [Tooltip("豆包网站 URL")]
        public string doubaoUrl = "https://www.doubao.com/chat/";
        
        [Header("查询设置")]
        [Tooltip("查询问题（将复制到剪贴板）")]
        public string question = "给出图片中花的坐标";
        
        [Header("临时文件设置")]
        [Tooltip("临时图片保存路径（留空则使用系统临时目录）")]
        public string tempImagePath = "";
        
        [Header("调试")]
        [Tooltip("是否在操作时打印日志")]
        public bool debugLog = true;
        
        private bool m_IsProcessing = false;
        
        void Start()
        {
            // 自动打开浏览器（延迟一帧，确保所有初始化完成）
            StartCoroutine(DelayedOpenBrowser());
        }
        
        IEnumerator DelayedOpenBrowser()
        {
            // 等待一帧
            yield return null;
            
            // 自动打开浏览器
            if (image != null)
            {
                OpenBrowserAndSaveImage();
            }
            else
            {
                if (image == null)
                {
                    Debug.LogWarning("DoubaoImageQuery: 图片未设置");
                }
            }
        }
        
        /// <summary>
        /// 手动触发打开浏览器
        /// </summary>
        public void OpenBrowserAndSaveImageManually()
        {
            if (m_IsProcessing)
            {
                Debug.LogWarning("DoubaoImageQuery: 正在处理中，请稍候...");
                return;
            }
            
            if (image == null)
            {
                Debug.LogError("DoubaoImageQuery: 图片未设置");
                return;
            }
            
            OpenBrowserAndSaveImage();
        }
        
        /// <summary>
        /// 打开浏览器并保存图片
        /// </summary>
        void OpenBrowserAndSaveImage()
        {
            m_IsProcessing = true;
            
            try
            {
                // 保存图片到临时文件
                string imagePath = SaveImageToTemp();
                
                if (string.IsNullOrEmpty(imagePath))
                {
                    Debug.LogError("DoubaoImageQuery: 保存图片失败");
                    m_IsProcessing = false;
                    return;
                }
                
                if (debugLog)
                {
                    Debug.Log($"DoubaoImageQuery: 图片已保存到: {imagePath}");
                }
                
                // 复制问题到剪贴板（如果支持）
                CopyQuestionToClipboard();
                
                // 打开浏览器
                OpenBrowser();
                
                if (debugLog)
                {
                    Debug.Log($"DoubaoImageQuery: 已打开浏览器，请手动上传图片: {imagePath}");
                    Debug.Log($"DoubaoImageQuery: 问题已复制到剪贴板: {question}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DoubaoImageQuery: 发生异常: {ex.Message}");
                Debug.LogError($"DoubaoImageQuery: 堆栈跟踪: {ex.StackTrace}");
            }
            finally
            {
                m_IsProcessing = false;
            }
        }
        
        /// <summary>
        /// 保存图片到临时文件
        /// </summary>
        string SaveImageToTemp()
        {
            try
            {
                // 检查纹理是否可读
                Texture2D readableTexture = null;
                
                if (image.isReadable)
                {
                    // 如果纹理可读，直接使用
                    readableTexture = image;
                }
                else
                {
                    // 如果纹理不可读，创建一个可读的副本
                    if (debugLog)
                    {
                        Debug.LogWarning("DoubaoImageQuery: 纹理不可读，正在创建可读副本...");
                    }
                    
                    // 使用 RenderTexture 来读取纹理内容
                    RenderTexture renderTexture = RenderTexture.GetTemporary(
                        image.width,
                        image.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);
                    
                    Graphics.Blit(image, renderTexture);
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = renderTexture;
                    
                    readableTexture = new Texture2D(image.width, image.height);
                    readableTexture.ReadPixels(new Rect(0, 0, image.width, image.height), 0, 0);
                    readableTexture.Apply();
                    
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(renderTexture);
                }
                
                // 将图片转换为 PNG
                byte[] imageBytes = readableTexture.EncodeToPNG();
                
                // 如果创建了临时纹理，释放它
                if (readableTexture != image)
                {
                    Destroy(readableTexture);
                }
                
                // 确定保存路径
                string savePath;
                if (string.IsNullOrEmpty(tempImagePath))
                {
                    // 使用系统临时目录
                    string tempDir = Path.GetTempPath();
                    string fileName = $"doubao_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    savePath = Path.Combine(tempDir, fileName);
                }
                else
                {
                    // 使用指定路径
                    string directory = Path.GetDirectoryName(tempImagePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    savePath = tempImagePath;
                }
                
                // 保存文件
                File.WriteAllBytes(savePath, imageBytes);
                
                if (debugLog)
                {
                    Debug.Log($"DoubaoImageQuery: 图片已保存到: {savePath}");
                }
                
                return savePath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DoubaoImageQuery: 保存图片失败: {ex.Message}");
                Debug.LogError($"DoubaoImageQuery: 堆栈跟踪: {ex.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// 复制问题到剪贴板
        /// </summary>
        void CopyQuestionToClipboard()
        {
            try
            {
                #if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.systemCopyBuffer = question;
                #else
                // 运行时可以使用 GUIUtility.systemCopyBuffer（但需要 Unity 2021.2+）
                GUIUtility.systemCopyBuffer = question;
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DoubaoImageQuery: 复制到剪贴板失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 打开浏览器
        /// </summary>
        void OpenBrowser()
        {
            string url = doubaoUrl;
            
            if (debugLog)
            {
                Debug.Log($"DoubaoImageQuery: 尝试打开浏览器，URL: {url}");
            }
            
            // 如果指定了浏览器路径，优先使用指定的浏览器
            if (!string.IsNullOrEmpty(browserPath))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = browserPath,
                        Arguments = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    if (debugLog)
                    {
                        Debug.Log($"DoubaoImageQuery: 使用指定浏览器路径打开: {browserPath}");
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"DoubaoImageQuery: 使用指定浏览器路径失败: {ex.Message}");
                    Debug.LogWarning($"DoubaoImageQuery: 将尝试使用默认浏览器");
                }
            }
            
            // 使用系统默认浏览器
            try
            {
                Application.OpenURL(url);
                if (debugLog)
                {
                    Debug.Log($"DoubaoImageQuery: 使用 Application.OpenURL 打开默认浏览器");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DoubaoImageQuery: Application.OpenURL 失败: {ex.Message}");
                
                // 备用方案：使用 Process.Start
                try
                {
                    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    if (debugLog)
                    {
                        Debug.Log($"DoubaoImageQuery: 使用 Windows Process.Start 打开浏览器");
                    }
                    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    Process.Start("open", url);
                    if (debugLog)
                    {
                        Debug.Log($"DoubaoImageQuery: 使用 macOS open 命令打开浏览器");
                    }
                    #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = url,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                    if (debugLog)
                    {
                        Debug.Log($"DoubaoImageQuery: 使用 Linux xdg-open 打开浏览器");
                    }
                    #endif
                }
                catch (Exception ex2)
                {
                    Debug.LogError($"DoubaoImageQuery: Process.Start 也失败: {ex2.Message}");
                }
            }
        }
    }
}

