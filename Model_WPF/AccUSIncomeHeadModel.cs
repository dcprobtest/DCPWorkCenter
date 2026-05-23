using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_WPF
{
    public class AccUSIncomeHeadModel : INotifyPropertyChanged
    {
        private string _incCode;
        private string _incDetail;
        private string _uid;
        private DateTime? _entryDate;

        // database column: IncCode char(2) Unchecked
        public string IncCode
        {
            get => _incCode;
            set
            {
                if (_incCode != value)
                {
                    _incCode = value;
                    OnPropertyChanged();
                }
            }
        }

        // database column: IncDetail varchar(25) Checked
        public string IncDetail
        {
            get => _incDetail;
            set
            {
                if (_incDetail != value)
                {
                    _incDetail = value;
                    OnPropertyChanged();
                }
            }
        }

        // database column: UID varchar(5) Checked
        public string UID
        {
            get => _uid;
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    OnPropertyChanged();
                }
            }
        }

        // database column: EntryDate datetime Checked
        public DateTime? EntryDate
        {
            get => _entryDate;
            set
            {
                if (_entryDate != value)
                {
                    _entryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}