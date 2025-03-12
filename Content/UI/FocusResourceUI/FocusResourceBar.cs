using System;
using System.Collections.Generic;
using MagicianClass.Content.UI.FocusResourceUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MagicianClass.Content.UI.FocusResourceUI
{
    public class FocusResourceBar : UIState
    {
        private UIElement _area;
        private UIElement _barFrame;

        public const int FocusCardValue = 20;
        public const int FrameWidth = 32;
        public const int FrameHeight = 48;

        private const string TexturePathFocusOff = "MagicianClass/Content/UI/FocusResourceUI/FocusCardOff";
        private const string TexturePathFocusOn = "MagicianClass/Content/UI/FocusResourceUI/FocusCardOn";
        private Asset<Texture2D> _textureFocusOff;
        private Asset<Texture2D> _textureFocusOn;

        public const int OffsetX = -400 - 10 * FrameWidth;
        private int _previousFocusPoints;
        private int _focusAnimationTimer;
        private int _ticksToRegenerateFullFocusCard;
        private int _ticksPerRegeneration;
        private int _currentFocusCard;

        public override void OnInitialize()
        {
            _textureFocusOff ??= ModContent.Request<Texture2D>(TexturePathFocusOff);
            _textureFocusOn ??= ModContent.Request<Texture2D>(TexturePathFocusOn);

            _area = new UIElement
            {
                Left = { Pixels = OffsetX, Percent = 1f },
                Top = { Pixels = 18 },
                Width = { Pixels = FrameWidth },
                Height = { Pixels = FrameHeight },
                OverflowHidden = true
            };

            _barFrame = new UIElement
            {
                Width = { Pixels = FrameWidth },
                Height = { Pixels = FrameHeight }
            };

            _area.Append(_barFrame);
            Append(_area);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
            
            
            _ticksPerRegeneration = (int)(60 / player.FocusResourceRegenRate); // 60 / 1f = 60

            _ticksToRegenerateFullFocusCard = 60 * (FocusCardValue / player.FocusResourceRegenAmount);
            
            var focusQuotient = player.FocusResourceCurrent / FocusCardValue;
            //Main.NewText($"FocusQuotient: {focusQuotient}");
            var maxFocusQuotients = player.FocusResourceMax2 / FocusCardValue;
            //Main.NewText($"MaxFocusQuotients: {maxFocusQuotients}");
            var screenOffsetX = (int)(OffsetX + 1f * Main.screenWidth);
            var screenOffsetY = 19;

            DrawFocusCards(spriteBatch, screenOffsetX, screenOffsetY, maxFocusQuotients, focusQuotient);
            UpdateFocusAnimation(focusQuotient, player, focusQuotient);
            base.Draw(spriteBatch);
        }
        
        private Vector2 CalculateCardPosition(int baseX, int baseY, int index)
        {
            var offsetY = index >= 10 ? FrameHeight : 0;
            var offsetX = index % 10 * FrameWidth;
            return new Vector2(baseX + offsetX, baseY + offsetY);
        }

        private void DrawFocusCards(SpriteBatch spriteBatch, int baseX, int baseY, int maxCards, int activeCards)
        {
            for (var i = 0; i < maxCards; i++)
            {
                var position = CalculateCardPosition(baseX, baseY, i);
                spriteBatch.Draw(_textureFocusOff.Value, position, Color.White);
            }

            for (var i = 0; i < activeCards; i++)
            {
                var position = CalculateCardPosition(baseX, baseY, i);
                spriteBatch.Draw(_textureFocusOn.Value, position, Color.White);
            }

            if (activeCards < maxCards){
                AnimateRemainingFocusCard(spriteBatch, baseX, baseY, activeCards);
            }
            
        }

        private void AnimateRemainingFocusCard(SpriteBatch spriteBatch, int baseX, int baseY, int activeCards)
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
                
                
            var remainderFocusPoints = player.FocusResourceCurrent % FocusCardValue;
            if (remainderFocusPoints <= 0) return;

            var percentageFull = (float)remainderFocusPoints / FocusCardValue;
            var alpha = (int)((255 * percentageFull) -40);
            var scale = Utils.Clamp(percentageFull, 0f, 1f);
            var lerpAlpha = (int) (Utils.Clamp(Utils.Lerp(alpha, 255,  (float) _focusAnimationTimer / (float)_ticksToRegenerateFullFocusCard ), 0, 255));

            var lerpScale = (float) (Utils.Clamp(Utils.Lerp(scale, 1f, (float) _focusAnimationTimer / (float)_ticksToRegenerateFullFocusCard ), 0, 1));
            
            var lerpScaleWithSin = (float) Math.Max(lerpScale, Math.Sin(((float)_focusAnimationTimer / _ticksToRegenerateFullFocusCard) * MathHelper.Pi));

        
            var position = CalculateCardPosition(baseX, baseY, activeCards);
            var centeredOrigin = CalculateOriginRemainingFocusCard(position);
            var centeredPosition = CalculateCenteredPositionRemainingFocusCard(position);
                
            spriteBatch.Draw(_textureFocusOn.Value,
                centeredPosition,
                null,
                new Color(255,
                    255,
                    255,
                    lerpAlpha),
                0f,
                centeredOrigin,
                //new Vector2(_textureFocusOn.Value.Width * lerpScale , _textureFocusOn.Value.Height * lerpScale) / 2f,
                //Vector2.Zero,
                scale,
                SpriteEffects.None,
                0);
        }
        
        private Vector2 CalculateCenteredPositionRemainingFocusCard(Vector2 originalPosition) =>
            new Vector2(originalPosition.X + FrameWidth / 2f, originalPosition.Y + FrameHeight / 2f);

        private Vector2 CalculateOriginRemainingFocusCard(Vector2 originalPosition)
        {
            var dummyRectangle = new Rectangle((int)originalPosition.X, (int)originalPosition.Y, FrameWidth, FrameHeight);
            
            return new Vector2(dummyRectangle.Width / 2f, dummyRectangle.Height / 2f);
        }

        private int CalculateStartTicks()
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
            var remainderFocusPoints = player.FocusResourceCurrent % FocusCardValue; // ex: 5
            var percentageRelativeToFocusCardValue = (float)remainderFocusPoints / FocusCardValue; // ex: 5 / 20 = 0.25
            if (remainderFocusPoints <= 0) return 0;
            
            // ex: 120 ticks to regenerate a quotient * 0.25 = 30 ticks
            return (int) (_ticksPerRegeneration * percentageRelativeToFocusCardValue); 
        }
        private void UpdateFocusAnimation(int currentQuotient, GlobalPlayer player, int activeCards)
        {
            if (_currentFocusCard != currentQuotient + 1)
                _focusAnimationTimer = CalculateStartTicks();
            if (_focusAnimationTimer >= _ticksToRegenerateFullFocusCard)
            {
                _focusAnimationTimer = 0;
            }

            _focusAnimationTimer++;
            _previousFocusPoints = player.FocusResourceCurrent;
            _currentFocusCard = activeCards + 1;
        }
        
        /*
        private float _timerHeartbeat;
    private const float TimerHeartbeatSpeed = 0.01f;
    private bool _reverseTimer;
    private UIImage _currentHeartbeatFocusCard;
    private int _amountOfTicksToFullyRegenerateAQuotient;
    private int _amountOfTicksToRegenerate;
    private int _timer;
    private int _previousQuotient = 0;
    private int _currentQuotient = 0;

    private int lastAmountOfFocus;

    public override void Draw(SpriteBatch spriteBatch)
    {
        var modPlayer = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
        _amountOfTicksToRegenerate = (int)(60 / modPlayer.FocusResourceRegenRate);
        var faltante = (modPlayer.FocusResourceCurrent % QuotientValue);
        var currentTickToRegenerate = (int) ((60 / modPlayer.FocusResourceRegenRate) * ((float)(QuotientValue - (modPlayer.FocusResourceCurrent % QuotientValue) ) / (float)modPlayer.FocusResourceRegenAmount));
        _amountOfTicksToFullyRegenerateAQuotient = (int) ((60 / modPlayer.FocusResourceRegenRate) * ((float)QuotientValue  / (float)modPlayer.FocusResourceRegenAmount));
        
        
        if (_lastCurrentFocus != modPlayer.FocusResourceCurrent)
            _timer = _amountOfTicksToFullyRegenerateAQuotient - currentTickToRegenerate;
        
        if (_timer >= _amountOfTicksToFullyRegenerateAQuotient || _currentQuotient != _previousQuotient) _timer = 0;
            
        

        _amountOfFocusCards = modPlayer.FocusResourceMax2 / QuotientValue;
        
        var uiWidth = Utils.Clamp(_amountOfFocusCards, 1, 10);

        var baseY = 19;
        var baseX = (int)(offsetPixelsPositionX + 1f * Main.screenWidth);


        var currentPixelsOffsetX = 0;
        var currentPixelsOffsetY = 0;

        for (var i = 0; i < _amountOfFocusCards; i++)
        {
            if (i == 10)
            {
                currentPixelsOffsetY = FrameHeight;
                currentPixelsOffsetX = 0;
            }

            var customAlpha = 220;
            var positionRectangle = new Rectangle(baseX + currentPixelsOffsetX, baseY + currentPixelsOffsetY, FrameWidth, FrameHeight);
            spriteBatch.Draw(_textureFocusOff.Value, positionRectangle, null, new Color(255, 255, 255, customAlpha), 0f,
                new Vector2(0, 0), SpriteEffects.None, 0);

            currentPixelsOffsetX += FrameWidth;
        }

        // drawing the on cards
        var currentPlayerFocus = modPlayer.FocusResourceCurrent;

        var amountOfCurrentFocusCards = currentPlayerFocus / QuotientValue;
        _currentQuotient = amountOfCurrentFocusCards - modPlayer.FocusResourceMax2 / QuotientValue;


        var remainderOfCurrentFocusCards = currentPlayerFocus % QuotientValue;
        var nextRemainderOfCurrentFocusCards = (currentPlayerFocus + modPlayer.FocusResourceRegenAmount) % QuotientValue;

        currentPixelsOffsetX = 0;
        currentPixelsOffsetY = 0;

        for (var i = 0; i < amountOfCurrentFocusCards; i++)
        {
            if (i == 10)
            {
                currentPixelsOffsetY = FrameHeight;
                currentPixelsOffsetX = 0;
            }

            var customAlpha = 255;
            var positionRectangle = new Rectangle(baseX + currentPixelsOffsetX, baseY + currentPixelsOffsetY, FrameWidth, FrameHeight);
            spriteBatch.Draw(_textureFocusOn.Value, positionRectangle, null, new Color(255, 255, 255, customAlpha), 0f,
                new Vector2(0, 0), SpriteEffects.None, 1);

            currentPixelsOffsetX += FrameWidth;
        }

        if (remainderOfCurrentFocusCards > 0)
        {
            
            _timer += 1;
            Main.NewText($"AmountOfTicksToFullyRegenerateAQuotient: {_amountOfTicksToFullyRegenerateAQuotient}");
            Main.NewText($"currentTickToRegenerate: {currentTickToRegenerate}");
            var percentageOfRemainder = (float)remainderOfCurrentFocusCards / QuotientValue;
            var nextPercentageOfRemainder = (float)nextRemainderOfCurrentFocusCards / QuotientValue;
            var customAlpha = (int)(255 * percentageOfRemainder);

            var nextAlpha = (int)Utils.Clamp(255 * nextPercentageOfRemainder, 0, 255);
            var customLerpAlpha = (int)Utils.Clamp(Utils.Lerp(customAlpha, 255,  (float) _timer / (float)_amountOfTicksToFullyRegenerateAQuotient ), 0, 255);
            
            Main.NewText($"CustomLerpAlpha: {customLerpAlpha}");
            Main.NewText($"calc: {(float) _timer / (float)_amountOfTicksToFullyRegenerateAQuotient}");

            var customScaleTemp = (float) (1f * percentageOfRemainder);
            var customScale = (float)Utils.Lerp(customScaleTemp, 1, (float) _timer / (float)_amountOfTicksToFullyRegenerateAQuotient );
            var customLerpScale = Utils.Clamp(customScale, 0, 1);
            
            Main.NewText($"CustomLerpScale: {customLerpScale}");
            
            Main.NewText($"timer: {_timer}");
            var positionRectangle = new Rectangle(baseX + currentPixelsOffsetX, baseY + currentPixelsOffsetY, FrameWidth, FrameHeight);
            var positionVector = new Vector2(baseX + currentPixelsOffsetX, baseY + currentPixelsOffsetY);
            spriteBatch.Draw(_textureFocusOn.Value, positionVector, null, new Color(255, 255, 255, customLerpAlpha), 0f,
                new Vector2(0,0), customLerpScale, SpriteEffects.None, 1f);
            _previousQuotient = _currentQuotient;
            _lastCurrentFocus = modPlayer.FocusResourceCurrent;
        }
        else
        {
            _currentQuotient = 0;
            _timer = 0;
        }

        base.Draw(spriteBatch);
    }
         
         */

        public override void Update(GameTime gameTime)
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
            var maxCards = player.FocusResourceMax2 / FocusCardValue;
            var rows = maxCards > 10 ? 2 : 1;

            _area.Width.Set(FrameWidth * Math.Min(maxCards, 10), 0f);
            _area.Height.Set(FrameHeight * rows, 0f);
            _barFrame.Width.Set(_area.Width.Pixels, 0f);
            _barFrame.Height.Set(_area.Height.Pixels, 0f);

            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();
            var hoverText = $"{FocusResourceUISystem.FocusResourceText.Format(player.FocusResourceCurrent, player.FocusResourceMax2)}";
            if (_area.IsMouseHovering) UICommon.TooltipMouseText(hoverText);
            base.DrawSelf(spriteBatch);
        }
    }
}

[Autoload(Side = ModSide.Client)]
internal class FocusResourceUISystem : ModSystem
{
    private UserInterface FocusResourceBarUserInterface;

    internal FocusResourceBar FocusResourceBar;

    public static LocalizedText FocusResourceText { get; set; }

    public override void Load()
    {
        FocusResourceBar = new FocusResourceBar();
        FocusResourceBarUserInterface = new UserInterface();
        FocusResourceBarUserInterface.SetState(FocusResourceBar);

        const string category = "UI";
        FocusResourceText ??= Mod.GetLocalization($"{category}.FocusResource");
    }

    public override void UpdateUI(GameTime gameTime)
    {
        FocusResourceBarUserInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (resourceBarIndex != -1)
            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer("MagicianClass: Focus Resource Bar", delegate
                {
                    FocusResourceBarUserInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }, InterfaceScaleType.UI
            ));
    }
}