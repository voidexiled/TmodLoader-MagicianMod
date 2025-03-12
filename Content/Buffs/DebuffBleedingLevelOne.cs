using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Buffs;

public class DebuffBleedingLevelOne : ModBuff
{
    public const int DamagePerBleeding = 5;
    public override LocalizedText Description => base.Description.WithFormatArgs(DamagePerBleeding);
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.pvpBuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<MCGlobalNPC>().BleedingLevelOne = true;
        
    }
    
    
}