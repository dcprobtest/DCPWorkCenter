using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_MAUI
{
    /// <summary>
    /// ClassSubject টেবিলের জন্য ডাটা মডেল।
    /// </summary>
    public class ClassSubjectModel : INotifyPropertyChanged
    {
        private string _subjectCode = string.Empty;
        private string? _subjectDesc;
        private string? _activate;
        private ObservableCollection<ClassSectionModel> _sections;

        // SQL: varchar(2) - Primary Key
        public string SubjectCode
        {
            get => _subjectCode;
            set { _subjectCode = value; OnPropertyChanged(); }
        }

        // SQL: varchar(200) - Subject Description
        public string? SubjectDesc
        {
            get => _subjectDesc;
            set { _subjectDesc = value; OnPropertyChanged(); }
        }

        // SQL: char(1) - "1" for Active, "0" for Inactive
        public string? Activate
        {
            get => _activate;
            set { _activate = value; OnPropertyChanged(); }
        }

        // ট্রি-ভিউতে সেকশনগুলো দেখানোর জন্য নেস্টেড লিস্ট
        public ObservableCollection<ClassSectionModel> Sections
        {
            get => _sections;
            set { _sections = value; OnPropertyChanged(); }
        }

        // অন্যান্য ডাটাবেস কলামসমূহ
        public DateTime? EntryDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EntryByUID { get; set; }
        public DateTime? LastUpdate { get; set; }

        public ClassSubjectModel()
        {
            Sections = new ObservableCollection<ClassSectionModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}