using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager
{
    private const string GlobalRoot = "@Gloabl_UI_Root";
    private const string SceneRoot = "@Scene_UI_Root";

    private const int GlobalSortingOrder = 9999;

    private PopupController _popupController;

    public GameObject GetorCreateGlobalRoot()
    {
        var globalRoot = GameObject.Find(GlobalRoot);
        if(globalRoot == null)
        {
            globalRoot = new GameObject(GlobalRoot);
            UnityEngine.Object.DontDestroyOnLoad(globalRoot);
        }

        return globalRoot;
    }

    public GameObject GetorCreateSceneRoot()
    {
        var sceneRoot = GameObject.Find(SceneRoot);
        if(sceneRoot == null)
        {
            sceneRoot = new GameObject(SceneRoot);
        }

        return sceneRoot;
    }

    public void Init()
    {
        _popupController = new PopupController();
        _popupController.InitBackgroundPopup();
    }

    public void OpenPopup<T>(string name, Action<T> callback = null) where T : PopupBase
    {
        _popupController?.OpenPopup(name, callback);
    }

    public void ClosePopup()
    {
        _popupController?.ClosePopup();
    }

    public void CloseAllPopup()
    {
        _popupController?.CloseAllPopup();
    }

    public void ClosePopupUntil<T>(string popupName) where T : PopupBase
    {
        _popupController?.ClosePopupUntil<T>(popupName);
    }

    public void SetCanvas(GameObject go)
    {
        Canvas canvas = Utils.GetorAddComponent<Canvas>(go);
        CanvasScaler scaler = Utils.GetorAddComponent<CanvasScaler>(go);
        Utils.GetorAddComponent<GraphicRaycaster>(go);

        // canvas init
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GetMainCamera();
        canvas.overrideSorting = true;

        // canvas scaler init
        int setWidth = 1080;
        int setHeight = 1920;
        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(setWidth, setHeight);

        if((float)setWidth/setHeight < (float)deviceWidth/deviceHeight)
        {
            scaler.matchWidthOrHeight = 1;
        }
        else
        {
            scaler.matchWidthOrHeight = 0;
        }
    }

    private Camera GetMainCamera()
    {
        Camera camera = Camera.main;

        if(camera == null)
        {
            var obj = GameObject.Find("Main Camera");
            if(obj != null)
                camera = obj.GetComponent<Camera>();
        }

        if(camera == null)
        {
            Debug.LogWarning("[UIManager] Cannot Find Camera");
        }

        return camera;
    }

}
