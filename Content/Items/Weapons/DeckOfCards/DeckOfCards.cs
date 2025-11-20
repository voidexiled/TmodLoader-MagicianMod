using System.Collections.Generic;
using MagicianClass.Content.Buffs;
using MagicianClass.Content.Classes.Enums;
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
    private const float InventoryScaleMultiplier = 0.5f;

    private Asset<Texture2D> _deckTexture;
    private int _focusResourceCost;
    public override string Texture => $"{nameof(MagicianClass)}/Content/Items/Weapons/DeckOfCards/DeckOfCards";

    public static LocalizedText UsesXFocusResourceText { get; set; }
    public static LocalizedText HeartsCardDescriptionText { get; set; }
    public static LocalizedText ClubsCardDescriptionText { get; set; }
    public static LocalizedText DiamondsCardDescriptionText { get; set; }
    public static LocalizedText SpadesCardDescriptionText { get; set; }

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
        Item.width = (int)(77 * InventoryScaleMultiplier);
        Item.height = (int)(72 * InventoryScaleMultiplier);
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
        Item.useTurn = true;
        Item.shoot = ModContent.ProjectileType<HeartsCard>();
        Item.noUseGraphic = true;

        _focusResourceCost = 12;
    }

    public override bool PreDrawInWorld(
        SpriteBatch spriteBatch,
        Color lightColor,
        Color alphaColor,
        ref float rotation,
        ref float scale,
        int whoAmI)
    {
        // Cargar la textura custom solo una vez
        _deckTexture ??= ModContent.Request<Texture2D>(
            $"{nameof(MagicianClass)}/Content/Items/Weapons/DeckOfCards/DeckOfCards",
            AssetRequestMode.ImmediateLoad
        );

        var texture = _deckTexture.Value;

        // Usamos todo el frame del sprite
        Rectangle frame = new(0, 0, texture.Width, texture.Height);

        // 👈 Origin en la parte de ABAJO de la carta (bottom-center)
        Vector2 origin = new(texture.Width * 0.5f, texture.Height);

        // Posición: bottom-center de la hitbox del item,
        // igual que haría vanilla, pero con nuestro origin.
        var worldPos =
            Item.position
            - Main.screenPosition
            + new Vector2(Item.width * 0.5f, Item.height);

        // Respetamos la escala vanilla (incluye la animación de pickup),
        // pero además la reducimos con nuestro multiplicador.
        var finalScale = scale * InventoryScaleMultiplier;

        // alphaColor ya incluye alpha/fade correcto
        var drawColor = alphaColor;

        spriteBatch.Draw(
            texture,
            worldPos,
            frame,
            drawColor,
            rotation,
            origin,
            finalScale,
            SpriteEffects.None,
            0f
        );

        // Evitamos que Terraria lo vuelva a dibujar
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var globalPlayer = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
        var heartsCardHealMultiplier = globalPlayer.HeartsCardHealMultiplier;
        var heartsCardHealAdditive = globalPlayer.HeartsCardHealAdditive;

        tooltips.Add(new TooltipLine(Mod, "FocusResourceCost", UsesXFocusResourceText.Format(_focusResourceCost)));
        tooltips.Add(new TooltipLine(Mod, "HeartsCardEffectsDescription",
            HeartsCardDescriptionText.Format(heartsCardHealAdditive * heartsCardHealMultiplier)));
        tooltips.Add(new TooltipLine(Mod, "ClubsCardEffectsDescription",
            ClubsCardDescriptionText.Format(DebuffDefenseReductionLow.DefenseReductionPercent,
                ClubsCard.DebuffDuration)));
        tooltips.Add(new TooltipLine(Mod, "DiamondsCardEffectsDescription",
            DiamondsCardDescriptionText.Format(BuffFocusRegenerationLow.FocusRegenAmountExtra,
                DiamondsCard.BounceDamageMultiplier)));
        tooltips.Add(new TooltipLine(Mod, "SpadesCardEffectsDescription",
            SpadesCardDescriptionText.Format(SpadesCard.BleedingDuration, DebuffBleedingLevelOne.DamagePerBleeding)));
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

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback)
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
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<HeartsCard>(),
                    damage, knockback, player.whoAmI);
                break;
            case CardType.Diamonds:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<DiamondsCard>(),
                    damage, knockback, player.whoAmI);
                break;
            case CardType.Clubs:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<ClubsCard>(),
                    damage, knockback, player.whoAmI);
                break;
            case CardType.Spades:
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<SpadesCard>(),
                    damage, knockback, player.whoAmI);
                break;
            default:
                Main.NewText("Error en DeckOfCards.Shoot, CardToShoot no reconocido");
                Projectile.NewProjectile(source, offsettedPosition, velocity, ModContent.ProjectileType<HeartsCard>(),
                    damage, knockback, player.whoAmI);
                break;
        }

        globalPlayer.CardsPile.RemoveAt(globalPlayer.CardsPile.Count - 1);

        return false;
    }
}