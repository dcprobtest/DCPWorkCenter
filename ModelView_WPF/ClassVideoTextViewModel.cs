using ModelView_WPF;
using ModelView_WPF.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using View_WPF;
using View_WPF.Model_WPF;

namespace View_WPF.ModelView_WPF
{
    public class ClassVideoTextViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        private ClassVideoTextModel _classVideo;
        private bool _isActive;
        private string _selectedFileSourcePath;
        private ClassVideoTextModel _selectedItemInGrid;

        public ObservableCollection<ClassSubject> SubjectList { get; set; } = new ObservableCollection<ClassSubject>();
        public ObservableCollection<ClassSection> SectionList { get; set; } = new ObservableCollection<ClassSection>();
        public ObservableCollection<ClassVideoTextModel> ClassVideoList { get; set; } = new ObservableCollection<ClassVideoTextModel>();
        public ObservableCollection<SubjectTreeItem> CourseContent { get; set; } = new ObservableCollection<SubjectTreeItem>();

        private ClassSubject _selectedSubject;
        public ClassSubject SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                if (value != null) { LoadSections(); }
            }
        }

        private ClassSection _selectedSection;
        public ClassSection SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged();
                if (value != null && ClassVideo != null) { ClassVideo.SectionID = value.SectionId; }
            }
        }

        public ClassVideoTextModel SelectedItemInGrid
        {
            get => _selectedItemInGrid;
            set { _selectedItemInGrid = value; OnPropertyChanged(); }
        }

        public ClassVideoTextViewModel(string uid)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _httpClient.Timeout = TimeSpan.FromMinutes(30);

            InitializeClassVideo();

            if (ClassVideo != null)
            {
                ClassVideo.EntryByUID = uid;
            }

            LoadSubjects();
            LoadClassVideos();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            BrowseCommand = new RelayCommand((obj) => ExecuteBrowse());

            AddCommand = new RelayCommand((obj) => ResetForm());
            EditCommand = new RelayCommand((obj) => ExecuteEdit());
            DeleteFileCommand = new RelayCommand(async (obj) => await ExecuteDelete());
            DownloadCommand = new RelayCommand((obj) => ExecuteDownload());
            ExitCommand = new RelayCommand((obj) => ExecuteExit(obj));
        }

        public ClassVideoTextModel ClassVideo
        {
            get => _classVideo;
            set { _classVideo = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (ClassVideo != null) { ClassVideo.Activate = value; }
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand BrowseCommand { get; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand ExitCommand { get; }

        public async void LoadClassVideos()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.GetClassVideosUrl());
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<ClassVideoTextModel>>(json);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ClassVideoList.Clear();
                        foreach (var item in list) { ClassVideoList.Add(item); }
                        BuildTreeView(list);
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("Video List Load Error: " + ex.Message); }
        }

        private void BuildTreeView(List<ClassVideoTextModel> allVideos)
        {
            CourseContent.Clear();
            foreach (var sub in SubjectList)
            {
                var subItem = new SubjectTreeItem { Title = sub.SubjectDesc };
                var relevantSections = SectionList.Where(s => s.SubjectCode == sub.SubjectCode);

                foreach (var sec in relevantSections)
                {
                    var secItem = new SectionTreeItem { Title = sec.SectionDesc };
                    var relevantVideos = allVideos.Where(v => v.SectionID == sec.SectionId);

                    foreach (var vid in relevantVideos)
                    {
                        secItem.Videos.Add(new VideoTreeItem
                        {
                            Title = vid.ClassContent,
                            FilePath = vid.ClassPath
                        });
                    }

                    if (secItem.Videos.Count > 0)
                        subItem.Sections.Add(secItem);
                }

                if (subItem.Sections.Count > 0)
                    CourseContent.Add(subItem);
            }
        }

        private async void LoadSubjects()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.GetClassSubjectsUrl());
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<ClassSubject>>(json);
                    SubjectList.Clear();
                    foreach (var item in list) SubjectList.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show("Subject Load Error: " + ex.Message); }
        }

        private async void LoadSections()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.GetClassSectionsUrl());
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<ClassSection>>(json);
                    SectionList.Clear();
                    foreach (var item in list) SectionList.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show("Section Load Error: " + ex.Message); }
        }

        private void ExecuteBrowse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Video files (*.*)|*.*|Video files (*.mp4;*.wmv;*.avi;*.mkv;*.mov)|*.mp4;*.wmv;*.avi;*.mkv;*.mov";
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFileSourcePath = openFileDialog.FileName;
                MessageBox.Show("File Selected: " + Path.GetFileName(_selectedFileSourcePath));
            }
        }

        private async Task ExecuteSave()
        {
            if (SelectedSubject == null || SelectedSection == null || string.IsNullOrEmpty(_selectedFileSourcePath))
            {
                MessageBox.Show("Please select Subject, Section and a Video File.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var fileStream = new FileStream(_selectedFileSourcePath, FileMode.Open, FileAccess.Read);
                    var streamContent = new StreamContent(fileStream);

                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    formData.Add(streamContent, "File", Path.GetFileName(_selectedFileSourcePath));

                    formData.Add(new StringContent(ClassVideo.ClassID.ToString()), "ClassID");
                    formData.Add(new StringContent(ClassVideo.ClassContent ?? ""), "ClassContent");
                    formData.Add(new StringContent(SelectedSection.SectionId.ToString()), "SectionId");
                    formData.Add(new StringContent(SelectedSubject.SubjectCode ?? ""), "SubjectCode");
                    formData.Add(new StringContent(SelectedSubject.SubjectDesc ?? ""), "SubjectDesc");
                    formData.Add(new StringContent(SelectedSection.SectionDesc ?? ""), "SectionDesc");
                    formData.Add(new StringContent(ClassVideo.EntryByUID ?? ""), "EntryByUID");
                    formData.Add(new StringContent(ClassVideo.Activate.ToString()), "Activate");

                    HttpResponseMessage response = await _httpClient.PostAsync("api/Data/UploadAndSaveVideo", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Video uploaded and path saved to database successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                        LoadClassVideos();
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        // ব্যাকএন্ড থেকে আসা সম্পূর্ণ আসল এরর মেসেজটি এখানে পপআপে দেখাবে
                        MessageBox.Show($"API Error: {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // ক্লায়েন্ট সাইডের কোনো এক্সেপশন বা নেটওয়ার্ক ফেইলিয়র হলে তার বিস্তারিত কারণ দেখাবে
                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Operation Error: {detailedError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit()
        {
            if (SelectedItemInGrid != null)
            {
                ClassVideo = SelectedItemInGrid;
                IsActive = ClassVideo.Activate;

                var section = SectionList.FirstOrDefault(s => s.SectionId == ClassVideo.SectionID);
                if (section != null)
                {
                    SelectedSection = section;
                    SelectedSubject = SubjectList.FirstOrDefault(s => s.SubjectCode == section.SubjectCode);
                }
            }
        }

        private async Task ExecuteDelete()
        {
            if (SelectedItemInGrid == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync(ApiSettings.DeleteClassVideoUrl(SelectedItemInGrid.ClassID.ToString()));
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Deleted Successfully");
                        LoadClassVideos();
                        ResetForm();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Delete Error: " + ex.Message); }
            }
        }

        private void ExecuteDownload()
        {
            if (SelectedItemInGrid == null || string.IsNullOrEmpty(SelectedItemInGrid.ClassPath)) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileName(SelectedItemInGrid.ClassPath);
            if (sfd.ShowDialog() == true)
            {
                try { File.Copy(SelectedItemInGrid.ClassPath, sfd.FileName, true); MessageBox.Show("Download Success"); }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void ExecuteExit(object obj)
        {
            if (obj is Window win) { win.Close(); }
        }

        private void InitializeClassVideo()
        {
            ClassVideo = new ClassVideoTextModel { ClassID = 0, EntryDate = DateTime.Now, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(7), Activate = false };
            _isActive = false;
        }

        private void ResetForm()
        {
            string currentUID = ClassVideo?.EntryByUID;
            InitializeClassVideo();
            if (ClassVideo != null) { ClassVideo.EntryByUID = currentUID; }
            SelectedSubject = null;
            SelectedSection = null;
            _selectedFileSourcePath = null;
            SelectedItemInGrid = null;
            OnPropertyChanged(nameof(IsActive));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }

    public class SubjectTreeItem
    {
        public string Title { get; set; }
        public ObservableCollection<SectionTreeItem> Sections { get; set; } = new ObservableCollection<SectionTreeItem>();
    }

    public class SectionTreeItem
    {
        public string Title { get; set; }
        public ObservableCollection<VideoTreeItem> Videos { get; set; } = new ObservableCollection<VideoTreeItem>();
    }

    public class VideoTreeItem
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
    }
}