namespace View_MAUI;

public partial class frmAccEmpSalary : ContentPage
{
    public frmAccEmpSalary()
    {
        InitializeComponent();
    }

    // Logic for ADD button
    private void AddCommand_Clicked(object sender, EventArgs e)
    {
        // Add your logic to clear fields or prepare for new entry
    }

    // Logic for SAVE button
    private void SaveCommand_Clicked(object sender, EventArgs e)
    {
        // Add your database save logic here
    }

    // Logic for EDIT button
    private void EditCommand_Clicked(object sender, EventArgs e)
    {
        // Add your logic to enable editing
    }

    // Logic for EXIT button
    private void ExitCommand_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}