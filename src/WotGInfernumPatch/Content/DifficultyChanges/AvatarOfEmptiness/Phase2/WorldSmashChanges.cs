using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class WorldSmashChanges : ModSystem
{
    public static int AdditivePlanetoidSmashCount => InfernumMode.InfernumMode.CanUseCustomAIs ? 2 : 0;

    public static float PlanetoidFlingSpeedMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.45f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Add(
            typeof(Avatar).GetMethod(nameof(Avatar.LoadState_WorldSmash), BindingFlags.Public | BindingFlags.Instance)!,
            LoadState_WorldSmash_WithOurValues
        );

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_WorldSmash), BindingFlags.Public | BindingFlags.Instance)!,
            WorldSmash_FasterAndMorePlentifulPlanetoids
        );
    }

    private static void LoadState_WorldSmash_WithOurValues(Avatar self)
    {
        self.StateMachine.RegisterTransition(
            Avatar.AvatarAIType.WorldSmash,
            null,
            false,
            () => self.WorldSmash_PlanetoidSummonCounter >= (Avatar.WorldSmash_PlanetoidSmashCount + AdditivePlanetoidSmashCount) && self.AITimer >= Avatar.WorldSmash_AttackTransitionDelay,
            () =>
            {
                self.WorldSmash_PlanetoidFlyOffDirection = 0f;
                self.WorldSmash_PlanetoidSummonCounter = 0f;
            }
        );

        // Load the AI state behavior.
        self.StateMachine.RegisterStateBehavior(Avatar.AvatarAIType.WorldSmash, self.DoBehavior_WorldSmash);

        self.StatesToNotStartTeleportDuring.Add(Avatar.AvatarAIType.WorldSmash);
    }

    private static void WorldSmash_FasterAndMorePlentifulPlanetoids(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.WorldSmash_PlanetoidSmashCount)}")))
        {
            c.EmitDelegate((int smashCount) => smashCount + AdditivePlanetoidSmashCount);
        }

        c.Index = 0;

        while (c.TryGotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.WorldSmash_PlanetoidFlingSpeed)}")))
        {
            c.EmitDelegate((float flingSpeed) => flingSpeed * PlanetoidFlingSpeedMultiplier);
        }
    }
}
