using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class DeadStarChanges : ModSystem
{
    public static float MaxBurstCountMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.75f : 1f;

    public static int RealMaxBurstCount => (int)(DeadStar.MaxBurstCount * MaxBurstCountMultiplier);

    public static float TelegraphMultiplierStart => 0.9f;

    public static float TelegraphMultiplierEnd => 0.35f;

    public override void Load()
    {
        base.Load();

        // Double total star bursts
        MonoModHooks.Modify(
            typeof(DeadStar).GetMethod(nameof(DeadStar.AI), BindingFlags.Public | BindingFlags.Instance)!,
            ApplyMaxBurstCountMultiplier
        );

        // Make star bursts progressively faster
        MonoModHooks.Modify(
            typeof(DeadStar).GetMethod(nameof(DeadStar.AI), BindingFlags.Public | BindingFlags.Instance)!,
            ApplyTelegraphTimeMultiplier
        );

        MonoModHooks.Modify(
            typeof(DeadStar).GetMethod(nameof(DeadStar.HandleShootingAndTelegraph), BindingFlags.Public | BindingFlags.Instance)!,
            ApplyTelegraphTimeMultiplier
        );

        MonoModHooks.Modify(
            typeof(DeadStar).GetMethod(nameof(DeadStar.DrawDeadSun), BindingFlags.Public | BindingFlags.Instance)!,
            ApplyTelegraphTimeMultiplier
        );
    }

    private static void ApplyMaxBurstCountMultiplier(ILContext il)
    {
        var c = new ILCursor(il);

        /*
        c.GotoNext(MoveType.After, x => x.MatchCall<DeadStar>($"get_{nameof(DeadStar.MaxBurstCount)}"));
        c.EmitDelegate(() => MaxBurstCountMultiplier);
        c.EmitMul();
        */

        c.GotoNext(MoveType.After, x => x.MatchCall<DeadStar>($"get_{nameof(DeadStar.MaxBurstCount)}"));
        c.EmitPop();
        c.EmitDelegate(() => RealMaxBurstCount);
    }

    private static void ApplyTelegraphTimeMultiplier(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.DyingStarsWind_TelegraphTime)}")))
        {
            c.EmitLdarg0();
            c.EmitDelegate(
                (int telegraphTime, DeadStar self) =>
                {
                    if (!InfernumMode.InfernumMode.CanUseCustomAIs)
                    {
                        return telegraphTime;
                    }

                    return (int)(telegraphTime * MathHelper.Lerp(TelegraphMultiplierStart, TelegraphMultiplierEnd, self.BurstCounter / (float)RealMaxBurstCount));
                }
            );
        }
    }
}
