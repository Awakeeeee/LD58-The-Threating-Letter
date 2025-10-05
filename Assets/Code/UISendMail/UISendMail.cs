using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISendMail : MonoBehaviour
{
    public Transform envelopTransform;
    public Transform envelopStartRef;
    public Transform envelopEnterRef;

    public Transform leftHandTrans;
    public Transform leftHandStartRef;
    public Transform leftHandTakeLetterRef;
    //这一步随envelop动
    public Transform leftHandExitRef;

    public Transform rightHandTrans;
    public Transform rightHandStartRef;
    public Transform rightHandTakeLetterRef;
    //这一步随envelop动
    public Transform rightHandExitRef;

    private void Start()
    {
        envelopTransform.position = envelopStartRef.position;

        leftHandTrans.position = leftHandExitRef.position;
        rightHandTrans.position = rightHandExitRef.position;

        leftHandTrans.gameObject.SetActive(false);
        rightHandTrans.gameObject.SetActive(false);
    }

    public void InitEnterAnim()
    {
        envelopTransform.position = envelopStartRef.position;
        //envelopTransform

        leftHandTrans.gameObject.SetActive(false);
        rightHandTrans.gameObject.SetActive(false);
    }

    public void OnEnterAnim(Action finishCallback = null)
    {
        leftHandTrans.gameObject.SetActive(true);
        rightHandTrans.gameObject.SetActive(true);

        //这是在屏幕中间
        //leftHandTrans.position = leftHandStartRef.position;
        //rightHandTrans.position = rightHandStartRef.position;

        leftHandTrans.position = leftHandTakeLetterRef.position;
        rightHandTrans.position = rightHandTakeLetterRef.position;

        Vector3 envelopDeltaV3 = envelopEnterRef.position - envelopStartRef.position;
        envelopTransform.DOMove(envelopEnterRef.position, 0.4f).SetEase(Ease.InOutSine);
        leftHandTrans.DOMove(leftHandTakeLetterRef.position + envelopDeltaV3, 0.4f).SetEase(Ease.InOutSine);
        rightHandTrans.DOMove(rightHandTakeLetterRef.position + envelopDeltaV3, 0.4f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            leftHandTrans.DOMove(leftHandExitRef.position, 0.4f).SetDelay(0.2f);
            rightHandTrans.DOMove(rightHandExitRef.position, 0.4f).SetDelay(0.2f).onComplete += () =>
            {
                finishCallback?.Invoke();
            };
        };

        //selectCoverPropTemplate
    }

    public void OnExitAnim()
    {

    }
}
