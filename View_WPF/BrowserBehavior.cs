using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Spire.Doc;

namespace View_WPF
{
    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(BrowserBehavior),
                new PropertyMetadata(string.Empty, OnHtmlChanged));

        public static string GetHtml(DependencyObject obj) => (string)obj.GetValue(HtmlProperty);
        public static void SetHtml(DependencyObject obj, string value) => obj.SetValue(HtmlProperty, value);

        private static async void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.WebBrowser browser)
            {
                string filePath = e.NewValue as string;

                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    browser.NavigateToString("<html><body><p>No file selected or file does not exist.</p></body></html>");
                    return;
                }

                try
                {
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".docx" || extension == ".doc")
                    {
                        browser.NavigateToString("<html><body style='font-family:Segoe UI; text-align:center; padding-top:50px;'><h3>Loading document preview, please wait...</h3></body></html>");

                        string htmlContent = await Task.Run(() => ConvertWordToHtmlUsingSpire(filePath));

                        if (!string.IsNullOrEmpty(htmlContent))
                        {
                            browser.NavigateToString(htmlContent);
                            return;
                        }
                    }

                    browser.Navigate(filePath);
                }
                catch (Exception ex)
                {
                    browser.NavigateToString($"<html><body><p>Preview Error: {ex.Message}</p></body></html>");
                }
            }
        }

        private static string ConvertWordToHtmlUsingSpire(string wordFilePath)
        {
            try
            {
                using (Document document = new Document())
                {
                    document.LoadFromFile(wordFilePath);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        document.SaveToStream(stream, FileFormat.Html);
                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("Conversion Error: " + ex.Message);
                });
                return null;
            }
        }
    }
}