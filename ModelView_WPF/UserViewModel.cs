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
using View_WPF.Models;

namespace View_WPF.ModelView_WPF
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _getUsersUrl;

        private string _uid;
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private DateTime? _entryDate = DateTime.Now;
        public DateTime? EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }

        private DateTime? _startDate = DateTime.Now;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime? _endDate = DateTime.Now;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        private string _isActive;
        public string IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        private ObservableCollection<UserModel> _userList;
        public ObservableCollection<UserModel> UserList
        {
            get => _userList;
            set { _userList = value; OnPropertyChanged(); }
        }

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                if (_selectedUser != null)
                {
                    UID = _selectedUser.UID;
                    Password = _selectedUser.Password;
                    Phone = _selectedUser.Phone;
                    Email = _selectedUser.Email;
                    StartDate = _selectedUser.StartDate ?? DateTime.Now;
                    EndDate = _selectedUser.EndDate ?? DateTime.Now;
                    EntryDate = _selectedUser.EntryDate ?? DateTime.Now;
                    IsActive = _selectedUser.IsActive;
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }

        public UserViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _apiUrl = ApiSettings.SaveUserUrl();
            _getUsersUrl = ApiSettings.GetUsersUrl;

            UserList = new ObservableCollection<UserModel>();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            AddCommand = new RelayCommand((obj) => ExecuteAdd());
            EditCommand = new RelayCommand((obj) => ExecuteEdit());
            DeleteCommand = new RelayCommand(async (obj) => await ExecuteDelete());
            ExitCommand = new RelayCommand((obj) => ExecuteExit(obj));

            _ = Task.Run(async () => await LoadUsersAsync());
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_getUsersUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<UserModel>>(json);
                    if (users != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UserList.Clear();
                            foreach (var user in users)
                            {
                                UserList.Add(user);
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
            var userData = new
            {
                Uid = UID,
                Password = Password,
                Phone = Phone,
                Email = Email,
                EntryDate = EntryDate,
                StartDate = StartDate,
                EndDate = EndDate,
                Activate = IsActive
            };

            try
            {
                var json = JsonConvert.SerializeObject(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("User Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsersAsync();
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

        private void ExecuteAdd()
        {
            UID = string.Empty;
            Password = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            EntryDate = DateTime.Now;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            IsActive = "1";
            SelectedUser = null;
        }

        private void ExecuteEdit()
        {
            MessageBox.Show("Edit functionality will be added later.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ExecuteDelete()
        {
            if (string.IsNullOrEmpty(UID))
            {
                MessageBox.Show("Please select a user to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete user {UID}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteUser/{UID}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("User Deleted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ExecuteAdd();
                        await LoadUsersAsync();
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}