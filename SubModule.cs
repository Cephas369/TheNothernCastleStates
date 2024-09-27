using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Helpers;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TheNorthernCastleStates;
using Path = System.IO.Path;


namespace TheNorthernCastleStates
{
    public partial class SubModule : MBSubModuleBase
    {
        private static readonly Harmony Harmony = new("com.Bannerlord.TradeBoundFix");
        private static float tradeBoundDistance;

        protected static string ModuleBin = ModuleHelper.GetModuleFullPath(Assembly.GetExecutingAssembly().GetName().Name);
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            tradeBoundDistance = Convert.ToSingle(File.ReadAllText(Path.Combine(ModuleBin, "TradeBoundDistance.txt")));

            MethodInfo original = AccessTools.Method(AccessTools.TypeByName("VillageTradeBoundCampaignBehavior"), "TryToAssignTradeBoundForVillage");
            MethodInfo method = AccessTools.Method(typeof(SubModule), "Transpiler");
            Harmony.Patch(original, transpiler: new HarmonyMethod(method));
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs(150))
                    instruction.operand = tradeBoundDistance;
                yield return instruction;
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new TNCSMenuBehavior());
                campaignGameStarter.AddBehavior(new TNCSManorsBehavior());
                
                campaignGameStarter.AddModel(new TNCSClanFinanceModel((ClanFinanceModel)campaignGameStarter.Models.Last(model => model.GetType().IsSubclassOf(typeof(ClanFinanceModel)))));
                
                campaignGameStarter.AddBehavior(new BechassardImperialMercenaryClanBehavior());
            }
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            if (initializerObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new WalneriaCampaignBehavior());
            }
        }
    }
}

public class TNCSSaveDefiner : SaveableTypeDefiner
{
    public TNCSSaveDefiner() : base(23_6545_343) { }

    protected override void DefineClassTypes()
    {
        AddClassDefinition(typeof(PayDebtQuest), 1);
        AddClassDefinition(typeof(TNCSMenuBehavior), 2);
        AddClassDefinition(typeof(TNCSManorsBehavior), 3);
        AddClassDefinition(typeof(Manors), 4);
    }

    protected override void DefineContainerDefinitions()
    {
        ConstructContainerDefinition(typeof(Dictionary<Village, Manors>));
        ConstructContainerDefinition(typeof(Dictionary<Hero, CampaignTime>));
    }
}