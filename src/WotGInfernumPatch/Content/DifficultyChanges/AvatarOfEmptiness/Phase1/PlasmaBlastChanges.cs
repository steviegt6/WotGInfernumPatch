using System.Reflection;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.ScreenShake;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using AvatarRift = NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm.AvatarRift;

namespace WotGInfernumPatch.Content.DifficultyChanges.AvatarOfEmptiness.Phase1;

internal sealed class PlasmaBlastChanges : ModSystem
{
    public override void Load()
    {
        base.Load();

        MonoModHooks.Add(
            typeof(AvatarRift).GetMethod(nameof(AvatarRift.DoBehavior_PlasmaBlasts), BindingFlags.Public | BindingFlags.Instance)!,
            SpawnMultiplePlasmaBlastWaves
        );
    }

    private static void SpawnMultiplePlasmaBlastWaves(AvatarRift self)
    {
        var teleportDelay = 9;
        var teleportAnimationTime = 13;
        var teleportDisappearTime = 3;
        var paleCometCount = 13;
        var postTeleportTimer = (int)self.AITimer - (teleportDelay + teleportAnimationTime * 2 + teleportDisappearTime);
        var shootDelay = 78;
        var attackTransitionDelay = shootDelay + 90;
        var gasShootCount = 16;
        var paleCometSpread = MathHelper.ToRadians(82f);
        var paleCometShootSpeed = 4.75f;
        
        // INF: Add frequency for shots so it can shoot multiple times.
        var infernumShootFrequency = 15;
        var infernumBlastCount = 3;

        // Disappear at first.
        self.NPC.scale = 1f - Utilities.InverseLerpBump(0f, teleportAnimationTime, teleportAnimationTime + teleportDisappearTime, teleportAnimationTime * 2f + teleportDisappearTime, self.AITimer - teleportDelay);
        self.NPC.Opacity = (self.NPC.scale >= 0.1f).ToInt();

        // Disable contact damage.
        self.NPC.damage = 0;

        // Suck in a bit before comets are released.
        self.SuckOpacity = Utilities.Convert01To010(Utilities.InverseLerp(0f, shootDelay, postTeleportTimer)).Cubed();
        self.SuckSoundVolumeOverride = 0f;
        if (self.SuckOpacity > 0f)
        {
            ScreenShakeSystem.SetUniversalRumble(self.SuckOpacity * 5f, MathHelper.TwoPi, null, 0.2f);
        }

        if (postTeleportTimer == 1)
        {
            var soundSlot = SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Inhale, Vector2.Lerp(self.NPC.Center, Main.LocalPlayer.Center, 0.5f));
            if (SoundEngine.TryGetActiveSound(soundSlot, out var sound))
            {
                sound.Volume *= 3.2f;
            }
        }

        if (postTeleportTimer < shootDelay / 2)
        {
            self.RiftRotationSpeedInterpolant = MathHelper.Lerp(self.RiftRotationSpeedInterpolant, self.SuckOpacity, 0.08f);
        }

        // Teleport near the player at first.
        if (self.AITimer == teleportDelay + teleportAnimationTime)
        {
            NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness.CreateTwinkle(self.NPC.Center, Vector2.One * 5.6f, false);

            // Destroy arms.
            self.Arms.Clear();

            self.TeleportTo(self.Target.Center + new Vector2(self.Target.direction * 700f + self.Target.velocity.X * 32f, -100f));
        }

        if (postTeleportTimer < shootDelay)
        {
            return;
        }

        // Release the comets.
        var canShoot = InfernumMode.InfernumMode.CanUseCustomAIs
            ? postTeleportTimer % infernumShootFrequency == 0 && postTeleportTimer < shootDelay + infernumShootFrequency * infernumBlastCount
            : postTeleportTimer == shootDelay;
        if (canShoot)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftEject, self.Target.Center);
            GeneralScreenEffectSystem.RadialBlur.Start(self.NPC.Center, 0.7f, 12);
            CustomScreenShakeSystem.Start(55, 11f).WithDistanceFadeoff(self.NPC.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Utilities.NewProjectileBetter(self.NPC.GetSource_FromAI(), self.NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkWave>(), 0, 0f, -1, 0f, 0f, 1f);
            }

            // Shoot the comets.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var applyHalfOffset = (postTeleportTimer - shootDelay) / infernumShootFrequency % 2 == 1;
                
                var cometSpawnPosition = self.NPC.Center;
                for (var i = 0; i < paleCometCount; i++)
                {
                    var localPaleCometSpread = MathHelper.Lerp(-paleCometSpread, paleCometSpread, i / (paleCometCount - 1f));

                    if (applyHalfOffset)
                    {
                        localPaleCometSpread += i / (paleCometCount - 1f) / 2f;
                    }
                    
                    var paleCometShootVelocity = self.NPC.SafeDirectionTo(self.Target.Center).RotatedBy(localPaleCometSpread) * paleCometShootSpeed;
                    Utilities.NewProjectileBetter(self.NPC.GetSource_FromAI(), cometSpawnPosition, paleCometShootVelocity, ModContent.ProjectileType<PaleComet>(), self.NPC.defDamage, 0f);
                }

                for (var i = 0; i < gasShootCount; i++)
                {
                    var localGasSpread = Main.rand.NextFloatDirection() * paleCometSpread * 0.5f;
                    var paleCometShootVelocity = self.NPC.SafeDirectionTo(self.Target.Center).RotatedBy(localGasSpread) * paleCometShootSpeed * Main.rand.NextFloat(1.7f, 2.5f);
                    Utilities.NewProjectileBetter(self.NPC.GetSource_FromAI(), cometSpawnPosition, paleCometShootVelocity, ModContent.ProjectileType<DarkGas>(), self.NPC.defDamage, 0f);
                }
            }
        }

        if (postTeleportTimer >= attackTransitionDelay)
        {
            self.SelectNextAttack();
        }
    }
}
