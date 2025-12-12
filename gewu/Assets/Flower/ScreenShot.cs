using UnityEngine;
using System.IO;

namespace Gewu.Flower
{
    /// <summary>
    /// ScreenShot: 保存相机截图
    /// </summary>
    public class ScreenShot : MonoBehaviour
    {
        [Header("相机设置")]
        [Tooltip("要截图的相机（如果为空则使用Main Camera）")]
        public Camera targetCamera;
        
        [Header("保存设置")]
        [Tooltip("保存目录（相对于项目根目录，如果为空则使用默认目录）")]
        public string saveDirectory = "Screenshots";
        
        [Tooltip("文件名前缀")]
        public string fileNamePrefix = "Screenshot";
        
        [Tooltip("是否在文件名中包含时间戳")]
        public bool includeTimestamp = true;
        
        [Tooltip("图片格式")]
        public ImageFormat imageFormat = ImageFormat.PNG;
        
        [Header("截图设置")]
        [Tooltip("截图分辨率宽度（0表示使用相机当前分辨率）")]
        public int width = 0;
        
        [Tooltip("截图分辨率高度（0表示使用相机当前分辨率）")]
        public int height = 0;
        
        [Tooltip("是否使用超采样（提高图片质量）")]
        public bool useSuperSampling = false;
        
        [Tooltip("超采样倍数（仅在useSuperSampling为true时有效）")]
        [Range(1, 4)]
        public int superSamplingFactor = 2;
        
        public enum ImageFormat
        {
            PNG,
            JPG
        }
        
        void Start()
        {
            // 如果没有指定相机，使用Main Camera
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("ScreenShot: 未找到Main Camera，请在Inspector中指定targetCamera");
                }
            }
        }
        
        /// <summary>
        /// 保存截图
        /// </summary>
        public void CaptureScreenshot()
        {
            if (targetCamera == null)
            {
                Debug.LogError("ScreenShot: 相机未设置，无法截图");
                return;
            }
            
            // 确定分辨率
            int captureWidth = width > 0 ? width : targetCamera.pixelWidth;
            int captureHeight = height > 0 ? height : targetCamera.pixelHeight;
            
            // 如果使用超采样，增加分辨率
            int finalWidth = captureWidth;
            int finalHeight = captureHeight;
            if (useSuperSampling)
            {
                finalWidth = captureWidth * superSamplingFactor;
                finalHeight = captureHeight * superSamplingFactor;
            }
            
            // 创建 RenderTexture
            RenderTexture renderTexture = new RenderTexture(finalWidth, finalHeight, 24);
            RenderTexture previous = targetCamera.targetTexture;
            targetCamera.targetTexture = renderTexture;
            
            // 渲染到 RenderTexture
            targetCamera.Render();
            
            // 读取像素
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(finalWidth, finalHeight, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, finalWidth, finalHeight), 0, 0);
            texture.Apply();
            
            // 如果使用超采样，缩小图片
            if (useSuperSampling && superSamplingFactor > 1)
            {
                Texture2D resizedTexture = ResizeTexture(texture, captureWidth, captureHeight);
                Destroy(texture);
                texture = resizedTexture;
            }
            
            // 恢复相机设置
            targetCamera.targetTexture = previous;
            RenderTexture.active = null;
            Destroy(renderTexture);
            
            // 转换为字节数组
            byte[] imageData;
            string extension;
            if (imageFormat == ImageFormat.PNG)
            {
                imageData = texture.EncodeToPNG();
                extension = "png";
            }
            else
            {
                imageData = texture.EncodeToJPG();
                extension = "jpg";
            }
            
            Destroy(texture);
            
            // 生成文件名
            string fileName = GenerateFileName(extension);
            
            // 确保目录存在
            string fullPath = GetFullPath(fileName);
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // 保存文件
            File.WriteAllBytes(fullPath, imageData);
            
            Debug.Log($"ScreenShot: 截图已保存到 {fullPath}");
        }
        
        /// <summary>
        /// 生成文件名
        /// </summary>
        string GenerateFileName(string extension)
        {
            string fileName = fileNamePrefix;
            
            if (includeTimestamp)
            {
                fileName += "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            }
            
            fileName += "." + extension;
            
            return fileName;
        }
        
        /// <summary>
        /// 获取完整路径
        /// </summary>
        string GetFullPath(string fileName)
        {
            string directory;
            if (string.IsNullOrEmpty(saveDirectory))
            {
                // 默认保存到项目根目录下的 Screenshots 文件夹
                directory = Path.Combine(Application.dataPath, "Screenshots");
            }
            else
            {
                // 如果 saveDirectory 是绝对路径，直接使用；否则相对于 Assets 目录
                if (Path.IsPathRooted(saveDirectory))
                {
                    directory = saveDirectory;
                }
                else
                {
                    directory = Path.Combine(Application.dataPath, saveDirectory);
                }
            }
            
            return Path.Combine(directory, fileName);
        }
        
        /// <summary>
        /// 调整纹理大小
        /// </summary>
        Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            
            Texture2D resized = new Texture2D(newWidth, newHeight);
            resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resized.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            
            return resized;
        }
    }
}

