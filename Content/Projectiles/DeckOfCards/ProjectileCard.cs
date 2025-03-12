using MagicianClass.Content.Classes.Enums;
using MagicianClass.Content.DamageClasses;
using MagicianClass.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicianClass.Content.Projectiles.DeckOfCards;

public abstract class ProjectileCard : ModProjectile
{
    //public sealed override string Texture => $"{nameof(MagicianClass)}/Content/Projectiles/DeckOfCards/ProjectileCard";
    private Color blackColor = new Color(255, 255, 255);
    private Color redColor = new Color(250, 0, 0);
    
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = 18;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3600;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        AIType = ProjectileID.WoodenArrowFriendly;
        Projectile.DamageType = ModContent.GetInstance<TrickeryDamage>();
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        Projectile.Hitbox = new Rectangle(0, 0, Projectile.width, Projectile.height - (Projectile.height - Projectile.width));
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        var shootCardType = Projectile.type == ModContent.ProjectileType<HeartsCard.HeartsCard>() ? CardType.Hearts :
            Projectile.type == ModContent.ProjectileType<DiamondsCard.DiamondsCard>() ? CardType.Diamonds :
            Projectile.type == ModContent.ProjectileType<ClubsCard.ClubsCard>() ? CardType.Clubs :
            Projectile.type == ModContent.ProjectileType<SpadesCard.SpadesCard>() ? CardType.Spades : CardType.Hearts;

        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

        var color = shootCardType switch
        {
            CardType.Hearts or CardType.Diamonds => redColor,
            CardType.Clubs or CardType.Spades => blackColor,
            _ => blackColor
        };
        
        var dustType = shootCardType switch
        {
            CardType.Hearts => ModContent.DustType<Hearts>(),
            CardType.Diamonds => ModContent.DustType<Diamonds>(),
            CardType.Clubs => ModContent.DustType<Clubs>(),
            _ => ModContent.DustType<Spades>()
        };
        
        for (var i = 0; i < 10; i++)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, newColor: color);
    }
    

    public override void AI()
    {
        base.AI();

        var shootCardType = Projectile.type == ModContent.ProjectileType<HeartsCard.HeartsCard>() ? CardType.Hearts :
            Projectile.type == ModContent.ProjectileType<DiamondsCard.DiamondsCard>() ? CardType.Diamonds :
            Projectile.type == ModContent.ProjectileType<ClubsCard.ClubsCard>() ? CardType.Clubs :
            Projectile.type == ModContent.ProjectileType<SpadesCard.SpadesCard>() ? CardType.Spades : CardType.Hearts;

        var color = shootCardType switch
        {
            CardType.Hearts or CardType.Diamonds => redColor,
            CardType.Clubs or CardType.Spades => blackColor,
            _ => blackColor
        };

        var dustType = shootCardType switch
        {
            CardType.Hearts => ModContent.DustType<Hearts>(),
            CardType.Diamonds => ModContent.DustType<Diamonds>(),
            CardType.Clubs => ModContent.DustType<Clubs>(),
            _ => ModContent.DustType<Spades>()
        };
        
        const float dustScale = 1f;
        const int dustAlpha = 0;
        Vector2? velocityScale = null;
        
        for (var i = 0; i < 1; i++){
            var dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworkFountain_Blue, velocityScale, dustAlpha, Color.White, dustScale);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            dust.scale = 0.8f;
            dust.noLight = false;
            dust.noLightEmittence = false;
        }
        
    }

    

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //target.AddBuff(BuffID.Poisoned, 300);
        base.OnHitNPC(target, hit, damageDone);
    }
}