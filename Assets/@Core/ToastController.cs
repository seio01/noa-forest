using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastController
{
    public void ShowToast(string text)
    {
        Managers.Coroutine.StartCoroutine(ShowToastRoutine(text));
    }

    private IEnumerator ShowToastRoutine(string text)
    {
        var req = Resources.LoadAsync<GameObject>("Prefabs/UI_Toast");

        yield return req;

        GameObject toast = req.asset as GameObject;
        if(toast != null)
        {
            var toastObj = Object.Instantiate(toast);
            toastObj.transform.SetParent(Managers.UI.GetorCreateToastRoot().transform, false);
            var uiToast = toastObj.GetComponent<UI_Toast>();
            if(uiToast != null)
            {
                uiToast.Show(text);
            }
        }
    }
}
