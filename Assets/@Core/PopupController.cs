using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupController
{
    private Stack<PopupBase> _popupStack = new Stack<PopupBase>();
    private HashSet<string> _uniquePopupSet = new HashSet<string>();
    private int _currentOrder = 10;
    private int _baseOrder = 10;
    private bool _isLoading;
    private bool _backgroundFailed;

    public Canvas BackgroundPopup;
    public PopupBase CurrentPopup
    {
        get
        {
            if (_popupStack.Count == 0)
                return null;
            
            return _popupStack.Peek();
        }
    }

    public void OpenPopup<T>(string name, Action<T> callback = null) where T : PopupBase
    {
        Debug.Log($"[PopupController] Popup Open Requested : {name}");

        InitBackgroundPopup();

        Managers.Coroutine.StartCoroutine(OpenPopupRoutine(name, callback));
    }

    private IEnumerator OpenPopupRoutine<T>(string name, Action<T> callback = null) where T : PopupBase
    {
        var req = Resources.LoadAsync<GameObject>($"Prefabs/Popups/{name}");

        yield return req;
        
        while(BackgroundPopup == null && !_backgroundFailed)
            yield return null;
        if(_backgroundFailed) yield break;

        GameObject popup = req.asset as GameObject;

        if(popup == null || popup.GetComponent<T>() == null)
        {
            Debug.LogError("[PopupController] popup is null");
            yield break;
        }

        var popupBase = popup.GetComponent<PopupBase>();
        //팝업 중복 호출 설정
        var allowDuplicatePopup = popupBase.AllowDuplicatePopup;
        if(!allowDuplicatePopup && _uniquePopupSet.Contains(name))
        {
            yield break;
        }

        var popupObj = UnityEngine.Object.Instantiate(popup);
        var popupObjBase = Utils.GetorAddComponent<PopupBase>(popupObj);

        popupObjBase.PopupKey = name;
        _popupStack.Push(popupObjBase);

        if(!popupObjBase.AllowDuplicatePopup)
            _uniquePopupSet.Add(name);

        Managers.UI.SetCanvas(popupObj);
        
        var canvas = Utils.GetorAddComponent<Canvas>(popupObj);
        SetCurrentSortingOrder();
        canvas.sortingOrder = _currentOrder;

        var enableBackgroundClick = popupObjBase.EnableBackgroundClick;
        if(BackgroundPopup)
            SetEnableBackgroundClick(enableBackgroundClick);

        popupObj.transform.SetParent(Managers.UI.GetorCreateSceneRoot().transform);

        callback?.Invoke(popupObj.GetComponent<T>());
        popupObjBase.OnOpen();
    }

    public void ClosePopup()
    {
        if(_popupStack.Count == 0)
        {
            Debug.LogError("[PopupController] Popup stack empty");
            return;
        }

        PopupBase popupBase = _popupStack.Pop();
        if(popupBase == null)
        {
            Debug.LogError("[PopupController] Popup is null");
            return;
        }

        SetCurrentSortingOrder();
        if (BackgroundPopup)
        {
            bool enable = CurrentPopup != null && CurrentPopup.EnableBackgroundClick;
            SetEnableBackgroundClick(enable);
        }

        if(!popupBase.AllowDuplicatePopup)
            _uniquePopupSet.Remove(popupBase.PopupKey);
        popupBase.OnClose();
    }

    public void CloseAllPopup()
    {
        while(_popupStack.Count > 0)
        {
            ClosePopup();
        }
    }

    /// <summary>
    /// 특정 팝업이 맨 위에 올 때까지 팝업 닫기
    /// </summary>
    /// <param name="popupName"></param>
    public void ClosePopupUntil<T>(string popupName) where T : PopupBase
    {
        while(_popupStack.Count > 0)
        {
            var popupBase = _popupStack.Peek();
            if(popupBase is T && popupBase.PopupKey.Equals(popupName))
            {
                break;
            }
            ClosePopup();
        }
    }

    private void SetCurrentSortingOrder()
    {
        _currentOrder = _baseOrder + _popupStack.Count * 2;
        if(BackgroundPopup)
            BackgroundPopup.sortingOrder = _currentOrder - 1;
    }

    //백패널 관리
    public void InitBackgroundPopup()
    {
        if(BackgroundPopup != null || _isLoading) return;

        _backgroundFailed = false;
        _isLoading = true;
        Managers.Coroutine.StartCoroutine(InitBackgroundPopupRoutine());
    }

    private IEnumerator InitBackgroundPopupRoutine()
    {
        var req = Resources.LoadAsync("Prefabs/Popups/UI_BackgroundPopup");

        yield return req;

        GameObject popup = req.asset as GameObject;
        _isLoading = false;
        if(popup == null)
        {
            _backgroundFailed = true;
            Debug.LogError("[PopupController] backgroundPopup is null");
            yield break;
        }

        var obj = UnityEngine.Object.Instantiate(popup);
        Managers.UI.SetCanvas(obj);
        BackgroundPopup = Utils.GetorAddComponent<Canvas>(obj);
        obj.transform.SetParent(Managers.UI.GetorCreateSceneRoot().transform);
        obj.transform.SetAsFirstSibling();
        BackgroundPopup.sortingOrder = _currentOrder -1;
        SetEnableBackgroundClick(false);
    }

    private void SetEnableBackgroundClick(bool enable)
    {
        BackgroundPopup?.gameObject.SetActive(enable);
    }
}
