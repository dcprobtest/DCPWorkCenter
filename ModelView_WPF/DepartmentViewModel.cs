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
using View_WPF.Model_WPF;

namespace View_WPF.ModelView_WPF
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        // আপনার DataController অনুযায়ী সঠিক API Route (DataController এর জন্য api/Data)
        private readonly string _apiUrl = "https://localhost:7155/api/Data/SaveDepartment";

        private DepartmentModel _dept;
        private bool _isActive;

        public DepartmentViewModel()
        {
            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
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
                // API এর Model এ যদি 'Activate' কলাম থাকে তবে সেখানে Y/N সেট হবে
                if (Dept != null) Dept.Activate = value ? "Y" : "N";
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        private async Task ExecuteSave()
        {
            // বেসিক ভ্যালিডেশন
            if (string.IsNullOrEmpty(Dept.DeptCode))
            {
                MessageBox.Show("Please enter Department Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // JSON এ কনভার্ট করা (নিশ্চিত করুন Dept এর প্রোপার্টিগুলো API এর Department মডেলের সাথে মিলেছে)
                    var json = JsonConvert.SerializeObject(Dept);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // API তে POST রিকোয়েস্ট পাঠানো
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Department Saved Successfully via API!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    else
                    {
                        // API থেকে কোনো এরর আসলে সেটি দেখানো
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Error: {response.StatusCode}\n{errorDetails}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // নেটওয়ার্ক বা কানেকশন এরর হ্যান্ডেল করা
                MessageBox.Show($"Could not connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            // নতুন ডাটা এন্ট্রির জন্য ফর্ম পরিষ্কার করা
            Dept = new DepartmentModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                Activate = "N" // ডিফল্ট 'N'
            };
            IsActive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}