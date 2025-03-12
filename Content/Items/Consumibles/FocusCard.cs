using System.Collections.Generic;
using MagicianClass.Content.UI.FocusResourceUI;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Items.Consumibles;

public class FocusCard : ModItem
{
    


    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 18;
        Item.height = 26;
        Item.rare = ItemRarityID.Green;
        Item.maxStack = 999;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 10);
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useAnimation = 15;
    }
    
    public override bool CanUseItem(Player player)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();
        return globalPlayer.FocusResourceMax2 < 400;
    }
    
    public override bool? UseItem(Player player)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();
        const int amount = 20;
        globalPlayer.FocusResourceMax += amount;
        globalPlayer.HealFocusResource(amount);
        SoundEngine.PlaySound(SoundID.Thunder, player.Center);
        
        return true;
    }


}