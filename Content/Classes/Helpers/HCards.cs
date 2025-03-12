using System;
using System.Collections.Generic;
using System.Linq;
using MagicianClass.Content.Classes.Enums;
using Terraria;
using Terraria.ModLoader;

namespace MagicianClass.Content.Classes.Helpers;

public static class HCards
{
    public static CardType GetRandomCard(Dictionary<CardType, float> chances)
    {
        // Verificar si el diccionario tiene valores válidos
        if (chances == null || chances.Count == 0)
        {
            Main.NewText("Error en HCards.GetRandomCard, el diccionario está vacío o es nulo.");
            return CardType.Hearts; // Valor por defecto
        }

        // Crear una lista ponderada basada en las probabilidades
        List<CardType> weightedList = new List<CardType>();

        foreach (var kvp in chances)
        {
            // Solo incluir valores con probabilidad mayor a 0
            if (kvp.Value > 0)
            {
                int weight = (int)(kvp.Value * 1000); // Multiplica para evitar problemas de precisión
                weightedList.AddRange(Enumerable.Repeat(kvp.Key, weight));
            }
        }

        // Verificar si la lista ponderada tiene elementos
        if (weightedList.Count == 0)
        {
            Main.NewText("Error en HCards.GetRandomCard, todas las probabilidades son 0.");
            return CardType.Hearts; // Valor por defecto
        }

        // Seleccionar un valor aleatorio de la lista ponderada
        var random = new Random();
        int index = random.Next(weightedList.Count);
        return weightedList[index];
    }
    

    public static List<CardType> GetRandomCard(Dictionary<CardType, float> chances, int amount)
    {
        var dummyList = new List<CardType>();
        for (var i = 0; i < amount; i++)
        {
            dummyList.Add(GetRandomCard(chances));
        }
        
        return dummyList;
    }

    public static void TryFillCards()
    {
        var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
        var dummyList = player.CardsPile;
        var amountOfCardsToGenerate = player.MaxCardsPileLength - dummyList.Count;
        
        for (var i = 0; i < amountOfCardsToGenerate; i++)
        {
            dummyList.Insert(0, GetRandomCard(player.ChancesOfCards));
        }
        
        player.CardsPile = dummyList;
    }
}