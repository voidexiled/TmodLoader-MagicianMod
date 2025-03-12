using System;
using MagicianClass.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicianClass.Content.Projectiles.DeckOfCards.DiamondsCard;

public class DiamondsCard : ProjectileCard
{
    public const int BounceTimesMax = 1;
    public const float BounceDamageMultiplier = 1.5f;
    private int _bounceTimes;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var player = Main.player[Projectile.owner];
        player.AddBuff(ModContent.BuffType<BuffFocusRegenerationLow>(), 60*4);

        if (_bounceTimes > 0)
        {
            var newDamage = (int)Math.Round(Projectile.damage * (BounceDamageMultiplier * _bounceTimes));
            hit.Damage = newDamage;
        }   
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        base.ModifyHitNPC(target, ref modifiers);
        
        if (_bounceTimes > 0)
        {
            var newDamage = (int)Math.Round(modifiers.FinalDamage.Multiplicative * (BounceDamageMultiplier * _bounceTimes));
            
            var newMultiplicative = BounceDamageMultiplier * _bounceTimes;
            modifiers.FinalDamage *= newMultiplicative;
        }
    }


    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (_bounceTimes < BounceTimesMax)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            
            _bounceTimes++;
            return false;
        }
        return true;
    }
    
}