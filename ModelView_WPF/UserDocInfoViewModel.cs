using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ModelView_WPF;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace View_WPF.ModelView_WPF
{
    public class UserDocInfoViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "https://localhost:7073/api/Data/SaveUserDoc";
        private readonly string _empDetailApiUrl = "https://localhost:7073/api/Data/GetEmployeeDetail/";

        // --- Properties for Main Window ---
        private string _uid;
        private string _employeeName;
        private string _selectedDocumentPath;
        private string _serverStatus = "Online";
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now;

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_uid))
                {
                    _ = LoadEmployeeNameAsync(_uid);
                }
            }
        }

        public string UserID
        {
            get => UID;
            set => UID = value;
        }

        public string EmployeeName { get => _employeeName; set { _employeeName = value; OnPropertyChanged(); } }
        public string SelectedDocumentPath { get => _selectedDocumentPath; set { _selectedDocumentPath = value; OnPropertyChanged(); } }
        public string ServerStatus { get => _serverStatus; set { _serverStatus = value; OnPropertyChanged(); } }
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        // --- Properties for Popup Window ---
        private bool _isPopupVisible = false;
        private string _popupTitle;
        private string _popupEmployerName;
        private string _popupJobTitle;
        private DateTime _popupStartDate = DateTime.Now;
        private DateTime _popupEndDate = DateTime.Now;
        private string _popupDocPath;

        public bool IsPopupVisible { get => _isPopupVisible; set { _isPopupVisible = value; OnPropertyChanged(); } }
        public string PopupTitle { get => _popupTitle; set { _popupTitle = value; OnPropertyChanged(); } }
        public string PopupEmployerName { get => _popupEmployerName; set { _popupEmployerName = value; OnPropertyChanged(); } }
        public string PopupJobTitle { get => _popupJobTitle; set { _popupJobTitle = value; OnPropertyChanged(); } }
        public DateTime PopupStartDate { get => _popupStartDate; set { _popupStartDate = value; OnPropertyChanged(); } }
        public DateTime PopupEndDate { get => _popupEndDate; set { _popupEndDate = value; OnPropertyChanged(); } }

        // --- Collections & Selection ---
        public ObservableCollection<DocumentModel> Documents { get; set; } = new ObservableCollection<DocumentModel>();

        private DocumentModel _selectedDocument;
        public DocumentModel SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                OnPropertyChanged();
                if (_selectedDocument != null) SelectedDocumentPath = _selectedDocument.Path;
            }
        }

        // --- Commands ---
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowPopupCommand { get; }
        public ICommand ClosePopupCommand { get; }
        public ICommand UploadInPopupCommand { get; }
        public ICommand SavePopupDataCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand BackCommand { get; } // নতুন ব্যাক বাটন কমান্ড

        public UserDocInfoViewModel()
        {
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            AddCommand = new RelayCommand(async (obj) => {
                string docType = obj as string;
                if (string.IsNullOrEmpty(UID)) { MessageBox.Show("Please enter or select a UID first!"); return; }

                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.pdf" };
                if (openFileDialog.ShowDialog() == true)
                {
                    AddDocumentToGrid(docType, openFileDialog.FileName);
                    await ExecuteSave();
                }
            });

            LogoutCommand = new RelayCommand((obj) => {
                if (MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            });

            ShowPopupCommand = new RelayCommand((obj) => {
                if (string.IsNullOrEmpty(UID)) { MessageBox.Show("UID is missing!"); return; }

                PopupEmployerName = this.EmployeeName;
                PopupTitle = obj as string;
                IsPopupVisible = true;
            });

            ClosePopupCommand = new RelayCommand((obj) => {
                IsPopupVisible = false;
                ClearPopupFields();
            });

            UploadInPopupCommand = new RelayCommand((obj) => {
                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.pdf" };
                if (openFileDialog.ShowDialog() == true)
                {
                    _popupDocPath = openFileDialog.FileName;
                    MessageBox.Show("Document Attached Successfully!");
                }
            });

            SavePopupDataCommand = new RelayCommand(async (obj) => {
                if (string.IsNullOrEmpty(_popupDocPath))
                {
                    MessageBox.Show("Please upload a document first!");
                    return;
                }

                var newDoc = new DocumentModel
                {
                    Type = PopupTitle,
                    Path = _popupDocPath,
                    Action = "View",
                    EmployerName = PopupEmployerName,
                    JobTitle = PopupJobTitle,
                    CustomStartDate = PopupStartDate,
                    CustomEndDate = PopupEndDate
                };
                Documents.Add(newDoc);
                SelectedDocument = newDoc;

                IsPopupVisible = false;
                await ExecuteSave();
                ClearPopupFields();
            });

            DownloadCommand = new RelayCommand((obj) => {
                if (SelectedDocument != null)
                {
                    DownloadFile(SelectedDocument.Path);
                }
                else
                {
                    MessageBox.Show("Please select a document from the list to download.");
                }
            });

            // --- ব্যাক বাটন লজিক ---
            BackCommand = new RelayCommand((obj) => {
                if (obj is Window currentWindow)
                {
                    currentWindow.Close(); // এটি বর্তমান উইন্ডো বন্ধ করে দিবে
                }
            });
        }

        private async Task LoadEmployeeNameAsync(string uid)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(_empDetailApiUrl + uid);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(json);
                        this.EmployeeName = data.fullName;
                    }
                    else
                    {
                        this.EmployeeName = "Unknown Employee";
                    }
                }
            }
            catch
            {
                this.EmployeeName = "Error Loading Name";
            }
        }

        private void AddDocumentToGrid(string type, string path)
        {
            var newDoc = new DocumentModel { Type = type, Path = path, Action = "View" };
            Documents.Add(newDoc);
            SelectedDocument = newDoc;
        }

        private void ClearPopupFields()
        {
            PopupEmployerName = string.Empty;
            PopupJobTitle = string.Empty;
            _popupDocPath = string.Empty;
            PopupStartDate = DateTime.Now;
            PopupEndDate = DateTime.Now;
        }

        private void DownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    MessageBox.Show("File does not exist on the local path.");
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = System.IO.Path.GetFileName(filePath),
                    Filter = "All Files|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(filePath, saveFileDialog.FileName, true);
                    MessageBox.Show("File Downloaded/Saved Successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Download failed: " + ex.Message);
            }
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
                EntryDate = DateTime.Now,
                Activate = "Y",
                DocumentList = this.Documents.Select(d => new {
                    Type = d.Type,
                    Path = d.Path,
                    Action = d.Action,
                    FileName = System.IO.Path.GetFileName(d.Path),
                    EmployerName = d.EmployerName,
                    JobTitle = d.JobTitle,
                    CustomStartDate = d.CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = d.CustomEndDate?.ToString("yyyy-MM-dd")
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
                        MessageBox.Show($"Server Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DocumentModel
    {
        public string Type { get; set; }
        public string Path { get; set; }
        public string Action { get; set; }
        public string EmployerName { get; set; }
        public string JobTitle { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
    }
}