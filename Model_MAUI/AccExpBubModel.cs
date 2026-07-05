using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_MAUI
{
    public class AccExpBubModel : INotifyPropertyChanged
    {
        private string _expSubCode;
        private string _expDetail;
        private string _expCode;
        private string _uid;
        private DateTime _entryDate = DateTime.Now;

        // ExpSubCode char(4)
        public string ExpSubCode
        {
            get => _expSubCode;
            set { _expSubCode = value; OnPropertyChanged(); }
        }

        // ExpDetail varchar(30)
        public string ExpDetail
        {
            get => _expDetail;
            set { _expDetail = value; OnPropertyChanged(); }
        }

        // ExpCode char(2)
        public string ExpCode
        {
            get => _expCode;
            set { _expCode = value; OnPropertyChanged(); }
        }

        // UID varchar(5)
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        // EntryDate datetime
        public DateTime EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}