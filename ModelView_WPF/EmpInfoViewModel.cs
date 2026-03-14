using ModelView_WPF;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using View_WPF.Model;

namespace View_WPF.ModelView_WPF
{
    public class EmpInfoViewModel : INotifyPropertyChanged
    {
        private readonly string _baseUrl = "https://localhost:7169/api/EmpInfo/";

        public EmpInfoModel Emp { get; set; } = new EmpInfoModel();

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }

        public ICommand SaveCommand { get; }

        public EmpInfoViewModel()
        {
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            Emp.Activate = IsActive ? "Y" : "N";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(Emp);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_baseUrl + "SaveEmployee", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Saved Successfully!");
                        // প্রয়োজনে এখানে Reset logic যোগ করতে পারেন
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Validation Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}