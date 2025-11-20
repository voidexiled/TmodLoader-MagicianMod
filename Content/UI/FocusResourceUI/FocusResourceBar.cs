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
        public const int FocusCardValue = 20;
        public const int FrameWidth = 32;
        public const int FrameHeight = 48;

        private const string TexturePathFocusOff = "MagicianClass/Content/UI/FocusResourceUI/FocusCardOff";
        private const string TexturePathFocusOn = "MagicianClass/Content/UI/FocusResourceUI/FocusCardOn";

        public const int OffsetX = -400 - 10 * FrameWidth;
        private UIElement _area;
        private UIElement _barFrame;
        private Asset<Texture2D> _textureFocusOff;
        private Asset<Texture2D> _textureFocusOn;


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

            var focusQuotient = player.FocusResourceCurrent / FocusCardValue;
            var maxFocusQuotients = player.FocusResourceMax2 / FocusCardValue;

            var screenOffsetX = (int)(OffsetX + 1f * Main.screenWidth);
            var screenOffsetY = 19;

            DrawFocusCards(spriteBatch, screenOffsetX, screenOffsetY, maxFocusQuotients, focusQuotient);

            base.Draw(spriteBatch);
        }

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
            var hoverText =
                $"{FocusResourceUISystem.FocusResourceText.Format(player.FocusResourceCurrent, player.FocusResourceMax2)}";
            if (_area.IsMouseHovering) UICommon.TooltipMouseText(hoverText);
            base.DrawSelf(spriteBatch);
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

            if (activeCards < maxCards) AnimateRemainingFocusCard(spriteBatch, baseX, baseY, activeCards);
        }

        private void AnimateRemainingFocusCard(SpriteBatch spriteBatch, int baseX, int baseY, int activeCards)
        {
            var player = Main.LocalPlayer.GetModPlayer<GlobalPlayer>();

            var focusCurrent = player.FocusResourceCurrent;
            var focusMax = player.FocusResourceMax2;

            // Si ya está al máximo, no hay carta parcial que animar
            if (focusCurrent >= focusMax)
                return;

            var focusPerCard = FocusCardValue;

            // Cuánto focus hay "lógico" dentro de la carta actual
            var focusInCurrentCard = focusCurrent % focusPerCard;

            // Fill lógico [0..1]
            var logicalFill = (float)focusInCurrentCard / focusPerCard;

            // ---------- SUAVIZADO CON EL TIMER DE REGEN (tipo maná Terraria) ----------
            var smoothFill = logicalFill;

            if (player.FocusResourceRegenRate > 0f)
            {
                // Debe coincidir con UpdateFocusResource
                var ticksPerStep = 15f / player.FocusResourceRegenRate;

                if (ticksPerStep > 0f)
                {
                    // Progreso [0..1] dentro del siguiente “tick” de regeneración
                    var stepProgress = Utils.Clamp(player.FocusResourceRegenTimer / ticksPerStep, 0f, 1f);

                    // Añadimos la parte “virtual” que se está rellenando pero aún no se ha sumado al recurso
                    smoothFill += stepProgress * (player.FocusResourceRegenAmount / (float)focusPerCard);
                }
            }

            smoothFill = Utils.Clamp(smoothFill, 0f, 1f);

            // Easing suave tipo smoothstep para que no sea lineal
            var easedFill = smoothFill * smoothFill * (3f - 2f * smoothFill);

            // ---------- ESCALA SEGÚN FILL ----------
            // 0 focus  -> carta pequeña
            // 100%     -> carta a tamaño normal
            const float minScale = 0.12f;
            const float maxScale = 1.0f;
            var baseScale = MathHelper.Lerp(minScale, maxScale, easedFill);

            // ---------- BREATH / HEARTBEAT ----------
            var breath = 0f;
            if (focusCurrent < focusMax)
            {
                // Velocidad del pulso (ajustable)
                const float breathSpeed = 1.2f; // breaths x por segundo
                breath = (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * breathSpeed);
            }

            const float breathAmplitude = 0.01f; // 1% de la escala base
            var finalScale = baseScale * (1f + breathAmplitude * breath);

            // Seguridad por si acaso
            finalScale = Utils.Clamp(finalScale, minScale * 0.8f, maxScale * 1.2f);

            // ---------- ALPHA SEGÚN FILL ----------
            var alpha = (byte)MathHelper.Lerp(30f, 255f, easedFill);

            // ---------- DIBUJO CENTRADO ----------
            var position = CalculateCardPosition(baseX, baseY, activeCards);
            var centeredOrigin = CalculateOriginRemainingFocusCard(position);
            var centeredPosition = CalculateCenteredPositionRemainingFocusCard(position);

            spriteBatch.Draw(
                _textureFocusOn.Value,
                centeredPosition,
                null,
                new Color(255, 255, 255, alpha),
                0f,
                centeredOrigin,
                finalScale,
                SpriteEffects.None,
                0f
            );
        }

        private Vector2 CalculateCenteredPositionRemainingFocusCard(Vector2 originalPosition)
        {
            return new Vector2(originalPosition.X + FrameWidth / 2f, originalPosition.Y + FrameHeight / 2f);
        }

        private Vector2 CalculateOriginRemainingFocusCard(Vector2 originalPosition)
        {
            var dummyRectangle =
                new Rectangle((int)originalPosition.X, (int)originalPosition.Y, FrameWidth, FrameHeight);

            return new Vector2(dummyRectangle.Width / 2f, dummyRectangle.Height / 2f);
        }
    }
}

[Autoload(Side = ModSide.Client)]
internal class FocusResourceUISystem : ModSystem
{
    internal FocusResourceBar FocusResourceBar;
    private UserInterface FocusResourceBarUserInterface;

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