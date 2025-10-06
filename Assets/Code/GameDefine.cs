using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    None,
    OnCutComplete,
    OnZoomChanged,
    OnModeChanged,
    OnStartSticking,
    OnCollectionChange,
    OnPrepareSendMail,
}

public enum GameMode
{
    None,
    Free, //左键切图 右键移动 滚轮缩放
    Sticking,
}

public class CommonSFX
{
    public static string button = "sfx_button";
}