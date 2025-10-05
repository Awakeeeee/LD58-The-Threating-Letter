using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    None,
    OnCutComplete,
    OnZoomChanged,
    OnModeChanged,
}

public enum GameMode
{
    None,
    Carve,
    Navigate,
    Free, //左键切图 右键移动 滚轮缩放
}
