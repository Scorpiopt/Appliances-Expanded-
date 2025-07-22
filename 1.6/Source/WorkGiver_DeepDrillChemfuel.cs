using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AppliancesExpanded
{
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
                    if (building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
