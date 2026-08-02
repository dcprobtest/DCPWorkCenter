using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class Student
    {
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime AssignDate { get; set; }
    }

    public class ClassWiseExamBatchStudentViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        public ClassWiseExamBatchStudentViewModel()
        {
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            StudentList = new ObservableCollection<Student>();
            AssignedStudentList = new ObservableCollection<Student>();

            LoadStudents();

            // বাটন কমান্ডসমূহ
            AssignCommand = new RelayCommand(async o => await AssignStudent(), o => SelectedStudent != null);
            RemoveCommand = new RelayCommand(async o => await RemoveStudent(), o => SelectedAssignedStudent != null);
            SaveCommand = new RelayCommand(o => SaveChanges());
            ExitCommand = new RelayCommand(o => ExitApplication());
        }

        private async void LoadStudents()
        {
            try
            {
                // Loading students using ApiSettings endpoint
                var students = await _httpClient.GetFromJsonAsync<List<Student>>(ApiSettings.GetClassWiseExamBatchStudentsUrl());
                if (students != null)
                {
                    StudentList.Clear();
                    foreach (var student in students)
                    {
                        StudentList.Add(student);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}");
            }
        }

        public ObservableCollection<Student> StudentList { get; set; }
        public ObservableCollection<Student> AssignedStudentList { get; set; }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }

        private Student _selectedAssignedStudent;
        public Student SelectedAssignedStudent
        {
            get => _selectedAssignedStudent;
            set { _selectedAssignedStudent = value; OnPropertyChanged(); }
        }

        public ICommand AssignCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExitCommand { get; }

        private async Task AssignStudent()
        {
            if (SelectedStudent == null) return;

            var student = SelectedStudent;

            // এখানে CWBSID যোগ করে দেওয়া হয়েছে যাতে API এর রিকোয়ারমেন্ট পূরণ হয়
            var payload = new
            {
                CWBSID = Guid.NewGuid().ToString().Substring(0, 10), // ইউনিক আইডি জেনারেট করা হলো
                BatchID = student.StudentID,
                BatchDate = DateTime.Now,
                Activate = "s"
            };

            // Saving assigned student using ApiSettings endpoint
            var response = await _httpClient.PostAsJsonAsync(ApiSettings.SaveClassWiseExamBatchStudentUrl(), payload);

            if (response.IsSuccessStatusCode)
            {
                student.AssignDate = DateTime.Now;
                AssignedStudentList.Add(student);
                StudentList.Remove(student);
                SelectedStudent = null;
            }
            else
            {
                // এখানে সার্ভার থেকে আসা আসল এরর মেসেজটি দেখা যাবে
                string errorDetail = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Failed to save to database.\nStatus: {response.StatusCode}\nDetails: {errorDetail}");
            }
        }

        private async Task RemoveStudent()
        {
            if (SelectedAssignedStudent == null) return;

            var student = SelectedAssignedStudent;

            // Deleting assigned student using ApiSettings endpoint
            var response = await _httpClient.DeleteAsync(ApiSettings.DeleteClassWiseExamBatchStudentUrl(student.StudentID));

            if (response.IsSuccessStatusCode)
            {
                StudentList.Add(student);
                AssignedStudentList.Remove(student);
                SelectedAssignedStudent = null;
            }
            else
            {
                MessageBox.Show("Error removing from database.");
            }
        }

        private void SaveChanges()
        {
            MessageBox.Show("Changes Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}