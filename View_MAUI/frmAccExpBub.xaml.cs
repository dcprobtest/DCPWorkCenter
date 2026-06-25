namespace View_MAUI;

public partial class frmAccExpBub : ContentPage
{
    public frmAccExpBub()
    {
        InitializeComponent();

        // If you are using a ViewModel, uncomment the line below:
        // this.BindingContext = new AccExpBubViewModel();
    }

    // Logic for ADD button
    private void AddCommand_Clicked(object sender, EventArgs e)
    {
        // Add your logic to enable input fields or reset for a new entry
    }

    // Logic for SAVE button
    private void SaveCommand_Clicked(object sender, EventArgs e)
    {
        // Add your logic to save the data to the database
    }

    // Logic for EDIT button
    private void EditCommand_Clicked(object sender, EventArgs e)
    {
        // Add your logic to edit the selected item
    }

    // Logic for EXIT button
    private void ExitCommand_Clicked(object sender, EventArgs e)
    {
        // Navigate back or close the page
        Navigation.PopAsync();
    }
}