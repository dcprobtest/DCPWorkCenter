using System.Net.Http.Headers;
using Spire.Doc;
using Spire.Doc.Documents;
using System.Text.Json;
using View_MAUI.Model;

namespace View_MAUI;

public partial class frm_createdocs : ContentPage
{
    private readonly string _baseUrl = "https://localhost:7073/api/Data/";
    private List<EmpInfoModel> _allEmployees = new List<EmpInfoModel>();

    private string _currentLoggedInUID;
    private string _currentLoggedInName;
    private string _selectedCandidateName = "";

    public frm_createdocs(string loggedInUID, string loggedInName)
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

            try
            {
                var nameResponse = await client.GetAsync(_baseUrl + $"GetUserName/{_currentLoggedInUID}");
                if (nameResponse.IsSuccessStatusCode)
                {
                    var nameJson = await nameResponse.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(nameJson))
                    {
                        if (doc.RootElement.TryGetProperty("fullName", out var nameProp))
                            txtUserName.Text = nameProp.GetString();
                    }
                }
                else txtUserName.Text = _currentLoggedInName;
            }
            catch { txtUserName.Text = _currentLoggedInName; }

            var response = await client.GetAsync(_baseUrl + "GetEmployeeInfo");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var employees = JsonSerializer.Deserialize<List<EmpInfoModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (employees != null)
                {
                    _allEmployees = employees;
                    cmbCandidateID.ItemsSource = _allEmployees;

                    cmbCandidateID.ItemDisplayBinding = new Binding("FName");
                }
            }
        }
        catch (Exception ex) { await DisplayAlert("Error", "Error loading data: " + ex.Message, "OK"); }
    }

    private void cmbDocType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        string selectedDocType = picker.SelectedItem?.ToString();

        if (selectedDocType == "STEM OPT")
        {
            txtHourlyRate.IsEnabled = true;
        }
        else
        {
            txtHourlyRate.IsEnabled = false;
            txtHourlyRate.Text = "0";
        }
    }

    private void cmbCandidateID_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbCandidateID.SelectedItem is EmpInfoModel selectedEmp)
        {
            _selectedCandidateName = $"{selectedEmp.FName} {selectedEmp.LName}".Trim();
        }
    }

    private void dtpStartDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        if (dtpEndDate.Date == default(DateTime))
        {
            dtpEndDate.Date = e.NewDate.AddYears(1);
        }
    }

    private async void btnCreateDocument_Clicked(object sender, EventArgs e)
    {
        try
        {
            string selectedDocType = cmbDocType.SelectedItem?.ToString();
            string templateFileName = (selectedDocType == "STEM OPT") ? "STEM opt offer letter.docx" : "cpt-opt offer letter.docx";

            using var stream = await FileSystem.OpenAppPackageFileAsync(templateFileName);

            string tempTemplatePath = Path.Combine(FileSystem.CacheDirectory, templateFileName);
            using (var fileStream = File.Create(tempTemplatePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            Spire.Doc.Document document = new Spire.Doc.Document();
            document.LoadFromFile(tempTemplatePath);

            DateTime startDateValue = dtpStartDate.Date;
            DateTime submissionDateValue = startDateValue.AddDays(-3);

            document.Replace("<Date>", DateTime.Now.ToString("MMMM dd, yyyy"), true, true);
            document.Replace("<EmployeeName>", _selectedCandidateName, true, true);
            document.Replace("<Designation>", txtDesignation.Text, true, true);
            document.Replace("<DocumentType>", selectedDocType ?? "", true, true);
            document.Replace("<JobResponsibility>", txtResponsibility.Text, true, true);
            document.Replace("<WeeklyHours>", txtWeeklyHours.Text, true, true);
            document.Replace("<StartDate>", startDateValue.ToString("MMMM dd, yyyy"), true, true);
            document.Replace("<EndDate>", dtpEndDate.Date.ToString("MMMM dd, yyyy"), true, true);
            document.Replace("<HourlyRate>", txtHourlyRate.Text, true, true);
            document.Replace("<SubmissionDate>", submissionDateValue.ToString("MMMM dd, yyyy"), true, true);

            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, $"{txtUserID.Text}_{selectedDocType}.docx");
            document.SaveToFile(tempFilePath, Spire.Doc.FileFormat.Docx);
            document.Close();

            using HttpClient client = new HttpClient();
            using var content = new MultipartFormDataContent();

            byte[] fileBytes = File.ReadAllBytes(tempFilePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            content.Add(fileContent, "file", Path.GetFileName(tempFilePath));

            // Adding fields to match the API Request.Form expectations
            content.Add(new StringContent(txtUserID.Text), "LoggedInUserID");
            content.Add(new StringContent(cmbCandidateID.SelectedItem is EmpInfoModel emp ? emp.UID : ""), "CandidateID");
            content.Add(new StringContent(selectedDocType ?? ""), "SelectedDocType");
            content.Add(new StringContent(txtDesignation.Text ?? ""), "JobDesignation");
            content.Add(new StringContent(txtResponsibility.Text ?? ""), "JobResponsibility");
            content.Add(new StringContent(dtpStartDate.Date.ToString("yyyy-MM-dd")), "JobStartDate");
            content.Add(new StringContent(dtpEndDate.Date.ToString("yyyy-MM-dd")), "JobEndDate");
            content.Add(new StringContent(txtHourlyRate.Text ?? "0"), "HourlyRate");
            content.Add(new StringContent(txtWeeklyHours.Text ?? "0"), "WeeklyHours");
            content.Add(new StringContent(txtEmail.Text ?? ""), "Email");

            var response = await client.PostAsync(_baseUrl + "CreateUserDocRequest", content);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Document uploaded!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", "Failed to upload: " + error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}