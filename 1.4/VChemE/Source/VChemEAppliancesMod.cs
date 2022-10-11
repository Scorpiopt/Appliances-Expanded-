using PipeSystem;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VChemEAppliances
{
    [StaticConstructorOnStartup]
    public class CompPumpjackInfinite : ThingComp
    {
        private static readonly Material PumpjackBottom = MaterialPool.MatFrom("Things/Buildings/Special/InfinitDeepchemExtractor/InfinitDeepchemExtractor_Bottom");

        private static readonly Material PumpjackPump = MaterialPool.MatFrom("Things/Buildings/Special/InfinitDeepchemExtractor/DeepchemPump");

        private Vector3 pumpPos = Vector3.zero;

        private Vector3 pumpPosMax;

        private Vector3 bottomPos;

        private Vector3 trueCenter;

        private CompResourceStorage resource;

        private CompPowerTrader powerComp;

        private Sustainer sustainer;

        private int nextProduceTick = -1;

        private bool noCapacity = false;

        private bool goingUp = true;

        private bool cycleOver = true;

        private List<IntVec3> adjCells;

        internal List<IntVec3> lumpCells;

        public CompProperties_DeepExtractor Props => (CompProperties_DeepExtractor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            //IL_0048: Unknown result type (might be due to invalid IL or missing references)
            //IL_004d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0073: Unknown result type (might be due to invalid IL or missing references)
            //IL_0087: Unknown result type (might be due to invalid IL or missing references)
            //IL_008c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0091: Unknown result type (might be due to invalid IL or missing references)
            //IL_0098: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ac: Unknown result type (might be due to invalid IL or missing references)
            //IL_00b1: Unknown result type (might be due to invalid IL or missing references)
            //IL_00b6: Unknown result type (might be due to invalid IL or missing references)
            //IL_00bd: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d1: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d6: Unknown result type (might be due to invalid IL or missing references)
            //IL_00db: Unknown result type (might be due to invalid IL or missing references)
            base.PostSpawnSetup(respawningAfterLoad);
            resource = parent.GetComp<CompResourceStorage>();
            powerComp = parent.GetComp<CompPowerTrader>();
            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
            trueCenter = parent.TrueCenter();
            if (pumpPos.x != trueCenter.x)
            {
                pumpPos = trueCenter + new Vector3(0f, 0.75f, 0f);
            }
            bottomPos = trueCenter + new Vector3(0f, 1f, 0f);
            pumpPosMax = trueCenter + new Vector3(0f, 0f, 1.1f);
            if (lumpCells.NullOrEmpty())
            {
                lumpCells = new List<IntVec3>();
                HashSet<IntVec3> hashSet = new();
                Queue<IntVec3> queue = new();
                IntVec3 cell = parent.Position;
                Map map = parent.Map;
                queue.Enqueue(cell);
                hashSet.Add(cell);
                while (queue.Count > 0)
                {
                    IntVec3 intVec = queue.Dequeue();
                    lumpCells.Add(intVec);
                    List<IntVec3> list = GenAdjFast.AdjacentCellsCardinal(intVec);
                    for (int num = 0; num < list.Count; num++)
                    {
                        IntVec3 intVec2 = list[num];
                        int num2;
                        if (!hashSet.Contains(intVec2))
                        {
                            ThingDef thingDef = map.deepResourceGrid.ThingDefAt(intVec2);
                            if (thingDef != null)
                            {
                                num2 = (thingDef.defName == "VCHE_Deepchem") ? 1 : 0;
                                goto IL_01b3;
                            }
                        }
                        num2 = 0;
                        goto IL_01b3;
                    IL_01b3:
                        if (num2 != 0)
                        {
                            hashSet.Add(intVec2);
                            queue.Enqueue(intVec2);
                        }
                    }
                }
                lumpCells.SortByDescending((IntVec3 c) => c.DistanceTo(cell));
            }
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                StartSustainer();
            });
        }

        public override void PostDeSpawn(Map map)
        {
            nextProduceTick = -1;
        }

        public override void PostExposeData()
        {
            //IL_005a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0060: Unknown result type (might be due to invalid IL or missing references)
            Scribe_Values.Look(ref nextProduceTick, "nextProduceTick", 0);
            Scribe_Values.Look(ref noCapacity, "noCapacity", defaultValue: false);
            Scribe_Values.Look(ref cycleOver, "cycleOver", defaultValue: false);
            Scribe_Values.Look(ref goingUp, "goingUp", defaultValue: false);
            Scribe_Values.Look(ref pumpPos, "pumpPos");
            Scribe_Collections.Look(ref lumpCells, "lumpCells", LookMode.Value);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned && lumpCells.Count > 0)
            {
                int ticksGame = Find.TickManager.TicksGame;
                if (nextProduceTick == -1)
                {
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                }
                else if (ticksGame >= nextProduceTick)
                {
                    TryProducePortion();
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                }
                if (!cycleOver)
                {
                    if (goingUp)
                    {
                        pumpPos.z += 0.01f;
                        if (pumpPos.z > pumpPosMax.z)
                        {
                            goingUp = false;
                        }
                    }
                    else
                    {
                        pumpPos.z -= 0.03f;
                        if (pumpPos.z < trueCenter.z)
                        {
                            cycleOver = true;
                            goingUp = true;
                        }
                    }
                }
                sustainer?.Maintain();
            }
            else
            {
                EndSustainer();
            }
        }

        public override string CompInspectStringExtra()
        {
            if (parent.Spawned)
            {
                if (noCapacity)
                {
                    return "VCHE_CantPump".Translate();
                }
                if (lumpCells.Count == 0)
                {
                    return "DeepDrillNoResources".Translate();
                }
            }
            return null;
        }

        public override void PostDraw()
        {
            //IL_000f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            base.PostDraw();
            DrawMat(PumpjackPump, pumpPos);
            DrawMat(PumpjackBottom, bottomPos);
        }

        private void TryProducePortion()
        {
            if (powerComp != null && !powerComp.PowerOn)
            {
                EndSustainer();
                return;
            }
            bool nextResource = GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell);
            if (resDef == null || resDef.defName != "VCHE_Deepchem")
            {
                return;
            }
            Map map = parent.Map;
            if (resource != null)
            {
                PipeNet pipeNet = resource.PipeNet;
                if (pipeNet.connectors.Count > 1)
                {
                    noCapacity = pipeNet.AvailableCapacity <= 0f;
                    if (!noCapacity)
                    {
                        pipeNet.DistributeAmongStorage(1f);
                        if (cycleOver)
                        {
                            cycleOver = false;
                        }
                        StartSustainer();
                    }
                    else
                    {
                        EndSustainer();
                    }
                    return;
                }
            }
            if (!nextResource)
            {
                return;
            }
            StartSustainer();
            for (int i = 0; i < adjCells.Count; i++)
            {
                IntVec3 intVec = adjCells[i];
                if (intVec.Walkable(map))
                {
                    Thing firstThing = intVec.GetFirstThing(map, resDef);
                    if (firstThing == null)
                    {
                        firstThing = ThingMaker.MakeThing(resDef);
                        firstThing.stackCount = 1;
                        GenPlace.TryPlaceThing(firstThing, intVec, map, ThingPlaceMode.Near);
                        break;
                    }
                    if (firstThing.stackCount + 1 <= firstThing.def.stackLimit)
                    {
                        firstThing.stackCount++;
                        break;
                    }
                }
            }
            if (cycleOver)
            {
                cycleOver = false;
            }
        }

        private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            Map map = parent.Map;
            lumpCells.RemoveAll((IntVec3 c) => map.deepResourceGrid.ThingDefAt(c) == null);
            if (lumpCells.Count > 0)
            {
                IntVec3 intVec = lumpCells[0];
                ThingDef thingDef = map.deepResourceGrid.ThingDefAt(intVec);
                if (thingDef != null)
                {
                    resDef = thingDef;
                    countPresent = map.deepResourceGrid.CountAt(intVec);
                    cell = intVec;
                    return true;
                }
                resDef = null;
                countPresent = 0;
                cell = intVec;
                lumpCells.RemoveAt(0);
                return false;
            }
            resDef = null;
            countPresent = 0;
            cell = IntVec3.Invalid;
            return false;
        }

        private void DrawMat(Material mat, Vector3 drawPos)
        {
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, parent.Rotation.AsQuat, new Vector3(3f, 1f, 3f)), mat, 0);
        }

        public void StartSustainer()
        {
            if (!Props.soundAmbient.NullOrUndefined() && sustainer == null)
            {
                SoundInfo info = SoundInfo.InMap(parent);
                sustainer = Props.soundAmbient.TrySpawnSustainer(info);
            }
        }

        public void EndSustainer()
        {
            if (sustainer != null)
            {
                sustainer.End();
                sustainer = null;
            }
        }
    }
}
