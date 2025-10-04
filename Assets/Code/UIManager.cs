using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public Transform leftUIRef;
    public Transform centerUIRef;
    public Transform rightUIRef;

    public UISelectCover selectCoverUI;
    public UICollage collageUI;
    public UISendMail sendMailUI;

    private void Start()
    {
        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = centerUIRef.position;
        sendMailUI.transform.position = rightUIRef.position;

        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(true);
    }

    public void ChangeFromSelectCoverToCollage(Action callback)
    {
        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = rightUIRef.position;
        collageUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            callback?.Invoke();
        };
        sendMailUI.transform.position = rightUIRef.position;
    }

    public void ChangeToSelectCover(Action callback)
    {
        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(false);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        selectCoverUI.InitEnterAnim();
        selectCoverUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            selectCoverUI.OnEnterAnim();
            callback?.Invoke();
        };
        collageUI.transform.position = rightUIRef.position;
        sendMailUI.transform.position = rightUIRef.position;
    }

    public void ChangeToSendMail(Action callback)
    {
        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(false);
        sendMailUI.gameObject.SetActive(true);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = leftUIRef.position;
        sendMailUI.transform.position = rightUIRef.position;
        sendMailUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            callback?.Invoke();
        };
    }

    public void ChangeFromSendMailToCollage(Action callback)
    {
        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = leftUIRef.position;
        collageUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            callback?.Invoke();
        };
        sendMailUI.transform.position = rightUIRef.position;

    }
}
