#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(JointInitialAngle))]
public class JointInitialAngleDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // 计算左右两部分的宽度
        float nameWidth = position.width * 0.6f;
        float angleWidth = position.width * 0.4f - 5f;
        
        // 左边显示名称（只读）
        Rect nameRect = new Rect(position.x, position.y, nameWidth, position.height);
        SerializedProperty nameProperty = property.FindPropertyRelative("jointName");
        EditorGUI.LabelField(nameRect, nameProperty.stringValue);
        
        // 右边显示角度输入框
        Rect angleRect = new Rect(position.x + nameWidth + 5f, position.y, angleWidth, position.height);
        SerializedProperty angleProperty = property.FindPropertyRelative("initialAngle");
        EditorGUI.PropertyField(angleRect, angleProperty, GUIContent.none);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif

