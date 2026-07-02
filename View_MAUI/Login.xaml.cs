using System.Net.Http.Json;
using System.Text.Json;

namespace View_MAUI;

public partial class Login : ContentPage
{
    // API base URL
    private readonly string apiBaseUrl = "https://localhost:7073/";

    public Login()
    {
        InitializeComponent();
    }

    private async void btnAuthenticate_Clicked(object sender, EventArgs e)
    {
        // 1. Get input
        string uid = txtUserId.Text?.Trim();
        string password = txtPassword.Text?.Trim();

        // 2. Validation
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Validation Error", "Please enter User ID and Password.", "OK");
            return;
        }

        // Disable button to prevent multiple clicks
        btnAuthenticate.IsEnabled = false;

        try
        {
            using (HttpClient client = new HttpClient())
            {
                // 3. Create request body
                var loginRequest = new { Uid = uid, Password = password };

                // 4. Call API
                // If using Android Emulator, you might need to use https://10.0.2.2:7073
                var response = await client.PostAsJsonAsync($"{apiBaseUrl}api/Data/Login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Read response
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loginData = JsonSerializer.Deserialize<JsonElement>(jsonString, options);

                    // Initialize variables
                    string authenticatedName = "";
                    int activateStatus = 0;

                    // Extract Name
                    if (loginData.TryGetProperty("fname", out var nameProp) || loginData.TryGetProperty("FName", out nameProp))
                        authenticatedName = nameProp.GetString();
                    else if (loginData.TryGetProperty("username", out var userProp))
                        authenticatedName = userProp.GetString();

                    // Extract Status
                    if (loginData.TryGetProperty("activate", out var act) || loginData.TryGetProperty("Activate", out act))
                    {
                        activateStatus = act.ValueKind == JsonValueKind.Number ? act.GetInt32() : int.Parse(act.GetString() ?? "0");
                    }

                    // 5. Role-based access logic
                    if (activateStatus == 0)
                    {
                        await DisplayAlert("Access Denied", "Your account is deactivated. Please contact the Administrator.", "OK");
                    }
                    else if (activateStatus == 1 || activateStatus == 3)
                    {
                        // Navigate on success
                        await DisplayAlert("Success", $"Welcome {authenticatedName}!", "OK");

                        // For navigating to the next page:
                        // await Navigation.PushAsync(new YourNextPageName(uid, authenticatedName, activateStatus));
                    }
                    else
                    {
                        await DisplayAlert("Access Denied", "You do not have permission to access this system.", "OK");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await DisplayAlert("Login Failed", "Invalid User ID or Password!", "OK");
                    txtPassword.Text = ""; // Clear password
                }
                else
                {
                    await DisplayAlert("Error", $"Server returned: {response.StatusCode}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", $"Could not connect to API: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            btnAuthenticate.IsEnabled = true;
        }
    }
}