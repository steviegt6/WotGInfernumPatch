using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class OtherworldlyThornsChanges : ModSystem
{
    public static int TeleportDelay => InfernumMode.InfernumMode.CanUseCustomAIs ? 25 : 30;

    public static int PortalSummonTime => InfernumMode.InfernumMode.CanUseCustomAIs ? 45 : 60;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_ReleaseOtherworldlyThorns), BindingFlags.Public | BindingFlags.Instance)!,
            FasterOtherworldlyThorns
        );
    }

    private static void FasterOtherworldlyThorns(ILContext il)
    {
        var c = new ILCursor(il);

        // teleportDelay = 30
        c.GotoNext(MoveType.After, x => x.MatchLdcI4(30));
        c.EmitPop();
        c.EmitDelegate(() => TeleportDelay);

        // portalSummonTime = 60
        c.GotoNext(MoveType.After, x => x.MatchLdcI4(60));
        c.EmitPop();
        c.EmitDelegate(() => PortalSummonTime);
    }
}
