using View_WPF.Model; // Ensure the correct namespace for your model
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Spire.Doc; // FreeSpire.Doc library is used
using ModelView_WPF.Helpers;

namespace View_WPF
{
    public partial class frm_createdocx : Window
    {
        // Changed 'EmployeeInfo' to 'EmpInfoModel' according to your model
        private List<EmpInfoModel> _allEmployees = new List<EmpInfoModel>();

        private string _currentLoggedInUID;
        private string _currentLoggedInName;
        private string _selectedCandidateName = "";

        public frm_createdocx(string loggedInUID, string loggedInName)
        {
            InitializeComponent();
            _currentLoggedInUID = loggedInUID;
            _currentLoggedInName = loggedInName;
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                txtUserID.Text = _currentLoggedInUID;
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                using HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

                // --- Fetch logged-in user name ---
                try
                {
                    var nameResponse = await client.GetAsync(ApiSettings.GetUserNameUrl(_currentLoggedInUID));
                    if (nameResponse.IsSuccessStatusCode)
                    {
                        var nameJson = await nameResponse.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(nameJson))
                        {
                            if (doc.RootElement.TryGetProperty("fullName", out var nameProp))
                            {
                                txtUserName.Text = nameProp.GetString();
                            }
                        }
                    }
                    else
                    {
                        txtUserName.Text = _currentLoggedInName;
                    }
                }
                catch
                {
                    txtUserName.Text = _currentLoggedInName;
                }

                // --- Load candidate data (using GetEmployeeInfo) ---
                var response = await client.GetAsync(ApiSettings.GetEmployeeInfoUrl());
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // Using 'EmpInfoModel' here
                    var employees = JsonSerializer.Deserialize<List<EmpInfoModel>>(jsonString, options);

                    if (employees != null)
                    {
                        _allEmployees = employees;
                        cmbCandidateID.ItemsSource = _allEmployees;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        // --- Handle Hourly Rate control based on dropdown change ---
        private void cmbDocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDocType.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedDocType = selectedItem.Content.ToString();

                // If STEM OPT, allow Hourly Rate input (Enable)
                if (selectedDocType == "STEM OPT")
                {
                    txtHourlyRate.IsEnabled = true;
                    txtHourlyRate.Opacity = 1.0;
                }
                else // For CPT or OPT, block the input (Disable)
                {
                    txtHourlyRate.IsEnabled = false;
                    txtHourlyRate.Text = "0";
                    txtHourlyRate.Opacity = 0.5;
                }
            }
        }

        private void cmbCandidateID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Using 'EmpInfoModel' and accessing FName/LName properties
            if (cmbCandidateID.SelectedItem is EmpInfoModel selectedEmp)
            {
                // Set full candidate name
                _selectedCandidateName = $"{selectedEmp.FName} {selectedEmp.LName}".Trim();

                // Show only ID in the textbox after selection
                Dispatcher.BeginInvoke(new Action(() => {
                    cmbCandidateID.Text = selectedEmp.UID;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void cmbCandidateID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.Up) return;

            string searchText = cmbCandidateID.Text;
            var filteredList = _allEmployees
                .Where(emp => emp.UID != null && emp.UID.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            cmbCandidateID.ItemsSource = filteredList;
            cmbCandidateID.IsDropDownOpen = true;
        }

        private async void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- Updated template selection logic ---
                string selectedDocType = (cmbDocType.SelectedItem as ComboBoxItem)?.Content.ToString();
                string templateFileName = "cpt-opt offer letter.docx"; // Default

                // Conditional template selection
                if (selectedDocType == "CPT" || selectedDocType == "OPT")
                {
                    templateFileName = "cpt-opt offer letter.docx";
                }
                else if (selectedDocType == "STEM OPT")
                {
                    templateFileName = "STEM opt offer letter.docx"; // New template
                }

                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asset", templateFileName);

                if (!File.Exists(templatePath))
                {
                    System.Windows.MessageBox.Show($"{templateFileName} not found in Asset folder!");
                    return;
                }

                string candidateID = cmbCandidateID.Text;
                string docType = selectedDocType;
                string fileName = $"{candidateID}_{docType}.docx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                Document document = new Document();
                document.LoadFromFile(templatePath);

                // Date calculation (Submission Date is 3 days before Start Date)
                DateTime startDateValue = dtpStartDate.SelectedDate ?? DateTime.Now;
                DateTime submissionDateValue = startDateValue.AddDays(-3);

                // Data replacement logic
                document.Replace("<Date>", DateTime.Now.ToString("MMMM dd, yyyy"), true, true);
                document.Replace("<EmployeeName>", _selectedCandidateName, true, true);
                document.Replace("< Employee Name>", _selectedCandidateName, true, true);
                document.Replace("<Designation>", txtDesignation.Text, true, true);
                document.Replace("<DocumentType>", docType ?? "", true, true);
                document.Replace("<JobResponsibility>", txtResponsibility.Text, true, true);
                document.Replace("<WeeklyHours>", txtWeeklyHours.Text, true, true);
                document.Replace("<StartDate>", startDateValue.ToString("MMMM dd, yyyy"), true, true);
                document.Replace("<EndDate>", dtpEndDate.SelectedDate?.ToString("MMMM dd, yyyy") ?? "", true, true);
                document.Replace("<HourlyRate>", txtHourlyRate.Text, true, true);

                // Submission Date will be replaced for all templates
                document.Replace("<SubmissionDate>", submissionDateValue.ToString("MMMM dd, yyyy"), true, true);

                document.SaveToFile(tempFilePath, FileFormat.Docx);
                document.Close();

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                using HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
                using var content = new MultipartFormDataContent();

                byte[] fileBytes = File.ReadAllBytes(tempFilePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                content.Add(fileContent, "file", fileName);

                content.Add(new StringContent(txtUserID.Text), "LoggedInUserID");
                content.Add(new StringContent(candidateID), "CandidateID");
                content.Add(new StringContent(docType ?? ""), "SelectedDocType");
                content.Add(new StringContent(txtDesignation.Text), "JobDesignation");
                content.Add(new StringContent(txtResponsibility.Text), "JobResponsibility");
                content.Add(new StringContent(dtpStartDate.SelectedDate?.ToString("yyyy-MM-dd") ?? ""), "JobStartDate");
                content.Add(new StringContent(dtpEndDate.SelectedDate?.ToString("yyyy-MM-dd") ?? ""), "JobEndDate");
                content.Add(new StringContent(txtHourlyRate.Text), "HourlyRate");
                content.Add(new StringContent(txtWeeklyHours.Text), "WeeklyHours");
                content.Add(new StringContent(txtEmail.Text), "Email");

                var response = await client.PostAsync(ApiSettings.CreateUserDocRequestUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    System.Windows.MessageBox.Show("Success: Document generated in WPF and uploaded to Server!");
                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    this.Close();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show("Failed to upload: " + error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dtpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpStartDate.SelectedDate.HasValue && dtpEndDate.SelectedDate == null)
            {
                dtpEndDate.SelectedDate = dtpStartDate.SelectedDate.Value.AddYears(1);
            }
        }
    }
}