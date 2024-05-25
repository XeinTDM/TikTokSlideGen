using System.Windows;
using System.Windows.Media;
using TemplateApp.Utilities;
using System.Windows.Controls;

namespace TemplateApp
{
    public partial class QuotesUC : UserControl
    {
        private readonly FontFamily _philosopherItalic;
        private const double QuoteFontSize = 62;
        private const double CaptionFontSize = 37;
        private const double LineWidth = 475;

        public QuotesUC()
        {
            InitializeComponent();
            _philosopherItalic = new FontFamily(new Uri("file:///C:/USERS/PRETTYJACKY/APPDATA/LOCAL/MICROSOFT/WINDOWS/FONTS/PHILOSOPHER-ITALIC.TTF"), "Philosopher Italic");
        }

        private void BrowseLogoFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(LogoPath, "Image Files|*.jpg;*.jpeg;*.png");
        }

        private void BrowseBackgroundImages_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(BackgroundImagePaths, "Image Files|*.jpg;*.jpeg;*.png", true);
        }

        private void BrowseQuotesFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(QuotesContent, "Text Files|*.txt");
        }

        private void BrowseCaptionsFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(CaptionsContent, "Text Files|*.txt");
        }

        private void GenerateImages_Click(object sender, RoutedEventArgs e)
        {
            string[] backgroundImagePaths = BackgroundImagePaths.Text.Split(';');
            string[] quotes = QuotesContent.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string[] captions = CaptionsContent.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            CommonUtils.GenerateImages(LogoPath.Text, backgroundImagePaths, quotes, captions, RenderImage);
        }

        private void RenderImage(string quote, string caption, string backgroundImagePath, int index)
        {
            CommonUtils.RenderImage(quote, caption, backgroundImagePath, LogoPath.Text, index, QuoteFontSize, CaptionFontSize, _philosopherItalic, LineWidth);
        }
    }
}