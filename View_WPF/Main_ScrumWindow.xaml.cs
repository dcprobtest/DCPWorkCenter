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
    /// Interaction logic for Main_ScrumWindow.xaml
    /// </summary>
    public partial class Main_ScrumWindow : Window
    {
        private string _loggedInUID = string.Empty;

        private frmAssignmentScrum assignmentScrumWindow = null;
        private frmAssignTechInfo assignTechInfoWindow = null;
        private frm_ScrumInfo scrumInfoWindow = null;
        private frm_TechInfo techInfoWindow = null;

        public Main_ScrumWindow()
        {
            InitializeComponent();
        }

        // Overloaded constructor to receive the UID from MainWindow
        public Main_ScrumWindow(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Minimized)
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.WindowState = WindowState.Normal;
                        mainWindow.Show();
                        mainWindow.Activate();
                        break;
                    }
                }
            }
        }

        private bool IsWindowOpen(Window window)
        {
            return System.Windows.Application.Current.Windows.Cast<Window>().Any(w => w == window);
        }

        // Click event for Assignment Scrum button
        private void btnAssignmentScrum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (assignmentScrumWindow == null || !IsWindowOpen(assignmentScrumWindow))
                {
                    assignmentScrumWindow = new frmAssignmentScrum(_loggedInUID);
                    assignmentScrumWindow.Closed += (s, args) => { assignmentScrumWindow = null; };
                    assignmentScrumWindow.Show();
                }
                else
                {
                    if (assignmentScrumWindow.WindowState == WindowState.Minimized)
                    {
                        assignmentScrumWindow.WindowState = WindowState.Normal;
                    }
                    assignmentScrumWindow.Show();
                    assignmentScrumWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Assignment Scrum: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Assign Tech Info button
        private void btnAssignTechInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (assignTechInfoWindow == null || !IsWindowOpen(assignTechInfoWindow))
                {
                    assignTechInfoWindow = new frmAssignTechInfo(_loggedInUID);
                    assignTechInfoWindow.Closed += (s, args) => { assignTechInfoWindow = null; };
                    assignTechInfoWindow.Show();
                }
                else
                {
                    if (assignTechInfoWindow.WindowState == WindowState.Minimized)
                    {
                        assignTechInfoWindow.WindowState = WindowState.Normal;
                    }
                    assignTechInfoWindow.Show();
                    assignTechInfoWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Assign Tech Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Scrum Info button
        private void btnScrumInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scrumInfoWindow == null || !IsWindowOpen(scrumInfoWindow))
                {
                    scrumInfoWindow = new frm_ScrumInfo(_loggedInUID);
                    scrumInfoWindow.Closed += (s, args) => { scrumInfoWindow = null; };
                    scrumInfoWindow.Show();
                }
                else
                {
                    if (scrumInfoWindow.WindowState == WindowState.Minimized)
                    {
                        scrumInfoWindow.WindowState = WindowState.Normal;
                    }
                    scrumInfoWindow.Show();
                    scrumInfoWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Scrum Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Tech Info button
        private void btnTechInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (techInfoWindow == null || !IsWindowOpen(techInfoWindow))
                {
                    techInfoWindow = new frm_TechInfo(_loggedInUID);
                    techInfoWindow.Closed += (s, args) => { techInfoWindow = null; };
                    techInfoWindow.Show();
                }
                else
                {
                    if (techInfoWindow.WindowState == WindowState.Minimized)
                    {
                        techInfoWindow.WindowState = WindowState.Normal;
                    }
                    techInfoWindow.Show();
                    techInfoWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Tech Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}