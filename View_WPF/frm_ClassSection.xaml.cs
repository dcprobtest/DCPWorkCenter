using System;
using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_ClassSection.xaml
    /// </summary>
    public partial class frm_ClassSection : Window
    {
        public frm_ClassSection()
        {
            InitializeComponent();

            if (this.DataContext == null)
            {
                this.DataContext = new ClassSectionViewModel(null);
            }
        }

        public frm_ClassSection(string uid)
        {
            InitializeComponent();

            this.DataContext = new ClassSectionViewModel(uid);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClassSectionViewModel viewModel)
            {
                await viewModel.LoadInitialDataAsync();
            }
        }
    }
}