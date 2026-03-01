using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase3;

internal sealed class UnclogChanges : ModSystem
{
    public static float BlobSpawnRateMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.75f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_Unclog), BindingFlags.Public | BindingFlags.Instance)!,
            Unclog_SpawnMoreBlobs
        );
    }

    private static void Unclog_SpawnMoreBlobs(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.Unclog_BlobSpawnRate)}"));
        c.EmitDelegate((int blobSpawnRate) => (int)(blobSpawnRate * BlobSpawnRateMultiplier));
    }
}
