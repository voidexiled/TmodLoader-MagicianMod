using Terraria;

namespace MagicianClass.Content.Projectiles.DeckOfCards.HeartsCard;

public class HeartsCard : ProjectileCard
{
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var player = Main.player[Projectile.owner];
        var globalPlayer = Main.player[Projectile.owner].GetModPlayer<GlobalPlayer>();
    
        var halfDamageDone = damageDone / 2f;
        var finalHealAdditive = globalPlayer.HeartsCardHealAdditive * globalPlayer.HeartsCardHealMultiplier;
        var amountToHeal = (int) (halfDamageDone + finalHealAdditive);
        player.Heal(amountToHeal);
        
        
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Projectile.ContinuouslyUpdateDamageStats = true;
    }
}