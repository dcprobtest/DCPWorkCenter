using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Windows.Input;
using ModelView_WPF.Helpers;

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
        public Login()
        {
            InitializeComponent();

            // Handle Enter key press on both input fields
            txtUserId.KeyDown += Input_KeyDown;
            txtPassword.KeyDown += Input_KeyDown;
        }

        private void Input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAuthenticate_Click(btnAuthenticate, new RoutedEventArgs());
            }
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
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) })
                {
                    // Create data according to API model
                    var loginRequest = new
                    {
                        Uid = uid,
                        Password = password
                    };

                    // Sending POST request to API using ApiSettings
                    var response = await client.PostAsJsonAsync(ApiSettings.LoginUrl(), loginRequest);

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
                        else if (activateStatus == 1 || activateStatus == 3 || activateStatus == 4)
                        {
                            // New Logic: Record Login Session
                            try
                            {
                                var sessionData = new
                                {
                                    UID = authenticatedUID,
                                    StartDate = DateTime.Now
                                };

                                var sessionResponse = await client.PostAsJsonAsync(ApiSettings.LoginSessionInfoUrl(), sessionData);

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
                            MessageBox.Show("You do not have permission to access this system. Only Status 1, 3 and 4 are allowed.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
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