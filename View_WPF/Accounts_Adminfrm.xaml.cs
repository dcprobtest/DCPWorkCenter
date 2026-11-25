using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Accounts_Adminfrm.xaml
    /// </summary>
    public partial class Accounts_Adminfrm : Window
    {
        private string _userUID = string.Empty;

        private frmAccExpBub expBubForm = null;
        private frmAccExpendHead expendHeadForm = null;
        private frmAccIncomeHead incomeHeadForm = null;
        private frmAccIncSub incSubForm = null;

        public Accounts_Adminfrm()
        {
            InitializeComponent();
        }

        public Accounts_Adminfrm(string uid)
        {
            InitializeComponent();
            this._userUID = uid;
        }

        private bool IsWindowOpen(Window window)
        {
            return System.Windows.Application.Current.Windows.Cast<Window>().Any(w => w == window);
        }

        // Expense Bub button click logic
        private void btnExpBub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (expBubForm == null || !IsWindowOpen(expBubForm))
                {
                    expBubForm = new frmAccExpBub(_userUID);
                    expBubForm.Closed += (s, args) => { expBubForm = null; };
                    expBubForm.Show();
                }
                else
                {
                    if (expBubForm.WindowState == WindowState.Minimized)
                    {
                        expBubForm.WindowState = WindowState.Normal;
                    }
                    expBubForm.Show();
                    expBubForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Expense Bub: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Expend Head button click logic
        private void btnExpendHead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (expendHeadForm == null || !IsWindowOpen(expendHeadForm))
                {
                    expendHeadForm = new frmAccExpendHead(_userUID);
                    expendHeadForm.Closed += (s, args) => { expendHeadForm = null; };
                    expendHeadForm.Show();
                }
                else
                {
                    if (expendHeadForm.WindowState == WindowState.Minimized)
                    {
                        expendHeadForm.WindowState = WindowState.Normal;
                    }
                    expendHeadForm.Show();
                    expendHeadForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Expend Head: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Income Head button click logic
        private void btnIncomeHead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (incomeHeadForm == null || !IsWindowOpen(incomeHeadForm))
                {
                    incomeHeadForm = new frmAccIncomeHead(_userUID);
                    incomeHeadForm.Closed += (s, args) => { incomeHeadForm = null; };
                    incomeHeadForm.Show();
                }
                else
                {
                    if (incomeHeadForm.WindowState == WindowState.Minimized)
                    {
                        incomeHeadForm.WindowState = WindowState.Normal;
                    }
                    incomeHeadForm.Show();
                    incomeHeadForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Income Head: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Income Sub button click logic
        private void btnIncSub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (incSubForm == null || !IsWindowOpen(incSubForm))
                {
                    incSubForm = new frmAccIncSub(_userUID);
                    incSubForm.Closed += (s, args) => { incSubForm = null; };
                    incSubForm.Show();
                }
                else
                {
                    if (incSubForm.WindowState == WindowState.Minimized)
                    {
                        incSubForm.WindowState = WindowState.Normal;
                    }
                    incSubForm.Show();
                    incSubForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Income Sub: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}