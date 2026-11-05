using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_WPF;
using ModelView_WPF.Helpers;
using System.ComponentModel;
using System.Windows;

namespace ModelView_WPF
{
    public class DesignationViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _loggedInUserId;
        private readonly string _getDesignationsUrl;

        private DesignationModel _designation = new DesignationModel();
        public DesignationModel Designation
        {
            get => _designation;
            set
            {
                _designation = value;
                OnPropertyChanged(nameof(Designation));
            }
        }

        private bool _isActiveChecked;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                _isActiveChecked = value;
                if (Designation != null)
                {
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        private ObservableCollection<DesignationModel> _designationList;
        public ObservableCollection<DesignationModel> DesignationList
        {
            get => _designationList;
            set { _designationList = value; OnPropertyChanged(nameof(DesignationList)); }
        }

        private DesignationModel _selectedDesignation;
        public DesignationModel SelectedDesignation
        {
            get => _selectedDesignation;
            set
            {
                _selectedDesignation = value;
                OnPropertyChanged(nameof(SelectedDesignation));
                if (_selectedDesignation != null)
                {
                    Designation = new DesignationModel
                    {
                        DesigCode = _selectedDesignation.DesigCode,
                        DesigName = _selectedDesignation.DesigName,
                        EntryDate = _selectedDesignation.EntryDate ?? DateTime.Now,
                        StartDate = _selectedDesignation.StartDate ?? DateTime.Now,
                        EndDate = _selectedDesignation.EndDate,
                        Responsiblity = _selectedDesignation.Responsiblity,
                        Description = _selectedDesignation.Description,
                        Activate = _selectedDesignation.Activate,
                        EntryByUid = _selectedDesignation.EntryByUid,
                        LastUpdate = _selectedDesignation.LastUpdate
                    };
                    IsActiveChecked = (_selectedDesignation.Activate == "Y" || _selectedDesignation.Activate == "1");
                    IsEditing = false;
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }

        public DesignationViewModel() : this("Admin")
        {
        }

        public DesignationViewModel(string userId)
        {
            _loggedInUserId = string.IsNullOrWhiteSpace(userId) ? "Admin" : userId;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _getDesignationsUrl = ApiSettings.GetDesignationsUrl; // Ensure this property exists in your ApiSettings

            DesignationList = new ObservableCollection<DesignationModel>();

            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await SaveDesignation());
            AddCommand = new RelayCommand((obj) => ExecuteAdd());
            EditCommand = new RelayCommand((obj) => ExecuteEdit());
            DeleteCommand = new RelayCommand(async (obj) => await ExecuteDelete());
            ExitCommand = new RelayCommand((obj) => ExecuteExit(obj));

            IsEditing = false;
            _ = Task.Run(async () => await LoadDesignationsAsync());
        }

        private async Task LoadDesignationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_getDesignationsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var designations = JsonConvert.DeserializeObject<List<DesignationModel>>(json);
                    if (designations != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DesignationList.Clear();
                            foreach (var desig in designations)
                            {
                                DesignationList.Add(desig);
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

        private async Task SaveDesignation()
        {
            if (Designation == null || string.IsNullOrWhiteSpace(Designation.DesigCode))
            {
                MessageBox.Show("Please enter a Designation Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Designation.LastUpdate = DateTime.Now;
                if (!string.IsNullOrEmpty(_loggedInUserId))
                {
                    Designation.EntryByUid = _loggedInUserId;
                }

                var json = JsonConvert.SerializeObject(Designation);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiSettings.SaveDesignationUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Designation Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditing = false;
                    await LoadDesignationsAsync();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to API.\nException: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd()
        {
            ResetForm();
            SelectedDesignation = null;
            IsEditing = true;
        }

        private void ExecuteEdit()
        {
            if (SelectedDesignation == null && (Designation == null || string.IsNullOrEmpty(Designation.DesigCode)))
            {
                MessageBox.Show("Please select a designation from the list first to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditing = true;
        }

        private async Task ExecuteDelete()
        {
            if (Designation == null || string.IsNullOrEmpty(Designation.DesigCode))
            {
                MessageBox.Show("Please select a designation to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete designation {Designation.DesigCode}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteDesignation/{Designation.DesigCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Designation Deleted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ExecuteAdd();
                        await LoadDesignationsAsync();
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
            Designation = new DesignationModel
            {
                DesigCode = string.Empty,
                DesigName = string.Empty,
                Description = string.Empty,
                Responsiblity = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = null,
                EntryByUid = _loggedInUserId,
                LastUpdate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}