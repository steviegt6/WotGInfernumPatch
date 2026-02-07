using System;
using System.Reflection;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase3;

internal sealed class WhirlingIceStormChanges : ModSystem
{
    public static float InitialArcticBlastMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.5f : 1f;
    
    public override void Load()
    {
        base.Load();

        // Spawn more arctic blast projectiles at the start.
        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_WhirlingIceStorm), BindingFlags.Public | BindingFlags.Instance)!,
            SpawnMoreInitialArcticBlasts
        );
        
        // Let arctic blasts go a lot faster.
        MonoModHooks.Add(
            typeof(ArcticBlast).GetMethod(nameof(ArcticBlast.SpinAround), BindingFlags.Public | BindingFlags.Static)!,
            SpinAround_Faster
        );
    }

    private static void SpawnMoreInitialArcticBlasts(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.WhirlingIceStorm_InitialArcticBlastCount)}"));
        c.EmitDelegate(
            (int initialArcticBlastCount) => (int)(initialArcticBlastCount * InitialArcticBlastMultiplier)
        );
    }

    private static Vector2 SpinAround_Faster(float horizontalSpinDirection, int time, float startingYPosition, ref float zPosition)
    {
        var velocityVariance = InfernumMode.InfernumMode.CanUseCustomAIs
            ? 10f
            : 6f;

        var velocityBaseline = InfernumMode.InfernumMode.CanUseCustomAIs
            ? 32f
            : 20f;
        
        var spinCompletion = Utilities.InverseLerp(0f, ArcticBlast.Lifetime - 60, time);
        var maxHorizontalSpeed = MathF.Cos(MathHelper.TwoPi * startingYPosition / 1040f) * velocityVariance + velocityBaseline;
        var spinArc = MathHelper.Pi * spinCompletion * 4f;
        zPosition = Utilities.Saturate(Utilities.Cos01(spinArc)).Cubed() * 3f;
        return spinArc.ToRotationVector2() * new Vector2(maxHorizontalSpeed * horizontalSpinDirection, 5f);
    }
}
