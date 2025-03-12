using System.Collections.Generic;
using Humanizer;
using MagicianClass.Content.Buffs;
using MagicianClass.Content.Classes.Enums;
using MagicianClass.Content.Classes.Helpers;
using MagicianClass.Content.DamageClasses;
using MagicianClass.Content.Projectiles.DeckOfCards.ClubsCard;
using MagicianClass.Content.Projectiles.DeckOfCards.DiamondsCard;
using MagicianClass.Content.Projectiles.DeckOfCards.HeartsCard;
using MagicianClass.Content.Projectiles.DeckOfCards.SpadesCard;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Items.Weapons.DeckOfCards;

public class DeckOfCards : ModItem
{
    public override string Texture => $"{nameof(MagicianClass)}/Content/Items/Weapons/DeckOfCards/DeckOfCards";
    
    private Asset<Texture2D> _textureInventoryItem;
    private int _focusResourceCost;

    public static LocalizedText UsesXFocusResourceText { get; set; }
    public static LocalizedText HeartsCardDescriptionText { get; set; }
    public static LocalizedText ClubsCardDescriptionText { get; set; }
    public static LocalizedText DiamondsCardDescriptionText { get; set; }
    public static LocalizedText SpadesCardDescriptionText { get; set; }
    
    private float _scaleMultiplier = 0.5f;
    
    public override void SetStaticDefaults()
    {
        UsesXFocusResourceText = this.GetLocalization("UsesXFocusResource");
        HeartsCardDescriptionText = this.GetLocalization("HeartsCardDescription");
        ClubsCardDescriptionText = this.GetLocalization("ClubsCardDescription");
        DiamondsCardDescriptionText = this.GetLocalization("DiamondsCardDescription");
        SpadesCardDescriptionText = this.GetLocalization("SpadesCardDescription");
    }


    public override void SetDefaults()
    {
        Item.damage = 4;
        Item.width = 36;
        Item.height = 52;
        Item.useTime = Item.useAnimation = 30;
        Item.DamageType = ModContent.GetInstance<TrickeryDamage>();
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.shootSpeed = 10f;
        Item.autoReuse = true;
        Item.knockBack = 3;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;
        Item.scale = 0.7f;
        Item.useTurn = true;
        Item.shoot = ModContent.ProjectileType<HeartsCard>();
        Item.noUseGraphic = true;

        _focusResourceCost = 12;
        Item.Hitbox = new Rectangle(0, 0, (int)(Item.width*_scaleMultiplier), (int)(Item.height*_scaleMultiplier));
}


    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
        Vector2 origin, float scale)
    {
        _textureInventoryItem ??= ModContent
            .Request<Texture2D>($"{nameof(MagicianClass)}/Content/Items/Weapons/DeckOfCards/DeckOfCards", AssetRequestMode.ImmediateLoad);
        
        var _frame = new Rectangle(0, 0, _textureInventoryItem.Value.Width, _textureInventoryItem.Value.Height);
        var color = Color.White;
        var _origin = new Vector2(_textureInventoryItem.Value.Width * 0.5f, _textureInventoryItem.Value.Height * 0.5f);
        var se = SpriteEffects.None;
        var layerDepth = 0f;
        
        spriteBatch.Draw(
            _textureInventoryItem.Value,
            position,
            _frame,
            color,
            0f,
            _origin,
            scale,
            se,
            layerDepth
        );
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        _textureInventoryItem ??= ModContent
            .Request<Texture2D>($"{nameof(MagicianClass)}/Content/Items/Weapons/DeckOfCards/DeckOfCards", AssetRequestMode.ImmediateLoad);
        
        
        var position = new Vector2(
            (Item.position.X - Main.screenPosition.X + Item.width * 0.5f) ,
            (Item.position.Y - Main.screenPosition.Y + Item.height * 0.5f) - 4);
        var frame = new Rectangle(0, 0, _textureInventoryItem.Value.Width, _textureInventoryItem.Value.Height);
        
        var origin = new Vector2(_textureInventoryItem.Value.Width * 0.5f, _textureInventoryItem.Value.Height * 0.5f);
        var se = SpriteEffects.None;
        var layerDepth = 0f;
        
        spriteBatch.Draw(
            _textureInventoryItem.Value,
            position,
            frame,
            lightColor,
            rotation,
            origin,
            _scaleMultiplier,
            se,
            layerDepth
        );
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var globalPlayer = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
        var heartsCardHealMultiplier = globalPlayer.HeartsCardHealMultiplier;
        var heartsCardHealAdditive = globalPlayer.HeartsCardHealAdditive;
        
        tooltips.Add(new TooltipLine(Mod, "FocusResourceCost", UsesXFocusResourceText.Format(_focusResourceCost)));
        tooltips.Add(new TooltipLine(Mod, "HeartsCardEffectsDescription", HeartsCardDescriptionText.Format(heartsCardHealAdditive * heartsCardHealMultiplier)));
        tooltips.Add(new TooltipLine(Mod, "ClubsCardEffectsDescription", ClubsCardDescriptionText.Format(DebuffDefenseReductionLow.DefenseReductionPercent, ClubsCard.DebuffDuration)));
        tooltips.Add(new TooltipLine(Mod, "DiamondsCardEffectsDescription", DiamondsCardDescriptionText.Format(BuffFocusRegenerationLow.FocusRegenAmountExtra, DiamondsCard.BounceDamageMultiplier)));
        tooltips.Add(new TooltipLine(Mod, "SpadesCardEffectsDescription", SpadesCardDescriptionText.Format(SpadesCard.BleedingDuration, DebuffBleedingLevelOne.DamagePerBleeding)));
    }

    // Make sure you can't use the item if you don't have enough resource
    public override bool CanUseItem(Player player)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();

        return globalPlayer.FocusResourceCurrent >= _focusResourceCost;
    }

    // Reduce resource on use
    public override bool? UseItem(Player player)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();

        globalPlayer.FocusResourceCurrent -= _focusResourceCost;
        
        
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var globalPlayer = player.GetModPlayer<GlobalPlayer>();
        
        //var cardToShoot = HCards.GetRandomCard(globalPlayer.ChancesOfCards);
        if (globalPlayer.CardsPile.Count == 0)
            return false;
                
        var cardToShoot = globalPlayer.CardsPile[^1];
        
        var offsettedPosition = position;
        
        switch (cardToShoot)
        {
            case CardType.Hearts:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<HeartsCard>(), damage, knockback, player.whoAmI);
                break;
            case CardType.Diamonds:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<DiamondsCard>(), damage, knockback, player.whoAmI);
                break;
            case CardType.Clubs:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<ClubsCard>(), damage, knockback, player.whoAmI);
                break;
            case CardType.Spades:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<SpadesCard>(), damage, knockback, player.whoAmI);
                break;
            default:
                Main.NewText("Error en DeckOfCards.Shoot, CardToShoot no reconocido");
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<HeartsCard>(), damage, knockback, player.whoAmI);
                break;
        }

        globalPlayer.CardsPile.RemoveAt(globalPlayer.CardsPile.Count - 1);
        
        return false;
    }
}