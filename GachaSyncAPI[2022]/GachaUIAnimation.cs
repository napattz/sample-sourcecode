using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GachaUIAnimation : MonoBehaviour
{
    [Header("Main UI")]
    [Space(10)]
    [SerializeField] private Image spinBtn;
    [SerializeField] private Image spinx10Btn;
    [SerializeField] private Image skipBtn;

    [Header("Reward Popup")]
    [Space(10)]
    [SerializeField] private CanvasGroup rewardPanel;
    [SerializeField] private RectTransform rewardItemTransform;
    [SerializeField] private GameObject particleAura;
    [SerializeField] private Button claimBtn;

    public void ShowHideSpinBtn(bool Show)
    {
        RectTransform spinRect = spinBtn.GetComponent<RectTransform>();
        RectTransform spinx10Rect = spinx10Btn.GetComponent<RectTransform>();
        RectTransform skipRect = skipBtn.GetComponent<RectTransform>();
        spinBtn.gameObject.SetActive(true);
        spinx10Btn.gameObject.SetActive(true);
        skipBtn.gameObject.SetActive(true);
        if (Show)
        {
            spinRect.anchoredPosition3D = new Vector2(-300, -190);
            spinx10Rect.anchoredPosition3D = new Vector2(300, -190);
            skipRect.anchoredPosition3D = new Vector2(-190, 0);

            spinBtn.DOFade(1, MasterAnimationManager.speed);
            spinx10Btn.DOFade(1, MasterAnimationManager.speed);
            skipBtn.DOFade(0, MasterAnimationManager.speed).OnComplete(()=> skipBtn.gameObject.SetActive(false));
            spinRect.DOAnchorPos3DX(-150, MasterAnimationManager.speed);
            spinx10Rect.DOAnchorPos3DX(150, MasterAnimationManager.speed);
            skipRect.DOAnchorPos3DY(-240, MasterAnimationManager.speed);
        }
        else
        {
            spinRect.anchoredPosition3D = new Vector2(-150, -190);
            spinx10Rect.anchoredPosition3D = new Vector2(150, -190);
            skipRect.anchoredPosition3D = new Vector2(0, -240);

            spinBtn.DOFade(0, MasterAnimationManager.speed);
            spinx10Btn.DOFade(0, MasterAnimationManager.speed);
            skipBtn.DOFade(1, MasterAnimationManager.speed);
            spinRect.DOAnchorPos3DX(-300, MasterAnimationManager.speed).OnComplete(()=>spinBtn.gameObject.SetActive(false));
            spinx10Rect.DOAnchorPos3DX(300, MasterAnimationManager.speed).OnComplete(()=>spinx10Btn.gameObject.SetActive(false));
            skipRect.DOAnchorPos3DY(-190, MasterAnimationManager.speed);
        }
    }

    public void ShowRewardAnimate()
    {
        particleAura.SetActive(false);
        AudioManager.instance.LoadResPlay(AudioNames.win);
        Image rewardItemImage = rewardItemTransform.GetComponent<Image>();
        rewardItemTransform.anchoredPosition = new Vector2(0, 100);
        Color tempColor = rewardItemImage.color;
        spinBtn.gameObject.SetActive(false);
        spinx10Btn.gameObject.SetActive(false);
        skipBtn.gameObject.SetActive(false);
        rewardPanel.gameObject.SetActive(true);
        rewardPanel.alpha = 0;
        tempColor.a = 0f;
        rewardItemImage.color = tempColor;
        rewardPanel.DOFade(1, MasterAnimationManager.speed * 2).OnComplete(() =>
        {
            rewardItemImage.DOFade(1, MasterAnimationManager.speed);
            rewardItemTransform.DOAnchorPos3DY(0, MasterAnimationManager.speed).OnComplete(() => particleAura.SetActive(true));
        });
    }
}
