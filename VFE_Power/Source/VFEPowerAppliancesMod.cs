using PipeSystem;
using RimWorld;
using System.Text;
using UnityEngine;
using VanillaPowerExpanded;
using Verse;

namespace VFEPowerAppliances
{
    public class CompChemfuelPumpInfinite : ThingComp
    {
        public Building_ChemfuelPond chemfuelPond;

        public float ticksInADay = 60000f;

        public int ticksCounter;

        public CompProperties_ChemfuelPump Props => (CompProperties_ChemfuelPump)props;
        private CompResourceStorage resource;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksCounter, "ticksCounter", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
            resource = parent.GetComp<CompResourceStorage>();
        }

        public override void CompTick()
        {
            base.CompTick();
            ticksCounter++;
            if ((float)ticksCounter > ticksInADay * Props.fuelInterval)
            {
                ticksCounter = 0;
                if (resource != null)
                {
                    PipeNet pipeNet = ((CompResource)resource).PipeNet;
                    if (pipeNet.connectors.Count > 1)
                    {
                        bool noCapacity = pipeNet.AvailableCapacity <= 0f;
                        if (!noCapacity)
                        {
                            pipeNet.DistributeAmongStorage(Props.fuelProduced);
                            return;
                        }
                    }
                }
                Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
                thing.stackCount = Props.fuelProduced;
                GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
            if (chemfuelPond != null && chemfuelPond.fuelLeft > 0)
            {
                stringBuilder.Append("VPE_PondHasFuel".Translate(chemfuelPond.fuelLeft));
                stringBuilder.AppendLine();
                int numTicks = (int)(ticksInADay * Props.fuelInterval) - ticksCounter;
                stringBuilder.Append("VPE_PumpProducing".Translate(Props.fuelProduced, numTicks.ToStringTicksToPeriod()));
                return stringBuilder.ToString();
            }
            stringBuilder.Append("VPE_PondNoFuel".Translate());
            return stringBuilder.ToString();
        }
    }
}
