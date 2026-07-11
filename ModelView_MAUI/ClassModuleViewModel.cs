using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_WPF;
using Model_MAUI; // আপনার মডেল ক্লাসের নেমস্পেস অনুযায়ী এটি পরিবর্তন করতে পারেন

namespace ModelView_MAUI
{
    public class ClassModuleViewModel : INotifyPropertyChanged
    {
        // Renamed to match the CurrentModule binding used in your MAUI XAML
        private Model_MAUI.ClassModuleModel _currentModule = new Model_MAUI.ClassModuleModel();
        public ClassModuleModel CurrentModule
        {
            get => _currentModule;
            set
            {
                _currentModule = value;
                OnPropertyChanged();
            }
        }

        private bool _isActiveChecked;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                _isActiveChecked = value;
                // Keeping your business logic ('1' for active, '0' for inactive)
                if (CurrentModule != null)
                {
                    CurrentModule.Activate = value ? "1" : "0";
                }
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        public ClassModuleViewModel()
        {
            ResetForm();

            // Using MAUI's native Command infrastructure instead of custom RelayCommand
            SaveCommand = new Command(async () => await SaveClassInfo());
        }

        private async Task SaveClassInfo()
        {
            // Validation for CourseCode
            if (string.IsNullOrEmpty(CurrentModule?.CourseCode))
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Alert", "Please enter a Course Code.", "OK");
                }
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(CurrentModule);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://74.208.227.170:8050//api/ClassModule", content);

                    if (Application.Current?.MainPage != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            await Application.Current.MainPage.DisplayAlert("Success", "Class Module Saved Successfully!", "OK");
                            ResetForm();
                        }
                        else
                        {
                            string errorMsg = await response.Content.ReadAsStringAsync();
                            await Application.Current.MainPage.DisplayAlert("Error", $"Error: {response.StatusCode}\nDetails: {errorMsg}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Connection Error", "API Connection Error: " + ex.Message, "OK");
                }
            }
        }

        private void ResetForm()
        {
            CurrentModule = new ClassModuleModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                Activate = "0"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}