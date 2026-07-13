using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_MAUI
{
    public class TechInfoModel : INotifyPropertyChanged
    {
        public string TechId { get; set; }
        public string TechDesc { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EntryByUid { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Activate { get; set; } = "N";

        public bool IsActivated
        {
            get => Activate == "Y";
            set
            {
                Activate = value ? "Y" : "N";
                OnPropertyChanged(nameof(IsActivated));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // এখানে protected থেকে public করা হয়েছে
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}