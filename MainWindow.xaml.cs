using System.Windows;
using System.Windows.Controls;
using TemplateApp.Resources.Usercontrols;

namespace TemplateApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateSelector.SelectedIndex = 0;
            ContentArea.Content = new QuotesUC();
        }

        private void TemplateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentArea != null && TemplateSelector.SelectedItem != null)
            {
                var selectedTemplate = (ComboBoxItem)TemplateSelector.SelectedItem;
                switch (selectedTemplate.Content.ToString())
                {
                    case "Quotes":
                        ContentArea.Content = new QuotesUC();
                        break;
                    case "Trivia Questions":
                        ContentArea.Content = new TriviaUC();
                        break;
                }
            }
        }
    }
}