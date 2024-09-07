using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace TheNorthernCastleStates;

public partial class SubModule
{
    private class WalneriaCampaignBehavior : CampaignBehaviorBase
    {
        private bool firstTick;

        private List<string> heroes = new() { "walneria_lord_1", "walneria_lord_2", "walneria_lord_4", "walneria_lord_5", "walneria_lord_6" };
        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        public WalneriaCampaignBehavior()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, (cgs) =>
            {
                RegisterEvents();
            });
        }
        private void InitializeClanParties()
        {
            InitializeMeylaParty();
            InitializePetrysParty();
            InitializeBroydenParty();
            InitializeDarkishanAndReydlonParty();
        }
        private void OnTick(float dt)
        {
            InitializeClanParties();
            if (!firstTick)
            {
                Clan.FindFirst(x => x.StringId == "walneria_clan_1").Leader.ChangeHeroGold(2000000);
                Clan.FindFirst(x => x.StringId == "walneria_clan_2").Leader.ChangeHeroGold(2000000);

                //Garrison initial troops

                Settlement settlement = Settlement.Find("castle_WA");

                PartyBase garrisonParty = settlement.Parties.Find(x => x.StringId.Contains("garrison_party_")).Party;

                garrisonParty.MemberRoster.RemoveIf(x => x.Character?.IsHero == false);

                garrisonParty.MemberRoster.AddToCounts(CharacterObject.Find("imperial_palatine_guard"), 60);
                garrisonParty.MemberRoster.AddToCounts(CharacterObject.Find("imperial_legionary"), 40);
                garrisonParty.MemberRoster.AddToCounts(CharacterObject.Find("imperial_elite_cataphract"), 20);
                garrisonParty.MemberRoster.AddToCounts(CharacterObject.Find("sturgian_ulfhednar"), 30);
                    
                //Village to have 4 notables

                Settlement villageWith4Notables = Settlement.Find("castle_village_ER_2");
                for (int index = villageWith4Notables.Notables.Count-1; index <= 3; ++index)
                    HeroCreator.CreateHeroAtOccupation(Occupation.RuralNotable, villageWith4Notables);

                firstTick = true;
            }
        }
        private void InitializeMeylaParty()
        {
            if (heroes.Contains("walneria_lord_1") && Hero.FindFirst(x => x.StringId == "walneria_lord_1").PartyBelongedTo != null)
            {
                MobileParty meylaParty = Hero.FindFirst(x => x.StringId == "walneria_lord_1").PartyBelongedTo;
                meylaParty.Party.MemberRoster.RemoveIf(x => x.Character != meylaParty.LeaderHero.CharacterObject);

                meylaParty.AddElementToMemberRoster(CharacterObject.Find("imperial_palatine_guard"), 40);
                meylaParty.AddElementToMemberRoster(CharacterObject.Find("imperial_legionary"), 35);
                meylaParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_menavliaton"), 30);
                meylaParty.AddElementToMemberRoster(CharacterObject.Find("imperial_cataphract"), 25);
                meylaParty.AddElementToMemberRoster(CharacterObject.Find("sturgian_berzerker"), 10);
                meylaParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_cataphract"), 8);
                heroes.Remove("walneria_lord_1");
            }

        }
        private void InitializePetrysParty()
        {
            if (heroes.Contains("walneria_lord_2") && Hero.FindFirst(x => x.StringId == "walneria_lord_2").PartyBelongedTo != null)
            {
                MobileParty petrysParty = Hero.FindFirst(x => x.StringId == "walneria_lord_2").PartyBelongedTo;
                petrysParty.MemberRoster.RemoveIf(x => x.Character != petrysParty.LeaderHero.CharacterObject);

                petrysParty.AddElementToMemberRoster(CharacterObject.Find("imperial_palatine_guard"), 25);
                petrysParty.AddElementToMemberRoster(CharacterObject.Find("imperial_legionary"), 10);
                petrysParty.AddElementToMemberRoster(CharacterObject.Find("imperial_menavliaton"), 10);
                petrysParty.AddElementToMemberRoster(CharacterObject.Find("sturgian_berzerker"), 10);
                petrysParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_cataphract"), 8);
                heroes.Remove("walneria_lord_2");
            }

        }
        private void InitializeBroydenParty()
        {
            if (heroes.Contains("walneria_lord_4") && Hero.FindFirst(x => x.StringId == "walneria_lord_4").PartyBelongedTo != null)
            {
                MobileParty broydenParty = Hero.FindFirst(x => x.StringId == "walneria_lord_4").PartyBelongedTo;
                broydenParty.Party.MemberRoster.RemoveIf(x => x.Character != broydenParty.LeaderHero.CharacterObject);

                broydenParty.AddElementToMemberRoster(CharacterObject.Find("imperial_palatine_guard"), 35);
                broydenParty.AddElementToMemberRoster(CharacterObject.Find("imperial_legionary"), 35);
                broydenParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_menavliaton"), 25);
                broydenParty.AddElementToMemberRoster(CharacterObject.Find("sturgian_berzerker"), 12);
                broydenParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_cataphract"), 20);
                heroes.Remove("walneria_lord_4");
            }
        }
        private void InitializeDarkishanAndReydlonParty()
        {
            if (heroes.Contains("walneria_lord_5") && Hero.FindFirst(x => x.StringId == "walneria_lord_5").PartyBelongedTo != null)
            {
                MobileParty reydlonParty = Hero.FindFirst(x => x.StringId == "walneria_lord_5").PartyBelongedTo;
                reydlonParty.Party.MemberRoster.RemoveIf(x => x.Character != reydlonParty.LeaderHero.CharacterObject);
                    
                reydlonParty.AddElementToMemberRoster(CharacterObject.Find("imperial_palatine_guard"), 25);
                reydlonParty.AddElementToMemberRoster(CharacterObject.Find("imperial_legionary"), 10);
                reydlonParty.AddElementToMemberRoster(CharacterObject.Find("imperial_menavliaton"), 10);
                reydlonParty.AddElementToMemberRoster(CharacterObject.Find("sturgian_berzerker"), 10);
                reydlonParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_cataphract"), 8);
                heroes.Remove("walneria_lord_5");
            }
            if (heroes.Contains("walneria_lord_6") && Hero.FindFirst(x => x.StringId == "walneria_lord_6").PartyBelongedTo != null)
            {
                MobileParty darkishanParty = Hero.FindFirst(x => x.StringId == "walneria_lord_6").PartyBelongedTo;
                    
                darkishanParty.Party.MemberRoster.RemoveIf(x => x.Character != darkishanParty.LeaderHero.CharacterObject);
                    
                darkishanParty.AddElementToMemberRoster(CharacterObject.Find("imperial_palatine_guard"), 25);
                darkishanParty.AddElementToMemberRoster(CharacterObject.Find("imperial_legionary"), 10);
                darkishanParty.AddElementToMemberRoster(CharacterObject.Find("imperial_menavliaton"), 10);
                darkishanParty.AddElementToMemberRoster(CharacterObject.Find("sturgian_berzerker"), 10);
                darkishanParty.AddElementToMemberRoster(CharacterObject.Find("imperial_elite_cataphract"), 8);
                heroes.Remove("walneria_lord_6");

            }
        }

        public override void SyncData(IDataStore dataStore)
        {
                
        }
    }
}