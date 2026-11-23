using Model_WPF;
using ModelView_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModelView_WPF
{
    public class ClassFocusMonitorViewModel
    {
        private readonly HttpClient _httpClient;

        public ObservableCollection<ClassFocusMonitorModel> ClassFocusList { get; set; } = new ObservableCollection<ClassFocusMonitorModel>();
        public ObservableCollection<string> UidList { get; set; } = new ObservableCollection<string>();

        private string _selectedUid;
        public string SelectedUid
        {
            get => _selectedUid;
            set
            {
                _selectedUid = value;
                if (!string.IsNullOrEmpty(_selectedUid))
                {
                    _ = LoadClassFocusDataAsync(_selectedUid);
                }
            }
        }

        private ClassFocusMonitorModel _selectedItem;
        public ClassFocusMonitorModel SelectedItem
        {
            get => _selectedItem;
            set => _selectedItem = value;
        }

        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }

        public ClassFocusMonitorViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            SaveCommand = new RelayCommand(async (o) => await SaveClickAsync());
            EditCommand = new RelayCommand(async (o) => await EditClickAsync());
            DeleteCommand = new RelayCommand(async (o) => await DeleteClickAsync());
            ExitCommand = new RelayCommand((o) => ExitWindow(o));

            _ = LoadUidsAsync();
        }

        public async Task LoadUidsAsync()
        {
            try
            {
                UidList.Clear();
                var response = await _httpClient.GetFromJsonAsync<List<string>>(ApiSettings.ClassFocusMonitorUIDsUrl);
                if (response != null)
                {
                    foreach (var uid in response)
                    {
                        UidList.Add(uid);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading UIDs: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadClassFocusDataAsync(string uid)
        {
            try
            {
                ClassFocusList.Clear();
                string url = ApiSettings.GetClassFocusByUidUrl(uid);
                var response = await _httpClient.GetFromJsonAsync<List<ClassFocusMonitorModel>>(url);
                if (response != null)
                {
                    foreach (var item in response)
                    {
                        item.MonitorTypeID = GetMonitorTypeDesc(item.MonitorTypeID);
                        ClassFocusList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Class Focus data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetMonitorTypeDesc(string monitorTypeId)
        {
            return monitorTypeId switch
            {
                "1" => "Check Every 60 Second",
                "2" => "Check SlideBar",
                "3" => "Previous",
                "4" => "Play",
                "5" => "Pause",
                "6" => "Stop",
                "7" => "Next",
                "8" => "Exit Class",
                "9" => "Other Video",
                _ => monitorTypeId
            };
        }

        public async Task<bool> SaveAsync(ClassFocusMonitorModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.ClassFocusMonitorBaseUrl, model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ClassFocusMonitorModel model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(ApiSettings.UpdateClassFocusUrl(model.SessionID), model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string uid, int sessionId, string sectionId)
        {
            try
            {
                string url = ApiSettings.DeleteClassFocusUrl(sessionId);
                var response = await _httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task SaveClickAsync()
        {
            if (SelectedItem != null)
            {
                bool success = await SaveAsync(SelectedItem);
                if (success)
                {
                    MessageBox.Show("Data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!string.IsNullOrEmpty(SelectedUid))
                    {
                        await LoadClassFocusDataAsync(SelectedUid);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a record to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task EditClickAsync()
        {
            if (SelectedItem != null)
            {
                bool success = await UpdateAsync(SelectedItem);
                if (success)
                {
                    MessageBox.Show("Data updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!string.IsNullOrEmpty(SelectedUid))
                    {
                        await LoadClassFocusDataAsync(SelectedUid);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a record to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task DeleteClickAsync()
        {
            if (SelectedItem != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await DeleteAsync(SelectedItem.UID, SelectedItem.SessionID, SelectedItem.SectionID);
                    if (success)
                    {
                        MessageBox.Show("Data deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (!string.IsNullOrEmpty(SelectedUid))
                        {
                            await LoadClassFocusDataAsync(SelectedUid);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExitWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}