using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
    [SaveableField(0)]
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
        CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
    }

    private void OnDailyTick()
    {
        foreach (var hero in Hero.AllAliveHeroes.FindAll(hero => hero.IsLord))
        {
            List<(Manors manors, int index)> foundManors = hero.GetHeroManors().ToList();
            foreach (var manorTuple in foundManors)
            {
                GiveGoldAction.ApplyBetweenCharacters(null, hero, manorTuple.manors.GetProfit());
            }
        }
    }

    private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
    {
        if (settlement.IsVillage && settlement.IsVillage && settlement.Village != null &&
            hero.IsLord)
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
        if (SettlementManors.IsEmpty())
        {
            foreach (var village in Settlement.All.FindAll(settlement => settlement.IsVillage))
            {
                for (int i = 0; i < 3; i++)
                {
                    SettlementManors.Add(village.Village, new Manors(village.Village));
                }
            }
        }
    }

    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        campaignGameStarter.AddGameMenu("manors", "", args => {});
        campaignGameStarter.AddGameMenuOption("town", "buy_manors", "{=buy_manors}manors", args =>
        {
            return Settlement.CurrentSettlement?.IsVillage == true;
        }, args => GameMenu.SwitchToMenu("manors"));
        
        campaignGameStarter.AddGameMenuOption("town", "manor_1", "Manor 1", args =>
        {
            return Settlement.CurrentSettlement?.IsVillage == true;
        }, args => GameMenu.SwitchToMenu("manors"));
        campaignGameStarter.AddGameMenuOption("town", "manor_2", "Manor 2", args =>
        {
            return Settlement.CurrentSettlement?.IsVillage == true;
        }, args => GameMenu.SwitchToMenu("manors"));
        
        campaignGameStarter.AddGameMenuOption("town", "manor_3", "Manor 3", args =>
        {
            return Settlement.CurrentSettlement?.IsVillage == true;
        }, args => GameMenu.SwitchToMenu("manors"));
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("settlementManors", ref SettlementManors);
    }

    public class Manors
    {
        public readonly Village Village;
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