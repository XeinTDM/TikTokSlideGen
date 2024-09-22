using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Threading;
using TemplateApp.Resources.Usercontrols;
using TemplateApp.Resources.Models;

namespace TemplateApp.Resources.Utilities
{
    public static class CommonUtils
    {
        private const int Width = 1080;
        private const int Height = 1920;
        private const double MaxTextWidth = Width - 100;
        public const double VerticalSpacing = 70;
        public const double LineThickness = 4;

        public static void BrowseFile(TextBox textBox, string filter, bool multiSelect = false)
        {
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                DefaultExt = Path.GetExtension(filter),
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

            string outputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_1.png");
            Task.Delay(1000);
            if (File.Exists(outputFilePath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{outputFilePath}\"",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show("No output files were generated.");
            }
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

        public static async void RenderPreviewImage(string text, string caption, string backgroundImagePath, string logoPath, double textFontSize, double captionFontSize, FontFamily fontFamily, double lineWidth, double verticalSpacing, double lineHeight, Image previewImageControl, int previewWidth, int previewHeight, Dispatcher dispatcher)
        {
            BitmapImage backgroundImage = null;

            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                backgroundImage = await ImageLoader.LoadImageAsync(backgroundImagePath);
            }

            RenderTargetBitmap renderBitmap = new(previewWidth, previewHeight, 96d, 96d, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, previewWidth, previewHeight));

                if (backgroundImage != null)
                {
                    DrawBackgroundImage(drawingContext, backgroundImage, previewWidth, previewHeight);
                }

                DrawLogo(drawingContext, logoPath, previewWidth, previewHeight);
                DrawTextAndLine(drawingContext, text, caption, textFontSize, captionFontSize, fontFamily, lineWidth, previewWidth, previewHeight, verticalSpacing, lineHeight);
            }

            renderBitmap.Render(drawingVisual);

            dispatcher.Invoke(() =>
            {
                previewImageControl.Source = renderBitmap;
            });
        }

        public static async void RenderImage(string text, string caption, string backgroundImagePath, string logoPath, int index, double textFontSize, double captionFontSize, FontFamily fontFamily, double lineWidth)
        {
            BitmapImage backgroundImage = null;

            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                backgroundImage = await ImageLoader.LoadImageAsync(backgroundImagePath);
            }

            RenderTargetBitmap renderBitmap = new(Width, Height, 96d, 96d, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, Width, Height));

                if (backgroundImage != null)
                {
                    DrawBackgroundImage(drawingContext, backgroundImage, Width, Height);
                }

                DrawLogo(drawingContext, logoPath, Width, Height);
                DrawTextAndLine(drawingContext, text, caption, textFontSize, captionFontSize, fontFamily, lineWidth, Width, Height, VerticalSpacing, LineThickness);
            }

            renderBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string outputFileName = $"output_{index}.png";
            using FileStream fileStream = new(outputFileName, FileMode.Create);
            encoder.Save(fileStream);
        }

        private static void DrawLogo(DrawingContext drawingContext, string logoPath, int canvasWidth, int canvasHeight)
        {
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath)) { return; }
            if (File.Exists(logoPath))
            {
                BitmapImage logo = new(new Uri(logoPath, UriKind.Absolute));
                double logoHeight = 80;
                double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;
                double logoWidth = logoHeight * aspectRatio;

                drawingContext.PushOpacity(0.4);
                drawingContext.DrawImage(logo, new Rect(canvasWidth - logoWidth - 10, 10, logoWidth, logoHeight));
                drawingContext.Pop();
            }
        }

        private static void DrawBackgroundImage(DrawingContext drawingContext, BitmapImage backgroundImage, int canvasWidth, int canvasHeight)
        {
            if (backgroundImage == null) { return; }
            double scaleX = canvasWidth / (double)backgroundImage.PixelWidth;
            double scaleY = canvasHeight / (double)backgroundImage.PixelHeight;
            double scale = Math.Max(scaleX, scaleY);
            double scaledWidth = backgroundImage.PixelWidth * scale;
            double scaledHeight = backgroundImage.PixelHeight * scale;
            double offsetX = (canvasWidth - scaledWidth) / 2;
            double offsetY = (canvasHeight - scaledHeight) / 2;

            drawingContext.PushOpacity(0.4);
            drawingContext.DrawImage(backgroundImage, new Rect(offsetX, offsetY, scaledWidth, scaledHeight));
            drawingContext.Pop();
        }

        public static void DrawTextAndLine(DrawingContext drawingContext, string text, string caption, double textFontSize, double captionFontSize, FontFamily fontFamily, double lineWidth, int canvasWidth, int canvasHeight, double verticalSpacing, double lineHeight)
        {
            double maxTextWidth = canvasWidth - 40;

            FormattedText formattedText = new(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Italic, FontWeights.Normal, FontStretches.Normal),
                textFontSize,
                Brushes.White,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip)
            {
                MaxTextWidth = maxTextWidth,
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
                MaxTextWidth = maxTextWidth,
                TextAlignment = TextAlignment.Center
            };

            double totalHeight = formattedText.Height + lineHeight + formattedCaption.Height + 2 * verticalSpacing;

            double startY = (canvasHeight - totalHeight) / 2;

            double textX = (canvasWidth - maxTextWidth) / 2;
            double captionX = (canvasWidth - maxTextWidth) / 2;
            double lineX = (canvasWidth - lineWidth) / 2;

            drawingContext.DrawText(formattedText, new Point(textX, startY));

            Pen whitePen = new(Brushes.White, lineHeight);
            drawingContext.DrawLine(whitePen, new Point(lineX, startY + formattedText.Height + verticalSpacing), new Point(lineX + lineWidth, startY + formattedText.Height + verticalSpacing));

            drawingContext.DrawText(formattedCaption, new Point(captionX, startY + formattedText.Height + verticalSpacing + lineHeight + verticalSpacing));
        }
    }
}