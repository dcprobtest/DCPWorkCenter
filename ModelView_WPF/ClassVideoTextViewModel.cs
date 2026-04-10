using ModelView_WPF;
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
        private readonly string _apiUrl = "https://localhost:7073/api/Data/SaveVideo";
        private readonly string _baseApiUrl = "https://localhost:7073/api/Data/";

        private ClassVideoTextModel _classVideo;
        private bool _isActive;
        private string _selectedFileSourcePath;

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

        // --- আপডেট করা কনস্ট্রাক্টর: UID গ্রহণ করার জন্য ---
        public ClassVideoTextViewModel(string uid)
        {
            InitializeClassVideo();

            // রিসিভ করা UID মডেলের EntryByUID-এ সেট করা হচ্ছে
            if (ClassVideo != null)
            {
                ClassVideo.EntryByUID = uid;
            }

            LoadSubjects();
            LoadClassVideos();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            BrowseCommand = new RelayCommand((obj) => ExecuteBrowse());
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

        public async void LoadClassVideos()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(_baseApiUrl + "GetVideos");
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
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(_baseApiUrl + "GetSubjects");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var list = JsonConvert.DeserializeObject<List<ClassSubject>>(json);
                        SubjectList.Clear();
                        foreach (var item in list) SubjectList.Add(item);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Subject Load Error: " + ex.Message); }
        }

        private async void LoadSections()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(_baseApiUrl + "GetSections");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var list = JsonConvert.DeserializeObject<List<ClassSection>>(json);
                        SectionList.Clear();
                        foreach (var item in list) SectionList.Add(item);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Section Load Error: " + ex.Message); }
        }

        private void ExecuteBrowse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4;*.wmv;*.avi)|*.mp4;*.wmv;*.avi";
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
                MessageBox.Show("Please select Subject, Section and Video File.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string dateStr = DateTime.Now.ToString("ddMMMyyyy");
                string fileName = $"{SelectedSubject.SubjectCode}_{SelectedSubject.SubjectDesc}_{ClassVideo.ClassID}_{dateStr}{Path.GetExtension(_selectedFileSourcePath)}";
                string rootPath = @"C:\DCP_Videos";
                string targetPath = Path.Combine(rootPath, SelectedSubject.SubjectDesc, SelectedSection.SectionDesc);

                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                string fullPath = Path.Combine(targetPath, fileName);

                File.Copy(_selectedFileSourcePath, fullPath, true);

                ClassVideo.ClassPath = fullPath;
                ClassVideo.LastUpdate = DateTime.Now;

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(ClassVideo);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Class Video Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                        LoadClassVideos();
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Error: {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Operation Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void InitializeClassVideo()
        {
            ClassVideo = new ClassVideoTextModel { ClassID = 0, EntryDate = DateTime.Now, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(7), Activate = false };
            _isActive = false;
        }

        private void ResetForm()
        {
            string currentUID = ClassVideo.EntryByUID; // রিসেট করার সময় UID ধরে রাখা
            InitializeClassVideo();
            ClassVideo.EntryByUID = currentUID;
            SelectedSubject = null;
            SelectedSection = null;
            _selectedFileSourcePath = null;
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