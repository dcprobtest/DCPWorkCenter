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
    public class SubjectWiseSectionAssignViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "https://localhost:7169/api/SubjectWiseSectionAssign/SaveSubjectWiseSection";

        public string UID { get; set; }
        public string SectionID { get; set; }
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }

        public SubjectWiseSectionAssignViewModel()
        {
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            if (UID?.Length > 6) { MessageBox.Show("UID max 6 chars!"); return; }
            if (SectionID?.Length > 10) { MessageBox.Show("Section ID max 10 chars!"); return; }

            var data = new
            {
                UID,
                SectionID,
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
                        MessageBox.Show("Assigned Successfully!");
                    else
                        MessageBox.Show("Error from API!");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}