using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_MAUI
{
    public class AccUSIncSubModel : INotifyPropertyChanged
    {
        private string _incSubCode;
        private string _incDetail;
        private string _incCode;
        private string _uid;
        private DateTime? _entryDate;

        // IncSubCode: char(4), Primary Key (Unchecked / Not Null)
        public string IncSubCode
        {
            get => _incSubCode;
            set
            {
                if (_incSubCode != value)
                {
                    _incSubCode = value;
                    OnPropertyChanged();
                }
            }
        }

        // IncDetail: varchar(30) (Checked / Nullable)
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

        // IncCode: char(2) (Checked / Nullable)
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

        // UID: varchar(5) (Checked / Nullable)
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

        // EntryDate: datetime (Checked / Nullable)
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