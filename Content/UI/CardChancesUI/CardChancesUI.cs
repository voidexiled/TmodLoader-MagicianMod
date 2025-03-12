using System.Collections.Generic;
using MagicianClass.Content.Classes.Enums;
using MagicianClass.Content.UI.FocusResourceUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace MagicianClass.Content.UI.CardChancesUI;

public class CardChancesUI : UIState
{
    private UIElement _area;
    private string _heartDisplayPercent, _diamondDisplayPercent, _clubDisplayPercent, _spadeDisplayPercent;
    private UIText _handDeckText;
    private float _offsetX;
    private float _offsetY;
    private int _cardOffsetX = 4;
    private const float Gap = 44f;

    private Asset<Texture2D> _textureClubsCard, _textureDiamondsCard, _textureHeartsCard, _textureSpadesCard;
    
    public override void OnInitialize()
    {
        _offsetX = FocusResourceBar.OffsetX + FocusResourceBar.FrameWidth * 6;
        _offsetY = FocusResourceBar.FrameHeight * 2 + 18 + 44;

        // _offsetX = 100;
        // _offsetY = 100;
        
        _textureClubsCard = ModContent.Request<Texture2D>("MagicianClass/Content/UI/CardChancesUI/ClubsCardUI");
        _textureDiamondsCard = ModContent.Request<Texture2D>("MagicianClass/Content/UI/CardChancesUI/DiamondsCardUI");
        _textureHeartsCard = ModContent.Request<Texture2D>("MagicianClass/Content/UI/CardChancesUI/HeartsCardUI");
        _textureSpadesCard = ModContent.Request<Texture2D>("MagicianClass/Content/UI/CardChancesUI/SpadesCardUI");
        
        _area = new UIElement
        {
            Left = { Pixels = _offsetX, Percent = 1f },
            Top = { Pixels = _offsetY, Precent = 0f},
            Width = { Pixels = FocusResourceBar.FrameWidth * 4f },
            Height = { Pixels = FocusResourceBar.FrameHeight },
        };
        
        _handDeckText = new UIText("Hand deck", 1f, false);
        _handDeckText.Top.Set(-30, 0f);
        _handDeckText.Left.Set(0, 0f);
        _handDeckText.Width.Set(FocusResourceBar.FrameWidth * 4f, 0f);
        _handDeckText.HAlign = 0.5f;
        
        
        Append(_area);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var maxPileLength = Main.LocalPlayer.GetModPlayer<GlobalPlayer>().MaxCardsPileLength;
        var currentPile = Main.LocalPlayer.GetModPlayer<GlobalPlayer>().CardsPile;
        var desiredWidth = _textureHeartsCard.Value.Width * currentPile.Count;
        
        _offsetX = FocusResourceBar.OffsetX + FocusResourceBar.FrameWidth * (10 - currentPile.Count);
        
        _area.Left.Set(_offsetX, 1f);
        _area.Width.Set(desiredWidth, 0f);
        
        var screenOffsetX = (int)(_offsetX + 1f * Main.screenWidth);
        
        var fontText = "Hand deck";
        var fontAsset = FontAssets.MouseText.Value;
        var fontSize = fontAsset.MeasureString(fontText);
        spriteBatch.DrawString(fontAsset, fontText, new Vector2(screenOffsetX + (fontSize.X / 4), _offsetY - 24), Color.Black);
        

        if (currentPile.Count > 0)
        {
            for (var i = 0; i < currentPile.Count; i++)
            {
                var currentCard = currentPile[i];
                var texture = currentCard switch
                {
                    CardType.Hearts => _textureHeartsCard,
                    CardType.Diamonds => _textureDiamondsCard,
                    CardType.Clubs => _textureClubsCard,
                    CardType.Spades => _textureSpadesCard,
                    _ => _textureSpadesCard
                };
                
                var texturePosition = new Vector2(((screenOffsetX + currentPile.Count * texture.Width()) - (i * texture.Width())-texture.Width()), _offsetY);
                
                //Main.NewText($"texturePosition: {texturePosition}");
                var desiredAlpha = ((i+1) / (float) currentPile.Count) * 255;
                
                
                spriteBatch.Draw(texture.Value,
                    texturePosition,
                    null,
                    new Color(255,
                        255,
                        255,
                        (int)desiredAlpha),
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    1);
            }
        }
        
        base.Draw(spriteBatch);
    }


    public override void Update(GameTime gameTime)
    {
        var modPlayer = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
        
        var sumOfChances = modPlayer.ChancesOfCards[CardType.Hearts] +
                           modPlayer.ChancesOfCards[CardType.Diamonds] +
                           modPlayer.ChancesOfCards[CardType.Clubs] +
                           modPlayer.ChancesOfCards[CardType.Spades];
        
        
        
        
        var heartsCardChance =  modPlayer.ChancesOfCards[CardType.Hearts] / sumOfChances * 100f;
        var diamondsCardChance = modPlayer.ChancesOfCards[CardType.Diamonds] / sumOfChances * 100f;
        var clubsCardChance = modPlayer.ChancesOfCards[CardType.Clubs] / sumOfChances * 100f;
        var spadesCardChance = modPlayer.ChancesOfCards[CardType.Spades] / sumOfChances * 100f;

        
        _heartDisplayPercent = heartsCardChance.ToString("0") + "%";
        _diamondDisplayPercent = diamondsCardChance.ToString("0") + "%";
        _clubDisplayPercent = clubsCardChance.ToString("0") + "%";
        _spadeDisplayPercent = spadesCardChance.ToString("0") + "%";
        
        
        
        base.Update(gameTime);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var hoverText = $"Chances\nHearts: {_heartDisplayPercent}\nDiamonds: {_diamondDisplayPercent}\nClubs: {_clubDisplayPercent}\nSpades: {_spadeDisplayPercent}";
        if (_area.IsMouseHovering) UICommon.TooltipMouseText(hoverText);
        base.DrawSelf(spriteBatch);
    }
}

[Autoload(Side = ModSide.Client)]
internal class CardChancesUISystem : ModSystem
{
    private UserInterface CardChancesUserInterface;

    internal CardChancesUI CardChancesUi;


    public override void Load()
    {
        CardChancesUi = new CardChancesUI();
        CardChancesUserInterface = new UserInterface();
        CardChancesUserInterface.SetState(CardChancesUi);

        // const string category = "UI";
        // FocusResourceText ??= Mod.GetLocalization($"{category}.FocusResource");
    }

    public override void UpdateUI(GameTime gameTime)
    {
        CardChancesUserInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (resourceBarIndex != -1)
            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer("MagicianClass: Card Chances UI", delegate
                {
                    CardChancesUserInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }, InterfaceScaleType.UI
            ));
    }
}