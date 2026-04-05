using View_WPF.Model; // আপনার মডেলের সঠিক নেমস্পেস
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
using Spire.Doc; // FreeSpire.Doc লাইব্রেরি ব্যবহার করা হয়েছে

namespace View_WPF
{
    public partial class frm_createdocx : Window
    {
        private readonly string _baseUrl = "https://localhost:7073/api/Data/";

        // আপনার মডেল অনুযায়ী 'EmployeeInfo' কে পরিবর্তন করে 'EmpInfoModel' করা হলো
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
                using HttpClient client = new HttpClient();

                // --- লগইন করা ইউজারের নাম নিয়ে আসা ---
                try
                {
                    var nameResponse = await client.GetAsync(_baseUrl + $"GetUserName/{_currentLoggedInUID}");
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

                // --- ক্যান্ডিডেট ডাটা লোড (GetEmployeeInfo ব্যবহার করে) ---
                var response = await client.GetAsync(_baseUrl + "GetEmployeeInfo");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // এখানেও 'EmpInfoModel' ব্যবহার করা হলো
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

        private void cmbCandidateID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // এখানেও 'EmpInfoModel' ব্যবহার করা হলো এবং আপনার মডেলের FName ও LName প্রপার্টি ব্যবহার করা হয়েছে
            if (cmbCandidateID.SelectedItem is EmpInfoModel selectedEmp)
            {
                // ক্যান্ডিডেটের পূর্ণ নাম সেট করা
                _selectedCandidateName = $"{selectedEmp.FName} {selectedEmp.LName}".Trim();

                // সিলেকশন হওয়ার পর টেক্সটবক্সে শুধু আইডি দেখানোর জন্য
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
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asset", "Offer Letter.docx");

                if (!File.Exists(templatePath))
                {
                    System.Windows.MessageBox.Show("Template file not found in Asset folder!");
                    return;
                }

                string candidateID = cmbCandidateID.Text;
                string docType = (cmbDocType.SelectedItem as ComboBoxItem)?.Content.ToString();
                string fileName = $"{candidateID}_{docType}.docx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                Document document = new Document();
                document.LoadFromFile(templatePath);

                document.Replace("<Date>", DateTime.Now.ToString("MMMM dd, yyyy"), true, true);
                document.Replace("<EmployeeName>", _selectedCandidateName, true, true);
                document.Replace("<Designation>", txtDesignation.Text, true, true);
                document.Replace("<DocumentType>", docType ?? "", true, true);
                document.Replace("<JobResponsibility>", txtResponsibility.Text, true, true);
                document.Replace("<WeeklyHours>", txtWeeklyHours.Text, true, true);
                document.Replace("<StartDate>", dtpStartDate.SelectedDate?.ToString("MMMM dd, yyyy") ?? "", true, true);
                document.Replace("<EndDate>", dtpEndDate.SelectedDate?.ToString("MMMM dd, yyyy") ?? "", true, true);
                document.Replace("<HourlyRate>", txtHourlyRate.Text, true, true);

                document.SaveToFile(tempFilePath, FileFormat.Docx);
                document.Close();

                using HttpClient client = new HttpClient();
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

                var response = await client.PostAsync(_baseUrl + "CreateUserDocRequest", content);

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