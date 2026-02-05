using InfernumMode.Content.Items.Relics;
using InfernumMode.Content.Tiles.Relics;
using Terraria.ModLoader;

namespace WotGInfernumPatch.Content.Relics;

public sealed class NamelessDeityRelicInfernumItem : BaseRelicItem
{
    public override string Texture => "WotGInfernumPatch/Content/Relics/NamelessDeityRelicInfernum_Item";

    public override string DisplayNameToUse => "Infernal Nameless Deity Relic";

    public override int TileID => ModContent.TileType<NamelessDeityRelicInfernumTile>();
}

public sealed class NamelessDeityRelicInfernumTile : BaseInfernumBossRelic
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

public sealed class NoxusRelicInfernumTile : BaseInfernumBossRelic
{
    public override string RelicTextureName => "WotGInfernumPatch/Content/Relics/NoxusRelicInfernum_Tile";

    public override int DropItemID => ModContent.ItemType<NoxusRelicInfernumItem>();
}
