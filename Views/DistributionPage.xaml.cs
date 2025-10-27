using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DistribuicaoTurmas.Views
{
    public partial class DistributionPage : UserControl
    {
        private FrameworkElement _viewerContent; // fallback seguro

        public DistributionPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.DistributionViewModel();

            Loaded += (_, __) =>
            {
                // tenta pegar o campo gerado pelo x:Name
                if (_viewerContent == null)
                {
                    var byName = FindName("ViewerContent") as FrameworkElement;
                    _viewerContent = byName ?? FindDescendantByName<FrameworkElement>(this, "ViewerContent");
                }
            };
        }

        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var content = _viewerContent ?? FindName("ViewerContent") as FrameworkElement;
            if (content == null)
            {
                MessageBox.Show("Conteúdo não encontrado para exportação.", "Exportar PDF",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Garante layout medido/arranjado
            if (content.ActualWidth <= 0 || content.ActualHeight <= 0)
            {
                content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                content.Arrange(new Rect(content.DesiredSize));
                content.UpdateLayout();
            }

            var dlg = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Distribuicao_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                ExportVisualToPdf(content, dlg.FileName);
                MessageBox.Show("PDF exportado com sucesso.", "Exportar PDF",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao exportar PDF:\n" + ex.Message, "Exportar PDF",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exporta o elemento em páginas A4, com altura útil fixa por página.
        /// Sem páginas em branco/duplicadas.
        /// </summary>
        // using PdfSharp.Pdf;
        // using PdfSharp.Drawing;
        // using System.IO;
        // using System.Windows.Media;
        // using System.Windows.Media.Imaging;
        // using System.Collections.Generic;

        private void ExportVisualToPdf(FrameworkElement content, string outputPath)
        {
            // ===== Constantes =====
            const double DPI = 150.0;             // 300.0 -> mais nítido (arquivo maior)
            const double PT_PER_INCH = 72.0;
            const double DIP_PER_INCH = 96.0;
            const double MM_PER_INCH = 25.4;

            // A4
            const double A4_W_MM = 210.0;
            const double A4_H_MM = 297.0;

            // Margem: 2 cm em cada lado
            const double MARGIN_CM = 2.0;
            double marginPt = (MARGIN_CM * 10.0) / MM_PER_INCH * PT_PER_INCH; // cm->mm->in->pt

            // Página e área útil (pt)
            double pageWpt = (A4_W_MM / MM_PER_INCH) * PT_PER_INCH;
            double pageHpt = (A4_H_MM / MM_PER_INCH) * PT_PER_INCH;
            double usableWpt = pageWpt - 2 * marginPt;
            double usableHpt = pageHpt - 2 * marginPt;

            // Garantir layout do conteúdo
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            content.Arrange(new Rect(content.DesiredSize));
            content.UpdateLayout();

            double contentWdip = Math.Max(1.0, content.ActualWidth);
            double contentHdip = Math.Max(1.0, content.ActualHeight);

            // Conversores
            Func<double, double> DipToPt = dip => dip * (PT_PER_INCH / DIP_PER_INCH); // 0.75
            Func<double, double> DipToPx = dip => dip * (DPI / DIP_PER_INCH);
            Func<double, double> PxToDip = px => px * (DIP_PER_INCH / DPI);

            // Escala uniforme: só REDUZ (não estica)
            double contentWpt = DipToPt(contentWdip);
            double scale = Math.Min(1.0, usableWpt / contentWpt);

            // Altura útil por página (em px), considerando a escala final
            int slicePxMax = (int)Math.Floor(usableHpt * DPI / (PT_PER_INCH * scale));
            if (slicePxMax < 16) slicePxMax = 16;

            // ===== Coleta dos "fins de card" (em DIP) para quebrar somente entre cards =====
            var safeBreaksDip = new List<double>();
            CollectCardBreaks(content, safeBreaksDip); // precisa de Tag="Card" no Border do DataTemplate
            safeBreaksDip.Sort();

            // Tamanhos totais do conteúdo em px
            int totalPxHeight = (int)Math.Ceiling(DipToPx(contentHdip));
            int bmpW = (int)Math.Ceiling(DipToPx(contentWdip));
            if (bmpW <= 0 || totalPxHeight <= 0) return;

            // Cursor em pixels
            int cursorPx = 0;

            var pdf = new PdfDocument();
            pdf.Info.Title = "Distribuição de Turmas";

            while (cursorPx < totalPxHeight)
            {
                int remainingPx = totalPxHeight - cursorPx;
                if (remainingPx < 8) break;

                // alvo bruto pela área útil
                int desiredSlicePx = Math.Min(slicePxMax, remainingPx);

                // ==== ajuste para quebrar no fim do card ====
                double cursorDip = PxToDip(cursorPx);
                double targetDip = PxToDip(cursorPx + desiredSlicePx);

                const double guardDip = 1.0; // 1 DIP de folga
                double bestBreakDip = -1.0;
                for (int i = 0; i < safeBreaksDip.Count; i++)
                {
                    double pos = safeBreaksDip[i];
                    if (pos > cursorDip + guardDip && pos <= targetDip - guardDip)
                        bestBreakDip = pos;
                    if (pos > targetDip) break;
                }

                // fatia desta página (em px)
                int thisSlicePx;
                int nextCursorPx;

                if (bestBreakDip > 0)
                {
                    // avança EXATAMENTE até o fim do card (arredondado p/ px)
                    nextCursorPx = (int)Math.Round(DipToPx(bestBreakDip));
                    thisSlicePx = Math.Max(1, nextCursorPx - cursorPx);
                }
                else
                {
                    // nenhum card inteiro coube -> fatia bruta (evita travar com card gigante)
                    thisSlicePx = desiredSlicePx;
                    nextCursorPx = cursorPx + thisSlicePx;
                }

                if (thisSlicePx < 16) break;

                // ==== Render da fatia usando Viewbox absoluto, com overscan anti-costura ====
                var rtb = new RenderTargetBitmap(bmpW, thisSlicePx, DPI, DPI, PixelFormats.Pbgra32);

                double yStartDip = PxToDip(cursorPx);
                double sliceHdip = PxToDip(thisSlicePx);
                const double overscanDip = 0.5; // cobre antialias na borda inferior

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    var vb = new VisualBrush(content)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, yStartDip, contentWdip, sliceHdip + overscanDip),
                        Stretch = Stretch.Fill
                    };
                    dc.DrawRectangle(vb, null, new Rect(0, 0, contentWdip, sliceHdip + overscanDip));
                }
                rtb.Render(dv);

                // ==== Grava a página ====
                // Converte o bitmap em PNG na memória
                byte[] pngBytes;
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                using (var ms = new MemoryStream())
                {
                    enc.Save(ms);
                    pngBytes = ms.ToArray();
                }

                // Cria a página A4
                var page = pdf.AddPage();
                page.Width = pageWpt;
                page.Height = pageHpt;

                using (var gfx = XGraphics.FromPdfPage(page))
                using (var msImg = new MemoryStream(pngBytes))
                using (var img = XImage.FromStream(msImg))
                {
                    // tamanho natural da imagem (pt)
                    double imgWpt = rtb.PixelWidth / DPI * PT_PER_INCH;
                    double imgHpt = rtb.PixelHeight / DPI * PT_PER_INCH;

                    // aplica a mesma escala (<=1), mantendo proporção
                    double drawW = imgWpt * scale;
                    double drawH = imgHpt * scale;

                    // clamps de segurança
                    if (drawW > usableWpt) { double f = usableWpt / drawW; drawW *= f; drawH *= f; }
                    if (drawH > usableHpt) { double f = usableHpt / drawH; drawW *= f; drawH *= f; }

                    // alinhado à esquerda/topo com margem de 2 cm
                    gfx.DrawImage(img, marginPt, marginPt, drawW, drawH);
                }

                // avança cursor exatamente até o fim desta página (garante +1px mínimo)
                cursorPx = Math.Max(nextCursorPx, cursorPx + 1);
            }

            pdf.Save(outputPath);
        }

        /// <summary>
        /// Coleta os "fins" dos cards (Bottom em DIPs) marcados com Tag="Card".
        /// </summary>
        private static void CollectCardBreaks(FrameworkElement root, List<double> breaksDip)
        {
            if (root == null) return;

            Traverse(root, root, breaksDip);
            breaksDip.Sort();

            // remove duplicados muito próximos (ruído)
            for (int i = breaksDip.Count - 2; i >= 0; i--)
                if (Math.Abs(breaksDip[i] - breaksDip[i + 1]) < 0.5)
                    breaksDip.RemoveAt(i);

            // ---- walker local
            void Traverse(FrameworkElement current, FrameworkElement ancestor, List<double> list)
            {
                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i) as FrameworkElement;
                    if (child == null) continue;

                    if (Equals(child.Tag, "Card"))
                    {
                        try
                        {
                            GeneralTransform gt = child.TransformToVisual(ancestor);
                            Rect r = gt.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
                            list.Add(r.Bottom);
                        }
                        catch { /* ignora elementos instáveis */ }
                    }

                    Traverse(child, ancestor, list);
                }
            }
        }


        // Utilitário: busca descendente por nome
        private static T FindDescendantByName<T>(DependencyObject root, string name) where T : FrameworkElement
        {
            if (root == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                T fe = child as T;
                if (fe != null && fe.Name == name) return fe;

                var result = FindDescendantByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
