using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_WPF
{
    public class ScrumInfoModel : INotifyPropertyChanged
    {
        private string _scrumID;
        private string _scrumDesc;
        private DateTime? _entryDate;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _entryByUID;
        private DateTime? _lastUpdate;
        private string _activate;

        // ScrumID - varchar(5), Cannot be null (Unchecked)
        public string ScrumID
        {
            get => _scrumID;
            set
            {
                _scrumID = value;
                OnPropertyChanged();
            }
        }

        // ScrumDesc - varchar(500), Can be null (Checked)
        public string ScrumDesc
        {
            get => _scrumDesc;
            set
            {
                _scrumDesc = value;
                OnPropertyChanged();
            }
        }

        // EntryDate - datetime, Can be null (Checked)
        public DateTime? EntryDate
        {
            get => _entryDate;
            set
            {
                _entryDate = value;
                OnPropertyChanged();
            }
        }

        // StartDate - datetime, Can be null (Checked)
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        // EndDate - datetime, Can be null (Checked)
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        // EntryByUID - varchar(6), Can be null (Checked)
        public string EntryByUID
        {
            get => _entryByUID;
            set
            {
                _entryByUID = value;
                OnPropertyChanged();
            }
        }

        // LastUpdate - datetime, Can be null (Checked)
        public DateTime? LastUpdate
        {
            get => _lastUpdate;
            set
            {
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }

        // Activate - char(1), Can be null (Checked)
        // Stores database value directly (e.g., "Y" / "N" or "1" / "0")
        public string Activate
        {
            get => _activate;
            set
            {
                _activate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsActivated));
            }
        }

        // Helper property to bind directly with the WPF CheckBox IsChecked property
        public bool IsActivated
        {
            get => _activate == "Y" || _activate == "1";
            set => Activate = value ? "Y" : "N";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}