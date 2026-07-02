namespace View_MAUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private string authenticatedName;
        private int activateStatus;

        public MainPage(string authenticatedUID)
        {
            InitializeComponent();
        }

        public MainPage(string authenticatedUID, string authenticatedName, int activateStatus) : this(authenticatedUID)
        {
            this.authenticatedName = authenticatedName;
            this.activateStatus = activateStatus;
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
