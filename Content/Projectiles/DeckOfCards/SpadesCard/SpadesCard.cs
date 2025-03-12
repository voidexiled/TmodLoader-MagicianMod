using MagicianClass.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace MagicianClass.Content.Projectiles.DeckOfCards.SpadesCard;

/// <summary>
/// Spades Card is a card that deals damage to the target.
/// Bleeding Level 1 debuff is applied to the target for 2 seconds if the target is an NPC.
/// </summary>
public class SpadesCard : ProjectileCard
{
    public const int BleedingDuration = 3;
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<DebuffBleedingLevelOne>(), 60*BleedingDuration);
    }
}