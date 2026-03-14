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
    public class UserDocInfoViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "https://localhost:7169/api/UserDocInfo/SaveUserDoc";

        public string UserDocID { get; set; }
        public string UID { get; set; }
        public string DocType { get; set; }
        public string DocPath { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }

        public UserDocInfoViewModel()
        {
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            if (UserDocID?.Length > 12) { MessageBox.Show("User Doc ID max 12 chars!"); return; }
            if (UID?.Length > 6) { MessageBox.Show("UID max 6 chars!"); return; }
            if (DocPath?.Length > 100) { MessageBox.Show("Path is too long (Max 100)!"); return; }

            var data = new
            {
                UserDocID,
                UID,
                DocType,
                DocPath,
                EntryDate,
                StartDate,
                EndDate,
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
                        MessageBox.Show("Document Info Saved Successfully!");
                    else
                        MessageBox.Show("Error: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}