using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace TheNorthernCastleStates;

public class BechassardImperialMercenaryClanBehavior: CampaignBehaviorBase
{
    private bool modifiedClan = false;

    public override void RegisterEvents()
    {
        CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, (Action<CampaignGameStarter>) (starter =>
        {
            TNCSHelper.ModifyWeapons();
            if (modifiedClan)
                return;
            foreach (Clan clan in Campaign.Current.Clans)
            {
                if (clan.Leader != null && clan.Leader.StringId == "imperial_merc_ruthas")
                {
                    clan.Leader.Gold = 200000;
                    modifiedClan = true;
                    break;
                }
            }
        }));
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("ModifiedClan", ref modifiedClan);
    }
}