#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixMaterialShader : EditorWindow
{
    [MenuItem("Tools/Fix Material Shader")]
    static void Init()
    {
        FixMaterialShader window = GetWindow<FixMaterialShader>("Fix Material Shader");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("修复材质Shader", EditorStyles.boldLabel);
        
        if (GUILayout.Button("修复 M_DEMOAtlas_LowPolyFlowers 材质"))
        {
            FixFlowerMaterial();
        }
        
        if (GUILayout.Button("修复所有丢失Shader的材质"))
        {
            FixAllMissingShaders();
        }
    }
    
    static void FixFlowerMaterial()
    {
        string materialPath = "Assets/Flower/DEMOLowPolyFlowers/Prefabs/M_DEMOAtlas_LowPolyFlowers.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        
        if (mat == null)
        {
            Debug.LogError($"无法找到材质: {materialPath}");
            return;
        }
        
        // 尝试使用URP Lit shader
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader != null)
        {
            mat.shader = urpLitShader;
            Debug.Log($"已将材质 {mat.name} 的Shader设置为: Universal Render Pipeline/Lit");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return;
        }
        
        // 如果URP不可用，尝试使用Standard shader
        Shader standardShader = Shader.Find("Standard");
        if (standardShader != null)
        {
            mat.shader = standardShader;
            Debug.Log($"已将材质 {mat.name} 的Shader设置为: Standard");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return;
        }
        
        Debug.LogError("无法找到合适的Shader！请确保项目已安装URP或使用Built-in渲染管线。");
    }
    
    static void FixAllMissingShaders()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;
        
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader.name == "Hidden/InternalErrorShader")
            {
                // 材质使用了丢失的shader
                Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLitShader != null)
                {
                    mat.shader = urpLitShader;
                    EditorUtility.SetDirty(mat);
                    fixedCount++;
                }
                else
                {
                    Shader standardShader = Shader.Find("Standard");
                    if (standardShader != null)
                    {
                        mat.shader = standardShader;
                        EditorUtility.SetDirty(mat);
                        fixedCount++;
                    }
                }
            }
        }
        
        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"已修复 {fixedCount} 个材质的Shader");
        }
        else
        {
            Debug.Log("没有找到需要修复的材质");
        }
    }
}
#endif

