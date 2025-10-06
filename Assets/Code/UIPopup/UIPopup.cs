using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviourSingleton<UIPopup>
{
    public Animator anim;
    public RectTransform popRoot;
    public Image popTitle;
    public Sprite nice;
    public Sprite good;
    public Sprite perfect;

    public void Show(int score, Vector3 pos)
    {
        transform.position = pos;
        Sprite img = nice;
        if (score <= 1)
        {
            img = nice;
        }
        else if (score >= 3)
        {
            img = perfect;
        }
        else
        {
            img = good;
        }
        popTitle.sprite = img;

        popRoot.gameObject.SetActive(true);
        anim.SetTrigger("pop");
    }

    public void Hide()
    {
        popRoot.gameObject.SetActive(false);
    }
}
