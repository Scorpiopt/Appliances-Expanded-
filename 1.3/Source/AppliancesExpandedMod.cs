using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AppliancesExpanded
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("AppliancesExpanded.Mod").PatchAll();
        }
    }

    public class CompChemfuelDeepDrill : CompDeepDrill
    {
        public CompRefuelable compRefuelable;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = this.parent.GetComp<CompRefuelable>();
        }

        [HarmonyPatch(typeof(CompDeepDrill), "DrillWorkDone")]
        public static class CompDeepDrill_DrillWorkDone_Patch
        {
            public static void Postfix(CompDeepDrill __instance)
            {
                if (__instance is CompChemfuelDeepDrill comp)
                {
                    comp.compRefuelable.Notify_UsedThisTick();
                }
            }
        }

        [HarmonyPatch(typeof(CompDeepDrill), "CanDrillNow")]
        public static class CompDeepDrill_CanDrillNow_Patch
        {
            public static void Postfix(CompDeepDrill __instance, ref bool __result)
            {
                if (__instance is CompChemfuelDeepDrill comp)
                {
                    __result = comp.compRefuelable.HasFuel;
                }
            }
        }
    }
    [DefOf]
    public static class AE_DefOf
    {
        public static ThingDef SCP_ChemDrill;
    }
    public class WorkGiver_DeepDrillChemfuel : WorkGiver_DeepDrill
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(AE_DefOf.SCP_ChemDrill);
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Building building = allBuildingsColonist[i];
                if (building.def == AE_DefOf.SCP_ChemDrill)
                {
                    CompPowerTrader comp = building.GetComp<CompPowerTrader>();
                    if ((comp == null || comp.PowerOn) && building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
