using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UI_Toast : UI_Base
{
    [SerializeField] private TextBase textToast;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.18f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.14f;

    [Header("Scale")]
    [SerializeField] private float appearFromScale = 0.96f;
    [SerializeField] private float disappearToScale = 0.98f;
    [SerializeField] private float popScale = 1.03f;

    private Sequence _seq;

    public void Show(string text)
    {
        if (!rectTransform || !canvasGroup || !textToast) return;

        textToast.text = text;

        KillTween();

        // 초기값
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * appearFromScale;

        _seq = DOTween.Sequence();

        // ===== 등장: FadeIn + 부드러운 살짝 팝 =====
        // Fade는 전체 fadeInDuration에 걸쳐 진행
        _seq.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic));

        // Scale은 fade 안에서 자연스럽게: (0.98 -> 1.03 -> 1.00)
        // 업/다운 사이에 아주 짧은 '숨'을 넣어서 이어지는 느낌 완화
        float up = fadeInDuration * 0.65f;
        float down = fadeInDuration * 0.35f;

        _seq.Join(rectTransform.DOScale(popScale, up).SetEase(Ease.OutCubic));
        _seq.AppendInterval(0.02f); // 아주 미세한 완충(필요없으면 0으로)
        _seq.Append(rectTransform.DOScale(1f, down).SetEase(Ease.OutQuad));
        // =========================================

        // 대기
        _seq.AppendInterval(holdDuration);

        // ===== 퇴장: FadeOut + 아주 살짝 축소 =====
        _seq.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InCubic));
        _seq.Join(rectTransform.DOScale(disappearToScale, fadeOutDuration).SetEase(Ease.InQuad));
        // ========================================

        _seq.SetUpdate(true); // UI라면 타임스케일 영향 안 받게(원하면 제거)

        _seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }


    private void KillTween()
    {
        if (_seq != null && _seq.IsActive())
        {
            _seq.Kill();
        }

        _seq = null;

        if (rectTransform)
            rectTransform.DOKill();
        if (canvasGroup)
            canvasGroup.DOKill();
    }
}
