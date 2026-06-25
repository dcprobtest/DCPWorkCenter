namespace View_MAUI;

public partial class frm_VideoConference : ContentPage
{
    public frm_VideoConference()
    {
        InitializeComponent();
    }

    // Toggle the visibility of the Chat Sidebar
    private void ChatToggle_Clicked(object sender, EventArgs e)
    {
        ChatSidebar.IsVisible = !ChatSidebar.IsVisible;
    }

    // Logic for Mic toggle
    private void ToggleMic_Clicked(object sender, EventArgs e)
    {
        // Add microphone control logic here
    }

    // Logic for Camera toggle
    private void ToggleCam_Clicked(object sender, EventArgs e)
    {
        // Add camera control logic here
    }

    // Logic to leave the conference session
    private void LeaveSession_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}