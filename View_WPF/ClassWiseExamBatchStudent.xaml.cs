using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    public partial class ClassWiseExamBatchStudent : Window
    {
        public ClassWiseExamBatchStudent()
        {
            InitializeComponent();

            // Connecting the View to the ViewModel
            this.DataContext = new ClassWiseExamBatchStudentViewModel();
        }
    }
}