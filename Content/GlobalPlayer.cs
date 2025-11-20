using System;
using System.Collections.Generic;
using System.IO;
using MagicianClass.Content.Classes.Enums;
using MagicianClass.Content.Classes.Helpers;
using MagicianClass.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MagicianClass.Content;

public class GlobalPlayer : ModPlayer
{
    public const int DefaultFocusResourceMax = 100;
    public static readonly Color HealFocusResourceColor = new(26, 127, 170);
    public List<CardType> CardsPile = [];

    public Dictionary<CardType, float> ChancesOfCards;

    public float FocusReductionCostMultiplier;
    public int FocusResourceCurrent;
    public int FocusResourceMax;
    public int FocusResourceMax2;
    public int FocusResourceRegenAmount;
    public float FocusResourceRegenRate;
    internal int FocusResourceRegenTimer;

    public bool HasMagicianEquipment;
    public int HeartsCardHealAdditive;

    // public bool FocusResourceMagnet = false;
    // public static readonly int FocusResourceMagnetGrabRange = 300;

    public float HeartsCardHealMultiplier;

    public bool IsFocusing;

    public int MaxCardsPileLength;


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

        ChancesOfCards = new Dictionary<CardType, float>
        {
            { CardType.Hearts, 0.02f },
            { CardType.Diamonds, 0.10f },
            { CardType.Clubs, 0.40f },
            { CardType.Spades, 0.48f }
        };


        MaxCardsPileLength = 4;

        HasMagicianEquipment = false;
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
        FocusResourceRegenTimer++;

        if (FocusResourceRegenTimer > 15 / FocusResourceRegenRate)
        {
            FocusResourceCurrent += FocusResourceRegenAmount; // amount of resource gained per second
            FocusResourceRegenTimer = 0;
        }

        FocusResourceCurrent = Utils.Clamp(FocusResourceCurrent, 0, FocusResourceMax2);
    }

    private void CapFocusResourceGodMode()
    {
        if (Main.myPlayer == Player.whoAmI && Player.creativeGodMode) FocusResourceCurrent = FocusResourceMax2;
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