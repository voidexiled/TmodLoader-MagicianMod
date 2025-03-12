using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Buffs;

public class DebuffDefenseReductionLow : ModBuff
{
    public const int DefenseReductionPercent = 25;
    public static float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;
    
    public override LocalizedText Description => base.Description.WithFormatArgs(DefenseReductionPercent);
    
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.pvpBuff[Type] = true;
        BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.Ichor);
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<MCGlobalNPC>().DefenseReductionLow = true;
    }
    
}