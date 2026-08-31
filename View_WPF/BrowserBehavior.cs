using System;
using System.IO;
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

        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
                        string pdfPath = ConvertWordToPdfUsingSpire(filePath);
                        if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
                        {
                            browser.Navigate(pdfPath);
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

        private static string ConvertWordToPdfUsingSpire(string wordFilePath)
        {
            try
            {
                string tempDir = Path.GetTempPath();
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(wordFilePath);
                string pdfPath = Path.Combine(tempDir, $"{fileNameWithoutExt}_preview.pdf");

                if (File.Exists(pdfPath))
                {
                    try { File.Delete(pdfPath); } catch { }
                }

                Document document = new Document();
                document.LoadFromFile(wordFilePath);
                document.SaveToFile(pdfPath, FileFormat.PDF);
                document.Close();

                return pdfPath;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Conversion Error: " + ex.Message);
                return null;
            }
        }
    }
}