using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class ClassSection
    {
        public string SectionId { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public string? SectionDesc { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EntryByUid { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? Activate { get; set; }
    }

    public class SubjectCodeModel
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectDesc { get; set; } = string.Empty;

        public string DisplayText => string.IsNullOrWhiteSpace(SubjectCode) ? SubjectDesc : $"{SubjectCode} - {SubjectDesc}";
    }

    public class ClassSectionViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        private ClassSection _classSection = new ClassSection();
        public ClassSection ClassSection
        {
            get => _classSection;
            set { _classSection = value; OnPropertyChanged(); }
        }

        private bool _isAddButtonVisible;
        public bool IsAddButtonVisible
        {
            get => _isAddButtonVisible;
            set { _isAddButtonVisible = value; OnPropertyChanged(); }
        }

        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set { _isFormVisible = value; OnPropertyChanged(); }
        }

        private bool _isFieldsEditable;
        public bool IsFieldsEditable
        {
            get => _isFieldsEditable;
            set { _isFieldsEditable = value; OnPropertyChanged(); }
        }

        public ObservableCollection<object> CourseList { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<ClassSection> ClassSectionList { get; set; } = new ObservableCollection<ClassSection>();
        public ObservableCollection<SubjectCodeModel> SubjectCodeList { get; set; } = new ObservableCollection<SubjectCodeModel>();

        private SubjectCodeModel _selectedSubjectCode;
        public SubjectCodeModel SubjectCodeModel
        {
            get => _selectedSubjectCode;
            set
            {
                _selectedSubjectCode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSubjectSelected));

                if (_selectedSubjectCode != null)
                {
                    IsAddButtonVisible = true;
                    IsFormVisible = false;
                    IsFieldsEditable = false;
                    ClassSection.SubjectCode = _selectedSubjectCode.SubjectCode;
                    _ = LoadSectionsForSubject(_selectedSubjectCode.SubjectCode);
                }
                else
                {
                    IsAddButtonVisible = false;
                    IsFormVisible = false;
                    IsFieldsEditable = false;
                }
            }
        }

        public SubjectCodeModel SelectedSubjectCode
        {
            get => _selectedSubjectCode;
            set => SubjectCodeModel = value;
        }

        public bool IsSubjectSelected => SelectedSubjectCode != null;

        private ClassSection _selectedClassSection;
        public ClassSection SelectedClassSection
        {
            get => _selectedClassSection;
            set
            {
                _selectedClassSection = value;
                OnPropertyChanged();

                if (_selectedClassSection != null)
                {
                    IsFieldsEditable = false;
                    string currentUid = ClassSection.EntryByUid;

                    ClassSection = new ClassSection
                    {
                        SectionId = _selectedClassSection.SectionId,
                        SubjectCode = _selectedClassSection.SubjectCode,
                        SectionDesc = _selectedClassSection.SectionDesc,
                        EntryDate = _selectedClassSection.EntryDate,
                        StartDate = _selectedClassSection.StartDate,
                        EndDate = _selectedClassSection.EndDate,
                        EntryByUid = !string.IsNullOrEmpty(_selectedClassSection.EntryByUid) ? _selectedClassSection.EntryByUid : currentUid,
                        LastUpdate = _selectedClassSection.LastUpdate,
                        Activate = _selectedClassSection.Activate
                    };
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ActiveClickCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public ClassSectionViewModel() : this(string.Empty)
        {
        }

        public ClassSectionViewModel(string uid)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ClassSection = new ClassSection { EntryByUid = uid };
            IsFieldsEditable = false;

            AddCommand = new RelayCommand(obj => OnAdd());
            SaveCommand = new RelayCommand(async obj => await OnSave());
            ActiveClickCommand = new RelayCommand(obj => OnActive());
            DeleteCommand = new RelayCommand(async obj => await OnDelete());
            EditCommand = new RelayCommand(obj => OnEdit());
            ExitCommand = new RelayCommand(obj => OnExit());
        }

        public async Task LoadInitialDataAsync()
        {
            try
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(5);

                var courses = await _httpClient.GetFromJsonAsync<List<object>>(ApiSettings.GetClassSectionCoursesUrl()).ConfigureAwait(false);
                if (courses != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CourseList.Clear();
                        foreach (var c in courses) CourseList.Add(c);
                    });
                }

                var subjectCodes = await _httpClient.GetFromJsonAsync<List<ClassSubject>>("api/Data/GetSubjects").ConfigureAwait(false);
                if (subjectCodes != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SubjectCodeList.Clear();
                        foreach (var sc in subjectCodes)
                        {
                            SubjectCodeList.Add(new SubjectCodeModel
                            {
                                SubjectCode = sc.SubjectCode,
                                SubjectDesc = sc.SubjectDesc
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"API Connection Failed: {ex.Message}");
                });
            }
        }

        private async Task LoadSectionsForSubject(string subjectCode)
        {
            try
            {
                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>(ApiSettings.GetClassSectionsUrl()).ConfigureAwait(false);
                if (sections != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ClassSectionList.Clear();
                        foreach (var s in sections.Where(x => x.SubjectCode == subjectCode))
                        {
                            ClassSectionList.Add(s);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sections: {ex.Message}");
            }
        }

        private void GenerateNewSectionId()
        {
            if (SelectedSubjectCode == null || string.IsNullOrEmpty(SelectedSubjectCode.SubjectCode))
                return;

            string subjectPrefix = SelectedSubjectCode.SubjectCode;

            var existingSectionsForSubject = ClassSectionList?
                .Where(s => s.SectionId != null && s.SectionId.StartsWith(subjectPrefix))
                .ToList();

            if (existingSectionsForSubject == null || !existingSectionsForSubject.Any())
            {
                ClassSection.SectionId = subjectPrefix + "01";
            }
            else
            {
                int maxSequence = existingSectionsForSubject
                    .Select(s => {
                        string suffix = s.SectionId.Substring(Math.Max(0, s.SectionId.Length - 2));
                        return int.TryParse(suffix, out int val) ? val : 0;
                    })
                    .Max();

                int nextSequence = maxSequence + 1;
                ClassSection.SectionId = subjectPrefix + nextSequence.ToString("00");
            }

            OnPropertyChanged(nameof(ClassSection));
        }

        private void OnAdd()
        {
            IsFieldsEditable = true;
            string currentUid = ClassSection?.EntryByUid;
            string currentSubject = SelectedSubjectCode?.SubjectCode;

            ClassSection = new ClassSection
            {
                EntryByUid = currentUid,
                SubjectCode = currentSubject,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                Activate = "0"
            };

            GenerateNewSectionId();

            IsFormVisible = true;
        }

        private void OnActive()
        {
            if (ClassSection != null)
            {
                ClassSection.EndDate = DateTime.Now;
                ClassSection.Activate = "1";
                OnPropertyChanged(nameof(ClassSection));
                MessageBox.Show("Section has been set as Active.");
            }
        }

        private void OnEdit()
        {
            if (SelectedClassSection == null)
            {
                MessageBox.Show("Please select a section from the list to edit.");
                return;
            }

            IsFieldsEditable = true;

            ClassSection = new ClassSection
            {
                SectionId = SelectedClassSection.SectionId,
                SubjectCode = SelectedClassSection.SubjectCode,
                SectionDesc = SelectedClassSection.SectionDesc,
                EntryDate = SelectedClassSection.EntryDate,
                StartDate = SelectedClassSection.StartDate,
                EndDate = SelectedClassSection.EndDate,
                EntryByUid = !string.IsNullOrEmpty(SelectedClassSection.EntryByUid) ? SelectedClassSection.EntryByUid : ClassSection.EntryByUid,
                LastUpdate = SelectedClassSection.LastUpdate,
                Activate = SelectedClassSection.Activate
            };
        }

        private async Task OnSave()
        {
            if (string.IsNullOrEmpty(ClassSection.SectionId))
            {
                MessageBox.Show("Please provide a Section ID.");
                return;
            }

            if (SelectedSubjectCode != null && string.IsNullOrEmpty(ClassSection.SubjectCode))
            {
                ClassSection.SubjectCode = SelectedSubjectCode.SubjectCode;
            }

            // Folder Creation Logic for C:\DCP_Videos\[SubjectDesc]\[SectionDesc]
            if (SelectedSubjectCode != null && !string.IsNullOrWhiteSpace(SelectedSubjectCode.SubjectDesc) && !string.IsNullOrWhiteSpace(ClassSection.SectionDesc))
            {
                try
                {
                    string targetFolder = Path.Combine(@"C:\DCP_Videos", SelectedSubjectCode.SubjectDesc, ClassSection.SectionDesc);
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Warning: Could not create section folder. {ex.Message}");
                }
            }

            ClassSection.LastUpdate = DateTime.Now;
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.SaveClassSectionUrl(), ClassSection);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully saved to database!");
                    IsFieldsEditable = false;
                    if (SelectedSubjectCode != null)
                    {
                        await LoadSectionsForSubject(SelectedSubjectCode.SubjectCode);
                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Could not save. Status Code: {response.StatusCode}\nServer Message: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}");
            }
        }

        private async Task OnDelete()
        {
            if (SelectedClassSection == null)
            {
                MessageBox.Show("Please select a section first to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this section?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync(ApiSettings.DeleteClassSectionUrl(SelectedClassSection.SectionId));
                    if (response.IsSuccessStatusCode)
                    {
                        string currentUid = ClassSection.EntryByUid;
                        MessageBox.Show("Successfully deleted.");
                        IsFieldsEditable = false;
                        if (SelectedSubjectCode != null)
                        {
                            await LoadNamesOrSections();
                            await LoadSectionsForSubject(SelectedSubjectCode.SubjectCode);
                        }
                        ClassSection = new ClassSection { EntryByUid = currentUid };
                    }
                    else
                    {
                        MessageBox.Show("Could not delete from server.");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
            }
        }

        private Task LoadNamesOrSections() => Task.CompletedTask;

        private void OnExit()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}