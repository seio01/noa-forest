using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonColorType
{
    None,
    White,
    Yellow,
    Gray
}

[Serializable]
public class ButtonBaseData
{
    public ButtonColorType colorType;
    public Sprite sprite;
}

public class ButtonBase : UI_Base, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("버튼 색상 데이터")]
    [SerializeField] private ButtonBaseData[] buttonColorStyleDataList;

    [Space(10)][Header("스타일 지정")]
    [SerializeField] private ButtonColorType buttonColorType;
    [SerializeField] private bool interactable;

    [Space(10)][Header("컴포넌트")]
    [SerializeField] private Image imageBackground;
    [SerializeField] private Button buttonClick;
    [SerializeField] private TextBase textButton;

    [Space(10)][Header("버튼 효과")]
    [SerializeField] private bool hasSound = true;
    [SerializeField] private bool hasAnimation = true;
    [SerializeField] private RectTransform buttonRect;
    [Range(0.5f, 1f)] [SerializeField] float pressedScale = 0.95f;
    [SerializeField] float downDuration = 0.08f;
    [SerializeField] float upDuration = 0.08f;

    private ButtonColorType _originalColorType;
    // 스케일 복원용
    private Vector3 _originalScale;
    //드래그 판정
    private ScrollRect _scrollRect;
    //버튼 액션
    public ActionEvent OnClick = new (); 
    
    public bool Interactable
    {
        get => buttonClick != null && buttonClick.interactable;
        set
        {
            if (buttonClick == null) return;
            
            buttonClick.interactable = value;
            interactable = value;
            ApplyButtonColorType();
        }
    }
    public bool Enabled
    {
         get => buttonClick != null && buttonClick.enabled;
            set
            {
                if (buttonClick == null) return;
                buttonClick.enabled = value;
            }
    }

    public ButtonColorType ColorType
    {
        get => buttonColorType;
        set
        {
            buttonColorType = value;
            _originalColorType = buttonColorType;
            ApplyButtonColorType();
        }
    }

    private void Awake() 
    {
        _originalColorType = buttonColorType;
        interactable = buttonClick!= null && buttonClick.interactable;
        _originalScale = buttonRect?.localScale ?? new Vector3(1, 1, 1);
        _scrollRect = GetComponentInParent<ScrollRect>();

        if(buttonClick)
        {
            buttonClick.onClick.AddListener(OnClickButton);
        }
    }

    public void ApplyButtonColorType()
    {
        if(_originalColorType == ButtonColorType.None)
        {
            return;
        }
        
        var targetColorType = Interactable ? _originalColorType : ButtonColorType.Gray;
        Debug.Log("ApplyButtonColorType" + interactable + targetColorType);
        SetButtonBackground(targetColorType);
    }

    public void SetButtonBackground(ButtonColorType type)
    {
        if(buttonColorStyleDataList == null || imageBackground == null)
            return;
        
        var targetData = buttonColorStyleDataList.FirstOrDefault(x => x.colorType == type);
        if(targetData != null)
        {
            imageBackground.sprite = targetData.sprite;
        }
    }

    public void SetButtonText(string text)
    {
        if(textButton)
        {
            textButton.text = text;
        }
    }

    private void OnClickButton()
    {
        Debug.Log("Click Event");
        // 공통 클릭 처리
        if(hasSound)
        {
            //TODO 사운드 재생 구현
        }

        //커스텀 클릭 처리
        OnClick?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!buttonRect) return;
        if(!hasAnimation) return;
        if(!Interactable) return;

        buttonRect.DOKill();
        buttonRect.DOScale(_originalScale * pressedScale, downDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        RestoreButtonScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestoreButtonScale();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!_scrollRect) return;

        //드래그 시작 순간 스케일 복원
        RestoreButtonScale();
        eventData.eligibleForClick = false; // 클릭 취소

        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!_scrollRect) return;
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!_scrollRect) return;
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
    }
    
    private void RestoreButtonScale()
    {
        if(!buttonRect) return;
        if(!hasAnimation) return;
        if(!Interactable) return;

        buttonRect.DOKill();
        buttonRect.DOScale(_originalScale, upDuration);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _originalColorType = buttonColorType;

        if (buttonClick)
            buttonClick.interactable = interactable;

        UnityEditor.EditorApplication.delayCall -= DelayedApply;
        UnityEditor.EditorApplication.delayCall += DelayedApply;
    }

    private void DelayedApply()
    {
        if (this == null) return;
        ApplyButtonColorType();
    }
#endif

}
