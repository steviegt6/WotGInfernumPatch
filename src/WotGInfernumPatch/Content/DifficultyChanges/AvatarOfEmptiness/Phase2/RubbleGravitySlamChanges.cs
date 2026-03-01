using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class RubbleGravitySlamChanges : ModSystem
{
    public static float RubbleCountMultiplier => InfernumMode.InfernumMode.CanUseCustomAIs ? 1.33f : 1f;

    public override void Load()
    {
        base.Load();

        MonoModHooks.Modify(
            typeof(Avatar).GetMethod(nameof(Avatar.DoBehavior_RubbleGravitySlam_MoveRubble), BindingFlags.Public | BindingFlags.Instance)!,
            MoveRubble_SpawnMoreRubble
        );
    }

    private static void MoveRubble_SpawnMoreRubble(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<Avatar>($"get_{nameof(Avatar.RubbleGravitySlam_RubbleCount)}"));
        c.EmitDelegate((int rubbleCount) => (int)(rubbleCount * RubbleCountMultiplier));
    }
}
