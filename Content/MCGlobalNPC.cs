using MagicianClass.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicianClass.Content;

public class MCGlobalNPC: GlobalNPC
{
    public override bool InstancePerEntity => true;
    public bool DefenseReductionLow;
    public bool BleedingLevelOne;

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (BleedingLevelOne)
        {
            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= DebuffBleedingLevelOne.DamagePerBleeding * 2;

            if (damage < DebuffBleedingLevelOne.DamagePerBleeding)
            {
                damage = DebuffBleedingLevelOne.DamagePerBleeding;
                Dust.NewDust(
                    npc.position,
                    npc.width,
                    npc.height,
                    DustID.Blood,
                    0f,
                    0f,
                    150,
                    Color.DarkRed,
                    1f
                );         
            }
        }
    }
    
    public override void ResetEffects(NPC npc)
    {
        DefenseReductionLow = false;
        BleedingLevelOne = false;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (DefenseReductionLow)
            modifiers.Defense *= DebuffDefenseReductionLow.DefenseMultiplier;
    }
    

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        base.DrawEffects(npc, ref drawColor);
        if (DefenseReductionLow)
            drawColor.G = 0;
        
        if (BleedingLevelOne){
            drawColor.R = 200;
            drawColor.G = 15;
            drawColor.B = 15;
        }
    }
}