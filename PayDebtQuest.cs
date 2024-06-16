using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TheNorthernCastleStates;

public class PayDebtQuest : QuestBase
{
    [SaveableField(0)] private int _debtToPay;
    [SaveableField(1)] public Settlement _relatedSettlement;
    public PayDebtQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, int debtToPay, Settlement settlement) : base(questId, questGiver, duration, rewardGold)
    {
        _debtToPay = (int)(debtToPay + (debtToPay * 0.2f));
        _relatedSettlement = settlement;
        InitializeQuestOnGameLoad();
    }

    protected override void SetDialogs()
    {

    }

    protected sealed override void InitializeQuestOnGameLoad()
    {
        AddMenu();
    }

    protected override void HourlyTick()
    {
        
    }

    private void AddMenu()
    {
        if (SandBoxManager.Instance.GameStarter is CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town_backstreet", "pay_debt", "{=pay_debt}Pay debt ({TOTAL_AMOUNT}{GOLD_ICON})",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                    if (Hero.MainHero.Gold < _debtToPay)
                    {
                        args.IsEnabled = false;
                        args.Tooltip = new TextObject("{=not_enough_money}You don't have enough money.");
                    }
                    MBTextManager.SetTextVariable("TOTAL_AMOUNT", _debtToPay);
                    return true;
                }, args =>
                {
                    GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, _relatedSettlement, _debtToPay);
                    foreach (var notable in _relatedSettlement.Notables)
                    {
                        if (notable.IsGangLeader)
                        {
                            ChangeRelationAction.ApplyPlayerRelation(notable, 1);
                        }
                    }
                    GameMenu.ExitToLast();
                    CompleteQuestWithSuccess();
                }, false, 3, false, this);
        }
    }

    protected override void OnTimedOut()
    {
        base.OnTimedOut();
        foreach (var notable in _relatedSettlement.Notables)
        {
            if (notable.IsGangLeader)
            {
                ChangeRelationAction.ApplyPlayerRelation(notable, -15);
            }
        }
        CompleteQuestWithFail();
    }

    public override TextObject Title
    {
        get
        {
            var title = new TextObject("{=pay_debt_to}Pay your debt to {SETTLEMENT} gang leaders.");
            title.SetTextVariable("SETTLEMENT", _relatedSettlement.Name);
            return title;
        }
    }

    public override bool IsRemainingTimeHidden => false;
    public override bool IsSpecialQuest => true;
}