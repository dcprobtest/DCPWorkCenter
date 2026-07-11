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
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ModelView_MAUI;
using Newtonsoft.Json;
using View_MAUI.Model_MAUI; // আপনার প্রজেক্টের সঠিক মডেল ফোল্ডারের নেমস্পেসটি দিন

namespace View_MAUI.ViewModel_MAUI
{
    public class ClassVideoTextViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "http://74.208.227.170:8050//api/Data/SaveVideo";
        private readonly string _baseApiUrl = "http://74.208.227.170:8050//api/Data/";

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

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand ExitCommand { get; }

        public ClassVideoTextViewModel(string uid)
        {
            InitializeClassVideo();

            if (ClassVideo != null)
            {
                ClassVideo.EntryByUID = uid;
            }

            LoadSubjects();
            LoadClassVideos();

            SaveCommand = new Command(async () => await ExecuteSave());
            BrowseCommand = new Command(async () => await ExecuteBrowse());
            AddCommand = new Command(() => ResetForm());
            EditCommand = new Command(() => ExecuteEdit());
            DeleteFileCommand = new Command(async () => await ExecuteDelete());
            DownloadCommand = new Command(async () => await ExecuteDownload());
            ExitCommand = new Command(async () => await ExecuteExit());
        }

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

                        // MAUI Main Thread Dispatcher
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ClassVideoList.Clear();
                            foreach (var item in list) { ClassVideoList.Add(item); }
                            BuildTreeView(list);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // MAUI UI Alert Error Handler Fixed
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Application.Current.MainPage.DisplayAlert("Error", "Video List Load Error: " + ex.Message, "OK"));
            }
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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Subject Load Error: " + ex.Message, "OK");
            }
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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Section Load Error: " + ex.Message, "OK");
            }
        }

        private async Task ExecuteBrowse()
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Please select a video file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".mp4", ".wmv", ".avi" } },
                        { DevicePlatform.MacCatalyst, new[] { "mp4", "wmv", "avi" } },
                        { DevicePlatform.Android, new[] { "video/*" } },
                        { DevicePlatform.iOS, new[] { "public.movie" } }
                    })
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    _selectedFileSourcePath = result.FullPath;
                    await Application.Current.MainPage.DisplayAlert("File Selected", $"File Selected: {result.FileName}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "File selection failed: " + ex.Message, "OK");
            }
        }

        private async Task ExecuteSave()
        {
            if (SelectedSubject == null || SelectedSection == null || (string.IsNullOrEmpty(_selectedFileSourcePath) && string.IsNullOrEmpty(ClassVideo.ClassPath)))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Please select Subject, Section and Video File.", "OK");
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_selectedFileSourcePath))
                {
                    string dateStr = DateTime.Now.ToString("ddMMMyyyy");
                    string fileName = $"{SelectedSubject.SubjectCode}_{SelectedSubject.SubjectDesc}_{ClassVideo.ClassID}_{dateStr}{Path.GetExtension(_selectedFileSourcePath)}";

                    string rootPath = FileSystem.AppDataDirectory;
                    string targetPath = Path.Combine(rootPath, "DCP_Videos", SelectedSubject.SubjectDesc, SelectedSection.SectionDesc);

                    if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                    string fullPath = Path.Combine(targetPath, fileName);

                    File.Copy(_selectedFileSourcePath, fullPath, true);
                    ClassVideo.ClassPath = fullPath;
                }

                ClassVideo.LastUpdate = DateTime.Now;

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(ClassVideo);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Class Video Saved Successfully!", "OK");
                        ResetForm();
                        LoadClassVideos();
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        await Application.Current.MainPage.DisplayAlert("Error", $"API Error: {errorMsg}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Operation Error", $"Error: {ex.Message}", "OK");
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

            bool result = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to delete this record?", "Yes", "No");
            if (result)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.DeleteAsync(_baseApiUrl + "DeleteVideo/" + SelectedItemInGrid.ClassID);
                        if (response.IsSuccessStatusCode)
                        {
                            await Application.Current.MainPage.DisplayAlert("Success", "Deleted Successfully", "OK");
                            LoadClassVideos();
                            ResetForm();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Delete Error", ex.Message, "OK");
                }
            }
        }

        private async Task ExecuteDownload()
        {
            if (SelectedItemInGrid == null || string.IsNullOrEmpty(SelectedItemInGrid.ClassPath)) return;

            try
            {
                string targetDir = Path.Combine(FileSystem.CacheDirectory, "Downloads");
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                string targetPath = Path.Combine(targetDir, Path.GetFileName(SelectedItemInGrid.ClassPath));
                File.Copy(SelectedItemInGrid.ClassPath, targetPath, true);

                await Application.Current.MainPage.DisplayAlert("Download Success", $"File saved to app cache: {targetPath}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteExit()
        {
            if (Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
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