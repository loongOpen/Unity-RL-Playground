// Magica Cloth 2.
// Copyright (c) 2023 MagicaSoft.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;


public class ModelController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> characterList = new List<GameObject>();

    [SerializeField]
    private float slowTime = 0.1f;

    private bool slow;

    protected void Start()
    {
        slow = false;
    }

    private void AnimatorAction(System.Action<Animator> act)
    {
        foreach (var chara in characterList)
        {
            if (chara && chara.activeInHierarchy)
            {
                var animator = chara.GetComponent<Animator>();
                if (animator)
                {
                    act(animator);
                }
            }
        }
    }


    public void OnNextButton()
    {
        AnimatorAction((ani) => ani.SetTrigger("Next"));
    }

    public void OnBackButton()
    {
        AnimatorAction((ani) => ani.SetTrigger("Back"));
    }

    public void OnSlowButton()
    {
        slow = !slow;

        float timeScale = slow ? slowTime : 1.0f;

        AnimatorAction((ani) => ani.speed = timeScale);
    }

}

