using System;
using System.Reflection;
using CalamityMod.World;
using Terraria;
using Terraria.ModLoader;

namespace WotGInfernumPatch.Content.DifficultyChanges;

// A few hacky patches to trick the game into thinking Death Mode is active when
// Infernum is active, for various stat changes.

internal readonly struct DeathInfernumOverrideScope : IDisposable
{
    private readonly bool oldDeathValue;

    public DeathInfernumOverrideScope()
    {
        oldDeathValue = CalamityWorld.death;
        CalamityWorld.death |= InfernumMode.InfernumMode.CanUseCustomAIs;
    }

    public void Dispose()
    {
        CalamityWorld.death = oldDeathValue;
    }
}

internal sealed class ApplyDeathChangesToProjectiles : GlobalProjectile
{
    public override void Load()
    {
        base.Load();

        On_Projectile.SetDefaults += SetDefaults_ApplyDeathModeChanges;
    }

    private static void SetDefaults_ApplyDeathModeChanges(On_Projectile.orig_SetDefaults orig, Projectile self, int type)
    {
        if (ProjectileLoader.GetProjectile(type) is not { } modProj || modProj.Mod != ModContent.GetInstance<NoxusBoss.NoxusBoss>())
        {
            orig(self, type);
            return;
        }

        using (new DeathInfernumOverrideScope())
        {
            orig(self, type);
        }
    }

    private static DeathInfernumOverrideScope? aiScope;

    public override bool PreAI(Projectile projectile)
    {
        if (Mod == ModContent.GetInstance<NoxusBoss.NoxusBoss>())
        {
            aiScope = new DeathInfernumOverrideScope();
        }

        return base.PreAI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        base.PostAI(projectile);

        aiScope?.Dispose();
        aiScope = null;
    }
}

internal sealed class ApplyDeathChangesToNpcs : GlobalNPC
{
    public override void Load()
    {
        base.Load();

        // Would rather hook NPC::SetDefaults, but it's too fat:
        // https://github.com/tModLoader/tModLoader/issues/3878
        MonoModHooks.Add(
            typeof(NPCLoader).GetMethod("SetDefaults", BindingFlags.NonPublic | BindingFlags.Static)!,
            SetDefaults_ApplyDeathModeChanges
        );
    }

    private static void SetDefaults_ApplyDeathModeChanges(Action<NPC, bool> orig, NPC npc, bool createModNpc)
    {
        if (NPCLoader.GetNPC(npc.type) is not { } modNpc || modNpc.Mod != ModContent.GetInstance<NoxusBoss.NoxusBoss>())
        {
            orig(npc, createModNpc);
            return;
        }

        using (new DeathInfernumOverrideScope())
        {
            orig(npc, createModNpc);
        }
    }

    private static DeathInfernumOverrideScope? aiScope;

    public override bool PreAI(NPC npc)
    {
        if (Mod == ModContent.GetInstance<NoxusBoss.NoxusBoss>())
        {
            aiScope = new DeathInfernumOverrideScope();
        }

        return base.PreAI(npc);
    }

    public override void PostAI(NPC npc)
    {
        base.PostAI(npc);

        aiScope?.Dispose();
        aiScope = null;
    }
}
