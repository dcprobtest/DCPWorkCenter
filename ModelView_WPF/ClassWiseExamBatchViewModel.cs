using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class ClassWiseExamBatchViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        private string _batchID;
        public string BatchID { get => _batchID; set { _batchID = value; OnPropertyChanged(); } }

        private DateTime? _batchDate = DateTime.Now;
        public DateTime? BatchDate { get => _batchDate; set { _batchDate = value; OnPropertyChanged(); } }

        public ObservableCollection<ClassWiseExamBatchModel> BatchList { get; set; }

        private ClassWiseExamBatchModel _selectedBatch;
        public ClassWiseExamBatchModel SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                _selectedBatch = value;
                if (_selectedBatch != null)
                {
                    BatchID = _selectedBatch.BatchID;
                    BatchDate = _selectedBatch.BatchDate;
                }
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public ClassWiseExamBatchViewModel()
        {
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            BatchList = new ObservableCollection<ClassWiseExamBatchModel>();

            AddCommand = new RelayCommand(o => ClearFields());
            SaveCommand = new RelayCommand(async o => await SaveData());
            EditCommand = new RelayCommand(o => EditData());
            ExitCommand = new RelayCommand(o => Application.Current.MainWindow.Close());

            _ = LoadData();
        }

        private void ClearFields()
        {
            BatchID = string.Empty;
            BatchDate = DateTime.Now;
            SelectedBatch = null;
        }

        private async Task SaveData()
        {
            try
            {
                // Added Activate = "Y" to satisfy API validation requirements
                var newBatch = new ClassWiseExamBatchModel
                {
                    BatchID = this.BatchID,
                    BatchDate = this.BatchDate,
                    Activate = "Y"
                };

                // Saving data using ApiSettings endpoint
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.GetClassWiseExamBatchesUrl(), newBatch);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data Saved Successfully for Batch: " + BatchID);
                    await LoadData();
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save data. Status: {response.StatusCode}\nDetails: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message);
            }
        }

        private void EditData()
        {
            if (SelectedBatch != null)
            {
                MessageBox.Show("Ready to edit Batch: " + SelectedBatch.BatchID);
            }
        }

        private async Task LoadData()
        {
            try
            {
                // Loading data using ApiSettings endpoint
                var response = await _httpClient.GetAsync(ApiSettings.GetClassWiseExamBatchesUrl());
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ObservableCollection<ClassWiseExamBatchModel>>();
                    BatchList.Clear();
                    foreach (var item in data)
                    {
                        BatchList.Add(item);
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to load data. Status: {response.StatusCode}\nDetails: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Data Error: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}