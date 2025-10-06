using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISendMailConfirm : MonoBehaviour
{
    public Transform handWithLetter;
    public Transform handWithLetterOutOfScreen;
    public Transform handWithLetterInScreen;

    public Button returnButton;
    public Transform returnButtonInScreenRef;
    public Transform returnButtonOutOfScreenRef;
    public Button confirmButton;
    public Transform confirmButtonInScreenRef;
    public Transform confirmButtonOutOfScreenRef;


    private void Start()
    {
        handWithLetter.transform.position = handWithLetterOutOfScreen.transform.position;
        returnButton.transform.position = returnButtonOutOfScreenRef.position;
        confirmButton.transform.position = confirmButtonOutOfScreenRef.position;

        returnButton.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            UIManager.Instance.OnReturnBtnOnSendMailClicked();
        });

        confirmButton.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            UIManager.Instance.OnConfirmBtnOnSendMailClicked();
        });
    }

    public void InitEnterAnim()
    {
        handWithLetter.transform.position = handWithLetterOutOfScreen.transform.position;
        returnButton.transform.position = returnButtonOutOfScreenRef.position;
        confirmButton.transform.position = confirmButtonOutOfScreenRef.position;
    }

    public void OnEnterAnim(Action finishCallback = null)
    {
        //handWithLetter.transform.DOMove(handWithLetterInScreen.position, 0.5f).SetEase(Ease.InOutSine)
        //{
        //    finishCallback?.Invoke();
        //};
        returnButton.transform.DOMove(returnButtonInScreenRef.position, 0.5f).SetEase(Ease.InOutSine).SetDelay(0.1f).onComplete += () =>
        {
            finishCallback?.Invoke();
        };
        confirmButton.transform.DOMove(confirmButtonInScreenRef.position, 0.5f).SetEase(Ease.InOutSine).SetDelay(0.1f);
    }

    public void OnExitAnim(Action finishCallback = null)
    {
        handWithLetter.transform.DOMove(handWithLetterOutOfScreen.position, 0.3f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            finishCallback?.Invoke();
        };
        returnButton.transform.DOMove(returnButtonOutOfScreenRef.position, 0.3f).SetEase(Ease.InOutSine);
        confirmButton.transform.DOMove(confirmButtonOutOfScreenRef.position, 0.3f).SetEase(Ease.InOutSine);
    }
}
