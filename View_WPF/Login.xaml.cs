using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;

// Aliases to avoid conflicts
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        // Updated Base URL for your Swagger API
        private readonly string apiBaseUrl = "https://localhost:7073/";

        public Login()
        {
            InitializeComponent();
        }

        private async void btnAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            // Taking input from UI
            string uid = txtUserId.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Validation
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter User ID and Password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable button to prevent multiple clicks
            btnAuthenticate.IsEnabled = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Create data according to API model
                    var loginRequest = new
                    {
                        Uid = uid,
                        Password = password
                    };

                    // Sending POST request to API
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}api/Data/Login", loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        // 1. Reading data from API response
                        var jsonString = await response.Content.ReadAsStringAsync();

                        // Case-insensitive deserialization option
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var loginData = JsonSerializer.Deserialize<JsonElement>(jsonString, options);

                        string authenticatedUID = txtUserId.Text.Trim();
                        string authenticatedName = "";
                        int activateStatus = 0;

                        // Extracting Name from API response
                        if (loginData.TryGetProperty("fname", out var nameProp) || loginData.TryGetProperty("FName", out nameProp))
                        {
                            authenticatedName = nameProp.GetString();
                        }
                        else if (loginData.TryGetProperty("username", out var userProp))
                        {
                            authenticatedName = userProp.GetString();
                        }

                        // Extracting 'Activate' status from API response
                        if (loginData.TryGetProperty("activate", out var activateProp) || loginData.TryGetProperty("Activate", out activateProp))
                        {
                            if (activateProp.ValueKind == JsonValueKind.Number)
                                activateStatus = activateProp.GetInt32();
                            else if (activateProp.ValueKind == JsonValueKind.String)
                                int.TryParse(activateProp.GetString(), out activateStatus);
                        }

                        // 2. Role-Based Access Control Logic
                        if (activateStatus == 0)
                        {
                            MessageBox.Show("Your account is deactivated (Status 0). Please contact the Administrator.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);
                        }
                        else if (activateStatus == 1 || activateStatus == 3)
                        {
                            // New Logic: Record Login Session
                            try
                            {
                                var sessionData = new
                                {
                                    UID = authenticatedUID,
                                    StartDate = DateTime.Now
                                };

                                var sessionResponse = await client.PostAsJsonAsync($"{apiBaseUrl}api/LoginSessionInfo", sessionData);

                                if (sessionResponse.IsSuccessStatusCode)
                                {
                                    var sessionResult = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>();
                                    if (sessionResult.TryGetProperty("sessionID", out var sidProp) || sessionResult.TryGetProperty("SessionID", out sidProp))
                                    {
                                        App.Current.Properties["CurrentSessionID"] = sidProp.GetInt32();
                                    }
                                }
                            }
                            catch { /* Log session creation error if necessary */ }

                            // Open MainWindow
                            MainWindow mainWin = new MainWindow(authenticatedUID, authenticatedName, activateStatus);
                            mainWin.Show();

                            // Close the current login window
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("You do not have permission to access this system. Only Status 1 and 3 are allowed.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Invalid User ID or Password! Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtPassword.Clear();
                        txtUserId.Focus();
                    }
                    else
                    {
                        MessageBox.Show($"Login failed. Status Code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnAuthenticate.IsEnabled = true;
            }
        }
    }
}