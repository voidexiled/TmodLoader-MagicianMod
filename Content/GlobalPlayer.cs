using System;
using System.Collections.Generic;
using System.IO;
using MagicianClass.Content.Classes.Enums;
using MagicianClass.Content.Classes.Helpers;
using MagicianClass.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MagicianClass.Content;

public class GlobalPlayer : ModPlayer
{
    public const int DefaultFocusResourceMax = 100;
    public const int FocusResourceRegenDelayMax = 20; // 60 ticks = 1 segundos aprox
    public static readonly Color HealFocusResourceColor = new(26, 127, 170);
    public List<CardType> CardsPile = [];

    public Dictionary<CardType, float> ChancesOfCards;


    public float FocusReductionCostMultiplier;
    public int FocusResourceCurrent;
    public int FocusResourceMax;
    public int FocusResourceMax2;
    public int FocusResourceRegenAmount;

    public int FocusResourceRegenDelayTimer;
    public float FocusResourceRegenRate;
    internal int FocusResourceRegenTimer;

    public bool HasMagicianEquipment;
    public int HeartsCardHealAdditive;

    // public bool FocusResourceMagnet = false;
    // public static readonly int FocusResourceMagnetGrabRange = 300;

    public float HeartsCardHealMultiplier;

    public bool IsFocusing;

    public int MaxCardsPileLength;
    public Dictionary<CardType, float> PermanentChancesOfCardsUpgrade;


    public override void Initialize()
    {
        FocusResourceMax = DefaultFocusResourceMax;
    }

    public override void ResetEffects()
    {
        ResetVariables();
    }

    public override void UpdateDead()
    {
        ResetVariables();
    }

    private void ResetVariables()
    {
        FocusResourceRegenAmount = 2;
        FocusResourceRegenRate = 1f;
        FocusResourceMax2 = FocusResourceMax;
        //FocusResourceMagnet = false;

        HeartsCardHealMultiplier = 1f;
        HeartsCardHealAdditive = 0;

        ChancesOfCards = CalculateChancesOfCards(
            new Dictionary<CardType, float>
            {
                { CardType.Hearts, 0.05f },
                { CardType.Diamonds, 0.15f },
                { CardType.Clubs, 0.30f },
                { CardType.Spades, 0.50f }
            }
        );


        MaxCardsPileLength = 4;

        HasMagicianEquipment = false;
    }

    private Dictionary<CardType, float> CalculateChancesOfCards(Dictionary<CardType, float> baseChances)
    {
        return baseChances;
    }

    public override void PostUpdateMiscEffects()
    {
        UpdateFocusResource();
    }

    public override void PostUpdate()
    {
        CapFocusResourceGodMode();

        if (CardsPile.Count < MaxCardsPileLength)
        {
            var diff = MaxCardsPileLength - CardsPile.Count;
            for (var i = 0; i < diff; i++)
            {
                var newCard = HCards.GetRandomCard(ChancesOfCards);
                CardsPile.Insert(0, newCard);
            }
        }
    }

    public bool ShouldShowMagicianUI()
    {
        // muerto o no activo => no mostrar
        if (!Player.active || Player.dead)
            return false;

        // 1) Arma en mano con TrickeryDamage
        var held = Player.HeldItem;
        if (held != null && !held.IsAir &&
            held.DamageType == ModContent.GetInstance<TrickeryDamage>())
            return true;

        // 2) Alguna armadura/accesorio de mago
        if (HasMagicianEquipment)
            return true;

        return false;
    }

    private void UpdateFocusResource()
    {
        // Si no hay regen configurada, no hagas nada
        if (FocusResourceRegenRate <= 0f || FocusResourceRegenAmount <= 0)
            return;

        // Clamp básico
        FocusResourceCurrent = Utils.Clamp(FocusResourceCurrent, 0, FocusResourceMax2);

        // Si ya estás al máximo, no hace falta seguir contando
        if (FocusResourceCurrent >= FocusResourceMax2)
        {
            FocusResourceCurrent = FocusResourceMax2;
            FocusResourceRegenTimer = 0;
            FocusResourceRegenDelayTimer = 0;
            return;
        }

        // --- DELAY TIPO MANA ---
        if (FocusResourceRegenDelayTimer > 0)
        {
            // Mientras haya delay, solo lo consumimos y NO regeneramos
            FocusResourceRegenDelayTimer--;
            return;
        }

        // A partir de aquí sí podemos regenerar
        FocusResourceRegenTimer++;

        var wasBelowMaxBefore = FocusResourceCurrent < FocusResourceMax2;

        // Misma lógica que tenías, pero protegida contra división por 0
        var ticksNeeded = 15f / FocusResourceRegenRate;
        if (ticksNeeded < 1f)
            ticksNeeded = 1f;

        if (FocusResourceRegenTimer > ticksNeeded)
        {
            FocusResourceCurrent += FocusResourceRegenAmount; // amount of resource gained per "tick"
            FocusResourceRegenTimer = 0;
        }

        FocusResourceCurrent = Utils.Clamp(FocusResourceCurrent, 0, FocusResourceMax2);

        // Si acabamos de llegar al máximo desde un valor menor -> lanzar efectos
        if (wasBelowMaxBefore && FocusResourceCurrent >= FocusResourceMax2) OnFocusResourceFullyRegen();
    }

    private void OnFocusResourceFullyRegen()
    {
        if (Main.dedServ)
            return;

        // Sonido tipo Max Mana solo para el jugador local
        if (Player.whoAmI == Main.myPlayer) SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);

        // Partículas alrededor del cuerpo del jugador (estilo mana regen)
        for (var i = 0; i < 25; i++)
        {
            var dustIndex = Dust.NewDust(
                Player.position,
                Player.width,
                Player.height,
                DustID.MagicMirror, // puedes cambiar el tipo de dust si quieres algo más "mágico"
                0f,
                0f,
                150,
                HealFocusResourceColor,
                1.4f
            );

            var dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.velocity *= 1.8f;
            dust.velocity += Player.velocity * 0.3f;
        }
    }

    private void CapFocusResourceGodMode()
    {
        if (Main.myPlayer == Player.whoAmI && Player.creativeGodMode) FocusResourceCurrent = FocusResourceMax2;
    }

    public void SpendFocusResource(int amount)
    {
        FocusResourceCurrent -= amount;
        if (FocusResourceCurrent < 0)
            FocusResourceCurrent = 0;

        // Cada vez que gastas focus:
        // - reinicia el timer de regen
        // - aplica el delay (igual que el mana cuando casteas)
        FocusResourceRegenTimer = 0;
        FocusResourceRegenDelayTimer = FocusResourceRegenDelayMax;
    }

    public void HealFocusResource(int healAmount)
    {
        FocusResourceCurrent = Math.Clamp(FocusResourceCurrent + healAmount, 0, FocusResourceMax2);
        if (Main.myPlayer == Player.whoAmI) HealFocusResourceEffect(healAmount);
    }

    public void HealFocusResourceEffect(int healAmount)
    {
        CombatText.NewText(Player.getRect(), HealFocusResourceColor, healAmount);
        if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
        {
            //  SendFocusResourceEffectMessage(Player.whoAmI, healAmount);
        }
    }

    public static void HandleFocusResourceEffectMessage(BinaryReader reader, int whoAmI)
    {
        int player = reader.ReadByte();
        if (Main.netMode == NetmodeID.Server) player = whoAmI;

        var healAmount = reader.ReadInt32();
        if (player != Main.myPlayer)
            Main.player[player].GetModPlayer<GlobalPlayer>().HealFocusResourceEffect(healAmount);

        if (Main.netMode == NetmodeID.Server) SendFocusResourceEffectMessage(player, healAmount);
    }

    public static void SendFocusResourceEffectMessage(int whoAmI, int healAmount)
    {
        var packet = ModContent.GetInstance<MagicianClass>().GetPacket();
        packet.Write((byte)MagicianClass.MessageType.FocusResourceEffect);
        packet.Write(whoAmI);
        packet.Write(healAmount);
        packet.Send(ignoreClient: whoAmI);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["magicianClassFocusResourceMax"] = FocusResourceMax;
        tag["magicianClassFocusResourceCurrent"] = FocusResourceCurrent;
    }

    public override void LoadData(TagCompound tag)
    {
        FocusResourceMax = tag.GetInt("magicianClassFocusResourceMax");
        FocusResourceCurrent = tag.GetInt("magicianClassFocusResourceCurrent");
    }
}