using System.Reflection;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class SuckPlayerInChanges : ModSystem
{
    public static float PortalMoveRateMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 3.25f : 1f;

    public static float MaxSuctionAccelerationMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 2f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_SuckPlayerIn), BindingFlags.Public | BindingFlags.Instance)!,
            SuckPlayerInFasterWithMoreAggression
        );
    }

    private static void SuckPlayerInFasterWithMoreAggression(ILContext il)
    {
        var c = new ILCursor(il);

        // Increase the maximum suction acceleration.
        c.GotoNext(x => x.MatchLdstr("SuckPlayerIn_MaxSuctionAcceleration"));
        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>(nameof(Avatar.GetAIFloat)));
        c.EmitDelegate((float maxSuctionAcceleration) => maxSuctionAcceleration * MaxSuctionAccelerationMultiplier);

        // Move the portal faster to the target player.
        c.GotoNext(MoveType.Before, x => x.MatchCall(typeof(Utils), nameof(Utils.MoveTowards)));
        c.Remove();
        c.EmitDelegate(
            (Vector2 currentPosition, Vector2 targetPosition, float maxAmountAllowedToMove) => currentPosition.MoveTowards(targetPosition, maxAmountAllowedToMove * PortalMoveRateMultiplier)
        );
    }
}
