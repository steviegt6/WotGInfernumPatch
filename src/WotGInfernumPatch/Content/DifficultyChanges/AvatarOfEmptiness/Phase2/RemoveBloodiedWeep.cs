using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Avatar = NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase2;

internal sealed class RemoveBloodiedWeep : ModSystem
{
    public override void Load()
    {
        base.Load();

        MonoModHooks.Add(
            typeof(Avatar).GetMethod(nameof(Avatar.ChooseNewPattern), BindingFlags.Public | BindingFlags.Instance)!,
            ChooseNewPattern_FilterOutBloodiedWeep
        );
    }

    private static List<Avatar.AvatarAIType> ChooseNewPattern_FilterOutBloodiedWeep(Func<Avatar, List<Avatar.AvatarAIType>> orig, Avatar self)
    {
        var attacks = orig(self);

        return !InfernumMode.InfernumMode.CanUseCustomAIs
            ? attacks
            : attacks.Where(x => x != Avatar.AvatarAIType.BloodiedWeep).ToList();
    }
}
