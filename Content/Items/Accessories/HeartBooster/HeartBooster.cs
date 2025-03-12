using MagicianClass.Content.Classes.Enums;
using Terraria;
using Terraria.ModLoader;

namespace MagicianClass.Content.Items.Accessories.HeartBooster;

public class HeartBooster : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.maxStack = 1;
        Item.value = Item.sellPrice(0, 0, 15, 0);
        Item.accessory = true;
    }
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<GlobalPlayer>().ChancesOfCards[CardType.Hearts] = 1f;
        player.GetModPlayer<GlobalPlayer>().HeartsCardHealAdditive = 4;
    }
}