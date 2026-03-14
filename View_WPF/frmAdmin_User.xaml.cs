using System.Windows;
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    public partial class frmAdmin_User : Window
    {
        public frmAdmin_User()
        {
            InitializeComponent();
            // ViewModel কানেক্ট করা হলো
            this.DataContext = new UserViewModel();
        }
    }
}