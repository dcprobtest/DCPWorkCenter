using ModelView_WPF;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace View_WPF.ModelView_WPF
{
    public class ClassVideoUploadViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "https://localhost:7169/api/ClassVideoUpload/SaveVideoUpload";

        private bool _isActive;

        public ClassVideoUploadViewModel()
        {
            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        public string ClassID { get; set; }
        public string SectionID { get; set; }
        public string SubjectCode { get; set; }
        public string CourseCode { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        private async Task ExecuteSave()
        {
            var data = new
            {
                ClassID = this.ClassID,
                SectionID = this.SectionID,
                SubjectCode = this.SubjectCode,
                CourseCode = this.CourseCode,
                Activate = IsActive ? "Y" : "N"
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Error saving data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ResetForm()
        {
            ClassID = SectionID = SubjectCode = CourseCode = "";
            IsActive = false;
            OnPropertyChanged(string.Empty); // শোরুমে সব প্রোপার্টি রিফ্রেশ করার জন্য
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}