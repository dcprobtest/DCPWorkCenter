using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_ClassSubject.xaml
    /// </summary>
    public partial class frmClass_ClassSubject : Window
    {
        
        private string _uID;

        public frmClass_ClassSubject()
        {
            InitializeComponent();

            if (this.DataContext == null)
            {
                this.DataContext = new ClassSubjectViewModel();
            }
        }

        public frmClass_ClassSubject(string uid)
        {
            InitializeComponent();
            _uID = uid;

            this.DataContext = new ClassSubjectViewModel(_uID);
        }
    }
}