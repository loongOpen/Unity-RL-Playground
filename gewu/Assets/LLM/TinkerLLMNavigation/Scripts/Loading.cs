using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Loading : MonoBehaviour
{
    public Slider slider;
    public Text PromoteInfo;
    float t = 0;
    void Start()
    {
        slider = transform.GetChild(1).GetComponent<Slider>();
        slider.maxValue = 100;
        slider.minValue = 0;
        slider.value = 0;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t < 7)
        {
            slider.value = t * 14.28f;
            PromoteInfo.text = (int)slider.value + "%";
        }
        else
        {
            PromoteInfo.text = "100%";
            SceneManager.LoadScene(2);
        }
    }
}
