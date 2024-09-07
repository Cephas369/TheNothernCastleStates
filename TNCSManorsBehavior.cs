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
    public static IEnumerable<(TNCSManorsBehavior.Manors, int)> GetHeroManors(this Hero hero)
    {
        foreach (var settlement in Settlement.All.Where(settlement => settlement.IsVillage))
        {
            if (TNCSManorsBehavior.Instance.SettlementManors.TryGetValue(settlement.Village,
                    out TNCSManorsBehavior.Manors manors))
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
    [SaveableField(1)]
    public Dictionary<Village, Manors> SettlementManors;
    
    private const int ManorPriceFactor = 5;
    private const int ManorProfitFactor = 3;
    public static TNCSManorsBehavior Instance { private set; get; }
    
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
        if (hero?.IsLord == true && settlement.IsVillage && settlement.Village != null)
        {
            int manorIndex = MostLikelyManorToBuy(hero, settlement);
            if (manorIndex > 0)
            {
                SettlementManors[settlement.Village].ManorOwners[manorIndex] = hero;
            }
        }
    }

    private int MostLikelyManorToBuy(Hero hero, Settlement settlement)
    {
        if (SettlementManors.TryGetValue(settlement.Village, out Manors manors))
        {
            if (manors.GetPrice() > hero.Gold)
            {
                return -1;
            }
            float result = 0f;
            
            result += hero.Gold > manors.GetPrice() ? 0.1f : 0;
            result += -(hero.GetHeroManors().Count() * 0.01f);

            float[] probabilities = new float[3] { result, result, result };
            for (int i = 0; i < 3; i++)
            {
                if (manors.ManorOwners[i] != null)
                    probabilities[i] += hero.GetRelation(manors.ManorOwners[i]) * 0.01f;
                else
                    probabilities[i] += 0.4f;
            }

            return probabilities.IndexOf(probabilities.Max());
        }
        return -1;
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
                return Settlement.CurrentSettlement?.IsVillage == true;
            }, args => GameMenu.SwitchToMenu("manors"));
        
        campaignGameStarter.AddGameMenuOption("manors", "manor_1", "Buy manor 1", ManorOptionCondition, args => 
            ManorClickConsequence(args, 0), false, -1, false, this);
        campaignGameStarter.AddGameMenuOption("manors", "manor_2", "Buy manor 2", ManorOptionCondition, args => 
            ManorClickConsequence(args, 1), false, -1, false, this);
        campaignGameStarter.AddGameMenuOption("manors", "manor_3", "Buy manor 3", ManorOptionCondition, args => 
            ManorClickConsequence(args, 2), false, -1, false, this);
        
        campaignGameStarter.AddGameMenuOption("manors", "leave", "{=3sRdGQou}Leave", args =>
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }, null, true);
    }
    
    private bool ManorOptionCondition(MenuCallbackArgs menuCallbackArgs)
    {
        menuCallbackArgs.optionLeaveType = GameMenuOption.LeaveType.Bribe;
        return true;
    }
    private void ManorClickConsequence(MenuCallbackArgs menuCallbackArgs, int index)
    {
        Manors manors = SettlementManors[Settlement.CurrentSettlement.Village];
        Hero owner = manors.ManorOwners[index];
        int price = manors.GetPrice();
        
        TextObject title = new TextObject("{=buy_manor_title}Buy manor");
        
        TextObject description = new TextObject("{=buy_manor_desc}This manor belongs to {HERO.NAME} and is presently valued at {PRICE}. Are you certain of purchasing it ?");
        description.SetCharacterProperties("HERO", owner.CharacterObject);
        description.SetTextVariable("PRICE", price);
        
        InformationManager.ShowInquiry(new InquiryData(title.ToString(), description.ToString(), true, false,
            GameTexts.FindText("str_proceed").ToString(), GameTexts.FindText("str_cancel").ToString(), () =>
            {
                manors.ManorOwners[index] = Hero.MainHero;
            }, null));
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("settlementManors", ref SettlementManors);
    }

    public class Manors
    {
        [SaveableField(1)]
        public readonly Village Village;
        [SaveableField(2)]
        public Hero[] ManorOwners;
        
        public Manors(Village village)
        {
            Village = village;
            ManorOwners = new Hero[3];
        }

        public int GetPrice()
        {
            return (int)(ManorPriceFactor * Village.Hearth);
        }
        
        public int GetProfit()
        {
            return (int)(ManorProfitFactor * Village.Hearth);
        }
    }
}