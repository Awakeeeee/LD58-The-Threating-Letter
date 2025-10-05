using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITitle : MonoBehaviour
{
    public List<GameObject> oGOList = new List<GameObject>();
    public List<GameObject> rGOList = new List<GameObject>();

    public Transform titleTrans;
    public Transform titleInScreenTrans;
    public Transform titleOutScreenTrans;
    public Button startButton;
    //public Button startButtonWithFreeMode;

    private float accumulatedTime;
    private float changeInterval = 0.5f;

    private int random1;
    private int random2;

    private void Start()
    {
        titleTrans.transform.position = titleInScreenTrans.transform.position;

        RandomOAndR();

        startButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OnStartClicked();
        });
    }

    private void Update()
    {
        accumulatedTime += Time.deltaTime;
        if (accumulatedTime >= changeInterval)
        {
            accumulatedTime = 0;
            RandomOAndR();
        }
    }

    private void RandomOAndR()
    {
        if (oGOList.Count > 1 && rGOList.Count > 1)
        {
            int newR1 = UnityEngine.Random.Range(0, oGOList.Count);
            int newR2 = UnityEngine.Random.Range(0, rGOList.Count);
            while (newR1 == random1)
            {
                newR1 = UnityEngine.Random.Range(0, oGOList.Count);
            }
            while (newR2 == random2)
            {
                newR2 = UnityEngine.Random.Range(0, rGOList.Count);
            }
            random1 = newR1;
            random2 = newR2;

            for (int i = 0; i < oGOList.Count; ++i)
            {
                oGOList[i].SetActive(i == random1);
                if (i == random1)
                    oGOList[i].transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-10, 10));
            }
            for (int i = 0; i < rGOList.Count; ++i)
            {
                rGOList[i].SetActive(i == random2);
                if (i == random2)
                    oGOList[i].transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-10, 10));
            }
        }
    }

    public void OnExitAnim(Action callback)
    {
        startButton.gameObject.SetActive(false);

        titleTrans.transform.DOMove(titleOutScreenTrans.position, 0.5f).onComplete += () =>
        {
            callback?.Invoke();
        };
    }
}
