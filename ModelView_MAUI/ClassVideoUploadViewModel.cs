using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls; // MAUI Command এর জন্য

namespace View_MAUI.ViewModel_MAUI
{
    public class ClassVideoUploadViewModel : INotifyPropertyChanged
    {
        // লোকালহোস্ট টেস্ট করার সময় অ্যান্ড্রয়েড এমুলেটর হলে 10.0.2.2 ব্যবহার করতে হতে পারে
        private readonly string _apiUrl = "https://localhost:7169/api/ClassVideoUpload/SaveVideoUpload";

        private bool _isActive;
        private string _classId;
        private string _sectionId;
        private string _subjectCode;
        private string _courseCode;

        public ClassVideoUploadViewModel()
        {
            ResetForm();
            // MAUI এর নিজস্ব Command ব্যবহার করা হয়েছে
            SaveCommand = new Command(async () => await ExecuteSave());
        }

        // MAUI-তে টু-ওয়ে বাইন্ডিং নিখুঁত রাখতে ফিল্ড ও অনপ্রোপার্টিচেঞ্জড যুক্ত করা হয়েছে
        public string ClassID
        {
            get => _classId;
            set { _classId = value; OnPropertyChanged(); }
        }

        public string SectionID
        {
            get => _sectionId;
            set { _sectionId = value; OnPropertyChanged(); }
        }

        public string SubjectCode
        {
            get => _subjectCode;
            set { _subjectCode = value; OnPropertyChanged(); }
        }

        public string CourseCode
        {
            get => _courseCode;
            set { _courseCode = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        private async Task ExecuteSave()
        {
            // ভ্যালিডেশন চেক (ঐচ্ছিক কিন্তু নিরাপদ)
            if (string.IsNullOrWhiteSpace(ClassID) || string.IsNullOrWhiteSpace(SectionID))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please fill required fields.", "OK");
                return;
            }

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
                        // MAUI স্টাইল অ্যালার্ট ডায়ালগ
                        await Shell.Current.DisplayAlert("Success", "Saved Successfully!", "OK");
                        ResetForm();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Error saving data.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        private void ResetForm()
        {
            ClassID = "";
            SectionID = "";
            SubjectCode = "";
            CourseCode = "";
            IsActive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}