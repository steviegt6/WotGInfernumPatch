using System.Reflection;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase3;

internal sealed class UnclogChanges : ModSystem
{
    public static float BlobSpawnRateMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.75f : 1f;

    public static float BlobSpawnVelocityMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.45f : 1f;

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

        // Make them spawn more frequently.
        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.Unclog_BlobSpawnRate)}"));
        c.EmitDelegate((int blobSpawnRate) => (int)(blobSpawnRate * BlobSpawnRateMultiplier));

        // Increase their initial velocity.
        c.Index = 0;

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

                    Utilities.NewProjectileBetter(
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
