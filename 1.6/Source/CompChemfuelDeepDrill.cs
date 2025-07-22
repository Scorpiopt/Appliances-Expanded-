using HarmonyLib;
using RimWorld;

namespace AppliancesExpanded
{
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
}
