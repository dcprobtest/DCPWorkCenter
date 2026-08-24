using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace View_WPF.Model_WPF
{
    public class ClassVideoTextModel : INotifyPropertyChanged
    {
        public int ClassID { get; set; }

        public string SectionID { get; set; }

        public string SectionDesc { get; set; }

        public string ClassContent { get; set; }

        public string ClassPath { get; set; }

        public string EntryByUID { get; set; }

        public DateTime? EntryDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastUpdate { get; set; }

        public bool Activate { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}