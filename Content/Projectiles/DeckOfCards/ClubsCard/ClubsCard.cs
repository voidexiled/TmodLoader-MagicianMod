using MagicianClass.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace MagicianClass.Content.Projectiles.DeckOfCards.ClubsCard;

public class ClubsCard : ProjectileCard
{
    public const int DebuffDuration = 5;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<DebuffDefenseReductionLow>(), 60*DebuffDuration);
        base.OnHitNPC(target, hit, damageDone);
    }
}