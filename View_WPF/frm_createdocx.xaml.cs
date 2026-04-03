using DCPWorkCenter_Api.Models; // নিশ্চিত করুন প্রোজেক্ট রেফারেন্স অ্যাড করা আছে
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
        private List<string> _allUserIds = new List<string>();
        private string _currentLoggedInUID;
        private string _currentLoggedInName; // লগইন করা ইউজারের নাম স্টোর করার জন্য
        private string _selectedCandidateName = ""; // ক্যান্ডিডেটের নাম স্টোর করার জন্য ভ্যারিয়েবল

        // কনস্ট্রাক্টরে loggedInName প্যারামিটার যোগ করা হয়েছে
        public frm_createdocx(string loggedInUID, string loggedInName)
        {
            InitializeComponent();
            _currentLoggedInUID = loggedInUID;
            _currentLoggedInName = loggedInName; // নাম রিসিভ করা হলো
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                // ১. সরাসরি রিসিভ করা আইডি UI-তে সেট করা
                txtUserID.Text = _currentLoggedInUID;

                using HttpClient client = new HttpClient();

                // --- নতুন যোগ করা অংশ: API থেকে UID অনুযায়ী ইউজার নেম নিয়ে আসা ---
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
                        // যদি এপিআই কল ফেল করে তবে রিসিভ করা নামটাই বসিয়ে দেওয়া হবে
                        txtUserName.Text = _currentLoggedInName;
                    }
                }
                catch
                {
                    txtUserName.Text = _currentLoggedInName;
                }
                // --- শেষ ---

                // ২. ক্যান্ডিডেট আইডি ড্রপডাউন ডাটা লোড
                var users = await client.GetFromJsonAsync<List<JsonElement>>(_baseUrl + "GetUsers");
                if (users != null)
                {
                    _allUserIds = users.Select(u => u.GetProperty("uid").ToString()).ToList();
                    cmbCandidateID.ItemsSource = _allUserIds;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        // ক্যান্ডিডেট আইডি সিলেক্ট করলে নাম নিয়ে আসার মেথড
        private async void cmbCandidateID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCandidateID.SelectedItem == null) return;

            string selectedID = cmbCandidateID.SelectedItem.ToString();
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(_baseUrl + $"GetEmployeeDetail/{selectedID}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var empDetail = JsonSerializer.Deserialize<JsonElement>(jsonString);

                    string fName = empDetail.TryGetProperty("FName", out var f) ? f.GetString() : "";
                    string lName = empDetail.TryGetProperty("LName", out var l) ? l.GetString() : "";

                    _selectedCandidateName = $"{fName} {lName}".Trim();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error fetching candidate name: " + ex.Message);
            }
        }

        private void cmbCandidateID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string searchText = cmbCandidateID.Text;
            var filteredList = _allUserIds.Where(id => id.StartsWith(searchText)).ToList();
            cmbCandidateID.ItemsSource = filteredList;
            cmbCandidateID.IsDropDownOpen = true;
        }

        private async void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ১. টেমপ্লেট পাথ ধরা (App ডিরেক্টরি থেকে)
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

                // ২. ফাইল জেনারেট করা (FreeSpire.Doc ব্যবহার করে)
                Document document = new Document();
                document.LoadFromFile(templatePath);

                // টেক্সট রিপ্লেস করার সঠিক মেথড
                document.Replace("<Date>", DateTime.Now.ToString("MMMM dd, yyyy"), true, true);

                // এখানে আপনার রিকোয়ারমেন্ট অনুযায়ী ক্যান্ডিডেটের নাম বসানো হয়েছে
                document.Replace("<EmployeeName>", _selectedCandidateName, true, true);

                document.Replace("<Designation>", txtDesignation.Text, true, true);
                document.Replace("<DocumentType>", docType ?? "", true, true);
                document.Replace("<JobResponsibility>", txtResponsibility.Text, true, true);
                document.Replace("<WeeklyHours>", txtWeeklyHours.Text, true, true);
                document.Replace("<StartDate>", dtpStartDate.SelectedDate?.ToString("MMMM dd, yyyy") ?? "", true, true);
                document.Replace("<EndDate>", dtpEndDate.SelectedDate?.ToString("MMMM dd, yyyy") ?? "", true, true);
                document.Replace("<HourlyRate>", txtHourlyRate.Text, true, true);

                // টেম্পোরারি ফোল্ডারে সেভ করা
                document.SaveToFile(tempFilePath, FileFormat.Docx);
                document.Close();

                // ৩. ফাইল এবং ডাটা API-তে পাঠানো (MultipartFormDataContent ব্যবহার করে)
                using HttpClient client = new HttpClient();
                using var content = new MultipartFormDataContent();

                // ফাইল যোগ করা
                byte[] fileBytes = File.ReadAllBytes(tempFilePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                content.Add(fileContent, "file", fileName);

                // অন্যান্য ডাটা যোগ করা
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

                    // টেম্পোরারি ফাইল মুছে ফেলা
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
    }
}