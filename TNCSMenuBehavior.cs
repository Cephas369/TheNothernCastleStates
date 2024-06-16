using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TheNorthernCastleStates;

internal class TNCSMenuBehavior : CampaignBehaviorBase
{
    private float _scamProgressHours;
    private float _fraudProgressHours;
    private const float ScamTargetHours = 6;
    private const float FraudTargetHours = 72;
    private const int FraudReward = 6500;

    private Dictionary<Settlement, (int amount, CharacterObject mercenary)>? _mercenaryData;

    private Dictionary<Settlement, (int amount, CharacterObject mercenary)> MercenaryData
    {
        get { return _mercenaryData ??= new Dictionary<Settlement, (int amount, CharacterObject mercenary)>(); }
    }

    private bool buy_mercenaries_condition(MenuCallbackArgs args)
    {
        if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown)
            return false;
        if (!MercenaryData.ContainsKey(Settlement.CurrentSettlement))
            InitializeSettlementMercenaries(Settlement.CurrentSettlement);

        var tuple = MercenaryData[Settlement.CurrentSettlement];
        if (tuple.amount == 0)
            return false;
        int troopRecruitmentCost =
            Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(tuple.mercenary, Hero.MainHero);

        if (Hero.MainHero.Gold >= troopRecruitmentCost)
        {
            int content = MathF.Min(tuple.amount, Hero.MainHero.Gold / troopRecruitmentCost);
            MBTextManager.SetTextVariable("MEN_COUNT", content);
            MBTextManager.SetTextVariable("MERCENARY_NAME", tuple.mercenary.Name, false);
            MBTextManager.SetTextVariable("TOTAL_AMOUNT", content * troopRecruitmentCost);
        }
        else
        {
            args.Tooltip = GameTexts.FindText("str_decision_not_enough_gold");
            args.IsEnabled = false;
            MBTextManager.SetTextVariable("MEN_COUNT", tuple.amount);
            MBTextManager.SetTextVariable("MERCENARY_NAME", tuple.mercenary.Name, false);
            MBTextManager.SetTextVariable("TOTAL_AMOUNT", tuple.amount * troopRecruitmentCost);
        }

        args.optionLeaveType = GameMenuOption.LeaveType.Bribe;
        return true;
    }

    private void buy_mercenaries_on_consequence(MenuCallbackArgs args)
    {
        if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown)
            return;
        var tuple = MercenaryData[Settlement.CurrentSettlement];
        int troopRecruitmentCost =
            Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(tuple.mercenary, Hero.MainHero);
        if (Hero.MainHero.Gold < troopRecruitmentCost)
            return;
        int count = MathF.Min(tuple.amount, Hero.MainHero.Gold / troopRecruitmentCost);
        MobileParty.MainParty.MemberRoster.AddToCounts(tuple.mercenary, count);
        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(count * troopRecruitmentCost));
        MercenaryData[Settlement.CurrentSettlement] = (0, null);
        GameMenu.SwitchToMenu("town_backstreet");
    }

    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
    }

    private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
    {
        if (hero == Hero.MainHero && settlement?.IsTown == true)
        {
            InitializeSettlementMercenaries(settlement);
        }
    }

    private CharacterObject GetSettlementMercenaryTroop()
    {
        return Settlement.CurrentSettlement?.OwnerClan?.Culture?.BasicMercenaryTroops != null
            ? Settlement.CurrentSettlement.OwnerClan.Culture.BasicMercenaryTroops.GetRandomElement()
            : CharacterObject.All.GetRandomElementWithPredicate(x =>
                !x.IsHero && x is { IsTemplate: false, IsObsolete: false, Occupation: Occupation.Mercenary });
    }

    private void InitializeSettlementMercenaries(Settlement settlement)
    {
        (int, CharacterObject) data = (MBRandom.RandomInt(0, 10), GetSettlementMercenaryTroop());
        if (!MercenaryData.ContainsKey(settlement))
        {
            MercenaryData.Add(settlement, data);
            return;
        }

        MercenaryData[settlement] = data;
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("_scamProgressHours", ref _scamProgressHours);
        dataStore.SyncData("_fraudProgressHours", ref _fraudProgressHours);
    }

    private void scam_menu_on_tick(MenuCallbackArgs args, CampaignTime dt)
    {
        _scamProgressHours += (float)dt.ToHours;
        args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_scamProgressHours / ScamTargetHours);

        if (_scamProgressHours >= ScamTargetHours)
        {
            GiveGoldAction.ApplyForSettlementToCharacter(Settlement.CurrentSettlement, Hero.MainHero, 100);
            ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.OwnerClan, 10);
        }
    }

    private void fraud_menu_on_tick(MenuCallbackArgs args, CampaignTime dt)
    {
        _fraudProgressHours += (float)dt.ToHours;
        args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_fraudProgressHours / FraudTargetHours);

        if (_fraudProgressHours >= FraudTargetHours)
        {
            GiveGoldAction.ApplyForSettlementToCharacter(Settlement.CurrentSettlement, Hero.MainHero, FraudReward);
            ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.OwnerClan, 35);
        }
    }

    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        campaignGameStarter.AddGameMenuOption("town_backstreet", "scam_townsfolk", "{=scam_townsfolk}Scam townsfolk",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                return true;
            }, args => { GameMenu.SwitchToMenu("scamming_townsfolk"); }, false, 3);
        campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_mercenaries",
            "{=NwO0CVzn}Recruit {MEN_COUNT} {MERCENARY_NAME} ({TOTAL_AMOUNT}{GOLD_ICON})", buy_mercenaries_condition,
            buy_mercenaries_on_consequence, false, 2);

        campaignGameStarter.AddWaitGameMenu("scamming_townsfolk", "{=scamming_townsfolk}Scamming...",
            args => _scamProgressHours = 0, null, null, scam_menu_on_tick,
            GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.Encounter,
            targetWaitHours: ScamTargetHours);

        campaignGameStarter.AddGameMenuOption("scamming_townsfolk", "leave_scam", "Leave",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, args => GameMenu.ExitToLast(), true, 2);


        campaignGameStarter.AddGameMenuOption("town_backstreet", "commit_fraud", "{=commit_fraud}Commit fraud ({TOTAL_AMOUNT}{GOLD_ICON})",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                MBTextManager.SetTextVariable("TOTAL_AMOUNT", FraudReward);
                return true;
            }, args => { GameMenu.SwitchToMenu("committing_fraud"); }, false, 3);

        campaignGameStarter.AddWaitGameMenu("committing_fraud", "{=committing_fraud}Applying fraud...",
            args => _fraudProgressHours = 0, null, null, fraud_menu_on_tick,
            GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.Encounter,
            targetWaitHours: FraudTargetHours);

        campaignGameStarter.AddGameMenuOption("committing_fraud", "leave_fraud", "Leave",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, args => GameMenu.ExitToLast(), true, 2);
        
        campaignGameStarter.AddGameMenuOption("town_backstreet", "borrow_money", "{=borrow_money}Borrow money ({TOTAL_AMOUNT}{GOLD_ICON})",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                int moneyToBorrow = (int)(Settlement.CurrentSettlement.Notables
                    .Where(hero => hero.IsGangLeader)
                    .Sum(hero => hero.Gold) * 0.8f);
                if (Settlement.CurrentSettlement?.Notables.Any(hero => hero.IsGangLeader) == false)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("{=no_gang_leaders}There's no gang leaders in this town.");
                    moneyToBorrow = 0;
                }
                else if (Settlement.CurrentSettlement?.MapFaction != Hero.MainHero.MapFaction)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("{=not_same_kingdom}You're not from this kingdom!");
                } 
                else if (Clan.PlayerClan.Tier < 1)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("{=low_clan_tier}Your clan tier needs to be at least 1.");
                }

                MBTextManager.SetTextVariable("TOTAL_AMOUNT", moneyToBorrow);
                return !Campaign.Current.QuestManager.Quests.Any(quest => quest is PayDebtQuest payDebtQuest && payDebtQuest._relatedSettlement == Settlement.CurrentSettlement);
            }, args =>
            {
                int moneyToBorrow = (int)(Settlement.CurrentSettlement.Notables
                    .Where(hero => hero.IsGangLeader)
                    .Sum(hero => hero.Gold) * 0.8f);
                Hero hero = Settlement.CurrentSettlement.Notables.GetRandomElementWithPredicate(hero => hero.IsGangLeader);
                new PayDebtQuest("pay_debt", hero, CampaignTime.DaysFromNow(30), 0, moneyToBorrow, Settlement.CurrentSettlement).StartQuest();
                GameMenu.ExitToLast();
            }, false, 3);
    }
}