using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase3;

internal sealed class ArmPortalStrikeChanges : ModSystem
{
    public static int OverriddenPortalSummonRate => 1;

    public static float PortalLifetimeMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 0.75f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_ArmPortalStrikes), BindingFlags.Public | BindingFlags.Instance)!,
            ArmPortalStrikes_SpawnMorePortalsMoreAggressively
        );
    }

    private static void ArmPortalStrikes_SpawnMorePortalsMoreAggressively(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.ArmPortalStrikes_PortalLifetime)}"));
        c.EmitDelegate((int portalLifetime) => (int)(portalLifetime * PortalLifetimeMultiplier));

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.ArmPortalStrikes_PortalSummonRate)}"));
        c.EmitDelegate((int portalSummonRate) => InfernumMode.InfernumMode.CanUseCustomAIs ? OverriddenPortalSummonRate : portalSummonRate);
    }
}
