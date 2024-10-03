using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaItemPanel : MonoBehaviour
{
    public int rewardID;
    public Image itemImage;
    [SerializeField] private Image itemBG;
    [SerializeField] private Sprite[] rarerityBG;
    [SerializeField] private TextMeshProUGUI amountText;
    public Reward rewardModel
    {
        get { return _rewardModel; }
        set
        {
            _rewardModel = value;
            UpdateUI();
        }
    }
    public Sprite iconSprite
    {
        get
        {
            string itemName = rewardModel.gpc == null ? rewardModel.item.name : "$Moo";
            Sprite temp = AssetBundleDB.instance.Load<Sprite>($"Item Sprites/Large Sprite", itemName);
            if (temp == null) temp = AssetBundleDB.instance.LoadAll<Sprite>($"Item Sprites/Large Sprite",true).Find(sprite => sprite.name == itemName);
            return temp;
        }
    }

    private Reward _rewardModel;

    private void UpdateUI()
    {
        itemImage.sprite = iconSprite;
        CheckRarity(_rewardModel.rarity);
        float gpc = rewardModel.gpc ?? 0;
        string amountString = "x" + (rewardModel.gpc == null ? rewardModel.quantity.ToString() : gpc.ToString("0.###"));
        amountText.SetText(amountString);
    }

    private void CheckRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Normal:
                itemBG.sprite = rarerityBG[0];
                break;

            case Rarity.Rare:
                itemBG.sprite = rarerityBG[1];
                break;

            case Rarity.SR:
                itemBG.sprite = rarerityBG[2];
                break;
        }
    }
}
