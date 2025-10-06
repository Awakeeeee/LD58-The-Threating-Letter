using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviourSingleton<TutorialManager>
{
    public TutorialTypeEnum RecentTutorial { get; private set; }

    //内存里记一下已完成的Tutorial 不持久化
    private List<TutorialTypeEnum> finishedTutorial = new List<TutorialTypeEnum>();

    public void CheckAndStartTutorial(TutorialTypeEnum targetTutorialType)
    {
        if (finishedTutorial.Contains(targetTutorialType))
            return;

        RecentTutorial = targetTutorialType;
        if (targetTutorialType == TutorialTypeEnum.EnterGameOutOfStock)
        {
            StartEnterGameOutOfStockTutorial();
        }
        else if (targetTutorialType == TutorialTypeEnum.EnterSelectCover)
        {
            StartEnterSelectCover();
        }
        else if (targetTutorialType == TutorialTypeEnum.EnterKnifeCutter)
        {
            StartEnterKnifeCutterTutorial();
        }
        else if (targetTutorialType == TutorialTypeEnum.BackToCollage)
        {
            StartBackToCollageTutorial();
        }
        else if (targetTutorialType == TutorialTypeEnum.FreeMode)
        {
            StartFreeModeTutorial();
        }
        else if (targetTutorialType == TutorialTypeEnum.FreeModeEnterSelectCover)
        {
            StartFreeModeEnterSelectCover();
        }
    }

    public void FinishTutorial(TutorialTypeEnum targetTutorialType)
    {
        if (RecentTutorial == targetTutorialType)
        {
            RecentTutorial = TutorialTypeEnum.None;
            finishedTutorial.Add(targetTutorialType);
        }
    }

    private void StartEnterGameOutOfStockTutorial()
    {
        UIManager.Instance.overLayUI.ShowDialouge("I'm writing a blackmail.\r\nBut without enough O & R letters, it won't be done.", () =>
        {
            UIManager.Instance.overLayUI.ShowDialouge("I need more O/R letters.\r\nNow its time To Collect O/R.", () =>
            {
                UIManager.Instance.overLayUI.ShowDialouge("Press \"To Collect\" to cut more O/R.",
                () =>
                {
                    UIManager.Instance.overLayUI.HideDialogue();
                    FinishTutorial(TutorialTypeEnum.EnterGameOutOfStock);
                });
            });
        });
    }

    private void StartEnterSelectCover()
    {
        UIManager.Instance.overLayUI.ShowDialouge("Look at there game cases.\r\nwith all newspapers are gone, I have no choice but cut those gamebox to Collect O/R.", () =>
        {
            UIManager.Instance.overLayUI.ShowDialouge("Press game case to start cutting letters.", () =>
            {
                UIManager.Instance.overLayUI.HideDialogue();
                FinishTutorial(TutorialTypeEnum.EnterSelectCover);
            });
        });
    }

    private void StartEnterKnifeCutterTutorial()
    {
        UIManager.Instance.overLayUI.ShowDialouge("Press&Drag to cut.\r\nRemember, I only have to Collect O/R, so just cut them.", () =>
        {
            UIManager.Instance.overLayUI.HideDialogue();
            FinishTutorial(TutorialTypeEnum.EnterKnifeCutter);
        });
    }

    private void StartBackToCollageTutorial()
    {
        UIManager.Instance.overLayUI.ShowDialouge("To paste O/R to the blackmail, Press \"Collected Letters\".\r\nThen Drag some O/R to the place it belongs.\r\n", () =>
        {
            UIManager.Instance.overLayUI.ShowDialouge("When It's thought to be done, Press \"Send Mail\".", () =>
            {
                UIManager.Instance.overLayUI.HideDialogue();
                FinishTutorial(TutorialTypeEnum.BackToCollage);
            });
        });
    }

    private void StartFreeModeTutorial()
    {
        UIManager.Instance.overLayUI.ShowDialouge("In Free Mode, you have an empty paper to play with.\r\nFeel free to cut any letter you like and paste them to the paper.", () =>
        {
            UIManager.Instance.overLayUI.HideDialogue();
            FinishTutorial(TutorialTypeEnum.FreeMode);
        });
    }

    private void StartFreeModeEnterSelectCover()
    {
        UIManager.Instance.overLayUI.ShowDialouge("In Free Mode ,there are more game cases you can cut.\r\nEach time you come to this view, random game cases will appear.", () =>
        {
            UIManager.Instance.overLayUI.HideDialogue();
            FinishTutorial(TutorialTypeEnum.FreeModeEnterSelectCover);
        });
    }
}

public enum TutorialTypeEnum
{
    None = 0,
    EnterGameOutOfStock = 1,//刚进入游戏 “我用完了所有的库存” 可能后续不是第一个？
    EnterSelectCover = 2,//I only have game cover
    EnterKnifeCutter = 3,//introducing mouse control
    BackToCollage = 4,//return to collage with at least one cutimage
    FreeMode = 5,
    FreeModeEnterSelectCover = 6,
}