using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class DisgustingStarChanges : ModSystem
{
    public static int StarReleaseRate => InfernumMode.InfernumMode.CanUseCustomAIs ? 2 : 3;
    
    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_RedirectingDisgustingStars), BindingFlags.Public | BindingFlags.Instance)!,
            SpawnMoreDisgustingStars
        );
    }

    private static void SpawnMoreDisgustingStars(ILContext il)
    {
        var c = new ILCursor(il);

        // starReleaseRate = 3
        c.GotoNext(MoveType.After, x => x.MatchLdcI4(3));
        c.EmitPop();
        c.EmitDelegate(() => StarReleaseRate);
    }
}
