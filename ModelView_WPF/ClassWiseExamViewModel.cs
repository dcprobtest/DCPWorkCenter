using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Collections.Generic;
using DCPWorkCenter.Model_WPF;
using System.Linq;

namespace ModelView_WPF
{
    public class ClassWiseExamViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("https://localhost:7073/api/") };

        private string _mcqId;
        public string McqId { get => _mcqId; set { _mcqId = value; OnPropertyChanged(); } }

        private DateTime? _entryDate = DateTime.Now;
        public DateTime? EntryDate { get => _entryDate; set { _entryDate = value; OnPropertyChanged(); } }

        private string _question;
        public string Question { get => _question; set { _question = value; OnPropertyChanged(); } }

        public string Ans1 { get; set; }
        public string Ans2 { get; set; }
        public string Ans3 { get; set; }
        public string Ans4 { get; set; }
        public string RightAns { get; set; }

        public ObservableCollection<ClassWiseExamModel> SavedMCQs { get; set; }
        public ObservableCollection<string> Sections { get; set; }

        private string _selectedSection;
        public string SelectedSection { get => _selectedSection; set { _selectedSection = value; OnPropertyChanged(); } }

        public ICommand CreateMCQCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ClassWiseExamViewModel()
        {
            SavedMCQs = new ObservableCollection<ClassWiseExamModel>();
            Sections = new ObservableCollection<string>();

            SaveCommand = new RelayCommand(async (o) => await SaveData());
            CreateMCQCommand = new RelayCommand(CreateNewMCQ);

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                var sectionResponse = await client.GetStringAsync("ClassWiseExam/Sections");
                var sectionList = JsonConvert.DeserializeObject<List<string>>(sectionResponse);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Sections.Clear();
                    if (sectionList != null) foreach (var item in sectionList) Sections.Add(item);
                });

                var mcqResponse = await client.GetStringAsync("ClassWiseExam");
                var mcqList = JsonConvert.DeserializeObject<List<ClassWiseExamModel>>(mcqResponse);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SavedMCQs.Clear();
                    if (mcqList != null) foreach (var item in mcqList) SavedMCQs.Add(item);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private async Task SaveData()
        {
            try
            {
                var newExam = new
                {
                    MCQID = McqId,
                    EntryDate = EntryDate,
                    SectionID = SelectedSection,
                    Question = Question,
                    Ans1 = Ans1,
                    Ans2 = Ans2,
                    Ans3 = Ans3,
                    Ans4 = Ans4,
                    RightAns = RightAns,
                    Activate = "Y"
                };

                var content = new StringContent(JsonConvert.SerializeObject(newExam), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("ClassWiseExam", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("MCQ Saved Successfully!");
                    await LoadInitialData();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Server Error: " + error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }

        private void CreateNewMCQ(object obj)
        {
            int nextId = 1;

            if (SavedMCQs != null && SavedMCQs.Count > 0)
            {
                // Find the highest numeric value from existing MCQIDs (assuming format Q-0001)
                var lastId = SavedMCQs
                    .Select(m => m.MCQID)
                    .Where(id => id.StartsWith("Q-"))
                    .Select(id => id.Replace("Q-", ""))
                    .Select(numStr => { int.TryParse(numStr, out int val); return val; })
                    .DefaultIfEmpty(0)
                    .Max();

                nextId = lastId + 1;
            }

            McqId = $"Q-{nextId:D4}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}