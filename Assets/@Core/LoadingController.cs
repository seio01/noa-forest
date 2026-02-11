using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingController
{
    private UI_Loading _uiLoading;
    private bool _isOpening = false;

    public void OpenLoading<T>(Action<T> callback = null) where T : UI_Loading
    {
        if(_uiLoading) return;
        if(_isOpening) return;

        _isOpening = true;

        var name = typeof(T).Name;
        Managers.Resource.LoadAsync<GameObject>($"Prefabs/Loadings/{name}", (prefab) =>
        {
            _isOpening = false;
            if(prefab != null)
            {
                var loadingObj = UnityEngine.Object.Instantiate(prefab);
                loadingObj.transform.SetParent(Managers.UI.GetorCreateLoadingRoot().transform, false);
                var uiLoading = loadingObj.GetComponent<UI_Loading>();
                if(uiLoading)
                {
                    _uiLoading = uiLoading;
                    callback?.Invoke(uiLoading as T);
                }
            }
        });
    }

    public void CloseLoading()
    {
        _isOpening = false;
        
        if(!_uiLoading) return;

        UnityEngine.Object.Destroy(_uiLoading);
        _uiLoading = null;
    }
}
