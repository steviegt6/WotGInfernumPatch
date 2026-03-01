using System.Reflection;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase3;

internal sealed class BloodiedFountainBlastChanges : ModSystem
{
    public static int OverriddenBloodReleaseRate => 2;

    // public static float GetUnderSolynGracePeriodReductionMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 0.85f : 1f;

    public static float BlobSpawnVelocityMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 2f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_BloodiedFountainBlasts), BindingFlags.Public | BindingFlags.Instance)!,
            BloodiedFountainBlasts_MoreAggressiveBloodBlobs
        );

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_BloodiedFountainBlasts), BindingFlags.Public | BindingFlags.Instance)!,
            FasterBloodBlobs
        );

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_BloodiedFountainBlasts_DenseBurst_CreateProjectilesNearSolyn), BindingFlags.Public | BindingFlags.Instance)!,
            FasterBloodBlobs
        );
    }

    private static void BloodiedFountainBlasts_MoreAggressiveBloodBlobs(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.BloodiedFountainBlasts_BloodReleaseRate)}"));
        c.EmitDelegate((int bloodReleaseRate) => InfernumMode.InfernumMode.CanUseCustomAIs ? OverriddenBloodReleaseRate : bloodReleaseRate);
    }

    private static void FasterBloodBlobs(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(typeof(Utilities), nameof(Utilities.NewProjectileBetter))))
        {
            c.Remove();

            c.EmitDelegate(
                (
                    IEntitySource source,
                    Vector2 center,
                    Vector2 velocity,
                    int type,
                    int damage,
                    float knockback,
                    int owner,
                    float ai0,
                    float ai1,
                    float ai2
                ) =>
                {
                    if (type == ModContent.ProjectileType<BloodBlob>())
                    {
                        velocity *= BlobSpawnVelocityMultiplier;
                    }

                    return Utilities.NewProjectileBetter(
                        source,
                        center,
                        velocity,
                        type,
                        damage,
                        knockback,
                        owner,
                        ai0,
                        ai1,
                        ai2
                    );
                }
            );
        }
    }
}
