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
using ModelView_WPF.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Controls;

namespace View_WPF.ModelView_WPF
{
    public class UserDocInfoViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _empDetailApiUrl;

        private string _uid;
        private string _employeeName;
        private string _selectedDocumentPath;
        private string _serverStatus = "Online";
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now;
        private string _selectedDocType;

        private List<DocumentModel> _allDocuments = new List<DocumentModel>();

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
                    _ = InitializeAsync();
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

        public string SelectedDocType
        {
            get => _selectedDocType;
            set
            {
                _selectedDocType = value;
                OnPropertyChanged();
                SetDatesByDocumentType(_selectedDocType);
                FilterDocumentsByType();
            }
        }

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

        public ObservableCollection<DocumentModel> Documents { get; set; } = new ObservableCollection<DocumentModel>();

        private DocumentModel _selectedDocument;
        public DocumentModel SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                OnPropertyChanged();
                if (_selectedDocument != null)
                {
                    SelectedDocumentPath = _selectedDocument.Path;
                    _ = CheckAndLoadDocumentPreview(_selectedDocument);
                }
            }
        }

        private async Task CheckAndLoadDocumentPreview(DocumentModel doc)
        {
            try
            {
                if (string.IsNullOrEmpty(doc.Path) || !File.Exists(doc.Path))
                {
                    if (!string.IsNullOrEmpty(UID) && !string.IsNullOrEmpty(doc.Type))
                    {
                        string tempDir = Path.GetTempPath();
                        string tempFilePath = Path.Combine(tempDir, $"{UID}_{doc.Type}_preview.docx");

                        string downloadUrl = $"{ApiSettings.BaseUrl}/api/Data/DownloadFileByUidAndType/{UID}/{doc.Type}";
                        var response = await _httpClient.GetAsync(downloadUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var bytes = await response.Content.ReadAsByteArrayAsync();
                            await File.WriteAllBytesAsync(tempFilePath, bytes);

                            doc.Path = tempFilePath;
                            SelectedDocumentPath = tempFilePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Auto-preview load failed: " + ex.Message);
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowPopupCommand { get; }
        public ICommand ClosePopupCommand { get; }
        public ICommand UploadInPopupCommand { get; }
        public ICommand SavePopupDataCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand BackCommand { get; }

        public UserDocInfoViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _apiUrl = ApiSettings.SaveUserDocUrl();
            _empDetailApiUrl = ApiSettings.GetEmployeeDetailUrl();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            AddCommand = new RelayCommand(async (obj) => {
                string docType = obj as string ?? SelectedDocType;
                if (string.IsNullOrEmpty(UID)) { MessageBox.Show("Please enter or select a UID first!"); return; }
                if (string.IsNullOrEmpty(docType)) { MessageBox.Show("Please select a Document Type!"); return; }

                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "All Files|*.*|Images|*.png;*.jpg;*.jpeg;*.pdf|Word Documents|*.docx;*.doc" };
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
                PopupTitle = obj as string ?? SelectedDocType;
                IsPopupVisible = true;

                PopupStartDate = this.StartDate;
                PopupEndDate = this.EndDate;
            });

            ClosePopupCommand = new RelayCommand((obj) => {
                IsPopupVisible = false;
                ClearPopupFields();
            });

            UploadInPopupCommand = new RelayCommand((obj) => {
                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "All Files|*.*|Images|*.png;*.jpg;*.jpeg;*.pdf|Word Documents|*.docx;*.doc" };
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

            DownloadCommand = new RelayCommand(async (obj) => {
                if (SelectedDocument != null && !string.IsNullOrEmpty(UID) && !string.IsNullOrEmpty(SelectedDocument.Type))
                {
                    await DownloadFileFromServerAsync(UID, SelectedDocument.Type);
                }
                else if (SelectedDocument != null)
                {
                    DownloadFile(SelectedDocument.Path);
                }
                else
                {
                    MessageBox.Show("Please select a document from the list to download.");
                }
            });

            BackCommand = new RelayCommand((obj) => {
                if (obj is Window currentWindow)
                {
                    currentWindow.Close();
                }
            });
        }

        private async Task DownloadFileFromServerAsync(string uid, string docType)
        {
            try
            {
                string downloadUrl = $"{ApiSettings.BaseUrl}/api/Data/DownloadFileByUidAndType/{uid}/{docType}";
                var response = await _httpClient.GetAsync(downloadUrl);

                if (response.IsSuccessStatusCode)
                {
                    string fileName = $"{uid}_{docType}.docx";
                    if (response.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                    {
                        fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = fileName,
                        Filter = "All Files|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(saveFileDialog.FileName, bytes);
                        MessageBox.Show("File Downloaded/Saved Successfully!");
                    }
                }
                else
                {
                    if (SelectedDocument != null && !string.IsNullOrEmpty(SelectedDocument.Path))
                    {
                        DownloadFile(SelectedDocument.Path);
                    }
                    else
                    {
                        MessageBox.Show($"Download failed from server: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (SelectedDocument != null && !string.IsNullOrEmpty(SelectedDocument.Path))
                {
                    DownloadFile(SelectedDocument.Path);
                }
                else
                {
                    MessageBox.Show("Download failed: " + ex.Message);
                }
            }
        }

        private void SetDatesByDocumentType(string docType)
        {
            if (string.IsNullOrEmpty(docType)) return;

            StartDate = DateTime.Now;

            switch (docType)
            {
                case "Passport":
                    EndDate = StartDate.AddYears(10);
                    break;
                case "Driving Licence":
                    EndDate = StartDate.AddYears(5);
                    break;
                case "I-20":
                case "EAD":
                case "I-983":
                    EndDate = StartDate.AddYears(1);
                    break;
                case "Resume":
                case "Picture":
                case "Certificate":
                case "Offer Letter":
                    EndDate = StartDate.AddYears(1);
                    break;
                default:
                    EndDate = StartDate.AddYears(1);
                    break;
            }
        }

        private void FilterDocumentsByType()
        {
            if (string.IsNullOrEmpty(SelectedDocType))
            {
                Documents.Clear();
                return;
            }

            var filteredList = _allDocuments
                .Where(x => x.Uid == UID && !string.IsNullOrEmpty(x.Path))
                .Where(x =>
                {
                    string fileName = System.IO.Path.GetFileName(x.Path).ToLower();
                    string docType = (x.Type ?? "").ToLower();
                    string selectedType = SelectedDocType.Trim().ToLower();

                    if (selectedType == "offer letter")
                    {
                        return fileName.Contains("opt") ||
                               fileName.Contains("stem") ||
                               fileName.Contains("cpt") ||
                               fileName.Contains("offer") ||
                               docType.Contains("opt") ||
                               docType.Contains("stem") ||
                               docType.Contains("cpt") ||
                               docType.Contains("offer");
                    }

                    return docType.Equals(selectedType, StringComparison.OrdinalIgnoreCase) ||
                           docType.Contains(selectedType, StringComparison.OrdinalIgnoreCase) ||
                           fileName.Contains(selectedType, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            Documents.Clear();
            foreach (var doc in filteredList)
            {
                Documents.Add(doc);
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Data/GetUserDocs");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var responseList = JsonConvert.DeserializeObject<List<DocumentModel>>(json);
                    if (responseList != null)
                    {
                        _allDocuments = responseList;
                    }
                }
            }
            catch (Exception ex)
            {
                ServerStatus = "Error loading documents: " + ex.Message;
            }
        }

        private async Task LoadEmployeeNameAsync(string uid)
        {
            try
            {
                var response = await _httpClient.GetAsync(_empDetailApiUrl + uid);
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
            catch
            {
                this.EmployeeName = "Error Loading Name";
            }
        }

        private void AddDocumentToGrid(string type, string path)
        {
            var newDoc = new DocumentModel
            {
                Type = type,
                Path = path,
                Action = "View",
                CustomStartDate = this.StartDate,
                CustomEndDate = this.EndDate,
                EntryDate = DateTime.Now
            };
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
                EntryDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Activate = "Y",
                DocumentList = this.Documents.Select(d => new {
                    Type = d.Type,
                    Path = d.Path,
                    Action = d.Action,
                    FileName = System.IO.Path.GetFileName(d.Path),
                    EmployerName = d.EmployerName,
                    JobTitle = d.JobTitle,
                    CustomStartDate = d.CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = d.CustomEndDate?.ToString("yyyy-MM-dd"),
                    EntryDate = d.EntryDate?.ToString("yyyy-MM-dd")
                }).ToList()
            };

            try
            {
                var json = JsonConvert.SerializeObject(saveRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    ServerStatus = $"Data Saved Successfully at {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    MessageBox.Show($"Server Error: {response.StatusCode}");
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

    public class DocumentModel : INotifyPropertyChanged
    {
        private string _uid;
        private string _type;
        private string _path;
        private string _action;
        private string _employerName;
        private string _jobTitle;
        private DateTime? _entryDate;
        private DateTime? _customStartDate;
        private DateTime? _customEndDate;

        [JsonProperty("uid")]
        public string Uid { get => _uid; set { _uid = value; OnPropertyChanged(); } }

        [JsonProperty("docType")]
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }

        [JsonProperty("docPath")]
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewSource));
            }
        }

        [JsonProperty("action")]
        public string Action { get => _action; set { _action = value; OnPropertyChanged(); } }

        [JsonProperty("employerName")]
        public string EmployerName { get => _employerName; set { _employerName = value; OnPropertyChanged(); } }

        [JsonProperty("jobTitle")]
        public string JobTitle { get => _jobTitle; set { _jobTitle = value; OnPropertyChanged(); } }

        [JsonProperty("entryDate")]
        public DateTime? EntryDate { get => _entryDate; set { _entryDate = value; OnPropertyChanged(); } }

        [JsonProperty("startDate")]
        public DateTime? CustomStartDate { get => _customStartDate; set { _customStartDate = value; OnPropertyChanged(); } }

        [JsonProperty("endDate")]
        public DateTime? CustomEndDate { get => _customEndDate; set { _customEndDate = value; OnPropertyChanged(); } }

        public object PreviewSource
        {
            get
            {
                if (string.IsNullOrEmpty(_path)) return null;

                try
                {
                    string ext = System.IO.Path.GetExtension(_path).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif" || ext == ".tiff")
                    {
                        return _path;
                    }
                    else
                    {
                        return new Uri(_path, UriKind.Absolute).AbsoluteUri;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}