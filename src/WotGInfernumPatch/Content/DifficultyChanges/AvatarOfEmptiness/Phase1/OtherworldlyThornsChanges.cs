using System.Reflection;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class OtherworldlyThornsChanges : ModSystem
{
    public static int TeleportDelay => InfernumMode.InfernumMode.CanUseCustomAIs ? 25 : 30;

    public static int PortalSummonTime => InfernumMode.InfernumMode.CanUseCustomAIs ? 75 : 60;

    public static float ThornReachTelegraphTimeReductionMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 0.75f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_ReleaseOtherworldlyThorns), BindingFlags.Public | BindingFlags.Instance)!,
            SpawnMoreOtherworldlyThornsFaster
        );

        MonoModHooks.Modify(
            typeof(OtherworldlyThorn).GetMethod(nameof(OtherworldlyThorn.AI), BindingFlags.Public | BindingFlags.Instance)!,
            FasterOtherworldlyThornsTelegraph
        );
    }

    private static void SpawnMoreOtherworldlyThornsFaster(ILContext il)
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

    private static void FasterOtherworldlyThornsTelegraph(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchLdstr("ReleaseOtherworldlyThorns_ThornReachTelegraphTime"));
        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>(nameof(Avatar.GetAIInt)));
        c.EmitDelegate((int thornReachTelegraphTime) => (int)(thornReachTelegraphTime * ThornReachTelegraphTimeReductionMultiplier));
    }
}
