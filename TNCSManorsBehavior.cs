using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TheNorthernCastleStates;

public static class ManorHelpers
{
    public static IEnumerable<(Manors, int)> GetHeroManors(this Hero hero)
    {
        foreach (var settlement in Settlement.All.Where(settlement => settlement.IsVillage))
        {
            if (TNCSManorsBehavior.Instance.SettlementManors.TryGetValue(settlement.Village,
                    out Manors manors))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (manors.ManorOwners[i] == hero)
                    {
                        yield return (manors, i);
                    }
                }
                
            }
        }
    }
}
public class TNCSManorsBehavior : CampaignBehaviorBase  
{
    public Dictionary<Village, Manors> SettlementManors;
    public const int ManorPriceFactor = 20;
    public const int ManorProfitFactor = 4;
    public static readonly string[] AllowedKingdoms = { "avaloria", "walneria", "belecross", "tryior", "erdwa" };
    
    public static TNCSManorsBehavior? Instance { private set; get; }
    
    public TNCSManorsBehavior()
    {
        Instance = this;
    }
    
    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
    }

    private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
    {
        if (hero == Hero.MainHero || hero?.IsLord != true || !settlement.IsVillage ||
            settlement.Village == null || settlement.MapFaction is Kingdom kingdom && !AllowedKingdoms.Contains(kingdom.StringId)) return;

        if (SettlementManors.TryGetValue(settlement.Village, out Manors manors))
        {
            int manorIndex = MostLikelyManorToBuy(hero, manors);
            if (manorIndex > -1 && manors.AvailableForPurchase(manorIndex))
            {
                if (manors.ManorOwners[manorIndex] == Hero.MainHero)
                {
                    TextObject sellManorText = new TextObject("{=sell_manor_desc}{HERO.NAME} wants to buy your manor at {SETTLEMENT} for {PRICE}{GOLD_ICON}.");
                    sellManorText.SetCharacterProperties("HERO", hero.CharacterObject);
                    sellManorText.SetTextVariable("SETTLEMENT", settlement.Name);
                    sellManorText.SetTextVariable("PRICE",
                        SettlementManors[settlement.Village].GetPrice(hero, manorIndex));
                
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=sell_manor_title}Manor sale proposal").ToString(), sellManorText.ToString(), true, true, 
                        new TextObject("{=tncs_accept}Accept").ToString(), new TextObject("{=tncs_decline}Decline").ToString(),
                        () =>
                        {
                            SettlementManors[settlement.Village].BuyManor(hero, manorIndex);
                        }, null), true);
                    
                    return;
                }
                SettlementManors[settlement.Village].BuyManor(hero, manorIndex);
            }
        }
    }

    private int MostLikelyManorToBuy(Hero hero, Manors manors)
    {
        float initialProbability = 0f;
        initialProbability += -(hero.GetHeroManors().Count() * 0.01f);
        initialProbability += MBRandom.RandomFloatRanged(0, 0.70f);

        float[] probabilities = { initialProbability, initialProbability, initialProbability };
        for (int i = 0; i < 3; i++)
        {
            if (manors.GetPrice(hero, i) > hero.Gold)
            {
                probabilities[i] = -1;
                continue;
            }

            if (manors.ManorOwners[i] != null)
            {
                probabilities[i] += hero.GetRelation(manors.ManorOwners[i]) * 0.30f;
            }
            else
                probabilities[i] += 0.4f;
        }

        var possibleOnes = probabilities.Where(p => p >= 1);
        if (possibleOnes.IsEmpty())
            return -1;
        
        return probabilities.IndexOf(possibleOnes.Where(p => p >= 1).Max());
    }

    private void OnAfterSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        if (SettlementManors == null || SettlementManors?.IsEmpty() == true)
        {
            SettlementManors = new();
            foreach (var village in Settlement.All.FindAll(settlement => settlement.IsVillage))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!SettlementManors.ContainsKey(village.Village))
                        SettlementManors.Add(village.Village, new Manors(village.Village));
                    else
                        SettlementManors[village.Village] = new Manors(village.Village);
                }
            }
        }
    }

    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        campaignGameStarter.AddGameMenu("manors", "", args => {});
        campaignGameStarter.AddGameMenuOption("village", "buy_manors", "{=buy_manors}Available manors",
            args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return Settlement.CurrentSettlement?.IsVillage == true && Settlement.CurrentSettlement.MapFaction is Kingdom kingdom && AllowedKingdoms.Contains(kingdom.StringId);
            }, args => GameMenu.SwitchToMenu("manors"));
        
        campaignGameStarter.AddGameMenuOption("manors", "manor_1", "{MANOR_OPTION_TEXT}", args => ManorOptionCondition(args, 0), 
            args => ManorClickConsequence(args, 0), false, -1, false, this);
        campaignGameStarter.AddGameMenuOption("manors", "manor_2", "{MANOR_OPTION_TEXT}", args => ManorOptionCondition(args, 1), 
            args => ManorClickConsequence(args, 1), false, -1, false, this);
        campaignGameStarter.AddGameMenuOption("manors", "manor_3", "{MANOR_OPTION_TEXT}", args => ManorOptionCondition(args, 2), 
            args => ManorClickConsequence(args, 2), false, -1, false, this);
        
        campaignGameStarter.AddGameMenuOption("manors", "leave", "{=3sRdGQou}Leave", args =>
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }, args =>
        {
            GameMenu.SwitchToMenu("village");
        }, true);
    }
    
    private bool ManorOptionCondition(MenuCallbackArgs menuCallbackArgs, int index)
    {
        Manors manors = SettlementManors[Settlement.CurrentSettlement.Village];
        menuCallbackArgs.optionLeaveType = GameMenuOption.LeaveType.Bribe;

        int price = manors.GetPrice(Hero.MainHero, index);
        if (manors.ManorOwners[index] == Hero.MainHero)
        {
            TextObject textObject = new TextObject("Owned Manor {INDEX}");
            textObject.SetTextVariable("INDEX", index + 1);
            GameTexts.SetVariable("MANOR_OPTION_TEXT", textObject);
        }
        else
        {
            TextObject textObject = new TextObject("Manor {INDEX} ({PRICE}{GOLD_ICON})");
            textObject.SetTextVariable("INDEX", index + 1);
            textObject.SetTextVariable("PRICE", price);
            GameTexts.SetVariable("MANOR_OPTION_TEXT", textObject);
        }
        
        if (Hero.MainHero == manors.ManorOwners[index])
        {
            menuCallbackArgs.Tooltip = new TextObject("{=owned_manor_tooltip}You already own this manor.");
            menuCallbackArgs.IsEnabled = false;
            return true;
        }
        
        if (Hero.MainHero.Gold < price)
        {
            TextObject tooltip = new TextObject("{=manor_not_available}This manor will be available on {DATE}.");
            tooltip.SetTextVariable("DATE", manors.NextAvailableSale.ToString());
            menuCallbackArgs.Tooltip = tooltip;
            menuCallbackArgs.IsEnabled = false;
        }

        if (Hero.MainHero.Gold < price)
        {
            menuCallbackArgs.Tooltip = GameTexts.FindText("str_decision_not_enough_gold");
            menuCallbackArgs.IsEnabled = false;
        }
        
        return true;
    }
    private void ManorClickConsequence(MenuCallbackArgs menuCallbackArgs, int index)
    {
        Manors manors = SettlementManors[Settlement.CurrentSettlement.Village];
        Hero owner = manors.ManorOwners[index];
        int price = manors.GetPrice(Hero.MainHero, index);
        
        TextObject title = new TextObject("{=buy_manor_title}Buy manor");
        
        TextObject description = new TextObject("{=buy_manor_desc}This manor has an average income of {PROFIT}{GOLD_ICON} per day and belongs to {NAME}, is presently valued at {PRICE}{GOLD_ICON}. Are you certain of purchasing it ?");
        if (owner != null)
        {
            description.SetTextVariable("NAME", owner.Name);
        }
        else
        {
            description.SetTextVariable("NAME", new TextObject("{=no_one}no one"));
        }
        
        description.SetTextVariable("PROFIT", manors.GetProfit());
        description.SetTextVariable("PRICE", price);
        
        InformationManager.ShowInquiry(new InquiryData(title.ToString(), description.ToString(), true, true,
            GameTexts.FindText("str_proceed").ToString(), GameTexts.FindText("str_cancel").ToString(), () =>
            {
                manors.BuyManor(Hero.MainHero, index);
                GameMenu.SwitchToMenu("manors");
            }, null));
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("settlementManors", ref SettlementManors);
    }
}

public class Manors
{
    [SaveableField(1)]
    public readonly Village Village;
    [SaveableField(2)]
    public Hero[] ManorOwners;
    [SaveableField(3)]
    public CampaignTime[] NextAvailableSale;
    
    public static readonly int[] SellManorTimeRange = { 12, 40 };
        
    public Manors(Village village)
    {
        Village = village;
        ManorOwners = new Hero[3];
        NextAvailableSale = new CampaignTime[3];
    }

    public int GetPrice(Hero buyer, int index)
    {
        int relationshipAddition = 1;
        int relationWithOwner = ManorOwners[index] != null ? buyer.GetRelation(ManorOwners[index]) : 1;
        relationshipAddition = -(relationWithOwner) * 15;
        int finalPrice = (int)(TNCSManorsBehavior.ManorPriceFactor * Village.Hearth) + relationshipAddition;
        return finalPrice > 100 ? finalPrice : 100;
    }
        
    public int GetProfit()
    {
        return (int)(TNCSManorsBehavior.ManorProfitFactor * Village.Hearth);
    }
    
    public bool AvailableForPurchase(int index) => CampaignTime.Now >= NextAvailableSale[index];
    
    public void BuyManor(Hero hero, int index)
    {
        ManorOwners[index] = hero;
        GiveGoldAction.ApplyBetweenCharacters(hero, null, -GetPrice(hero, index));

        CampaignTime nextSale = CampaignTime.DaysFromNow(MBRandom.RandomInt(SellManorTimeRange[0], SellManorTimeRange[1]));
        NextAvailableSale[index] = nextSale;
    }
}