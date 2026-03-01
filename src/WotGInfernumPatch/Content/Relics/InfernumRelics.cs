using System;
using CalamityMod;
using InfernumMode.Content.Items.Relics;
using InfernumMode.Content.Tiles.Relics;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using Terraria;
using Terraria.ModLoader;

namespace WotGInfernumPatch.Content.Relics;

internal sealed class RelicDropManager : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        base.ModifyNPCLoot(npc, npcLoot);

        if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
        {
            npcLoot.AddIf(_ => InfernumMode.InfernumMode.CanUseCustomAIs, ModContent.ItemType<NamelessDeityRelicInfernumItem>());
        }

        if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
        {
            npcLoot.AddIf(_ => InfernumMode.InfernumMode.CanUseCustomAIs, ModContent.ItemType<NoxusRelicInfernumItem>());
        }
    }
}

// Make our relics use WotG's rendering logic for proper visual offsets.
public abstract class OurBaseInfernumBossRelic : BaseInfernumBossRelic
{
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var offScreen = new Vector2(Main.offScreenRange);
        if (Main.drawToScreen)
        {
            offScreen = Vector2.Zero;
        }

        // Take the tile, check if it actually exists.
        var p = new Point(i, j);
        var tile = Main.tile[p.X, p.Y];
        if (!tile.HasTile)
        {
            return;
        }

        // Get the initial draw parameters
        var texture = RelicTexture.Value;

        var frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item.
        var frame = texture.Frame(1, 1, 0, frameY);

        var origin = frame.Size() / 2f;
        var worldPos = p.ToWorldCoordinates(24f, 64f);

        var color = Lighting.GetColor(p.X, p.Y);

        var direction = tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before.
        var effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        // Some math magic to make it smoothly move up and down over time.
        var offset = Utilities.Sin01(Main.GlobalTimeWrappedHourly * MathF.Tau / 5f);
        var drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -44f) - Vector2.UnitY * offset * 4f;

        // Draw the main texture.
        spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

        // Draw the periodic glow effect.
        var scale = MathF.Sin(Main.GlobalTimeWrappedHourly * MathF.Tau / 2f) * 0.3f + 0.7f;
        var effectColor = color;
        effectColor.A = 0;
        effectColor = effectColor * 0.1f * scale;
        for (var offsetAngle = 0f; offsetAngle < 1f; offsetAngle += 0.1666f)
        {
            spriteBatch.Draw(texture, drawPos + (MathF.Tau * offsetAngle).ToRotationVector2() * (3f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
        }
    }
}

public sealed class NamelessDeityRelicInfernumItem : BaseRelicItem
{
    public override string Texture => "WotGInfernumPatch/Content/Relics/NamelessDeityRelicInfernum_Item";

    public override string DisplayNameToUse => "Infernal Nameless Deity Relic";

    public override int TileID => ModContent.TileType<NamelessDeityRelicInfernumTile>();
}

public sealed class NamelessDeityRelicInfernumTile : OurBaseInfernumBossRelic
{
    public override string RelicTextureName => "WotGInfernumPatch/Content/Relics/NamelessDeityRelicInfernum_Tile";

    public override int DropItemID => ModContent.ItemType<NamelessDeityRelicInfernumItem>();
}

public sealed class NoxusRelicInfernumItem : BaseRelicItem
{
    public override string Texture => "WotGInfernumPatch/Content/Relics/NoxusRelicInfernum_Item";

    public override string DisplayNameToUse => "Infernal Avatar of Emptiness Relic";

    public override int TileID => ModContent.TileType<NoxusRelicInfernumTile>();
}

public sealed class NoxusRelicInfernumTile : OurBaseInfernumBossRelic
{
    public override string RelicTextureName => "WotGInfernumPatch/Content/Relics/NoxusRelicInfernum_Tile";

    public override int DropItemID => ModContent.ItemType<NoxusRelicInfernumItem>();
}
