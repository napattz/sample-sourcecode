using SIDGIN.Patcher.SceneManagment;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GachaUIController : MonoBehaviour
{
    public static void LoadScene(Roulettes result,bool isPVP)
    {
        AllModelSingleton.instance.selectedRoulette = result;
        SGSceneManager.LoadScene(SceneNames.luckyDrawSpin);
        gachaPvp = isPVP;
    }

    [Header("Main UI")]
    [Space(10)]
    [SerializeField] private RectTransform container;
    [SerializeField] private Button spinBtn, spinx10Btn, skipBtn, backBtn;
    [SerializeField] private TokenGroupPanel tokenPanel;
    [SerializeField] private Transform tokenTransform;
    [SerializeField] private TextMeshProUGUI ticketText, timerText, slotEventNameText, preTitleNameText, titleNameText;
    [SerializeField] private GachaItemPanel rewardItemPanel;
    [SerializeField] private BuyTokenPopUpPanel buyTokenPopUpPanel;
    [SerializeField] private UI_InfiniteScroll infiniteScrollUI;

    [Header("Reward Popup")]
    [Space(10)]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Image rewardItemImage, rewardBGImage;
    [SerializeField] private TextMeshProUGUI rewardItemNameText;
    [SerializeField] private Button claimBtn;

    [Header("Particle Reward")]
    [Space(10)]
    [SerializeField] private Material auraMaterial, fireworkMaterial, fireworkMaterial1;
    [SerializeField] private GameObject fireworkGroup;

    [SerializeField] private GachaController gachaController;
    public static bool gachaPvp;
    private List<GachaItemPanel> _allReward = new List<GachaItemPanel>();
    private List<Reward> _shuffleReward = new List<Reward>();
    private GachaUIAnimation _gachaAnimation;
    private Roulettes _rouletteModel;
    private GachaChangeUI _gachaChangeUI;
    private int _ticketAmount;
    private float _currentCountdownTime;
    private bool _onRewardPanel;
    private bool _moreReward;

    private void Awake()
    {
        _rouletteModel = AllModelSingleton.instance.selectedRoulette;
        InitReward(_rouletteModel);
        AllModelSingleton.instance.FetchPlayerInventoryModel(() => CheckTicketAmount());
        infiniteScrollUI.Init();
        _onRewardPanel = false;
        _moreReward = false;
        GetTicketAmount();
    }

    private void Start()
    {
        _gachaAnimation = GetComponent<GachaUIAnimation>();
        _gachaChangeUI = GetComponent<GachaChangeUI>();
        skipBtn.onClick.AddListener(() => { OnClickSkipBtn(); AudioManager.instance.LoadResPlay(AudioNames.menuButton); });
        claimBtn.onClick.AddListener(() => { OnClickClaimBtn(); AudioManager.instance.LoadResPlay(AudioNames.menuButton); });
        backBtn.onClick.AddListener(() => { SGSceneManager.LoadScene(SceneNames.luckyDrawMenu); AudioManager.instance.LoadResPlay(AudioNames.clickBack); });
        CheckSpin();
        _currentCountdownTime = 5;
        _gachaChangeUI.SetGachaImage(gachaPvp);
    }
    private void Update()
    {
        if(_moreReward && _onRewardPanel)
        {
            if (_currentCountdownTime >= 0)
            {
                _currentCountdownTime -= Time.deltaTime;
                timerText.text = "Spin Continue in " + _currentCountdownTime.ToString("0") + " sec.";
            }
            else
            {
                OnClickClaimBtn();
            }
        }
    }

    public void ShowReward()
    {
        SetRewardPanel();
        _gachaAnimation.ShowRewardAnimate();
        gachaController.ResetSpin();
        _onRewardPanel = true;
        PlayerDataController.instance.FetchAllData();
    }
    private void CheckTicket(int spinAmount)
    {
        int valueSpin = spinAmount * _rouletteModel.ticketQuantity;
        if (GetTicketAmount() >= valueSpin)
        {
            ApiManager.instance.RouletteSpin(_rouletteModel.id, spinAmount, (response) =>
            {
                for (int i = 0; i < response.rewards.Length; i++)
                {
                    AllModelSingleton.instance.rouletteReward.Add(response.rewards[i]);
                }
                AllModelSingleton.instance.FetchPlayerInventoryModel();
                CheckSpin();
            }, (errMsg) => { });
        }
        else
        {
            if (!gachaPvp) buyTokenPopUpPanel.SetBuyItemData(_rouletteModel.ticketItemId, GetTicketAmount());
        }
    }
    private void CheckSpin()
    {
        if (AllModelSingleton.instance.rouletteReward.Count > 0)            
        {
            List<GachaItemPanel> rewards = _allReward.Where(ID => ID.rewardID == AllModelSingleton.instance.rouletteReward[0].id).ToList();
            if (rewards.Count > 0)
            {
                GachaItemPanel reward = rewards.First();
                gachaController.selectedObject = reward;
                gachaController.StartSpin();
                _gachaAnimation.ShowHideSpinBtn(false);
                tokenTransform.gameObject.SetActive(false);
                backBtn.gameObject.SetActive(false);
                AllModelSingleton.instance.rouletteReward.RemoveAt(0);
            }
        }
        else
        {
            spinBtn.onClick.AddListener(() => { CheckTicket(1); AudioManager.instance.LoadResPlay(AudioNames.menuButton); });
            spinx10Btn.onClick.AddListener(() => { CheckTicket(10); AudioManager.instance.LoadResPlay(AudioNames.menuButton); });
            backBtn.gameObject.SetActive(true);
            tokenTransform.gameObject.SetActive(true);
        }
        if (AllModelSingleton.instance.rouletteReward.Count >= 1)
        {
            _moreReward = true;
        }
    }
    private void OnClickSkipBtn()
    {
        ShowReward();
        gachaController.isSkip = true;
    }

    private void OnClickClaimBtn()
    {
        SGSceneManager.LoadScene(SceneNames.luckyDrawSpin);
    }

    private void SetRewardPanel()
    {
        fireworkGroup.SetActive(false);
        GachaItemPanel reward = gachaController.selectedObject;

        switch (reward.rewardModel.rarity)
        {
            case Rarity.Normal:
                rewardBGImage.color = new Color32(0xA4, 0xA4, 0xA4, 100);
                auraMaterial.color = Color.white;
                break;
            case Rarity.Rare:
                rewardBGImage.color = new Color32(0x7A, 0xC7, 0xEE, 100);
                auraMaterial.color = Color.cyan;
                fireworkGroup.SetActive(true);
                fireworkMaterial.color = Color.cyan;
                fireworkMaterial1.color = Color.cyan;
                break;
            case Rarity.SR:
                rewardBGImage.color = new Color32(0xEE, 0xDB, 0x7A, 100);
                auraMaterial.color = Color.yellow;
                fireworkGroup.SetActive(true);
                fireworkMaterial.color = Color.yellow;
                fireworkMaterial1.color = Color.yellow;
                break;
            default:
                break;
        }

        rewardItemImage.sprite = reward.itemImage.sprite;
        float gpc = reward.rewardModel.gpc ?? 0;
        string amountString = reward.rewardModel.gpc == null ? ("x" + reward.rewardModel.quantity.ToString()) : "";
        string itemNameString = reward.rewardModel.gpc == null ? reward.rewardModel.item.name : "Moo x" + gpc.ToString("0.###");
        rewardItemNameText.text = $"{itemNameString} {amountString}";
    }

    public void CheckTicketAmount()
    {
        ticketText.text = $"{ GetTicketAmount()}";
    }

    private int GetTicketAmount()
    {
        List<Item> items = AllModelSingleton.instance.playerItems.Where(item => item.item.name == _rouletteModel.ticketItem.name).ToList();
        if (items.Count > 0)
        {
            Item item = items.First();
            _ticketAmount = item.quantity ?? 1;
        }
        else { _ticketAmount = 0; }
        return _ticketAmount;
    }
    private void InitReward(Roulettes model)
    {
        preTitleNameText.text = model.preTitle;
        titleNameText.text = model.title;
        _shuffleReward = model.rewards.ToList();
        Shuffle(_shuffleReward);
        for (int i = 0; i < _shuffleReward.Count; i++)
        {
            GachaItemPanel newItemView = Instantiate(rewardItemPanel, container);
            newItemView.rewardModel = _shuffleReward[i];
            newItemView.rewardID = _shuffleReward[i].id;
            _allReward.Add(newItemView);
        }
        CheckLayoutContainer();

        TokenGroupPanel newTokenGroup = Instantiate(tokenPanel, tokenTransform);
        RectTransform tokenGroupRect = newTokenGroup.GetComponent<RectTransform>();
        tokenGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
        tokenGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
        newTokenGroup.tokenModel = model.ticketItem.ToItem();
        newTokenGroup.tokenID = model.ticketItemId;
        newTokenGroup.Init();
        ticketText = newTokenGroup.amountText;
    }

    private void CheckLayoutContainer()
    {
        if(_allReward.Count % 2 == 0)
        {
            container.GetComponent<HorizontalLayoutGroup>().padding.right = 160;
        }
        else
        {
            container.GetComponent<HorizontalLayoutGroup>().padding.right = 0;
        }
    }
    private void Shuffle<T>(List<T> inputList)
    {
        for (int i = 0; i < inputList.Count - 1; i++)
        {
            T temp = inputList[i];
            int rand = Random.Range(i, inputList.Count);
            inputList[i] = inputList[rand];
            inputList[rand] = temp;
        }
    }

}
