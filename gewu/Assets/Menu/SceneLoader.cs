using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class SceneLoader : MonoBehaviour
{
    // 通过Inspector指定场景名称（需与Build Settings中的名称一致）
    public string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }
    public void ReturnToMainMenu()
    {
        // 主菜单场景索引（0表示第一个场景）
        int mainMenuIndex = 0;
        
        if (mainMenuIndex >= 0 && mainMenuIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(mainMenuIndex,LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("无效的主菜单索引");
        }
    }
}