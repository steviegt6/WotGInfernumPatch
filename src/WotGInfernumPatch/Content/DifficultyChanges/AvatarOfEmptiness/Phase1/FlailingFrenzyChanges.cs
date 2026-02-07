using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class FlailingFrenzyChanges : ModSystem
{
    public static int PortalSummonDelay => InfernumMode.InfernumMode.CanUseCustomAIs ? 64 : 76;

    public static int PortalSummonRate => InfernumMode.InfernumMode.CanUseCustomAIs ? 6 : 8;

    public static int PortalLifetime => InfernumMode.InfernumMode.CanUseCustomAIs ? 104 : 116;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_FlailingFrenzy), BindingFlags.Public | BindingFlags.Instance)!,
            MoreFrequentFlailingFrenzyPortals
        );
    }

    private static void MoreFrequentFlailingFrenzyPortals(ILContext il)
    {
        var c = new ILCursor(il);

        const int portal_summon_delay = 76;
        const int portal_summon_rate = 8;
        const int portal_lifetime = 116;

        c.GotoNext(MoveType.After, x => x.MatchLdcI4(portal_summon_delay));
        c.EmitPop();
        c.EmitDelegate(() => PortalSummonDelay);

        c.GotoNext(MoveType.After, x => x.MatchLdcI4(portal_summon_rate));
        c.EmitPop();
        c.EmitDelegate(() => PortalSummonRate);

        c.GotoNext(MoveType.After, x => x.MatchLdcI4(portal_lifetime));
        c.EmitPop();
        c.EmitDelegate(() => PortalLifetime);
    }
}
