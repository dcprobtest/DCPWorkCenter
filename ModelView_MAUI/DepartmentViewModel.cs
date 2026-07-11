using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls; // MAUI Command ব্যবহারের জন্য
using View_WPF.Model_MAUI; // আপনার প্রোজেক্ট স্ট্রাকচার অনুযায়ী এই মডেল নেমস্পেসটি পরিবর্তন করতে পারেন

namespace View_MAUI.ViewModel_MAUI
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "http://74.208.227.170:8050//api/Data/SaveDepartment";

        private DepartmentModel _dept;
        private bool _isActive;

        public DepartmentViewModel()
        {
            ResetForm();
            // MAUI এর বিল্ট-ইন Command মেকানিজম ব্যবহার করা হয়েছে
            SaveCommand = new Command(async () => await ExecuteSave());
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

        private async Task ExecuteSave()
        {
            // ১. বেসিক ভ্যালিডেশন চেক
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                await Shell.Current.DisplayAlert("Validation Error", "দয়া করে Department Code প্রদান করুন।", "OK");
                return;
            }

            try
            {
                // ২. SSL সার্টিফিকেট হ্যান্ডলার (WPF এর মতোই রাখা হয়েছে)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (HttpClient client = new HttpClient(handler))
                {
                    // ৩. JSON এ রূপান্তর
                    var json = JsonConvert.SerializeObject(Dept);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // ৪. API-তে ডাটা পাঠানো
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        await Shell.Current.DisplayAlert("Success", "Department সফলভাবে সেভ হয়েছে!", "OK");
                        ResetForm();
                    }
                    else
                    {
                        // ৫. এরর ডিটেইলস রিড করা
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        await Shell.Current.DisplayAlert("Error", $"API Error ({response.StatusCode}):\nপাথটি পাওয়া যাচ্ছে না বা ডাটা ফরম্যাটে সমস্যা আছে।\n{errorDetails}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Connection Error", $"API-র সাথে সংযোগ স্থাপন করা সম্ভব হয়নি।\nপোর্ট এবং API সচল আছে কিনা চেক করুন।\nবিস্তারিত: {ex.Message}", "OK");
            }
        }

        private void ResetForm()
        {
            Dept = new DepartmentModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now, // XAML-এর ডাটা বাইন্ডিং সেফ রাখতে ইনিশিয়ালাইজ করা হলো
                Activate = "N"
            };
            IsActive = false;

            // ফর্মে থাকা সমস্ত ফিল্ড রিফ্রেশ করার জন্য খালি স্ট্রিং পাঠানো হলো
            OnPropertyChanged(string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}