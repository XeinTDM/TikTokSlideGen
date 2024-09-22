using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TemplateApp.Resources.Models
{
    public static class ImageLoader
    {
        private static Dictionary<string, WeakReference<BitmapImage>> _imageCache = new();

        public static async Task<BitmapImage> LoadImageAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return await Task.FromResult(CreateBlackBackground());
            }

            if (_imageCache.TryGetValue(path, out var weakRef) && weakRef.TryGetTarget(out var cachedImage))
            {
                return cachedImage;
            }

            byte[] imageData = await Task.Run(() => File.ReadAllBytes(path));

            var bitmap = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var bmp = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = stream;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                return bmp;
            });

            _imageCache[path] = new WeakReference<BitmapImage>(bitmap);
            return bitmap;
        }

        private static BitmapImage CreateBlackBackground()
        {
            var blackBackground = new RenderTargetBitmap(1080, 1920, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 1080, 1920));
            }

            blackBackground.Render(drawingVisual);

            var bitmapImage = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(blackBackground));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }
    }
}
