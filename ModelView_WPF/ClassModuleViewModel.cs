using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_WPF;
using System.ComponentModel;
using System.Windows;

namespace ModelView_WPF
{
    public class ClassModuleViewModel : INotifyPropertyChanged
    {
        // Model reference updated to ClassModuleModel
        private ClassModuleModel _classModule = new ClassModuleModel();
        public ClassModuleModel ClassModule
        {
            get => _classModule;
            set { _classModule = value; OnPropertyChanged(nameof(ClassModule)); }
        }

        private bool _isActiveChecked;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                _isActiveChecked = value;
                // 'Y' or 'N' logic preserved as per your previous code
                ClassModule.Activate = value ? "1" : "0";
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        public ClassModuleViewModel()
        {
            ResetForm();
            // Using a unique RelayCommand name to avoid ambiguity
            SaveCommand = new ClassModuleRelayCommand(async () => await SaveClassInfo());
        }

        private async Task SaveClassInfo()
        {
            // Validation for CourseCode as seen in your XAML
            if (string.IsNullOrEmpty(ClassModule.CourseCode))
            {
                MessageBox.Show("Please enter a Course Code.");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(ClassModule);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // API endpoint updated to ClassModule
                    var response = await client.PostAsync("http://74.208.227.170:8050//api/ClassModule", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Class Module Saved Successfully!");
                        ResetForm();
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API Connection Error: " + ex.Message);
            }
        }

        private void ResetForm()
        {
            ClassModule = new ClassModuleModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                Activate = "0"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Unique RelayCommand for ClassModule to prevent naming conflicts
    public class ClassModuleRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public ClassModuleRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public async void Execute(object parameter) => await _execute();
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}