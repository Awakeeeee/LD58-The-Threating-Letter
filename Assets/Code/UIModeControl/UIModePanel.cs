using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class UIModePanel : MonoBehaviour
{
    public Button btnNavigateMode;
    public Button btnCarveMode;
    public Slider zoomSlider;

    void Start()
    {
        btnCarveMode.onClick.AddListener(() =>
        {
            Game.Instance.SwitchMode(GameMode.Carve);
            UpdateState();
        });

        btnNavigateMode.onClick.AddListener(() =>
        {
            Game.Instance.SwitchMode(GameMode.Navigate);
            UpdateState();
        });

        zoomSlider.minValue = Game.Instance.zoomLimit.x;
        zoomSlider.maxValue = Game.Instance.zoomLimit.y;
        zoomSlider.value = Game.Instance.GetZoom();
        zoomSlider.onValueChanged.AddListener(Game.Instance.OnZoomSliderChanged);

        Utils.EventManager.StartListening(GameEvent.OnZoomChanged, OnZoomChanged);

        UpdateState();
    }

    void OnDestroy()
    {
        Utils.EventManager.StopListening(GameEvent.OnZoomChanged, OnZoomChanged);
    }

    private void OnZoomChanged(object args)
    {
        if (args is float zoomValue)
        {
            zoomSlider.SetValueWithoutNotify(zoomValue);
        }
    }

    private void UpdateState()
    {
        Image imgCarve = btnCarveMode.GetComponent<Image>();
        Image imgNavi = btnNavigateMode.GetComponent<Image>();
        imgCarve.color = Color.white;
        imgNavi.color = Color.white;
        zoomSlider.gameObject.SetActive(false);

        if (Game.Instance.CurrentMode == GameMode.Navigate)
        {
            imgNavi.color = Color.yellow;
            zoomSlider.gameObject.SetActive(true);
        }
        else if (Game.Instance.CurrentMode == GameMode.Carve)
        {
            imgCarve.color = Color.yellow;
        }
    }
}
