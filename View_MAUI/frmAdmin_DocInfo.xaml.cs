using View_MAUI;

namespace View_MAUI
{
    public partial class frmAdmin_DocInfo : ContentPage
    {
        private UserDocInfoViewModel _viewModel;

        public frmAdmin_DocInfo(string uid)
        {
            InitializeComponent();

            // Initialize ViewModel
            _viewModel = new UserDocInfoViewModel();

            // Set DataContext for binding
            this.BindingContext = _viewModel;

            // Handle logic after page is loaded
            this.Loaded += (s, e) =>
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    _viewModel.UID = uid;
                    _viewModel.UID = uid;

                    UpdateUIDInUI(uid);
                }
            };
        }

        private void UpdateUIDInUI(string uid)
        {
            // Find the entry field in MAUI (assuming x:Name="txtUID" exists in XAML)
            var txtEntry = this.FindByName("txtUID") as Entry;

            if (txtEntry != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    txtEntry.Text = uid;
                    txtEntry.IsReadOnly = true;
                });
            }
        }

        // MAUI does not use ContextMenu directly like WPF, 
        // usually handled via Command or Tapped events.
        private void PlusButton_Clicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                // Logic for handling plus button action
            }
        }

        private void UploadButton_Clicked(object sender, EventArgs e)
        {
            // Upload logic will be handled via ViewModel commands
        }
    }
}