using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class LilyStarChanges : ModSystem
{
    public static int LilyStarSpawnModulo => InfernumMode.InfernumMode.CanUseCustomAIs ? 3 : 5;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_LilyStars_ReleaseStars), BindingFlags.Public | BindingFlags.Instance)!,
            ReleaseStarsMoreFrequently
        );
    }

    private static void ReleaseStarsMoreFrequently(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.Before,
            x => x.MatchLdcI4(5),
            x => x.MatchRem()
        );

        c.Index++;

        c.EmitPop();
        c.EmitDelegate(() => LilyStarSpawnModulo);
    }
}
