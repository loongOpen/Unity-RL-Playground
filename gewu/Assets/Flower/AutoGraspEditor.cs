using UnityEngine;
using UnityEditor;

namespace Gewu.Flower
{
    [CustomEditor(typeof(AutoGrasp))]
    public class AutoGraspEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            AutoGrasp autoGrasp = (AutoGrasp)target;
            
            EditorGUILayout.Space();
            
            // 显示位置信息
            if (autoGrasp.target != null)
            {
                EditorGUILayout.LabelField("位置信息", EditorStyles.boldLabel);
                EditorGUILayout.Vector3Field("初始位置", autoGrasp.initialPosition);
                EditorGUILayout.Vector3Field("当前位置", autoGrasp.target.position);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("位置差值 (X, Z)", EditorStyles.boldLabel);
                EditorGUILayout.FloatField("X 差值", autoGrasp.deltaX);
                EditorGUILayout.FloatField("Z 差值", autoGrasp.deltaZ);
            }
            else
            {
                EditorGUILayout.HelpBox("请设置 target 以查看位置信息", MessageType.Info);
            }
        }
    }
}

