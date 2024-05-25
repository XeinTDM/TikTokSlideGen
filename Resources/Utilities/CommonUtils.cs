using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TemplateApp.Utilities
{
    public static class CommonUtils
    {
        private const int Width = 1080;
        private const int Height = 1920;
        private const double MaxTextWidth = Width - 100;
        private const double VerticalSpacing = 70;
        private const double LineThickness = 4;

        public static void BrowseFile(TextBox textBox, string filter, bool multiSelect = false)
        {
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                DefaultExt = System.IO.Path.GetExtension(filter),
                Filter = filter,
                Multiselect = multiSelect
            };

            if (dlg.ShowDialog() == true)
            {
                if (filter.Contains("Text Files"))
                {
                    textBox.Text = File.ReadAllText(dlg.FileName);
                }
                else
                {
                    textBox.Text = multiSelect ? string.Join(";", dlg.FileNames) : dlg.FileName;
                }
            }
        }

        public static void GenerateImages(string logoPath, string[] backgroundImagePaths, string[] texts, string[] captions, Action<string, string, string, int> renderImage, bool isTrivia = false)
        {
            int numTexts = texts.Length;
            int numImages = backgroundImagePaths.Length;
            int numCaptions = captions.Length;

            if (numImages == 0 || numCaptions == 0)
            {
                MessageBox.Show("Please select at least one background image and one caption.");
                return;
            }

            backgroundImagePaths = ExpandArray(backgroundImagePaths, numTexts);
            captions = ExpandArray(captions, numTexts);

            for (int i = 0; i < numTexts; i++)
            {
                string text = texts[i];
                string caption = captions[i % numCaptions];
                string backgroundImagePath = backgroundImagePaths[i % numImages];

                renderImage(text, caption, backgroundImagePath, i + 1);
            }

            System.Diagnostics.Process.Start("explorer.exe", "/select,\"output_1.png\"");
        }

        private static string[] ExpandArray(string[] array, int length)
        {
            return array.Length == 1 ? Enumerable.Repeat(array[0], length).ToArray() : array;
        }

        public static void GenerateTriviaImages(string logoPath, string[] backgroundImagePaths, string[] questions, string[] answers, string caption, Action<string, string, string, int> renderImage)
        {
            string[] combined = questions.Select((q, i) => new { q, a = answers[i] })
                                          .SelectMany(x => new[] { x.q, x.a })
                                          .ToArray();

            GenerateImages(logoPath, backgroundImagePaths, combined, Enumerable.Repeat(caption, combined.Length).ToArray(), renderImage, true);
        }

        public static void RenderImage(string text, string caption, string backgroundImagePath, string logoPath, int index, double textFontSize, double captionFontSize, FontFamily fontFamily, double lineWidth)
        {
            RenderTargetBitmap renderBitmap = new(Width, Height, 96d, 96d, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, Width, Height));
                DrawBackgroundImage(drawingContext, backgroundImagePath);
                DrawLogo(drawingContext, logoPath);
                DrawTextAndLine(drawingContext, text, caption, textFontSize, captionFontSize, fontFamily, lineWidth);
            }

            renderBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string outputFileName = $"output_{index}.png";
            using FileStream fileStream = new(outputFileName, FileMode.Create);
            encoder.Save(fileStream);
        }

        private static void DrawLogo(DrawingContext drawingContext, string logoPath)
        {
            if (File.Exists(logoPath))
            {
                BitmapImage logo = new(new Uri(logoPath, UriKind.Absolute));
                double logoHeight = 80;
                double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;
                double logoWidth = logoHeight * aspectRatio;

                drawingContext.PushOpacity(0.4);
                drawingContext.DrawImage(logo, new Rect(Width - logoWidth - 10, 10, logoWidth, logoHeight));
                drawingContext.Pop();
            }
        }

        private static void DrawBackgroundImage(DrawingContext drawingContext, string backgroundImagePath)
        {
            if (File.Exists(backgroundImagePath))
            {
                BitmapImage bitmap = new(new Uri(backgroundImagePath, UriKind.Absolute));
                double scaleX = Width / (double)bitmap.PixelWidth;
                double scaleY = Height / (double)bitmap.PixelHeight;
                double scale = Math.Max(scaleX, scaleY);
                double scaledWidth = bitmap.PixelWidth * scale;
                double scaledHeight = bitmap.PixelHeight * scale;
                double offsetX = (Width - scaledWidth) / 2;
                double offsetY = (Height - scaledHeight) / 2;

                drawingContext.PushOpacity(0.4);
                drawingContext.DrawImage(bitmap, new Rect(offsetX, offsetY, scaledWidth, scaledHeight));
                drawingContext.Pop();
            }
        }

        private static void DrawTextAndLine(DrawingContext drawingContext, string text, string caption, double textFontSize, double captionFontSize, FontFamily fontFamily, double lineWidth)
        {
            FormattedText formattedText = new(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Italic, FontWeights.Normal, FontStretches.Normal),
                textFontSize,
                Brushes.White,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip)
            {
                MaxTextWidth = MaxTextWidth,
                TextAlignment = TextAlignment.Center
            };

            FormattedText formattedCaption = new(
                caption,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Italic, FontWeights.Normal, FontStretches.Normal),
                captionFontSize,
                Brushes.White,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip)
            {
                MaxTextWidth = MaxTextWidth,
                TextAlignment = TextAlignment.Center
            };

            double totalHeight = formattedText.Height + LineThickness + formattedCaption.Height + 2 * VerticalSpacing;
            double startY = (Height - totalHeight) / 2;
            double textX = (Width - MaxTextWidth) / 2;
            double captionX = (Width - MaxTextWidth) / 2;
            double lineX = (Width - lineWidth) / 2;

            drawingContext.DrawText(formattedText, new Point(textX, startY));

            Pen whitePen = new(Brushes.White, LineThickness);
            drawingContext.DrawLine(whitePen, new Point(lineX, startY + formattedText.Height + VerticalSpacing), new Point(lineX + lineWidth, startY + formattedText.Height + VerticalSpacing));

            drawingContext.DrawText(formattedCaption, new Point(captionX, startY + formattedText.Height + VerticalSpacing + LineThickness + VerticalSpacing));
        }
    }
}