namespace TemplateApp.Resources.Models
{
    public class QuoteItem
    {
        public string Quote { get; set; }
        public string Caption { get; set; }
        public string BackgroundImagePath { get; set; }

        public QuoteItem(string quote, string caption, string backgroundImagePath)
        {
            Quote = quote;
            Caption = caption;
            BackgroundImagePath = backgroundImagePath;
        }
    }
}
