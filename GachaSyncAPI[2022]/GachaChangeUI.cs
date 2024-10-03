using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaChangeUI : MonoBehaviour
{
    [Header("UI")]
    [Space(10)]
    [SerializeField] TextMeshProUGUI namePreTitleText;
    [SerializeField] TextMeshProUGUI nameTitleText;
    [SerializeField] Image backgroundImage , BackgroundFillImage , slotHeadBGImage , slotBGImage , cogMainImage;
    [SerializeField] Image[] cogSubImage;
    [Header("Standard UI")]
    [Space(10)]
    [SerializeField] Sprite BackgroundStandardSprite;
    [SerializeField] Sprite slotHeadBGStandardSprite , slotBGStandardSprite , cogMainStandardSprite
                          , cogSubStandardSprite;
    [Header("PVP UI")]
    [Space(10)]
    [SerializeField] Sprite BackgroundPVPSprite;
    [SerializeField] Sprite slotHeadBGPVPSprite , slotBGPVPSprite , cogMainPVPSprite , cogSubPVPSprite;

    private readonly Color _blackHeadTitleColor = new Color(0xEE, 0xDB, 0x7A, 0xFF);
    private readonly Color _whiteHeadTitleColor = new Color(0x00, 0x00, 0x00, 0xFF);

    public void SetGachaImage(bool isPVP)
    {
        namePreTitleText.color = isPVP ? _whiteHeadTitleColor : _blackHeadTitleColor;
        nameTitleText.color = isPVP ? _whiteHeadTitleColor : _blackHeadTitleColor;
        backgroundImage.sprite = isPVP ? BackgroundPVPSprite : BackgroundStandardSprite;
        slotHeadBGImage.sprite = isPVP ? slotHeadBGPVPSprite : slotHeadBGStandardSprite;
        slotBGImage.sprite = isPVP ? slotBGPVPSprite : slotBGStandardSprite;
        cogMainImage.sprite = isPVP ? cogMainPVPSprite : cogMainStandardSprite;
        foreach (Image cogSub in cogSubImage)
        {
            cogSub.sprite = isPVP ? cogSubPVPSprite : cogSubStandardSprite;
        }
        BackgroundFillImage.color = isPVP ? new Color32(0,0,0,120) : new Color32(0,0,0,195);
    }
}
