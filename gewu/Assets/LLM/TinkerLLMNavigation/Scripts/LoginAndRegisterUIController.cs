using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.ShaderData;

public class LoginAndRegisterUIController : MonoBehaviour
{
    [Header("注册和登录按钮")]
    public Button btn_Register;
    public Button btn_Login;
    public Button btn_ConfirmRegister;
    public Button btn_ConfirmLogin;
    public Button btn_CloseRegisterPanel;
    public Button btn_CloseLoginPanel;

    [Header("注册和登录面板")]
    public GameObject RegisterPanel;
    public GameObject LoginPanel;

    [Header("错误提示信息")]
    public Text errorText;

    [Header("输入框")]
    public InputField RegisterAccount;
    public InputField RegisterPassword;
    public InputField ConfirmRegisterPassword;
    public InputField LoginAccount;
    public InputField LoginPassword;

    string path;

    // 临时存储
    string userName = "";
    string password = "";
    void Start()
    {
        // 初始化组件
        RegisterPanel.SetActive(false);
        LoginPanel.SetActive(false);
        errorText.text = string.Empty;
        errorText.gameObject.SetActive(false);

        // 注册按钮和登录按钮监听事件
        btn_Register.onClick.AddListener(() =>
        {
            RegisterPanel.SetActive(true);
        });
        btn_Login.onClick.AddListener(() =>
        {
            LoginPanel.SetActive(true);
        });
        // 关闭按钮监听事件
        btn_CloseRegisterPanel.onClick.AddListener(() =>
        {
            RegisterPanel.SetActive(false);
        });
        btn_CloseLoginPanel.onClick.AddListener(() =>
        {
            LoginPanel.SetActive(false);
        });

        // 数据存储位置
        path = Application.dataPath + "/TinkerLLMNavigation/Data/RegisterInfo.txt";

        // 确认注册按钮和确认登录按钮监听事件
        btn_ConfirmRegister.onClick.AddListener(() => {
            ConfirmRegister();
        });
        btn_ConfirmLogin.onClick.AddListener(() =>
        {
            ConfirmLogin();
        });
    }

    public void ConfirmRegister()
    {
        if (string.IsNullOrEmpty(RegisterAccount.text) || string.IsNullOrEmpty(RegisterPassword.text) || string.IsNullOrEmpty(ConfirmRegisterPassword.text))
        {
            return;
        }
        else if (RegisterPassword.text != ConfirmRegisterPassword.text)
        {
            ShowError("两次密码输入不一致！");
            RegisterAccount.text = "";
            RegisterPassword.text = "";
            ConfirmRegisterPassword.text = "";
        }
        else if (RegisterAccount.text.Length == 0 || RegisterAccount.text.Length > 5 || RegisterPassword.text.Length < 3 || RegisterPassword.text.Length > 10
            || ConfirmRegisterPassword.text.Length < 3 || ConfirmRegisterPassword.text.Length > 10)
        {
            ShowError("账号或密码长度不符合要求！");
            RegisterAccount.text = "";
            RegisterPassword.text = "";
            ConfirmRegisterPassword.text = "";
        }
        else {
            // 临时存储
            userName=RegisterAccount.text;
            password=RegisterPassword.text;

            // 把注册的账号密码存入文本文档中
            string s = File.ReadAllText(path);
            if (string.IsNullOrEmpty(s))
            {
                File.WriteAllText(path, userName + "," + password); //存第一组账号密码，用逗号隔开
                ShowError("注册成功！");
            }
            else {
                // 只有一组账号密码
                if (!s.Contains("|"))
                {
                    // 定义数组ss，存储一组账号密码
                    string[] ss = s.Split(',');
                    // 账号相同，注册失败
                    if (ss[0] == userName)
                    {
                        RegisterAccount.text = "";
                        RegisterPassword.text = "";
                        ConfirmRegisterPassword.text = "";
                        ShowError("账号相同，注册失败！");
                    }
                    else if (ss[0] != userName)
                    {
                        ShowError("注册成功！");
                        File.WriteAllText(path, s + "|" + userName + "," + password);
                    }
                }
                // 有多组账号密码
                else
                {
                    // 定义数组sss,按照竖线切割每一组账号密码
                    string[] sss = s.Split('|');
                    bool accountExist = false;
                    for (int i = 0; i < sss.Length; i++)
                    {
                        // 定义数组ss，存储一组账号密码
                        string[] ss = sss[i].Split(',');
                        if (ss[0] == userName)
                        {
                            RegisterAccount.text = "";
                            RegisterPassword.text = "";
                            ConfirmRegisterPassword.text = "";
                            ShowError("账号相同，注册失败！");
                            accountExist = true;
                            break;
                        }
                    }
                    // 如果账号不存在
                    if (!accountExist)
                    {
                        ShowError("注册成功！");
                        // 添加新账号到文件
                        File.WriteAllText(path, s + "|" + userName + "," + password);
                    }
                    else
                    {
                        // 如果账号存在，确保注册成功面板不被激活
                        ShowError("账号已存在！");
                    }
                }
            }
        }
        RegisterAccount.text = "";
        RegisterPassword.text = "";
        ConfirmRegisterPassword.text = "";
    }

    public void ConfirmLogin()
    {
        string s = File.ReadAllText(path);

        if (!string.IsNullOrEmpty(s))
        {
            // 只有一组账号密码
            if (!s.Contains("|"))
            {
                // 定义数组ss，存储一组账号密码
                string[] ss = s.Split(','); 
                if (ss[0] == LoginAccount.text && ss[1] == LoginPassword.text)
                {
                    // 跳转场景
                    SceneManager.LoadScene(1);
                }
                // 不能登录成功
                else if (ss[0] != LoginAccount.text || ss[1] != LoginPassword.text)
                {
                    ShowError("登录失败！");
                }
            }
            // 有多组账号密码
            else
            {
                // 定义数组sss,按照竖线切割每一组账号密码
                string[] sss = s.Split('|');
                // 引入一个标志变量来记录是否登录成功
                bool loginSuccess = false;
                for (int i = 0; i < sss.Length; i++)
                {
                    // 定义数组ss，存储一组账号密码
                    string[] ss = sss[i].Split(',');
                    if (ss[0] == LoginAccount.text && ss[1] == LoginPassword.text)
                    {
                        // 跳转场景
                        SceneManager.LoadScene(1);
                        // 设置登录成功标志
                        loginSuccess = true; 
                        break;
                    }
                }
                // 如果循环结束后仍未找到匹配项，则显示错误提示
                if (!loginSuccess)
                {
                    ShowError("登录失败！");
                }
            }
        }
        //文本文档为空时，不能登录成功
        else
        {
            ShowError("登录失败！");
        }
        LoginAccount.text = "";
        LoginPassword.text = "";
    }

    public void ShowError(string errorInfo)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = errorInfo;
        Invoke("HideError", 1f);
    }
    private void HideError()
    {
        errorText.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }
}
