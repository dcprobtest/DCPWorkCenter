using ModelView_WPF;
using ModelView_WPF.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using View_WPF.Model_WPF;

namespace View_WPF.ModelView_WPF
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _getDepartmentsUrl;
        private string _uid;

        private DepartmentModel _dept;
        private bool _isActive;
        private bool _isEditing = false;

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DepartmentModel> _deptList;
        public ObservableCollection<DepartmentModel> DeptList
        {
            get => _deptList;
            set { _deptList = value; OnPropertyChanged(); }
        }

        private DepartmentModel _selectedDept;
        public DepartmentModel SelectedDept
        {
            get => _selectedDept;
            set
            {
                _selectedDept = value;
                OnPropertyChanged();
                if (_selectedDept != null)
                {
                    Dept = new DepartmentModel
                    {
                        DeptCode = _selectedDept.DeptCode,
                        DeptName = _selectedDept.DeptName,
                        EntryDate = _selectedDept.EntryDate ?? DateTime.Now,
                        StartDate = _selectedDept.StartDate ?? DateTime.Now,
                        EndDate = _selectedDept.EndDate ?? DateTime.Now,
                        Responsiblity = _selectedDept.Responsiblity,
                        Description = _selectedDept.Description,
                        Activate = _selectedDept.Activate,
                        EntryByUID = _selectedDept.EntryByUID
                    };
                    IsActive = (_selectedDept.Activate == "Y" || _selectedDept.Activate == "1");
                    IsEditing = false;
                }
            }
        }

        public DepartmentViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _getDepartmentsUrl = ApiSettings.GetDepartmentsUrl; // Ensure this endpoint exists in your ApiSettings

            DeptList = new ObservableCollection<DepartmentModel>();

            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            AddCommand = new RelayCommand((obj) => ExecuteAdd());
            EditCommand = new RelayCommand((obj) => ExecuteEdit());
            DeleteCommand = new RelayCommand(async (obj) => await ExecuteDelete());
            ExitCommand = new RelayCommand((obj) => ExecuteExit(obj));

            IsEditing = false;
            _ = Task.Run(async () => await LoadDepartmentsAsync());
        }

        public DepartmentViewModel(string uid) : this()
        {
            _uid = uid;
        }

        public DepartmentModel Dept
        {
            get => _dept;
            set { _dept = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (Dept != null) Dept.Activate = value ? "Y" : "N";
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_getDepartmentsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var departments = JsonConvert.DeserializeObject<List<DepartmentModel>>(json);
                    if (departments != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DeptList.Clear();
                            foreach (var dept in departments)
                            {
                                DeptList.Add(dept);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task ExecuteSave()
        {
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                MessageBox.Show("Please provide a Department Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_uid))
                {
                    Dept.EntryByUID = _uid;
                }

                var json = JsonConvert.SerializeObject(Dept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(ApiSettings.SaveDepartmentUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Department saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditing = false;
                    await LoadDepartmentsAsync();
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"API Error ({response.StatusCode}):\nPath not found or data format issue.\n{errorDetails}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to establish connection with the API.\nPlease check if the API is running.\nDetails: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd()
        {
            ResetForm();
            SelectedDept = null;
            IsEditing = true;
        }

        private void ExecuteEdit()
        {
            if (SelectedDept == null && (Dept == null || string.IsNullOrEmpty(Dept.DeptCode)))
            {
                MessageBox.Show("Please select a department from the list first to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditing = true;
        }

        private async Task ExecuteDelete()
        {
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                MessageBox.Show("Please select a department to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete department {Dept.DeptCode}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteDepartment/{Dept.DeptCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Department Deleted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ExecuteAdd();
                        await LoadDepartmentsAsync();
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.Close();
                        return;
                    }
                }

                if (Application.Current.MainWindow != null && Application.Current.MainWindow != Application.Current.Windows[0])
                {
                    Application.Current.MainWindow.Close();
                }
            }
        }

        private void ResetForm()
        {
            Dept = new DepartmentModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Activate = "N"
            };
            IsActive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}