using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace View_WPF
{
    public class DocumentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate BrowserTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string path && !string.IsNullOrEmpty(path))
            {
                string extension = Path.GetExtension(path).ToLower();

                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                {
                    return ImageTemplate;
                }
            }

            return BrowserTemplate;
        }
    }
}