using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T mPlayModeInstance;
    private static T mEditModeInstance;

    public static T Instance
    {
        get
        {
            if(Application.isPlaying)
            {
                return mPlayModeInstance;
            }
            else
            {
                if(mEditModeInstance == null)
                {
                    mEditModeInstance = GameObject.FindAnyObjectByType<T>();
                }
                return mEditModeInstance;
            }
        }
    }


    protected virtual void Awake()
    {
        if (mPlayModeInstance == null)
        {
            mPlayModeInstance = this as T;
        }
        else if (mPlayModeInstance != this)
        {
            Destroy(gameObject);
        }
        //DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    //[InitializeOnLoadMethod]
    //private static void OnDomainReload()
    //{
    //    mEditModeInstance = null;
    //    EditorApplication.playModeStateChanged += OnPlayModeChange;
    //}

    //private static void OnPlayModeChange(PlayModeStateChange mode)
    //{
    //    if(mode == PlayModeStateChange.EnteredEditMode || mode == PlayModeStateChange.ExitingEditMode)
    //    {
    //        mEditModeInstance = null;
    //    }
    //}
#endif
}