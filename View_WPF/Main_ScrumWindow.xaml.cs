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

        // Click event for Assignment Scrum button
        private void btnAssignmentScrum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmAssignmentScrum assignmentScrum = new frmAssignmentScrum(_loggedInUID);

                // Hide the current window
                this.Hide();

                // Subscribe to the Closed event to restore Main_ScrumWindow
                assignmentScrum.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                assignmentScrum.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Assignment Scrum: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure current window remains visible on failure
                this.Show();
            }
        }

        // Click event for Assign Tech Info button
        private void btnAssignTechInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmAssignTechInfo assignTechInfo = new frmAssignTechInfo(_loggedInUID);

                // Hide the current window
                this.Hide();

                // Subscribe to the Closed event to restore Main_ScrumWindow
                assignTechInfo.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                assignTechInfo.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Assign Tech Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure current window remains visible on failure
                this.Show();
            }
        }

        // Click event for Scrum Info button
        private void btnScrumInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frm_ScrumInfo scrumInfo = new frm_ScrumInfo(_loggedInUID);

                // Hide the current window
                this.Hide();

                // Subscribe to the Closed event to restore Main_ScrumWindow
                scrumInfo.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                scrumInfo.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Scrum Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure current window remains visible on failure
                this.Show();
            }
        }

        // Click event for Tech Info button
        private void btnTechInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frm_TechInfo techInfo = new frm_TechInfo(_loggedInUID);

                // Hide the current window
                this.Hide();

                // Subscribe to the Closed event to restore Main_ScrumWindow
                techInfo.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                techInfo.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Tech Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure current window remains visible on failure
                this.Show();
            }
        }
    }
}