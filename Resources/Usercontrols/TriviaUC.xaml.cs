using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using TemplateApp.Resources.Utilities;

namespace TemplateApp
{
    public partial class TriviaUC : UserControl
    {
        private readonly FontFamily _philosopherItalic;
        private const double QuestionFontSize = 62;
        private const double CaptionFontSize = 37;
        private const double LineWidth = 475;

        public TriviaUC()
        {
            InitializeComponent();
            _philosopherItalic = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Philosopher");
        }

        private void BrowseLogoFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(LogoPath, "Image Files|*.jpg;*.jpeg;*.png");
        }

        private void BrowseBackgroundImages_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(BackgroundImagePaths, "Image Files|*.jpg;*.jpeg;*.png", true);
        }

        private void BrowseQuestionAnswerFile_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.BrowseFile(QuestionAnswerContent, "Text Files|*.txt");
        }

        private void GenerateTrivia_Click(object sender, RoutedEventArgs e)
        {
            string[] backgroundImagePaths = BackgroundImagePaths.Text.Split(';');
            string[] lines = QuestionAnswerContent.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string subjectCaption = (SubjectCaptionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "General";

            if (backgroundImagePaths.Length == 0 || string.IsNullOrEmpty(subjectCaption))
            {
                MessageBox.Show("Please select background images and a subject caption.");
                return;
            }

            var questions = new List<string>();
            var answers = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].EndsWith('?'))
                {
                    questions.Add(lines[i]);
                    if (i + 1 < lines.Length && !lines[i + 1].EndsWith('?') && !lines[i + 1].All(char.IsDigit))
                    {
                        answers.Add(lines[i + 1]);
                        i++;
                    }
                    else
                    {
                        MessageBox.Show("The input format is incorrect. Ensure each question is followed by its answer.");
                        return;
                    }
                }
            }

            if (questions.Count != answers.Count)
            {
                MessageBox.Show("The input format is incorrect. Ensure each question is followed by its answer.");
                return;
            }

            string logoPath = string.IsNullOrEmpty(LogoPath.Text) ? string.Empty : LogoPath.Text;

            CommonUtils.GenerateTriviaImages(LogoPath.Text, backgroundImagePaths, [.. questions], [.. answers], subjectCaption, RenderImage);
        }

        private void RenderImage(string text, string caption, string backgroundImagePath, int index)
        {
            CommonUtils.RenderImage(text, caption, backgroundImagePath, LogoPath.Text, index, QuestionFontSize, CaptionFontSize, _philosopherItalic, LineWidth);
        }
    }
}