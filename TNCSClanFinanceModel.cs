using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace TheNorthernCastleStates;

public class TNCSClanFinanceModel : DefaultClanFinanceModel
{
    private ClanFinanceModel _previousModel;

    public TNCSClanFinanceModel(ClanFinanceModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false,
        bool applyWithdrawals = false, bool includeDetails = false)
    {
        ExplainedNumber baseNumber = _previousModel.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);
        
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
            if (clan != Clan.PlayerClan)
            {
                int i = 0;
            }
            baseNumber.Add(totalAmount, new TextObject("{=manor_profit_desc}Clan Manors"));
        }

        return baseNumber;
    }
}