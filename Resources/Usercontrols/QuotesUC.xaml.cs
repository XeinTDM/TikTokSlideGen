using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using TemplateApp.Resources.Models;
using TemplateApp.Resources.Utilities;

namespace TemplateApp.Resources.Usercontrols
{
    public partial class QuotesUC : UserControl
    {
        private readonly FontFamily _philosopherItalic;
        private const double CaptionFontSize = 37;
        private const double QuoteFontSize = 62;
        private const double LineWidth = 475;

        private List<QuoteItem> _quoteItemsIntro = [];
        private int _currentIndexIntro = 0;

        private List<QuoteItem> _quoteItemsMain = [];
        private int _currentIndexMain = 0;

        private readonly DispatcherTimer _debounceTimer;
        private string _activeTab = string.Empty;

        public QuotesUC()
        {
            InitializeComponent();
            _philosopherItalic = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Philosopher");

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;

            SetUpTextChangedHandlers();
        }

        private void SetUpTextChangedHandlers()
        {
            QuotesContent.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Intro");
            CaptionsContent.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Intro");
            BackgroundImagePaths.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Intro");

            QuotesContentMain.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Main");
            CaptionsContentMain.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Main");
            BackgroundImagePathsMain.TextChanged += (s, e) => DebounceUpdatePreviewForTab("Main");
        }

        private void DebounceUpdatePreviewForTab(string tabName)
        {
            _activeTab = tabName;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            UpdatePreviewForTab(
                _activeTab == "Intro" ? LogoPath : LogoPathMain,
                _activeTab == "Intro" ? BackgroundImagePaths : BackgroundImagePathsMain,
                _activeTab == "Intro" ? QuotesContent : QuotesContentMain,
                _activeTab == "Intro" ? CaptionsContent : CaptionsContentMain,
                _activeTab == "Intro" ? PreviewImage : PreviewImageMain,
                _activeTab == "Intro" ? PreviewCountTextBlock : PreviewCountTextBlockMain,
                _activeTab == "Intro" ? LeftArrowButton : LeftArrowButtonMain,
                _activeTab == "Intro" ? RightArrowButton : RightArrowButtonMain,
                ref (_activeTab == "Intro" ? ref _quoteItemsIntro : ref _quoteItemsMain),
                ref (_activeTab == "Intro" ? ref _currentIndexIntro : ref _currentIndexMain)
            );
        }

        private void BrowseLogoFile(TextBox logoPathTextBox)
        {
            CommonUtils.BrowseFile(logoPathTextBox, "Image Files|*.jpg;*.jpeg;*.png");
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e, TextBox targetTextBox, string filter, bool multiSelect = false)
        {
            CommonUtils.BrowseFile(targetTextBox, filter, multiSelect);
        }

        private void BrowseLogoFile_Click(object sender, RoutedEventArgs e) => BrowseFile_Click(sender, e, LogoPath, "Image Files|*.jpg;*.jpeg;*.png");
        private void BrowseLogoFileMain_Click(object sender, RoutedEventArgs e) => BrowseFile_Click(sender, e, LogoPathMain, "Image Files|*.jpg;*.jpeg;*.png");

        private void BrowseBackgroundImages_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(BackgroundImagePaths, "Image Files|*.jpg;*.jpeg;*.png", true);
            DebounceUpdatePreviewForTab("Intro");
        }

        private void BrowseBackgroundImagesMain_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(BackgroundImagePathsMain, "Image Files|*.jpg;*.jpeg;*.png", true);
            DebounceUpdatePreviewForTab("Main");
        }

        private void BrowseQuotesFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(QuotesContent, "Text Files|*.txt");
            DebounceUpdatePreviewForTab("Intro");
        }

        private void BrowseQuotesFileMain_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(QuotesContentMain, "Text Files|*.txt");
            DebounceUpdatePreviewForTab("Main");
        }

        private void BrowseCaptionsFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(CaptionsContent, "Text Files|*.txt");
            DebounceUpdatePreviewForTab("Intro");
        }

        private void BrowseCaptionsFileMain_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(CaptionsContentMain, "Text Files|*.txt");
            DebounceUpdatePreviewForTab("Main");
        }

        private void GenerateImages_Click(object sender, RoutedEventArgs e)
        {
            GenerateImagesForTab(LogoPath, _quoteItemsIntro);
        }

        private void GenerateImagesMain_Click(object sender, RoutedEventArgs e)
        {
            GenerateImagesForTab(LogoPathMain, _quoteItemsMain);
        }

        private void NavigatePreview_Click(object sender, RoutedEventArgs e, int direction, ref int currentIndex, List<QuoteItem> quoteItems, TextBox logoPath, Image previewImage, TextBlock previewCountTextBlock, Button leftArrowButton, Button rightArrowButton)
        {
            NavigatePreview(direction, ref currentIndex, quoteItems, logoPath, previewImage, previewCountTextBlock, leftArrowButton, rightArrowButton);
        }

        private void LeftArrowButton_Click(object sender, RoutedEventArgs e) => NavigatePreview_Click(sender, e, -1, ref _currentIndexIntro, _quoteItemsIntro, LogoPath, PreviewImage, PreviewCountTextBlock, LeftArrowButton, RightArrowButton);
        private void RightArrowButton_Click(object sender, RoutedEventArgs e) => NavigatePreview_Click(sender, e, 1, ref _currentIndexIntro, _quoteItemsIntro, LogoPath, PreviewImage, PreviewCountTextBlock, LeftArrowButton, RightArrowButton);

        private void LeftArrowButtonMain_Click(object sender, RoutedEventArgs e)
        {
            NavigatePreview(-1, ref _currentIndexMain, _quoteItemsMain, LogoPathMain, PreviewImageMain, PreviewCountTextBlockMain, LeftArrowButtonMain, RightArrowButtonMain);
        }

        private void RightArrowButtonMain_Click(object sender, RoutedEventArgs e)
        {
            NavigatePreview(1, ref _currentIndexMain, _quoteItemsMain, LogoPathMain, PreviewImageMain, PreviewCountTextBlockMain, LeftArrowButtonMain, RightArrowButtonMain);
        }

        private void UpdatePreviewForTab(TextBox logoPath, TextBox backgroundPaths, TextBox quotesContent, TextBox captionsContent, Image previewImage, TextBlock previewCountTextBlock, Button leftArrowButton, Button rightArrowButton, ref List<QuoteItem> quoteItems, ref int currentIndex)
        {
            var quotes = string.IsNullOrWhiteSpace(quotesContent.Text)
                ? new string[] { "Default Quote" }
                : quotesContent.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var captions = string.IsNullOrWhiteSpace(captionsContent.Text)
                ? new string[] { "Default Caption" }
                : captionsContent.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var backgrounds = string.IsNullOrWhiteSpace(backgroundPaths.Text)
                ? new string[] { null }
                : backgroundPaths.Text.Split(';');

            quoteItems.Clear();

            for (int i = 0; i < Math.Max(quotes.Length, Math.Max(captions.Length, backgrounds.Length)); i++)
            {
                string quote = i < quotes.Length ? quotes[i] : "Default Quote";
                string caption = i < captions.Length ? captions[i] : "Default Caption";
                string background = i < backgrounds.Length ? backgrounds[i] : null;

                quoteItems.Add(new QuoteItem(quote, caption, background));
            }

            currentIndex = 0;
            UpdateNavigationButtonsForTab(currentIndex, quoteItems.Count, leftArrowButton, rightArrowButton);
            UpdatePreviewBasedOnCurrentIndexForTab(logoPath, previewImage, previewCountTextBlock, currentIndex, quoteItems);
        }

        private void UpdateNavigationButtonsForTab(int currentIndex, int itemCount, Button leftArrowButton, Button rightArrowButton)
        {
            leftArrowButton.IsEnabled = currentIndex > 0;
            rightArrowButton.IsEnabled = currentIndex < itemCount - 1;
        }

        private void UpdatePreviewBasedOnCurrentIndexForTab(TextBox logoPath, Image previewImage, TextBlock previewCountTextBlock, int currentIndex, List<QuoteItem> quoteItems)
        {
            var currentQuoteItem = quoteItems[currentIndex];
            RenderPreviewForTab(currentQuoteItem.Quote, currentQuoteItem.Caption, currentQuoteItem.BackgroundImagePath, logoPath, previewImage);
            UpdatePreviewCountForTab(previewCountTextBlock, currentIndex, quoteItems.Count);
        }

        private void UpdatePreviewCountForTab(TextBlock previewCountTextBlock, int currentIndex, int itemCount)
        {
            previewCountTextBlock.Text = $"{currentIndex + 1} of {itemCount}";
        }

        private async Task RenderContent(string quote, string caption, string backgroundImagePath, TextBox logoPath, Image previewImage, bool isPreview, int index = 0)
        {
            BitmapImage backgroundImage = !string.IsNullOrEmpty(backgroundImagePath)
                ? await ImageLoader.LoadImageAsync(backgroundImagePath)
                : null;

            if (isPreview)
            {
                Dispatcher.Invoke(() => {
                    CommonUtils.RenderPreviewImage(quote, caption, backgroundImagePath, logoPath.Text, QuoteFontSize * 0.24, CaptionFontSize * 0.25, _philosopherItalic, LineWidth * 0.25, CommonUtils.VerticalSpacing * 0.25, CommonUtils.LineThickness * 0.25, previewImage, 270, 480, Dispatcher);
                });
            }
            else
            {
                CommonUtils.RenderImage(quote, caption, backgroundImagePath, logoPath?.Text, index, QuoteFontSize, CaptionFontSize, _philosopherItalic, LineWidth);
            }
        }

        private void RenderPreviewForTab(string quote, string caption, string backgroundImagePath, TextBox logoPath, Image previewImage)
        {
            _ = RenderContent(quote, caption, backgroundImagePath, logoPath, previewImage, true);
        }

        private void RenderImage(string quote, string caption, string backgroundImagePath, int index)
        {
            _ = RenderContent(quote, caption, backgroundImagePath, null, null, false, index);
        }

        private void RenderBlackBackgroundPreviewForTab(string quote, string caption, double scaledQuoteFontSize, double scaledCaptionFontSize, double scaledLineWidth, double scaledVerticalSpacing, double scaledLineHeight, Image previewImage)
        {
            RenderTargetBitmap renderBitmap = new(270, 480, 96d, 96d, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 270, 480));
                CommonUtils.DrawTextAndLine(drawingContext, quote, caption, scaledQuoteFontSize, scaledCaptionFontSize, _philosopherItalic, scaledLineWidth, 270, 480, scaledVerticalSpacing, scaledLineHeight);
            }

            renderBitmap.Render(drawingVisual);
            Dispatcher.Invoke(() =>
            {
                previewImage.Source = renderBitmap;
            });
        }

        private void GenerateImagesForTab(TextBox logoPath, List<QuoteItem> quoteItems)
        {
            var backgroundImagePaths = quoteItems.Select(item => item.BackgroundImagePath).ToArray();
            var quotes = quoteItems.Select(item => item.Quote).ToArray();
            var captions = quoteItems.Select(item => item.Caption).ToArray();

            if (backgroundImagePaths.Length == 0 || captions.Length == 0)
            {
                MessageBox.Show("Please select at least one background image and one caption.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CommonUtils.GenerateImages(logoPath.Text, backgroundImagePaths, quotes, captions, RenderImage);
        }

        private void NavigatePreview(int direction, ref int currentIndex, List<QuoteItem> quoteItems, TextBox logoPath, Image previewImage, TextBlock previewCountTextBlock, Button leftArrowButton, Button rightArrowButton)
        {
            currentIndex += direction;
            if (currentIndex < 0)
                currentIndex = 0;
            if (currentIndex >= quoteItems.Count)
                currentIndex = quoteItems.Count - 1;

            UpdateNavigationButtonsForTab(currentIndex, quoteItems.Count, leftArrowButton, rightArrowButton);
            UpdatePreviewBasedOnCurrentIndexForTab(logoPath, previewImage, previewCountTextBlock, currentIndex, quoteItems);
        }

        private void QOTDCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            QuotesContent.IsEnabled = false;
            BrowseQuotesButton.IsEnabled = false;
            QOTDStartNumber.IsEnabled = true;
        }

        private void QOTDCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            QuotesContent.IsEnabled = true;
            BrowseQuotesButton.IsEnabled = true;
            QOTDStartNumber.IsEnabled = false;
        }
    }
}
