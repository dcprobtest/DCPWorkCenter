using ModelView_WPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; // File Dialog এর জন্য
using View_WPF;
using View_WPF.Model_WPF;

namespace View_WPF.ModelView_WPF
{
    public class ClassVideoTextViewModel : INotifyPropertyChanged
    {
        // API URL আপনার কন্ট্রোলার অনুযায়ী আপডেট করা হয়েছে
        private readonly string _apiUrl = "https://localhost:7073/api/Data/SaveVideo";
        private readonly string _baseApiUrl = "https://localhost:7073/api/Data/";

        private ClassVideoTextModel _classVideo;
        private bool _isActive;
        private string _selectedFileSourcePath;

        // ড্রপডাউন লিস্টের জন্য প্রোপার্টি
        public ObservableCollection<ClassSubject> SubjectList { get; set; } = new ObservableCollection<ClassSubject>();
        public ObservableCollection<ClassSection> SectionList { get; set; } = new ObservableCollection<ClassSection>();

        // ইন্টারফেসের লিস্টে Class ID এবং Class Content দেখানোর জন্য নতুন কালেকশন
        public ObservableCollection<ClassVideoTextModel> ClassVideoList { get; set; } = new ObservableCollection<ClassVideoTextModel>();

        // সিলেক্টেড আইটেম হ্যান্ডেল করার জন্য
        private ClassSubject _selectedSubject;
        public ClassSubject SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                if (value != null)
                {
                    LoadSections(); // সাবজেক্ট সিলেক্ট করলে সেকশন লোড হবে
                }
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
                if (value != null && ClassVideo != null)
                {
                    ClassVideo.SectionID = value.SectionId; // অটোমেটিক সেকশন আইডি সেট
                }
            }
        }

        public ClassVideoTextViewModel()
        {
            InitializeClassVideo();
            LoadSubjects(); // শুরুতে সাবজেক্ট লিস্ট লোড হবে
            LoadClassVideos(); // শুরুতে ইন্টারফেসের লিস্টে ডেটা লোড করার জন্য কল করা হলো

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

        // API থেকে ইন্টারফেসের লিস্টের জন্য ডেটা নিয়ে আসা
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
                            foreach (var item in list)
                            {
                                ClassVideoList.Add(item);
                            }
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Video List Load Error: " + ex.Message); }
        }

        // API থেকে সাবজেক্ট লিস্ট নিয়ে আসা
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

        // API থেকে সেকশন লিস্ট নিয়ে আসা
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

        // ভিডিও ফাইল সিলেক্ট করা
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
            // ভ্যালিডেশন
            if (SelectedSubject == null || SelectedSection == null || string.IsNullOrEmpty(_selectedFileSourcePath))
            {
                MessageBox.Show("Please select Subject, Section and Video File.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ১. ফাইল নেমিং ফরম্যাট তৈরি
                string dateStr = DateTime.Now.ToString("ddMMMyyyy");
                string fileName = $"{SelectedSubject.SubjectCode}_{SelectedSubject.SubjectDesc}_{ClassVideo.ClassID}_{dateStr}{Path.GetExtension(_selectedFileSourcePath)}";

                // ২. ফোল্ডার পাথ তৈরি
                string rootPath = @"C:\DCP_Videos";
                string targetPath = Path.Combine(rootPath, SelectedSubject.SubjectDesc, SelectedSection.SectionDesc);

                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

                string fullPath = Path.Combine(targetPath, fileName);

                // ৩. ফাইল কপি করা
                File.Copy(_selectedFileSourcePath, fullPath, true);

                // ৪. ডাটাবেসে পাথ পাঠানোর জন্য মডেল আপডেট
                ClassVideo.ClassPath = fullPath;
                ClassVideo.LastUpdate = DateTime.Now;

                // ৫. API Call
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(ClassVideo);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Class Video Saved & Uploaded Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                        LoadClassVideos(); // সেভ হওয়ার পর লিস্টটি রিফ্রেশ করা হলো
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Error: {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Operation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeClassVideo()
        {
            ClassVideo = new ClassVideoTextModel
            {
                ClassID = 0,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Activate = false
            };
            _isActive = false;
        }

        private void ResetForm()
        {
            InitializeClassVideo();
            SelectedSubject = null;
            SelectedSection = null;
            _selectedFileSourcePath = null;
            OnPropertyChanged(nameof(IsActive));
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}