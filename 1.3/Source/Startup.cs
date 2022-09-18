using HarmonyLib;
using Verse;

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
}
