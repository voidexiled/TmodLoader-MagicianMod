using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MagicianClass.Content.Buffs;

public class BuffFocusRegenerationLow: ModBuff
{
    public const int FocusRegenAmountExtra = 1;
    public override LocalizedText Description => base.Description.WithFormatArgs(FocusRegenAmountExtra);
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = true;
        Main.pvpBuff[Type] = true;
        
    }

    public override void Update(Player player, ref int buffIndex)
    {
           player.GetModPlayer<GlobalPlayer>().FocusResourceRegenAmount += 5;
           
    }
}