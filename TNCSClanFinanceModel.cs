using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace TheNorthernCastleStates;

public class TNCSClanFinanceModel : DefaultClanFinanceModel
{
    public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false,
        bool includeDetails = false)
    {
        ExplainedNumber baseValue = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);
        
        int totalAmount = 0;
        foreach (var hero in clan.Heroes)
        {
            var foundManors = hero.GetHeroManors();
            foreach (var manorTuple in foundManors)
            {
                totalAmount += manorTuple.Item1.GetProfit();
            }
        }

        if (totalAmount > 0)
        {
            baseValue.Add(totalAmount, new TextObject("{=manor_profit_desc}Clan Manors"));
        }

        return baseValue;
    }
}