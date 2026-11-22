using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Model_WPF
{
    // 1. First, define the VideoTreeItem class (this was missing)
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
    /// Core model for tree-view and data management of the DCP EduTrack video player.
    /// </summary>
    public class ClassPlayerModel : INotifyPropertyChanged
    {
        private ObservableCollection<ClassSubjectModel> _subjects;
        public ObservableCollection<ClassSubjectModel> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(); }
        }


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