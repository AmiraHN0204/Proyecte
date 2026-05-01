using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ReVita
{
    public class EstiloMenu : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => ColorTranslator.FromHtml("#2F6F5E");
        public override Color MenuStripGradientEnd => ColorTranslator.FromHtml("#2F6F5E");
        public override Color MenuItemSelected => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuItemBorder => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuBorder => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#1F4D40");
        public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#1F4D40");
    }

    // Usaremos un solo Renderer que incluye el Fade y limpia los bordes
    public class FadeRenderer : ToolStripProfessionalRenderer
    {
        private Dictionary<ToolStripItem, float> animacion = new Dictionary<ToolStripItem, float>();
        private Timer timer;

        private static readonly Color COLOR_NORMAL = ColorTranslator.FromHtml("#2F6F5E");
        private static readonly Color COLOR_HOVER = ColorTranslator.FromHtml("#3F8A74");

        public FadeRenderer() : base(new EstiloMenu())
        {
            timer = new Timer();
            timer.Interval = 10; // Más rápido y fluido
            timer.Tick += Animar;
            timer.Start();
        }

        private void Animar(object sender, EventArgs e)
        {
            var itemsActivos = new List<ToolStripItem>(animacion.Keys);

            foreach (var item in itemsActivos)
            {
                // Evitamos animar el logo principal (Asegúrate que se llame así en tu diseño)
                if (item.Name == "iNICIOToolStripMenuItem") continue;

                float valor = animacion[item];
                float velocidad = 0.1f; // Ajuste de suavidad

                if (item.Selected)
                    valor += velocidad;
                else
                    valor -= velocidad;

                valor = Math.Max(0f, Math.Min(1f, valor));

                if (animacion[item] != valor)
                {
                    animacion[item] = valor;
                    // SOLUCIÓN AL LOGO CORTADO: Usar item.Bounds en lugar de Point.Empty
                    item.Owner?.Invalidate(item.Bounds);
                }
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            // Si es el logo principal, solo pintamos el fondo base y salimos
            if (e.Item.Name == "iNICIOToolStripMenuItem")
            {
                using (SolidBrush brush = new SolidBrush(COLOR_NORMAL))
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                return;
            }

            if (!animacion.ContainsKey(e.Item))
                animacion[e.Item] = 0f;

            float t = animacion[e.Item];
            Color final = Mezclar(COLOR_NORMAL, COLOR_HOVER, t);

            // Pintamos el fondo completo del ítem
            using (SolidBrush brush = new SolidBrush(final))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
            }
        }

        // Dibuja el fondo general limpio
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(COLOR_NORMAL))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null) return;
            e.Graphics.DrawImage(e.Image, e.ImageRectangle);
        }

        // SOLUCIÓN A LOS COLORES EXTRAÑOS: Forzar canal Alpha a 255
        private Color Mezclar(Color c1, Color c2, float t)
        {
            int r = (int)(c1.R + (c2.R - c1.R) * t);
            int g = (int)(c1.G + (c2.G - c1.G) * t);
            int b = (int)(c1.B + (c2.B - c1.B) * t);
            return Color.FromArgb(255, r, g, b);
        }
    }
}