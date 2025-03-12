using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Items.Weapons.ExampleSword;

public class ExampleSword : ModItem
{
    private int FocusResourceCost;
    public static LocalizedText UsesXFocusResourceText { get; set; }
    public override void SetStaticDefaults()
    {
        UsesXFocusResourceText = this.GetLocalization("UsesXFocusResource");
    }

    public override void SetDefaults()
    {
        Item.damage = 15;
        Item.width = 32;
        Item.height = 32;
        Item.value = Item.buyPrice(silver: 15);
        Item.rare = ItemRarityID.Blue;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 30;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.knockBack = 6;
        Item.crit = 32;

        FocusResourceCost = 5;
    }
    
    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "FocusResourceCost", UsesXFocusResourceText.Format(FocusResourceCost)));
    }

    public override bool CanUseItem(Player player)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();
        
        var hasEnoughFocusResource = globalPlayer.FocusResourceCurrent >= FocusResourceCost;
        
        return hasEnoughFocusResource && true;
    }

    public override bool? UseItem(Player player) {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();

        globalPlayer.FocusResourceCurrent -= FocusResourceCost;

        return true;
    }

    public override void AddRecipes() {
        
        var recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.DirtBlock, 1);
        recipe.Register();
    }
    
    
}