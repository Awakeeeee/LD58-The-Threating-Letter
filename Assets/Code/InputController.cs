using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private InputConfigure mConfig;

    //press
    private bool mIsPressing;
    private Vector2 mPrimaryStartPosition;

    //hold
    public float holdDelay = 0.5f;
    private float mHoldTimer;

    private Camera WorldCamera
    {
        get
        {
            if(mCamera == null)
            {
                mCamera = Camera.main;
            }
            return mCamera;
        }
    }
    private Camera mCamera;

    public void Init()
    {
        mConfig = new InputConfigure();
        mConfig.Enable();

        mConfig.PlayerControl.Point.performed += OnPointerDown;
        mConfig.PlayerControl.Point.canceled += OnPointerUp;

        mIsPressing = false;
        mPrimaryStartPosition = Vector2.zero;
        mHoldTimer = 0f;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if(mIsPressing)
        {
            if(holdDelay >= 0f) //hold enabled
            {
                mHoldTimer += Time.deltaTime;
                if (mHoldTimer >= holdDelay)
                {
                    OnHolding();
                }
            }

            //
        }
    }

    private void OnPointerDown(InputAction.CallbackContext context)
    {
        mIsPressing = true;
        mHoldTimer = 0f;
        mPrimaryStartPosition = Pointer.current.position.value;
    }

    private void OnPointerUp(InputAction.CallbackContext context)
    {
        mIsPressing = false;
        mHoldTimer = 0f;
    }

    private void OnHolding()
    {
        //float holdingTime = mHoldTimer - holdTime;
    }

    private Vector3 GetCurrentPointerWorldPosition()
    {
        Vector2 currentPosition = Pointer.current.position.value;
        Vector3 s2w = WorldCamera.ScreenToWorldPoint(new Vector3(currentPosition.x, currentPosition.y, 0f));
        return new Vector3(s2w.x, s2w.y, 0f);
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
    }
#endif
}