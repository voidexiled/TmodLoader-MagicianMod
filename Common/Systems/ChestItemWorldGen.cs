using System;
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
        int[] itemsToPlaceInNormalChests =
        {
            ModContent.ItemType<DeckOfCards>(),
            ModContent.ItemType<FocusCard>(),
        };
        int itemsToPlaceInNormalChestsChoice = 0;
        int itemsPlaced = 0;
        int maxItems = 40;
        for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
        {
            Chest chest = Main.chest[chestIndex];
            if (chest == null)
            {
                continue;
            }
            Tile chestTile = Main.tile[chest.x, chest.y];

            // Tiles_21.png
            //&& chestTile.TileFrameX is 0 * 36 or 1 * 36
            if (chestTile is { TileType: TileID.Containers, TileFrameX: 0 * 36 or 1 * 36 })
            {
                for (int inventoryIndex = 0; inventoryIndex < Chest.maxItems; inventoryIndex++)
                {
                    if (chest.item[inventoryIndex].type == ItemID.None)
                    {
                        chest.item[inventoryIndex].SetDefaults(itemsToPlaceInNormalChests[itemsToPlaceInNormalChestsChoice]);
                        itemsToPlaceInNormalChestsChoice = (itemsToPlaceInNormalChestsChoice + 1) % itemsToPlaceInNormalChests.Length;
                        // Alternate approach: Random instead of cyclical: chest.item[inventoryIndex].SetDefaults(WorldGen.genRand.Next(itemsToPlaceInFrozenChests));
                        Console.WriteLine($"{itemsToPlaceInNormalChests[itemsToPlaceInNormalChestsChoice].ToString()} Placed at X {chest.x}, Y {chest.y}");
                        itemsPlaced++;
                        break;
                    }
                }
            }
            Player player = Main.player[Main.myPlayer];
            if (itemsPlaced >= maxItems)
                break;
        }
    }
}