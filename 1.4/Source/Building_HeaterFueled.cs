using RimWorld;
using UnityEngine;
using Verse;

namespace AppliancesExpanded
{
    public class Building_CoolerFueled : Building_TempControl
    {
        private const float HeatOutputMultiplier = 1.25f;

        private const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

        public CompRefuelable compRefuelable;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = this.GetComp<CompRefuelable>();
        }
        public override void TickRare()
        {
            if (!compRefuelable.HasFuel)
            {
                return;
            }
            IntVec3 intVec = base.Position + IntVec3.South.RotatedBy(base.Rotation);
            IntVec3 intVec2 = base.Position + IntVec3.North.RotatedBy(base.Rotation);
            bool flag = false;
            if (!intVec2.Impassable(base.Map) && !intVec.Impassable(base.Map))
            {
                float temperature = intVec2.GetTemperature(base.Map);
                float temperature2 = intVec.GetTemperature(base.Map);
                float num = temperature - temperature2;
                if (temperature - 40f > num)
                {
                    num = temperature - 40f;
                }
                float num2 = 1f - num * 0.0076923077f;
                if (num2 < 0f)
                {
                    num2 = 0f;
                }
                float num3 = compTempControl.Props.energyPerSecond * num2 * 4.16666651f;
                float num4 = GenTemperature.ControlTemperatureTempChange(intVec, base.Map, num3, compTempControl.targetTemperature);
                flag = !Mathf.Approximately(num4, 0f);
                if (flag)
                {
                    intVec.GetRoom(base.Map).Temperature += num4;
                    GenTemperature.PushHeat(intVec2, base.Map, (0f - num3) * 1.25f);
                }
            }
            if (flag)
            {
                compRefuelable.ConsumeFuel(compRefuelable.ConsumptionRatePerTick);
            }
            else
            {
                compRefuelable.ConsumeFuel(compRefuelable.ConsumptionRatePerTick * compTempControl.Props.lowPowerConsumptionFactor);
            }
            compTempControl.operatingAtHighPower = flag;
        }
    }
    public class Building_HeaterFueled : Building_TempControl
    {
        private const float EfficiencyFalloffSpan = 100f;

        public CompRefuelable compRefuelable;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = this.GetComp<CompRefuelable>();
        }
        public override void TickRare()
        {
            if (compRefuelable.HasFuel)
            {
                float ambientTemperature = base.AmbientTemperature;
                float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 120f)) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f));
                float energyLimit = compTempControl.Props.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, compTempControl.targetTemperature);
                bool flag = !Mathf.Approximately(num2, 0f);
                if (flag)
                {
                    this.GetRoom().Temperature += num2;
                    compRefuelable.ConsumeFuel(compRefuelable.ConsumptionRatePerTick);
                }
                else
                {
                    compRefuelable.ConsumeFuel(compRefuelable.ConsumptionRatePerTick * compTempControl.Props.lowPowerConsumptionFactor);
                }
                compTempControl.operatingAtHighPower = flag;
            }
        }
    }
}
