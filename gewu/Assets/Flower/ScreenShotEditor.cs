using UnityEngine;
using UnityEditor;
using System.IO;

namespace Gewu.Flower
{
    [CustomEditor(typeof(ScreenShot))]
    public class ScreenShotEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            ScreenShot screenShot = (ScreenShot)target;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // 截图按钮
            if (GUILayout.Button("保存截图", GUILayout.Width(150), GUILayout.Height(30)))
            {
                screenShot.CaptureScreenshot();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 显示保存路径信息
            if (!string.IsNullOrEmpty(screenShot.saveDirectory))
            {
                string fullPath;
                if (Path.IsPathRooted(screenShot.saveDirectory))
                {
                    fullPath = screenShot.saveDirectory;
                }
                else
                {
                    fullPath = Path.Combine(Application.dataPath, screenShot.saveDirectory);
                }
                EditorGUILayout.HelpBox($"保存目录: {fullPath}", MessageType.Info);
            }
            else
            {
                string defaultPath = Path.Combine(Application.dataPath, "Screenshots");
                EditorGUILayout.HelpBox($"保存目录: {defaultPath} (默认)", MessageType.Info);
            }
        }
    }
}

