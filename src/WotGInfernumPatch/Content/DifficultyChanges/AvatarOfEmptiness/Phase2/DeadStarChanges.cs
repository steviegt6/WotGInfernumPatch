using System.Reflection;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria.ModLoader;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class DeadStarChanges : ModSystem
{
    public static int MaxBurstCountMultiplier => 2;
    
    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(DeadStar).GetMethod(nameof(DeadStar.AI), BindingFlags.Public | BindingFlags.Instance)!,
            Ai_AddMoreBursts
        );
    }

    private static void Ai_AddMoreBursts(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<DeadStar>($"get_{nameof(DeadStar.MaxBurstCount)}"));
        c.EmitDelegate(() => MaxBurstCountMultiplier);
        c.EmitMul();
    }
}
