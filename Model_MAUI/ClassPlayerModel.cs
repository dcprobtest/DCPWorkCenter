using Model_MAUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_MAUI
{
    // ১. প্রথমে VideoTreeItem ক্লাসটি ডিফাইন করতে হবে (এটিই আপনি মিস করেছিলেন)
    public class VideoTreeItem : INotifyPropertyChanged
    {
        private string _videoTitle = string.Empty;
        private string _filePath = string.Empty;

        public string VideoTitle
        {
            get => _videoTitle;
            set { _videoTitle = value; OnPropertyChanged(); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// DCP EduTrack ভিডিও প্লেয়ারের ট্রি-ভিউ এবং ডাটা ম্যানেজমেন্টের মূল মডেল।
    /// </summary>
    public class ClassPlayerModel : INotifyPropertyChanged
    {
        private ObservableCollection<ClassSubjectModel> _subjects;
        public ObservableCollection<ClassSubjectModel> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(); }
        }

        // এখন এই VideoTreeItem প্রোপার্টিটি এরর দেখাবে না
        private VideoTreeItem? _selectedVideo;
        public VideoTreeItem? SelectedVideo
        {
            get => _selectedVideo;
            set { _selectedVideo = value; OnPropertyChanged(); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        public ClassPlayerModel()
        {
            Subjects = new ObservableCollection<ClassSubjectModel>();
            IsPlaying = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}