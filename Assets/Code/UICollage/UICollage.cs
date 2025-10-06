using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICollage : MonoBehaviour
{
    public GameObject letterPaper;
    public Transform letterInScreenTrans;
    public Transform letterOutOfScreenTrans;

    private void OnEnable()
    {
        letterPaper.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        letterPaper.gameObject.SetActive(false);
    }

    public void InitAnim()
    {
        //letterPaper.transform.position = letterOutOfScreenTrans.position;
    }

    public void OnEnterAnim(Action callback = null)
    {
        callback?.Invoke();

        //letterPaper.transform.DOMove(letterInScreenTrans.position, 0.5f).onComplete += () =>
        //{
        //    callback?.Invoke();
        //};
    }

    public void OnExitAnim(Action callback = null)
    {
        callback?.Invoke();

        //letterPaper.transform.DOMove(letterOutOfScreenTrans.position, 0.5f).onComplete += () =>
        //{
        //    callback?.Invoke();
        //};
    }
}
