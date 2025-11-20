using MagicianClass.Content.Items.Consumibles;
using MagicianClass.Content.Items.Weapons.DeckOfCards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicianClass.Common.Systems;

public class ChestItemWorldGen : ModSystem
{
    public override void PostWorldGen()
    {
        // ==========================
        // 1) Cofre de madera cercano al spawn con DeckOfCards garantizada
        // ==========================
        var deckItemType = ModContent.ItemType<DeckOfCards>();

        var spawnX = Main.spawnTileX;
        var spawnY = Main.spawnTileY;

        var guaranteedChestIndex = -1;
        var bestDistSq = double.MaxValue;

        // Buscar el cofre de madera más cercano al spawn
        for (var i = 0; i < Main.maxChests; i++)
        {
            var chest = Main.chest[i];
            if (chest == null)
                continue;

            var chestTile = Main.tile[chest.x, chest.y];
            if (chestTile.TileType != TileID.Containers)
                continue;

            var style = chestTile.TileFrameX / 36;

            // style == 0 → cofre de madera
            if (style == 0)
            {
                var dx = chest.x - spawnX;
                var dy = chest.y - spawnY;
                double distSq = dx * dx + dy * dy;

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    guaranteedChestIndex = i;
                }
            }
        }

        // Si encontramos algún cofre de madera, le ponemos sí o sí una DeckOfCards
        if (guaranteedChestIndex != -1)
        {
            var chest = Main.chest[guaranteedChestIndex];
            var placed = false;

            // Intentar usar un slot vacío
            for (var slot = 0; slot < Chest.maxItems; slot++)
                if (chest.item[slot].type == ItemID.None)
                {
                    chest.item[slot].SetDefaults(deckItemType);
                    placed = true;
                    break;
                }

            // Si el cofre está lleno, sobrescribimos el último slot
            if (!placed) chest.item[Chest.maxItems - 1].SetDefaults(deckItemType);
        }

        // ==========================
        // 2) Tu lógica actual de cofres (ajustada para no tocar el cofre garantizado)
        // ==========================

        int[] itemsToPlaceInNormalChests =
        {
            ModContent.ItemType<DeckOfCards>(),
            ModContent.ItemType<FocusCard>()
        };
        var itemsToPlaceInNormalChestsChoice = 0;

        var itemsPlaced = 0;
        var maxItems = 40;

        for (var chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
        {
            var chest = Main.chest[chestIndex];
            if (chest == null)
                continue;

            // 🔽 No meter más items extra en el cofre garantizado
            if (chestIndex == guaranteedChestIndex)
                continue;

            var chestTile = Main.tile[chest.x, chest.y];

            if (chestTile is { TileType: TileID.Containers, TileFrameX: 0 * 36 or 1 * 36 })
                for (var inventoryIndex = 0; inventoryIndex < Chest.maxItems; inventoryIndex++)
                    if (chest.item[inventoryIndex].type == ItemID.None)
                    {
                        chest.item[inventoryIndex].SetDefaults(
                            itemsToPlaceInNormalChests[itemsToPlaceInNormalChestsChoice]
                        );

                        itemsToPlaceInNormalChestsChoice =
                            (itemsToPlaceInNormalChestsChoice + 1) % itemsToPlaceInNormalChests.Length;

                        itemsPlaced++;
                        break;
                    }

            if (itemsPlaced >= maxItems)
                break;
        }
    }
}