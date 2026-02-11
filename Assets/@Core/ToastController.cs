using UnityEngine;

public class ToastController
{
    public void ShowToast(string text)
    {
        Managers.Resource.LoadAsync<GameObject>("Prefabs/UI_Toast", (prefab) =>
        {
            if (prefab != null)
            {
                CreateToast(prefab, text);
            }
            else
            {
                Debug.LogError("[ToastController] UI_Toast prefab is null.");
            }
        });
    }

    private void CreateToast(GameObject prefab, string text)
    {
        GameObject go = Object.Instantiate(prefab);
        go.transform.SetParent(Managers.UI.GetorCreateToastRoot().transform, false);
        var uiToast = go.GetComponent<UI_Toast>();
        if(uiToast)
        {
            uiToast.Show(text);
        }
    }
}