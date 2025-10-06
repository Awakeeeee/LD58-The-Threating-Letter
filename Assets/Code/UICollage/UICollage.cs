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

    public Transform handExitStartRef;
    public Transform handExitEndRef;

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
        letterPaper.transform.position = letterOutOfScreenTrans.position;
    }

    public void OnEnterAnim(Action callback = null)
    {
        letterPaper.transform.DOMove(letterInScreenTrans.position, 0.5f).onComplete += () =>
        {
            callback?.Invoke();
        };
    }

    public void OnExitAnim(Action callback = null)
    {
        UIManager.Instance.rightHand.transform.position = handExitStartRef.transform.position;
        UIManager.Instance.rightHand.transform.DOMove(handExitEndRef.transform.position, 0.7f);
        letterPaper.transform.DOMove(letterOutOfScreenTrans.position, 0.5f).SetDelay(0.2f).onComplete += () =>
        {
            callback?.Invoke();
        };
    }
}
