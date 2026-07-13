using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_MAUI; // Namespace for your project models

namespace View_MAUI
{
    public class UserDocInfoViewModel : INotifyPropertyChanged
    {
        private readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _apiUrl => $"{_baseUrl}api/Data/SaveUserDoc";
        private string _empDetailApiUrl => $"{_baseUrl}api/Data/GetEmployeeDetail/";

        private readonly HttpClient _httpClient;

        private string _uid;
        private string _employeeName;
        private string _selectedDocumentPath;
        private string _serverStatus = "Online";
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now;
        private string _selectedDocType;

        // Properties
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); if (!string.IsNullOrEmpty(_uid)) _ = LoadEmployeeNameAsync(_uid); }
        }

        public string UserID { get => UID; set => UID = value; }
        public string EmployeeName { get => _employeeName; set { _employeeName = value; OnPropertyChanged(); } }
        public string SelectedDocumentPath { get => _selectedDocumentPath; set { _selectedDocumentPath = value; OnPropertyChanged(); } }
        public string ServerStatus { get => _serverStatus; set { _serverStatus = value; OnPropertyChanged(); } }
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        public string SelectedDocType
        {
            get => _selectedDocType;
            set { _selectedDocType = value; OnPropertyChanged(); SetDatesByDocumentType(_selectedDocType); }
        }

        // Popup Properties
        private bool _isPopupVisible = false;
        public bool IsPopupVisible { get => _isPopupVisible; set { _isPopupVisible = value; OnPropertyChanged(); } }
        public string PopupTitle { get; set; }

        private string _popupEmployerName;
        public string PopupEmployerName
        {
            get => _popupEmployerName;
            set { _popupEmployerName = value; OnPropertyChanged(); }
        }

        public string PopupJobTitle { get; set; }
        public DateTime PopupStartDate { get; set; } = DateTime.Now;
        public DateTime PopupEndDate { get; set; } = DateTime.Now;
        private string _popupDocPath;

        public ObservableCollection<UserDocInfoModel> Documents { get; set; } = new ObservableCollection<UserDocInfoModel>();

        private UserDocInfoModel _selectedDocument;
        public UserDocInfoModel SelectedDocument
        {
            get => _selectedDocument;
            set { _selectedDocument = value; OnPropertyChanged(); if (_selectedDocument != null) SelectedDocumentPath = _selectedDocument.DocPath; }
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ClosePopupCommand { get; }
        public ICommand UploadInPopupCommand { get; }
        public ICommand SavePopupDataCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand BackCommand { get; }

        public ICommand OpenCptPopupCommand { get; }
        public ICommand OpenOptPopupCommand { get; }
        public ICommand OpenStemOptPopupCommand { get; }

        public UserDocInfoViewModel()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);

            SaveCommand = new Command(async () => await ExecuteSave());
            AddCommand = new Command(async () => await HandleAddDocument());
            LogoutCommand = new Command(() => Application.Current.Quit());

            OpenCptPopupCommand = new Command(() => ShowCustomPopup("CPT"));
            OpenOptPopupCommand = new Command(() => ShowCustomPopup("OPT"));
            OpenStemOptPopupCommand = new Command(() => ShowCustomPopup("STEM OPT"));

            ClosePopupCommand = new Command(() => { IsPopupVisible = false; ClearPopupFields(); });
            UploadInPopupCommand = new Command(async () => await PickFile());
            SavePopupDataCommand = new Command(async () => await SavePopupData());
            DownloadCommand = new Command(() => { /* Platform specific download logic */ });
            BackCommand = new Command(() => Shell.Current.Navigation.PopAsync());
        }

        private void ShowCustomPopup(string title)
        {
            if (string.IsNullOrEmpty(UID)) return;

            PopupEmployerName = this.EmployeeName;
            PopupTitle = title;
            IsPopupVisible = true;

            PopupStartDate = this.StartDate;
            PopupEndDate = this.EndDate;
        }

        public UserDocInfoViewModel(string uid) : this() => UID = uid;

        private async Task HandleAddDocument()
        {
            var result = await FilePicker.PickAsync();
            if (result != null)
            {
                AddDocumentToGrid(SelectedDocType ?? "General", result.FullPath, StartDate, EndDate);
                await ExecuteSave();
            }
        }

        private async Task PickFile()
        {
            var result = await FilePicker.PickAsync();
            if (result != null) _popupDocPath = result.FullPath;
        }

        private async Task SavePopupData()
        {
            if (string.IsNullOrEmpty(_popupDocPath)) return;

            Documents.Add(new UserDocInfoModel
            {
                UID = this.UID,
                DocType = PopupTitle,
                DocPath = _popupDocPath,
                StartDate = PopupStartDate,
                EndDate = PopupEndDate,
                EntryDate = DateTime.Now
            });

            IsPopupVisible = false;
            await ExecuteSave();
            ClearPopupFields();
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(UID))
            {
                ServerStatus = "Error: UID is required";
                return;
            }

            if (Documents.Count == 0) return;

            var saveRequest = new
            {
                UID = this.UID,
                EmployeeName = this.EmployeeName,
                StartDate = this.StartDate.ToString("yyyy-MM-dd"),
                EndDate = this.EndDate.ToString("yyyy-MM-dd"),
                EntryDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Activate = "Y",
                DocumentList = this.Documents.Select(d => new {
                    Type = d.DocType,
                    Path = d.DocPath,
                    Action = "View",
                    FileName = System.IO.Path.GetFileName(d.DocPath),
                    EmployerName = this.EmployeeName,
                    JobTitle = this.PopupJobTitle,
                    CustomStartDate = d.StartDate.ToString("yyyy-MM-dd"),
                    CustomEndDate = d.EndDate.ToString("yyyy-MM-dd")
                }).ToList()
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(saveRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        ServerStatus = $"Data Saved Successfully at {DateTime.Now:HH:mm:ss}";
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Server Error", $"Status Code: {response.StatusCode}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Connection Failed", ex.Message, "OK");
            }
        }

        private async Task LoadEmployeeNameAsync(string uid)
        {
            try
            {
                string url = _empDetailApiUrl + uid;
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    if (data != null) EmployeeName = data.fullName?.ToString();
                }
            }
            catch (Exception ex) { EmployeeName = "Error: " + ex.Message; }
        }

        private void SetDatesByDocumentType(string docType)
        {
            StartDate = DateTime.Now;
            EndDate = docType switch
            {
                "Passport" => StartDate.AddYears(10),
                "Driving Licence" => StartDate.AddYears(5),
                _ => StartDate.AddYears(1)
            };
        }

        private void AddDocumentToGrid(string type, string path, DateTime start, DateTime end)
        {
            Documents.Add(new UserDocInfoModel
            {
                UID = this.UID,
                DocType = type,
                DocPath = path,
                StartDate = start,
                EndDate = end,
                EntryDate = DateTime.Now
            });
        }

        private void ClearPopupFields() { _popupDocPath = string.Empty; PopupStartDate = DateTime.Now; PopupEndDate = DateTime.Now; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}